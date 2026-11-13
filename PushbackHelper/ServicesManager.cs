using System;
using System.Diagnostics;
namespace PushbackHelper
{
    class ServicesManager
    {
        public event BoolChanged ParkingBrakeEvent;
        private bool _parkingBrakeOn;
        public bool ParkingBrakeOn { get { return _parkingBrakeOn; } private set { _parkingBrakeOn = value; ParkingBrakeEvent?.Invoke(value); } }
        private SimConnectManager myManager;
        // The aircraft, and its parking brake profile; null reads the standard BRAKE PARKING POSITION.
        private string aircraftTitle = "";
        private string aircraftPath = "";
        private ParkingBrakeProfile parkingBrakeProfile;
        private bool standardParkingBrake;

        public ServicesManager(SimConnectManager manager)
        {
            myManager = manager;
            myManager.DataRxEvent += MyManager_DataRxEvent;
            myManager.AircraftPathEvent += MyManager_AircraftPathEvent;
            myManager.ConnectStatusEvent += MyManager_ConnectStatusEvent;
        }

        private void MyManager_DataRxEvent(RequestsEnum request, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            try
            {
                if (request == RequestsEnum.RefreshDataRequest)
                {
                    RefreshDataStruct receivedData = (RefreshDataStruct)data.dwData[0];
                    standardParkingBrake = receivedData.parkingBrakeState;

                    if (parkingBrakeProfile == null)
                        SetParkingBrakeState(standardParkingBrake);
                }
                else if (request == RequestsEnum.ParkingBrakeLVarRequest)
                {
                    if (parkingBrakeProfile != null)
                        SetParkingBrakeState(parkingBrakeProfile.IsSet((double)data.dwData[0]));
                }
                else if (request == RequestsEnum.AircraftTitleRequest)
                {
                    aircraftTitle = ((AircraftTitleStruct)data.dwData[0]).title ?? "";
                    ResolveParkingBrakeProfile();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[PushbackHelper] Services data handler failed: {ex.Message}");
            }
        }

        private void MyManager_AircraftPathEvent(string path)
        {
            aircraftPath = path ?? "";
            ResolveParkingBrakeProfile();
        }

        private void MyManager_ConnectStatusEvent(bool connected)
        {
            if (connected) return;
            // A new connection has new data definitions; resolve the aircraft again once it is reported.
            aircraftTitle = "";
            aircraftPath = "";
            parkingBrakeProfile = null;
        }

        private void ResolveParkingBrakeProfile()
        {
            var next = ParkingBrakeProfiles.Find(aircraftTitle, aircraftPath);
            if (next == parkingBrakeProfile) return;

            parkingBrakeProfile = next;
            if (next != null)
            {
                Trace.WriteLine($"[PushbackHelper] Parking brake read from L:{next.LVar} ({next.Name})");
                myManager.WatchLVar(next.LVar);
            }
            else
            {
                Trace.WriteLine("[PushbackHelper] Parking brake read from BRAKE PARKING POSITION");
                myManager.UnwatchLVar();
                SetParkingBrakeState(standardParkingBrake);
            }
        }

        private void SetParkingBrakeState(bool on)
        {
            if (ParkingBrakeOn != on)
                ParkingBrakeOn = on;
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
            var profile = parkingBrakeProfile;
            if (profile != null && profile.WriteLVar)
                myManager.SetData(DefinitionsEnum.ParkingBrakeLVar, ParkingBrakeOn ? profile.ReleaseValue : profile.SetValue);
            else
                myManager.TransmitEvent(EventsEnum.TOGGLE_PARKING_BRAKES, 1);
        }

        public delegate void BoolChanged(bool value);
    }
}
