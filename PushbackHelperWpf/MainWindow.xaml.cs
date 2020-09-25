using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PushbackHelperWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HwndSource gHs;
        private LowLevelKeyboardListener _listener;
        private DispatcherTimer _timer;
        private DispatcherTimer _connectTimer;
        private bool _simConnectionStatus;
        private int _lastHeading;
        private SimConnect _simClient;

        public MainWindow()
        {
            InitializeComponent();
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();
            _connectTimer = new DispatcherTimer();
            _connectTimer.Interval = TimeSpan.FromSeconds(10);
            _connectTimer.Tick += ConnectTimer_Tick;
            _connectTimer.Start();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_simConnectionStatus) return;
            _simClient.RequestDataOnSimObjectType(RequestsEnum.HeadingRequest, DefinitionsEnum.HeadingDataStruct, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        private void ConnectTimer_Tick(object sender, EventArgs e)
        {
            Connect();
        }

        private void Connect()
        {
            try
            {
                if (_simConnectionStatus) return;
                WindowInteropHelper lWih = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
                IntPtr lHwnd = lWih.Handle;
                gHs = HwndSource.FromHwnd(lHwnd);
                gHs.AddHook(new HwndSourceHook(WndProc));

                _simClient = new SimConnect("Pushback Helper", lHwnd, 0x402, null, 0);
                // Sim connect configurations
                _simClient.OnRecvOpen += SimClient_OnRecvOpen;
                _simClient.OnRecvQuit += SimClient_OnRecvQuit;
                _simClient.OnRecvException += SimClient_OnRecvException;
                _simClient.AddToDataDefinition(DefinitionsEnum.HeadingDataStruct, "Plane Heading Degrees True", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                _simClient.RegisterDataDefineStruct<HeadingDataStruct>(DefinitionsEnum.HeadingDataStruct);
                _simClient.OnRecvSimobjectDataBytype += SimClient_OnRecvSimobjectDataBytype;
                //
                _simConnectionStatus = true;
                lblSimStatus.Content = "CONNECTED";
                _timer.Start();
            }
            catch (Exception)
            {
                _simConnectionStatus = false;
                lblSimStatus.Content = "ERROR!";
            }
        }

        private void SimClient_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            _simConnectionStatus = false;
            lblSimStatus.Content = "DISCONNECTED";
        }

        private void SimClient_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            _simConnectionStatus = true;
        }

        private void SimClient_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            lblSimStatus.Content = "CONNECTED";
            if (data.dwRequestID == 0)
            {
                HeadingDataStruct receivedData = (HeadingDataStruct)data.dwData[0];
                _lastHeading = (int)receivedData.trueheading;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            // if message is coming from simconnect and the connection is not null;
            // continue and receive message
            if (msg == 0x402 && _simClient != null)
            {
                _simClient.ReceiveMessage();
                handled = true;
            }
            return (IntPtr)0;
        }

        private void SimClient_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            lblSimStatus.Content = "ERROR!";
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

        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            //lblKey.Content = e.KeyPressed.ToString();
            if (e.KeyPressed == Key.PageUp && WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Minimized;
                base.OnStateChanged(e);
            }
            else if (e.KeyPressed == Key.PageUp && WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                base.OnStateChanged(e);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //_listener.UnHookKeyboard();
        }

        private void btnActivate_Click(object sender, RoutedEventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_PUSHBACK_SET, "TOGGLE_PUSHBACK");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_PUSHBACK_SET, 1, NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_TUG_HEADING, "KEY_TUG_HEADING");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_TUG_HEADING, GetTugHeading(TugDirection.Right), NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_TUG_HEADING, "KEY_TUG_HEADING");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_TUG_HEADING, GetTugHeading(TugDirection.Left), NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private void btnStraight_Click(object sender, RoutedEventArgs e)
        {
            _simClient.MapClientEventToSimEvent(PushbackEventsEnum.KEY_TUG_HEADING, "KEY_TUG_HEADING");
            _simClient.TransmitClientEvent(0U, PushbackEventsEnum.KEY_TUG_HEADING, GetTugHeading(TugDirection.Straight), NotificationGroupsEnum.Group0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }
    }
}
