using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PushbackHelper
{
    class TugManager
    {
        // Tug geometry: the tug pushes or pulls the nose gear at an angle to the aircraft, and the nose follows
        // the tug at tug speed. The longitudinal speed is speed × cos(angle) and the yaw rate is
        // speed × sin(angle) / NoseGearDistance, so a higher rotation level turns tighter without going faster.
        // Rotation level 1, 2, 3 → tug angle 20°, 40°, 60°, kept below the towing limits of airliners.
        private const double TugAnglePerRotationLevel = 20;
        private const double MaxTugAngle = 60;
        // Distance from the aircraft's reference point to the nose gear; about 14 m, between A320 and A321.
        private const double NoseGearDistanceFeet = 45;
        // Tug speed setting (5 to 50) to feet per second along the nose gear's path.
        private const double SpeedToFeetPerSecond = .2;

        private TugStatus _status;
        public TugStatus Status { get { return _status; } private set { _status = value; TugStatusEvent?.Invoke(value, TugRotationSetting); } }
        public event TugStatusChanged TugStatusEvent;
        public bool TugActive { get { return Status != TugStatus.Disabled; } }
        private SimConnectManager myManager;
        private double tugHeading;
        private double tugSpeed;
        public uint SpeedFactor { get; private set; }
        public double TugRotationSetting { get; private set; } = 0;
        private bool parkingBrakeSet;
        private readonly DispatcherTimer fastTimer;

        public TugManager(SimConnectManager manager)
        {
            Status = TugStatus.Disabled;
            myManager = manager;
            myManager.ConnectStatusEvent += MyManager_ConnectStatusEvent;
            myManager.DataRxEvent += MyManager_DataRxEvent;
            fastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            fastTimer.Tick += FastTimer_Tick;
            parkingBrakeSet = false;
            SpeedFactor = 25;
        }
        public void SetSpeed(uint value5to50)
        {
            SpeedFactor = value5to50;
            switch (Status)
            {
                case TugStatus.Forward:
                    SetTugSpeed(SpeedFactor);
                    break;
                case TugStatus.Reverse:
                    SetTugSpeed(-SpeedFactor);
                    break;
            }
        }
        public void Disable()
        {
            if (TugActive)
            {
                myManager.TransmitEvent(EventsEnum.TOGGLE_PUSHBACK, 1);
                myManager.SetData(DefinitionsEnum.PushbackWait, 0);
                fastTimer.Stop();
                SetTugSpeed(0);
                SetTugRotation(0);
            }
        }
        public void Enable()
        {
            if (!TugActive)
            {
                StartTugMode();
                myManager.TransmitEvent(EventsEnum.TOGGLE_PUSHBACK, 1);
            }
        }
        public void Stop()
        {
            SetTugSpeed(0);
        }

        public void Forward()
        {
            switch (Status)
            {
                case TugStatus.Forward:
                    SetTugRotation(0);
                    break;
                case TugStatus.Reverse:
                    SetTugSpeed(0);
                    break;
                case TugStatus.Waiting:
                    SetTugSpeed(SpeedFactor);
                    break;
            }
        }
        public void Reverse()
        {
            switch (Status)
            {
                case TugStatus.Reverse:
                    SetTugRotation(0);
                    break;
                case TugStatus.Forward:
                    SetTugSpeed(0);
                    break;
                case TugStatus.Waiting:
                    SetTugSpeed(-SpeedFactor);
                    break;
            }
        }
        public void Left()
        {
            if (Status == TugStatus.Reverse || Status == TugStatus.Forward)
            {
                SetTugRotation(-1);
            }
        }
        public void Right()
        {
            if (Status == TugStatus.Reverse || Status == TugStatus.Forward)
            {
                SetTugRotation(1);
            }
        }
        /// <summary>The parking brake as ServicesManager reads it, standard or the aircraft's own.</summary>
        public void SetParkingBrake(bool on)
        {
            if (parkingBrakeSet == on) return;
            parkingBrakeSet = on;
            switch (Status)
            {
                case TugStatus.Forward:
                    SetTugSpeed(SpeedFactor);
                    break;
                case TugStatus.Reverse:
                    SetTugSpeed(-SpeedFactor);
                    break;
            }
        }
        private void MyManager_ConnectStatusEvent(bool Connected)
        {
            try
            {
                if (!Connected)
                {
                    Status = TugStatus.Disabled;
                    fastTimer.Stop();
                }
            }
            catch (Exception) { }
        }
        private void MyManager_DataRxEvent(RequestsEnum request, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            try
            {
                if (request == RequestsEnum.RefreshDataRequest)
                {
                    RefreshDataStruct receivedData = (RefreshDataStruct)data.dwData[0];
                    tugHeading = receivedData.trueHeading;

                    if (receivedData.pushbackState == 3)
                        Status = TugStatus.Disabled;
                    else if (receivedData.pushbackState == 0 && !TugActive)
                    {
                        Status = TugStatus.Waiting;
                        StartTugMode();
                    }
                }
                else if (request == RequestsEnum.HeadingRequest)
                {
                    tugHeading = (double)data.dwData[0];
                }
                else if (request == RequestsEnum.StartTugMode)
                {
                    RefreshDataStruct receivedData = (RefreshDataStruct)data.dwData[0];
                    Status = TugStatus.Waiting;
                }
            }
            catch (Exception) { }
        }
        private void StartTugMode()
        {
            if (myManager.Connected)
            {
                fastTimer.Start();
                tugSpeed = 0;
            }
        }
        private void SetTugHeading()
        {
            if (tugSpeed == 0)
            {
                myManager.SetData(DefinitionsEnum.RotationY, 0.0);
            }
            else
            {
                // Same sides and signs as the former fixed 90° tug: a positive (right) setting puts the tug at
                // heading - angle, and the yaw rate takes the sign of setting × speed.
                double angle = TugAngleDegrees();
                double heading = (tugHeading - angle) % 360;
                if (heading < 0)
                    heading += 360;

                double yawRate = tugSpeed * SpeedToFeetPerSecond * Math.Sin(angle * Math.PI / 180) / NoseGearDistanceFeet;

                myManager.TransmitEvent(EventsEnum.SET_TUG_HEADING, 11930465 * (uint)heading);
                myManager.SetData(DefinitionsEnum.RotationY, yawRate);
            }
        }
        /// <summary>The tug's angle to the aircraft in degrees, positive for a right setting.</summary>
        private double TugAngleDegrees()
        {
            double angle = TugRotationSetting * TugAnglePerRotationLevel;
            return Math.Max(-MaxTugAngle, Math.Min(MaxTugAngle, angle));
        }
        private async void SetTugSpeed(double targetSpeed)
        {
            if (TugActive)
            {
                double startSpeed = tugSpeed;
                double adjustedSpeed;

                if (parkingBrakeSet)
                    adjustedSpeed = targetSpeed * .1;
                else
                    adjustedSpeed = targetSpeed;

                if (targetSpeed == 0)
                    Status = TugStatus.Waiting;
                else if (targetSpeed > 0)
                    Status = TugStatus.Forward;
                else if (targetSpeed < 0)
                    Status = TugStatus.Reverse;

                for (int i = 9; i > 0; i--)
                {
                    tugSpeed = adjustedSpeed + (startSpeed - adjustedSpeed) * i / 10;
                    await Task.Delay(100);
                }
                tugSpeed = adjustedSpeed;
            }
        }
        private async void SetTugRotation(double targetRotation)
        {
            // Initial check if there is work to do
            if (TugRotationSetting == 0 && targetRotation == 0)
            {
                return;
            }

            // Init

            double stepSize = targetRotation / 10;

            double startRotation = TugRotationSetting;
            TugStatus originalStatus = Status;

            // Check if we've exceeded the max already
            if (Math.Abs(TugRotationSetting + targetRotation) > 3)
            {
                return;
            }
            else
            {
                if (targetRotation == 0)
                {
                    stepSize = -TugRotationSetting / 10;
                    TugRotationSetting = 0;
                }
                else
                {
                    TugRotationSetting += targetRotation;
                }
            }

            double newTargetRotation = TugRotationSetting;

            Status = TugStatus.ProcessingTurn;

            // Gradually - in 10 steps

            for (int i = 1; i <= 10; i++) 
            {
                TugRotationSetting = startRotation + (stepSize * i);
                await Task.Delay(50);
            }

            Status = originalStatus;
        }
        private void FastTimer_Tick(object sender, EventArgs e)
        {
            if (TugActive)
            {
                myManager.SetData(DefinitionsEnum.PushbackWait, tugSpeed == 0 ? 1 : 0);
                myManager.SetData(DefinitionsEnum.VelocityX, 0.0);
                myManager.SetData(DefinitionsEnum.VelocityY, 0.0);
                myManager.SetData(DefinitionsEnum.VelocityZ, tugSpeed * SpeedToFeetPerSecond * Math.Cos(TugAngleDegrees() * Math.PI / 180));
                myManager.SetData(DefinitionsEnum.RotationX, 0.0);
                myManager.SetData(DefinitionsEnum.RotationZ, 0.0);
                SetTugHeading();
            }
        }

        public delegate void TugStatusChanged(TugStatus Status, double TugRotationSpeed);

        public enum TugStatus
        {
            Disabled,
            Waiting,
            Forward,
            Reverse,
            ProcessingTurn
        }
    }
}
