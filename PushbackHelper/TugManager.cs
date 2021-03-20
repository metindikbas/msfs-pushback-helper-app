using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PushbackHelper
{
    class TugManager
    {
        private TugStatus _status;
        public TugStatus Status { get { return _status; } private set { _status = value; TugStatusEvent?.Invoke(value); } }
        public event TugStatusChanged TugStatusEvent;
        public bool TugActive { get { return Status != TugStatus.Disabled; } }
        private SimConnectManager myManager;
        private double tugHeading;
        private double tugSpeed;
        private double tugRotationActual;
        private double tugRotationSetting;
        public uint SpeedFactor { get; private set; }
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

                    if (parkingBrakeSet != receivedData.parkingBrakeState)
                    {
                        parkingBrakeSet = receivedData.parkingBrakeState;
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

                    if (receivedData.pushbackState == 3)
                        Status = TugStatus.Disabled;
                    else if (receivedData.pushbackState == 0 && !TugActive)
                    {
                        Status = TugStatus.Waiting;
                        StartTugMode();
                    }
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
                myManager.SetData(DefinitionsEnum.RotationY, 0);
            }
            else
            {
                uint direction = 0;
                if (tugRotationSetting > 0)
                    direction = 270;
                else if (tugRotationSetting < 0)
                    direction = 90;

                var heading = (uint)tugHeading;
                heading += direction;
                heading %= 360;

                myManager.TransmitEvent(EventsEnum.SET_TUG_HEADING, 11930465 * heading);
                myManager.SetData(DefinitionsEnum.RotationY, tugRotationActual * .002 * tugSpeed);
            }
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
            double startRotation = tugRotationActual;
            tugRotationSetting = targetRotation;
            for (int i = 9; i > 0; i--)
            {
                tugRotationActual = targetRotation + (startRotation - targetRotation) * i / 10;
                await Task.Delay(200);
            }
            tugRotationActual = targetRotation;
        }
        private void FastTimer_Tick(object sender, EventArgs e)
        {
            if (TugActive)
            {
                myManager.SetData(DefinitionsEnum.PushbackWait, tugSpeed == 0 ? 1 : 0);
                myManager.SetData(DefinitionsEnum.VelocityX, 0);
                myManager.SetData(DefinitionsEnum.VelocityY, 0);
                myManager.SetData(DefinitionsEnum.VelocityZ, tugSpeed * .2);
                myManager.SetData(DefinitionsEnum.RotationX, 0);
                myManager.SetData(DefinitionsEnum.RotationZ, 0);
                SetTugHeading();
            }
        }

        public delegate void TugStatusChanged(TugStatus Status);

        public enum TugStatus
        {
            Disabled,
            Waiting,
            Forward,
            Reverse
        }
    }
}
