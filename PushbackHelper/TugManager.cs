using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.FlightSimulator.SimConnect;

namespace PushbackHelper
{
    class TugManager
    {
        private TugStatus _status;
        public TugStatus Status { get { return _status; } private set { _status = value; TugStatusEvent?.Invoke(value); } }
        public event TugStatusChanged TugStatusEvent;
        public bool TugActive {get {return Status != TugStatus.Disabled;} }
        private SimConnectManager myManager;
        private double tugHeading;
        private double tugVelocity;
        private double tugRotation;
        private double tugVelocitySet;
        private double tugRotationSet;
        private bool parkingBrakeSet;
        private readonly DispatcherTimer fastTimer;
        public TugManager(SimConnectManager manager)
        {
            Status = TugStatus.Disabled;
            myManager = manager;
            myManager.ConnectStatusEvent += MyManager_ConnectStatusEvent;
            myManager.DataRxEvent += MyManager_DataRxEvent;
            fastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            fastTimer.Tick += FastTimer_Tick;
            parkingBrakeSet = false;
        }
        public void Disable()
        {
            if (TugActive)
            {
                myManager.TransmitEvent(EventsEnum.KEY_PUSHBACK_SET, 1);
                myManager.SetData(DefinitionsEnum.PushbackWait, 0);
                fastTimer.Stop();
                SetTugVelocity(0);
                SetTugRotation(0);
            }
        }
        public void Enable()
        {
            if (!TugActive)
            {
                StartTugMode();
                myManager.TransmitEvent(EventsEnum.KEY_PUSHBACK_SET, 1);
            }
        }
        public void Stop()
        {
            SetTugVelocity(0);
        }
        public void Forward()
        {
            switch (Status)
            {
                case TugStatus.Forward:
                    SetTugRotation(0);
                    break;
                case TugStatus.Reverse:
                    SetTugVelocity(0);
                    break;
                case TugStatus.Waiting:
                    SetTugVelocity(1);
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
                    SetTugVelocity(0);
                    break;
                case TugStatus.Waiting:
                    SetTugVelocity(-1);
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
        private void MyManager_DataRxEvent(RequestsEnum request, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            try
            {
                if (request == RequestsEnum.RefreshDataRequest)
                {
                    RefreshDataStruct receivedData = (RefreshDataStruct)data.dwData[0];
                    tugHeading = receivedData.trueHeading;

                    if(parkingBrakeSet != receivedData.parkingBrakeState)
                    {
                        parkingBrakeSet = receivedData.parkingBrakeState;
                        SetTugVelocity(tugVelocitySet);
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
                myManager.RequestData(RequestsEnum.StartTugMode, DefinitionsEnum.RefreshDataStruct);
                tugVelocity = 0;
            }
        }
        private void SetTugHeading()
        {
            if (tugVelocity == 0)
            {
                myManager.SetData(DefinitionsEnum.RotationY, 0);
            }
            else
            {
                uint direction = 0;
                if (tugRotationSet > 0)
                    direction = 270;
                else if (tugRotationSet < 0)
                    direction = 90;

                var heading = (uint)tugHeading;
                heading += direction;
                heading %= 360;

                myManager.TransmitEvent(EventsEnum.KEY_TUG_HEADING, 11930465 * heading);
                myManager.SetData(DefinitionsEnum.RotationY, tugRotation * .1 * tugVelocity);
            }
        }
        private async void SetTugVelocity(double targetVelocity)
        {
            if (TugActive)
            {
                double startVelocity = tugVelocity;
                tugVelocitySet = targetVelocity;
                double adjustedVelocity;

                if (parkingBrakeSet)
                    adjustedVelocity = targetVelocity * .1;
                else
                    adjustedVelocity = targetVelocity;

                if (tugVelocitySet == 0)
                    Status = TugStatus.Waiting;
                else if (tugVelocitySet > 0)
                    Status = TugStatus.Forward;
                else if (tugVelocitySet < 0)
                    Status = TugStatus.Reverse;

                for (int i = 9; i > 0; i--)
                {
                    tugVelocity = adjustedVelocity + (startVelocity - adjustedVelocity) * i / 10;
                    await Task.Delay(100);
                }
                tugVelocity = adjustedVelocity;
            }
        }
        private async void SetTugRotation(double targetRotation)
        {
            double startRotation = tugRotation;
            tugRotationSet = targetRotation;
            for (int i = 9; i > 0; i--)
            {
                tugRotation = targetRotation + (startRotation - targetRotation) * i / 10;
                await Task.Delay(100);
            }
            tugRotation = targetRotation;
        }
        private void FastTimer_Tick(object sender, EventArgs e)
        {
            if (TugActive)
            {
                myManager.SetData(DefinitionsEnum.PushbackWait, tugVelocity == 0 ? 1 : 0);
                myManager.SetData(DefinitionsEnum.VelocityX, 0);
                myManager.SetData(DefinitionsEnum.VelocityY, 0);
                myManager.SetData(DefinitionsEnum.VelocityZ, tugVelocity * 8);
                myManager.SetData(DefinitionsEnum.RotationX, .0);
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
