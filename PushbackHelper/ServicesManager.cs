using System;
namespace PushbackHelper
{
    class ServicesManager
    {
        public event BoolChanged ParkingBrakeEvent;
        private bool _parkingBrakeOn;
        public bool ParkingBrakeOn { get { return _parkingBrakeOn; } private set { _parkingBrakeOn = value; ParkingBrakeEvent?.Invoke(value); } }
        private SimConnectManager myManager;
        public ServicesManager(SimConnectManager manager)
        {
            myManager = manager;
            myManager.DataRxEvent += MyManager_DataRxEvent;
        }

        private void MyManager_DataRxEvent(RequestsEnum request, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            try
            {
                if (request == RequestsEnum.RefreshDataRequest)
                {
                    RefreshDataStruct receivedData = (RefreshDataStruct)data.dwData[0];

                    if (ParkingBrakeOn != receivedData.parkingBrakeState)
                        ParkingBrakeOn = receivedData.parkingBrakeState;
                }
            }
            catch (Exception) { }
        }

        public void ToggleJetway()
        {
            myManager.TransmitEvent(EventsEnum.TOGGLE_JETWAY, 1);
        }
        public void ToggleFuel()
        {
            myManager.TransmitEvent(EventsEnum.REQUEST_FUEL, 1);
        }
        public void ToggleRampTruck()
        {
            myManager.TransmitEvent(EventsEnum.TOGGLE_RAMPTRUCK, 1);
        }
        public void RequestLuggage()
        {
            myManager.TransmitEvent(EventsEnum.REQUEST_LUGGAGE, 1);
        }
        public void RequestPowerSupply()
        {
            myManager.TransmitEvent(EventsEnum.REQUEST_POWER_SUPPLY, 1);
        }
        public void RequestCatering()
        {
            myManager.TransmitEvent(EventsEnum.REQUEST_CATERING, 1);
        }
        public void ToggleParkingBrake()
        {
            myManager.TransmitEvent(EventsEnum.TOGGLE_PARKING_BRAKES, 1);
        }

        public delegate void BoolChanged(bool value);
    }
}
