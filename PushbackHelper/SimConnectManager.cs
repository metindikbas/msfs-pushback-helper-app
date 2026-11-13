using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PushbackHelper
{
    class SimConnectManager
    {
        private const int WM_USER_SIMCONNECT = 0x402;
        // If the sim accepts the pipe but never sends the open message, drop the client and try again.
        private static readonly TimeSpan OpenTimeout = TimeSpan.FromSeconds(60);
        // Consecutive SimConnect call failures (e.g. broken pipe 0xC000014B) before the connection is treated as lost.
        private const int MaxConsecutiveCallFailures = 3;
        private int consecutiveCallFailures;

        private HwndSource gHs;
        private IntPtr lHwnd;
        private bool _connected;
        public bool Connected { get { return _connected; } private set { _connected = value; ConnectStatusEvent?.Invoke(Connected); } }
        private SimConnect simClient;
        private DateTime clientCreatedAt;
        private readonly DispatcherTimer connectTimer;
        public event DataReceived DataRxEvent;
        public event ConnectStatusChanged ConnectStatusEvent;

        public SimConnectManager()
        {
            Connected = false;
            connectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            connectTimer.Tick += ConnectTimer_Tick;
        }
        public void Start()
        {
            // Show the disconnected state to listeners that subscribed after construction.
            Connected = false;
            connectTimer.Start();
        }
        public void Stop()
        {
            connectTimer.Stop();
            CloseClient();
            // Set the field directly: raising the event here would re-enter the app's exit handler.
            _connected = false;
        }
        public void TransmitEvent(EventsEnum newEvent, uint data)
        {
            if (!Connected || simClient == null) return;
            try
            {
                simClient.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, newEvent, data, NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                consecutiveCallFailures = 0;
            }
            catch (Exception ex)
            {
                HandleCallFailure($"TransmitEvent {newEvent}", ex);
            }
        }
        public void SetData(DefinitionsEnum definition, object data)
        {
            if (!Connected || simClient == null) return;
            try
            {
                simClient.SetDataOnSimObject(definition, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, data);
                consecutiveCallFailures = 0;
            }
            catch (Exception ex)
            {
                HandleCallFailure($"SetData {definition}", ex);
            }
        }
        private void HandleCallFailure(string call, Exception ex)
        {
            consecutiveCallFailures++;
            // Log only the first failure of a run so a dead connection does not flood the trace from the 10 ms tug timer.
            if (consecutiveCallFailures == 1)
                Trace.WriteLine($"[PushbackHelper] {call} failed: {ex.Message}");

            if (ex is COMException && consecutiveCallFailures >= MaxConsecutiveCallFailures)
            {
                Trace.WriteLine("[PushbackHelper] SimConnect calls keep failing, treating the connection as lost");
                ConnectionLost(simClient);
            }
        }
        private void ConnectionLost(SimConnect client)
        {
            // Leave any SimConnect callback before disposing, and ignore reports about a client that was already replaced.
            connectTimer.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (client == null || !ReferenceEquals(simClient, client)) return;
                bool wasConnected = _connected;
                CloseClient();
                // Restart before raising the event: a listener that shuts the app down calls Stop(), which must win.
                connectTimer.Start();
                if (wasConnected)
                    Connected = false;
            }));
        }
        private void HookWindow()
        {
            if (gHs != null) return;

            WindowInteropHelper lWih = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
            lHwnd = lWih.Handle;
            gHs = HwndSource.FromHwnd(lHwnd);
            gHs.AddHook(new HwndSourceHook(WndProc));
        }
        private void CloseClient()
        {
            var client = simClient;
            simClient = null;
            consecutiveCallFailures = 0;
            if (client == null) return;
            try
            {
                client.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[PushbackHelper] SimConnect dispose failed: {ex.Message}");
            }
        }
        private void Connect()
        {
            if (Connected)
            {
                connectTimer.Stop();
                return;
            }

            if (simClient != null)
            {
                // A client is already waiting for the open message; don't open a second one.
                if (DateTime.UtcNow - clientCreatedAt < OpenTimeout) return;
                Trace.WriteLine("[PushbackHelper] No open message from the sim, reconnecting");
                CloseClient();
            }

            try
            {
                HookWindow();

                var client = new SimConnect("Pushback Helper", lHwnd, WM_USER_SIMCONNECT, null, 0);
                simClient = client;
                clientCreatedAt = DateTime.UtcNow;
                simClient.OnRecvOpen += SimClient_OnRecvOpen;
                simClient.OnRecvQuit += (sender, data) => ConnectionLost(client);
                simClient.OnRecvException += SimClient_OnRecvException;

                simClient.AddToDataDefinition(DefinitionsEnum.RefreshDataStruct, "Plane Heading Degrees True", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RefreshDataStruct, "Pushback State", "Enum", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RefreshDataStruct, "Brake Parking Position", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.PushbackWait, "Pushback Wait", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                // Small epsilon so heading noise on a parked aircraft does not send a message every frame.
                simClient.AddToDataDefinition(DefinitionsEnum.Heading, "Plane Heading Degrees True", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.01f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.VelocityX, "Velocity Body X", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.VelocityY, "Velocity Body Y", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.VelocityZ, "Velocity Body Z", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RotationX, "Rotation Velocity Body X", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RotationY, "Rotation Velocity Body Y", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simClient.AddToDataDefinition(DefinitionsEnum.RotationZ, "Rotation Velocity Body Z", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                for (uint i = 0; i < 20; i++)
                {
                    simClient.AddToDataDefinition(DefinitionsEnum.ExitTypeStruct, string.Format("Exit Type:{0}", i), "Enum", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                }
                for (uint i = 0; i < 20; i++)
                {
                    simClient.AddToDataDefinition(DefinitionsEnum.ExitOpenStruct, string.Format("Exit Open:{0}", i), "percent over 100", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                }
                simClient.RegisterDataDefineStruct<RefreshDataStruct>(DefinitionsEnum.RefreshDataStruct);
                simClient.RegisterDataDefineStruct<ExitDataStruct>(DefinitionsEnum.ExitTypeStruct);
                simClient.RegisterDataDefineStruct<ExitDataStruct>(DefinitionsEnum.ExitOpenStruct);
                simClient.RegisterDataDefineStruct<int>(DefinitionsEnum.PushbackWait);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.Heading);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.VelocityX);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.VelocityY);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.VelocityZ);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.RotationX);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.RotationY);
                simClient.RegisterDataDefineStruct<double>(DefinitionsEnum.RotationZ);

                simClient.OnRecvSimobjectData += SimClient_OnRecvSimobjectData;

                simClient.MapClientEventToSimEvent(EventsEnum.TOGGLE_PUSHBACK, "TOGGLE_PUSHBACK");
                simClient.MapClientEventToSimEvent(EventsEnum.TOGGLE_JETWAY, "TOGGLE_JETWAY");
                simClient.MapClientEventToSimEvent(EventsEnum.TOGGLE_AIRCRAFT_EXIT, "TOGGLE_AIRCRAFT_EXIT");
                simClient.MapClientEventToSimEvent(EventsEnum.TOGGLE_PARKING_BRAKES, "PARKING_BRAKES");
                simClient.MapClientEventToSimEvent(EventsEnum.TOGGLE_RAMPTRUCK, "TOGGLE_RAMPTRUCK");
                simClient.MapClientEventToSimEvent(EventsEnum.SET_TUG_HEADING, "KEY_TUG_HEADING");
                simClient.MapClientEventToSimEvent(EventsEnum.REQUEST_FUEL, "REQUEST_FUEL_KEY");
                simClient.MapClientEventToSimEvent(EventsEnum.REQUEST_LUGGAGE, "REQUEST_LUGGAGE");
                simClient.MapClientEventToSimEvent(EventsEnum.REQUEST_POWER_SUPPLY, "REQUEST_POWER_SUPPLY");
                simClient.MapClientEventToSimEvent(EventsEnum.REQUEST_CATERING, "REQUEST_CATERING");

                simClient.RequestDataOnSimObject(RequestsEnum.RefreshDataRequest, DefinitionsEnum.RefreshDataStruct, 0, SIMCONNECT_PERIOD.SECOND, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
                // Heading on its own request every sim frame: the tug heading is aircraft heading ±90°, sent every 10 ms. With the
                // 1 s heading above the tug drifted up to ~10° past 90° in fast turns, a likely cause of speed changes and the tug
                // detaching at max rotation. Kept separate so pushback state and parking brake keep their original 1 s timing.
                simClient.RequestDataOnSimObject(RequestsEnum.HeadingRequest, DefinitionsEnum.Heading, 0, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
                simClient.RequestDataOnSimObject(RequestsEnum.ExitTypeRequest, DefinitionsEnum.ExitTypeStruct, 0, SIMCONNECT_PERIOD.SECOND, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
                simClient.RequestDataOnSimObject(RequestsEnum.ExitOpenRequest, DefinitionsEnum.ExitOpenStruct, 0, SIMCONNECT_PERIOD.SECOND, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
            }
            catch (Exception ex)
            {
                // The constructor throws a COMException while the sim is not running; that is the normal case, retry on next tick.
                if (simClient != null || !(ex is COMException))
                    Trace.WriteLine($"[PushbackHelper] SimConnect setup failed: {ex.Message}");
                CloseClient();
            }
        }

        private void SimClient_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            try
            {
                DataRxEvent?.Invoke((RequestsEnum)data.dwRequestID, data);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[PushbackHelper] Data handler failed: {ex.Message}");
            }
        }

        private void SimClient_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Trace.WriteLine($"[PushbackHelper] Connected to {data.szApplicationName} {data.dwApplicationVersionMajor}.{data.dwApplicationVersionMinor} (SimConnect {data.dwSimConnectVersionMajor}.{data.dwSimConnectVersionMinor})");
            Connected = true;
        }
        private void SimClient_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Trace.WriteLine($"[PushbackHelper] SimConnect exception {(SIMCONNECT_EXCEPTION)data.dwException} (send id {data.dwSendID}, index {data.dwIndex})");
        }
        private void ConnectTimer_Tick(object sender, EventArgs e)
        {
            Connect();
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            // if message is coming from simconnect and the connection is not null;
            // continue and receive message
            try
            {
                if (msg == WM_USER_SIMCONNECT && simClient != null)
                {
                    simClient.ReceiveMessage();
                    consecutiveCallFailures = 0;
                    handled = true;
                }
            }
            catch (Exception ex)
            {
                HandleCallFailure("ReceiveMessage", ex);
            }
            return (IntPtr)0;
        }

        public delegate void DataReceived(RequestsEnum request, SIMCONNECT_RECV_SIMOBJECT_DATA data);
        public delegate void ConnectStatusChanged(bool Connected);
    }
}
