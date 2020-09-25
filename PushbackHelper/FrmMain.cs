using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PushbackHelper
{
    public partial class FrmMain : Form
    {
        private bool _simConnectionStatus;
        private int _lastHeading;
        private SimConnect _simClient;
        private Timer _connectTimer;
        private Timer _timer;

        public FrmMain()
        {
            InitializeComponent();
            SetInitialState();
            _timer = new Timer();
            _timer.Interval = 500;
            _timer.Stop();
            _timer.Tick += _timer_Tick;
        }

        private void SetInitialState()
        {
            _simConnectionStatus = false;
            grpPushBack.Enabled = _simConnectionStatus;
            SimConnectionStatusChangedEvents();
        }

        private void SimConnectionStatusChangedEvents()
        {
            // connection status text
            lblSimStatusValue.Text = _simConnectionStatus ? "CONNECTED" : "NOT CONNECTED";
            lblSimStatusValue.ForeColor = _simConnectionStatus ? Color.DarkGreen : Color.Red;
            // connect button
            btnConnect.Text = _simConnectionStatus ? "Disconnect" : "Connect to Sim";
            // push back controls
            grpPushBack.Enabled = _simConnectionStatus;
            // heading
            lblHeadingValue.Text = "-";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void SimulationInterface_SimulationDisconnected(object sender, EventArgs e)
        {
            _simConnectionStatus = false;
            SimConnectionStatusChangedEvents();
        }

        private void SimulationInterface_SimulationConnected(object sender, EventArgs e)
        {
            _simConnectionStatus = true;
            SimConnectionStatusChangedEvents();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_simConnectionStatus)
            {
                Disconnect();
            }
            else
            {
                Connect();
            }
        }

        // SIMCONNECT
        private void Connect()
        {
            try
            {
                if (_simConnectionStatus) return;
                _simClient = new SimConnect("Pushback Helper", Handle, 0x402, null, 0);
                // Sim connect configurations
                _simClient.OnRecvOpen += SimClient_OnRecvOpen;
                _simClient.OnRecvQuit += SimClient_OnRecvQuit;
                _simClient.OnRecvException += SimClient_OnRecvException;
                _simClient.AddToDataDefinition(DefinitionsEnum.HeadingDataStruct, "Plane Heading Degrees True", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                _simClient.RegisterDataDefineStruct<HeadingDataStruct>(DefinitionsEnum.HeadingDataStruct);
                _simClient.OnRecvSimobjectDataBytype += SimClient_OnRecvSimobjectDataBytype;
                //
                _simConnectionStatus = true;
                SimConnectionStatusChangedEvents();
                _timer.Start();
            }
            catch (Exception ex)
            {
                _simConnectionStatus = false;
                MessageBox.Show(ex.Message);
            }
            SimConnectionStatusChangedEvents();
        }

        private void Disconnect()
        {
            try
            {
                if (!_simConnectionStatus) return;
                _timer.Stop();
                _simClient.Dispose();
                _simConnectionStatus = false;
                SimConnectionStatusChangedEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!_simConnectionStatus)
            {
                _timer.Stop();
                return;
            }

            _simClient.RequestDataOnSimObjectType(RequestsEnum.HeadingRequest, DefinitionsEnum.HeadingDataStruct, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        private void SimClient_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            _simConnectionStatus = false;
            SimConnectionStatusChangedEvents();
        }

        private void SimClient_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            _simConnectionStatus = true;
            SimConnectionStatusChangedEvents();
        }

        private void SimClient_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (data.dwRequestID == 0)
            {
                HeadingDataStruct receivedData = (HeadingDataStruct)data.dwData[0];
                _lastHeading = (int)receivedData.trueheading;
                lblHeadingValue.Text = _lastHeading.ToString();
            }
        }

        private void SimClient_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            // error received
        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == 0x402)
            {
                if (_simClient != null && _simConnectionStatus)
                {
                    _simClient.ReceiveMessage();
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void btnTogglePushback_Click(object sender, EventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_PUSHBACK_SET, "TOGGLE_PUSHBACK");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_PUSHBACK_SET, 1, NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_TUG_HEADING, "KEY_TUG_HEADING");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_TUG_HEADING, GetTugHeading(TugDirection.Left), NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_TUG_HEADING, "KEY_TUG_HEADING");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_TUG_HEADING, GetTugHeading(TugDirection.Right), NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private void btnStraight_Click(object sender, EventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_TUG_HEADING, "KEY_TUG_HEADING");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_TUG_HEADING, GetTugHeading(TugDirection.Straight), NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private uint GetTugHeading(TugDirection direction)
        {
            var heading = (uint)_lastHeading;
            switch (direction)
            {
                case TugDirection.Left:
                    heading += 90;
                    heading %= 360;
                    break;
                case TugDirection.Right:
                    heading += 270;
                    heading %= 360;
                    break;
                case TugDirection.Straight:
                default:
                    break;
            }
            return 11930464 * heading;
        }

        // DATA TYPES
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct HeadingDataStruct
        {
            public double trueheading;
        }

        private enum TugDirection
        {
            Left,
            Right,
            Straight
        }

        private enum RequestsEnum
        {
            HeadingRequest
        }

        private enum DefinitionsEnum
        {
            HeadingDataStruct
        }

        private enum PushbackEventsEnum
        {
            KEY_PUSHBACK_SET,
            KEY_TUG_HEADING
        }

        private enum NotificationGroupsEnum
        {
            Group0
        }
    }
}
