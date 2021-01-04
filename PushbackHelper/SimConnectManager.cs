﻿using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PushbackHelper
{
    class SimConnectManager
    {
        private bool _connected;
        public bool Connected { get { return _connected; } private set { _connected = value; ConnectStatusEvent?.Invoke(Connected); } }
        private SimConnect simClient;
        private HwndSource gHs;
        private readonly DispatcherTimer connectTimer;
        private readonly DispatcherTimer refreshTimer;
        public event DataReceived DataRxEvent;
        public event ConnectStatusChanged ConnectStatusEvent;

        public SimConnectManager()
        {
            Connected = false;
            connectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            connectTimer.Tick += ConnectTimer_Tick;
            refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            refreshTimer.Tick += RefreshTimer_Tick;
        }
        public void Start()
        {
            connectTimer.Start();
        }
        public void Stop()
        {
            connectTimer.Stop();
            if (Connected) simClient.Dispose();
        }
        public void TransmitEvent(EventsEnum newEvent, uint data)
        {
            if (!Connected) return;
            simClient.TransmitClientEvent(0U, newEvent, data, NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }
        public void RequestData(RequestsEnum request, DefinitionsEnum definition)
        {
            if (!Connected) return;
            simClient.RequestDataOnSimObjectType(request, definition, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }
        public void SetData(DefinitionsEnum definition, object data)
        {
            if (!Connected) return;
            simClient.SetDataOnSimObject(definition, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, data);
        }
        private void Connect()
        {
            try
            {
                if (Connected) return;
                WindowInteropHelper lWih = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
                IntPtr lHwnd = lWih.Handle;
                gHs = HwndSource.FromHwnd(lHwnd);
                gHs.AddHook(new HwndSourceHook(WndProc));

                simClient = new SimConnect("Pushback Helper", lHwnd, 0x402, null, 0);
                // Sim connect configurations
                simClient.OnRecvOpen += SimClient_OnRecvOpen;
                simClient.OnRecvQuit += SimClient_OnRecvQuit;
                simClient.OnRecvException += SimClient_OnRecvException;

                simClient.AddToDataDefinition(DefinitionsEnum.RefreshDataStruct, "Plane Heading Degrees True", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RefreshDataStruct, "Pushback State", "Enum", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RefreshDataStruct, "Brake Parking Position", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.PushbackWait, "Pushback Wait", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.VelocityX, "Velocity Body X", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.VelocityY, "Velocity Body Y", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.VelocityZ, "Velocity Body Z", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RotationX, "Rotation Velocity Body X", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RotationY, "Rotation Velocity Body Y", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RotationZ, "Rotation Velocity Body Z", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                for (uint i = 0; i < 50; i++)
                {
                    simClient.AddToDataDefinition(DefinitionsEnum.ExitTypeStruct, string.Format("Exit Type:{0}", i), "Enum", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                }
                for (uint i = 0; i < 50; i++)
                {
                    simClient.AddToDataDefinition(DefinitionsEnum.ExitOpenStruct, string.Format("Exit Open:{0}", i), "percent over 100", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                }
                simClient.RegisterDataDefineStruct<RefreshDataStruct>(DefinitionsEnum.RefreshDataStruct);
                simClient.RegisterDataDefineStruct<ExitDataStruct>(DefinitionsEnum.ExitTypeStruct);
                simClient.RegisterDataDefineStruct<ExitDataStruct>(DefinitionsEnum.ExitOpenStruct);
                simClient.RegisterDataDefineStruct<int>(DefinitionsEnum.PushbackWait);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.VelocityX);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.VelocityY);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.VelocityZ);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.RotationX);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.RotationY);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.RotationZ);

                simClient.OnRecvSimobjectDataBytype += SimClient_OnRecvSimobjectDataBytype;

                simClient.MapClientEventToSimEvent(EventsEnum.KEY_PUSHBACK_SET, "TOGGLE_PUSHBACK");
                simClient.MapClientEventToSimEvent(EventsEnum.KEY_TUG_HEADING, "KEY_TUG_HEADING");
                simClient.MapClientEventToSimEvent(EventsEnum.KEY_TOGGLE_JETWAY, "TOGGLE_JETWAY");
                simClient.MapClientEventToSimEvent(EventsEnum.KEY_REQUEST_FUEL, "REQUEST_FUEL_KEY");
                simClient.MapClientEventToSimEvent(EventsEnum.KEY_TOGGLE_AIRCRAFT_EXIT, "TOGGLE_AIRCRAFT_EXIT");
            }
            catch (Exception)
            {
                Connected = false;
            }
        }
        private void SimClient_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Connected = false;
            refreshTimer.Stop();
        }
        private void SimClient_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Connected = true;
            refreshTimer.Start();
        }
        private void SimClient_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {

        }
        private void ConnectTimer_Tick(object sender, EventArgs e)
        {
            Connect();
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (!Connected) return;

            simClient.RequestDataOnSimObjectType(RequestsEnum.RefreshDataRequest, DefinitionsEnum.RefreshDataStruct, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            simClient.RequestDataOnSimObjectType(RequestsEnum.ExitTypeRequest, DefinitionsEnum.ExitTypeStruct, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            simClient.RequestDataOnSimObjectType(RequestsEnum.ExitOpenRequest, DefinitionsEnum.ExitOpenStruct, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            // if message is coming from simconnect and the connection is not null;
            // continue and receive message
            try
            {
                if (msg == 0x402 && simClient != null)
                {
                    simClient.ReceiveMessage();
                    handled = true;
                }
            }
            catch (Exception) { }
            return (IntPtr)0;
        }
        private void SimClient_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            try
            {
                DataRxEvent?.Invoke((RequestsEnum)data.dwRequestID, data);
            }
            catch (Exception) { }
        }

        public delegate void DataReceived(RequestsEnum request, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data);
        public delegate void ConnectStatusChanged(bool Connected);
    }
}
