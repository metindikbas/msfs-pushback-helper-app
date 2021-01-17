using Microsoft.FlightSimulator.SimConnect;
using System;

namespace PushbackHelper
{
    class ExitManager
    {
        private bool _mainExit;
        private bool _cargoExit;
        private bool _emergencyExit;
        public bool MainExit { get { return _mainExit; } private set { _mainExit = value; ExitEvent?.Invoke(ExitType.Main, _mainExit); } }
        public bool CargoExit { get { return _cargoExit; } private set { _cargoExit = value; ExitEvent?.Invoke(ExitType.Cargo, _cargoExit); } }
        public bool EmergencyExit { get { return _emergencyExit; } private set { _emergencyExit = value; ExitEvent?.Invoke(ExitType.Emergency, _emergencyExit); } }
        private double[] exitTypeArray;
        private double[] exitOpenArray;
        private uint mainExitIndex;
        private uint emergencyExitIndex;
        private uint cargoExitIndex;
        private SimConnectManager myManager;
        public event ExitChanged ExitEvent;
        public ExitManager(SimConnectManager manager)
        {
            myManager = manager;
            myManager.DataRxEvent += MyManager_DataRxEvent;
            exitTypeArray = new double[50];
            exitOpenArray = new double[50];

            mainExitIndex = 0;
            emergencyExitIndex = 3;
            cargoExitIndex = 5;
        }
        public enum ExitType
        {
            Main,
            Cargo,
            Emergency,
            Unknown
        }
        public void ToggleExit(ExitType exitType)
        {
            if (myManager.Connected)
            {
                //Increase index by 1 for the toggle event. This works better but seems incorrect since 0 is a valid exit index as well
                if (exitType == ExitType.Main)
                    myManager.TransmitEvent(EventsEnum.KEY_TOGGLE_AIRCRAFT_EXIT, mainExitIndex + 1);
                else if (exitType == ExitType.Cargo)
                    myManager.TransmitEvent(EventsEnum.KEY_TOGGLE_AIRCRAFT_EXIT, cargoExitIndex + 1);
                else if (exitType == ExitType.Emergency)
                    myManager.TransmitEvent(EventsEnum.KEY_TOGGLE_AIRCRAFT_EXIT, emergencyExitIndex + 1);
            }
        }
        private void MyManager_DataRxEvent(RequestsEnum request, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            try
            {
                if (request == RequestsEnum.ExitTypeRequest)
                {
                    uint i = 0;
                    ExitDataStruct exitData = (ExitDataStruct)data.dwData[0];
                    foreach (var field in (exitData.GetType().GetFields()))
                    {
                        exitTypeArray[i] = (double)field.GetValue(exitData);
                        i++;
                    }

                    /*
                    * The exit data types returned from SimConnect do not match what is expected. This method below attempts to map each aircraft to known working values per plane type
                    * Exit type 0 = Main
                    * Exit type 1 = Cargo
                    * Exit type 2 = Emergency
                    * Exit type 99 = Unused
                    */
                    if (exitTypeArray[3] == 0)   //Type A320
                    {
                        mainExitIndex = 0;
                        emergencyExitIndex = 3;
                        cargoExitIndex = 5;
                    }
                    else if (exitTypeArray[13] == 1)   //Type 747
                    {
                        mainExitIndex = 10;
                        emergencyExitIndex = 1;
                        cargoExitIndex = 12;
                    }
                    else if (exitTypeArray[10] == 1)   //Type 787
                    {
                        mainExitIndex = 0;
                        emergencyExitIndex = 7;
                        cargoExitIndex = 8;
                    }
                    else
                    {
                        mainExitIndex = 0;
                        emergencyExitIndex = 1;
                        cargoExitIndex = 2;
                    }
                }
                else if (request == RequestsEnum.ExitOpenRequest)
                {
                    uint i = 0;
                    ExitDataStruct exitData = (ExitDataStruct)data.dwData[0];
                    foreach (var field in (exitData.GetType().GetFields()))
                    {
                        exitOpenArray[i] = (double)field.GetValue(exitData);
                        i++;
                    }

                    MainExit = exitOpenArray[mainExitIndex] > .2;
                    CargoExit = exitOpenArray[cargoExitIndex] > .2;
                    EmergencyExit = exitOpenArray[emergencyExitIndex] > .2;
                }
            }
            catch (Exception) { }
        }

        public delegate void ExitChanged(ExitType Exit, bool ExitIsOpen);
    }
}
