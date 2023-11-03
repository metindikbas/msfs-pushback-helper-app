using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using FSUIPC;
using PushbackHelper.MSFSLocalService;

namespace PushbackHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LowLevelKeyboardListener listener;
        private readonly SimConnectManager simConnectManager;
        private readonly TugManager tugManager;
        private readonly ExitManager exitManager;
        private readonly ServicesManager servicesManager;
        private readonly SimLocalService simLocalService;
        private bool ConnectedOnce;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString(), "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ConnectedOnce = false;
            uint tugSpeed = 25;

            try
            {
                if (Properties.Settings.Default.CallUpgrade)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.CallUpgrade = false;
                }
                var top = Properties.Settings.Default.WindowTop;
                var left = Properties.Settings.Default.WindowLeft;
                var height = Properties.Settings.Default.WindowHeight;
                tugSpeed = Properties.Settings.Default.TugSpeed;

                // Restore window location
                if (top != 0 || left != 0 || height != 480)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    Top = top;
                    Left = left;
                    SetHeight(height);
                }
            }
            catch
            {
                MessageBox.Show("Unable to load settings file", "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                listener = new LowLevelKeyboardListener();
                listener.OnKeyPressed += Listener_OnKeyPressed;
                listener.HookKeyboard();
            }
            catch
            {
                MessageBox.Show("Unable to connect to keyboard", "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                simConnectManager = new SimConnectManager();
                simConnectManager.ConnectStatusEvent += SimConnectManager_ConnectStatusEvent;
                tugManager = new TugManager(simConnectManager);
                tugManager.TugStatusEvent += TugManager_TugStatusEvent;
                exitManager = new ExitManager(simConnectManager);
                exitManager.ExitEvent += ExitManager_ExitEvent;
                servicesManager = new ServicesManager(simConnectManager);
                servicesManager.ParkingBrakeEvent += ServicesManager_ParkingBrakeEvent;
                speedSlider.Value = tugSpeed;
                speedSlider.Minimum = 5;
                speedSlider.Maximum = 50;
                tugManager.SetSpeed(tugSpeed);
                btnOpenMainDoor.IsEnabled = false;
                btnOpenCargoDoor.IsEnabled = false;
                btnOpenEmergencyDoor.IsEnabled = false;
                btnForward.IsEnabled = false;
                btnReverse.IsEnabled = false;
                btnLeft.IsEnabled = false;
                btnRight.IsEnabled = false;
                simConnectManager.Start();

                simLocalService = new SimLocalService();
                simLocalService.ParkingBrakeStateChanged += SimLocalService_ParkingBrakeStateChanged;
            }
            catch
            {
                MessageBox.Show("Unable to initialize SimConnect manager", "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            SetHeight(sizeInfo.NewSize.Height);
        }

        public void ExitApp()
        {
            try
            {
                // Save position
                if (WindowState == WindowState.Normal)
                {
                    Properties.Settings.Default.WindowTop = Top;
                    Properties.Settings.Default.WindowLeft = Left;
                    Properties.Settings.Default.WindowHeight = Height;
                }
                Properties.Settings.Default.TugSpeed = tugManager.SpeedFactor;
                Properties.Settings.Default.Save();
                // Stop
                tugManager.Disable();
                simConnectManager.Stop();
                listener.UnHookKeyboard();
                simLocalService?.Dispose();
            }
            catch (Exception)
            {

            }
            Close();
        }

        private void SetHeight(double height)
        {
            if (height < 120)
            {
                Width = 75;
                Height = 120;
            }
            else if (height > 960)
            {
                Width = 600;
                Height = 960;
            }
            else
            {
                Height = height;
                Width = height * 15 / 24;
            }
        }

        private void SetParkBrake(bool active)
        {
            lblParkingBrake.Foreground = active
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.LightGray);
        }

        private void SimConnectManager_ConnectStatusEvent(bool Connected)
        {
            if (Connected)
            {
                ConnectedOnce = true;
                lblSimStatus.Content = "CONNECTED";
                lblSimStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
                simLocalService.Connect();
            }
            else
            {
                lblSimStatus.Content = "DISCONNECTED";
                lblSimStatus.Foreground = new SolidColorBrush(Colors.Red);

                if (ConnectedOnce == true)
                {
                    ExitApp();
                }
            }
        }

        private void Listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
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
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }

        private void TugManager_TugStatusEvent(TugManager.TugStatus Status, double TugRotationSetting)
        {
            try
            {
                if (Status == TugManager.TugStatus.Disabled)
                {
                    lblPushbackStatus.Content = "DISABLED";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.Gray);
                    lblTug.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblForward.Content = "FORWARD";
                    lblForward.Foreground = new SolidColorBrush(Colors.DarkGray);
                    lblReverse.Content = "REVERSE";
                    lblReverse.Foreground = new SolidColorBrush(Colors.DarkGray);
                    lblLeft.Foreground = new SolidColorBrush(Colors.DarkGray);
                    lblRight.Foreground = new SolidColorBrush(Colors.DarkGray);
                    btnForward.IsEnabled = false;
                    btnReverse.IsEnabled = false;
                    btnLeft.IsEnabled = false;
                    btnRight.IsEnabled = false;
                }
                else if (Status == TugManager.TugStatus.Waiting)
                {
                    lblPushbackStatus.Content = "ACTIVE";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblTug.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblForward.Content = "FORWARD";
                    lblForward.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblReverse.Content = "REVERSE";
                    lblReverse.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblLeft.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblRight.Foreground = new SolidColorBrush(Colors.LightGray);
                    btnForward.IsEnabled = true;
                    btnReverse.IsEnabled = true;
                    btnLeft.IsEnabled = true;
                    btnRight.IsEnabled = true;
                }
                else if (Status == TugManager.TugStatus.Forward)
                {
                    lblPushbackStatus.Content = "ACTIVE";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblTug.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblReverse.Content = "STOP";
                    lblReverse.Foreground = new SolidColorBrush(Colors.Red);
                    lblForward.Content = "STRAIGHT";
                    lblForward.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblLeft.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblRight.Foreground = new SolidColorBrush(Colors.LightGray);
                    btnForward.IsEnabled = true;
                    btnReverse.IsEnabled = true;
                    btnLeft.IsEnabled = true;
                    btnRight.IsEnabled = true;
                }
                else if (Status == TugManager.TugStatus.Reverse)
                {
                    lblPushbackStatus.Content = "ACTIVE";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblTug.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblForward.Content = "STOP";
                    lblForward.Foreground = new SolidColorBrush(Colors.Red);
                    lblReverse.Content = "STRAIGHT";
                    lblReverse.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblLeft.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblRight.Foreground = new SolidColorBrush(Colors.LightGray);
                    btnForward.IsEnabled = true;
                    btnReverse.IsEnabled = true;
                    btnLeft.IsEnabled = true;
                    btnRight.IsEnabled = true;
                }
                else if (Status == TugManager.TugStatus.ProcessingTurn)
                {
                    lblPushbackStatus.Content = "ACTIVE";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblTug.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    //lblForward.Content = "STOP";
                    //lblForward.Foreground = new SolidColorBrush(Colors.Red);
                    //lblReverse.Content = "STRAIGHT";
                    //lblReverse.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblLeft.Foreground = new SolidColorBrush(Colors.DarkGray);
                    lblRight.Foreground = new SolidColorBrush(Colors.DarkGray);
                    btnForward.IsEnabled = true;
                    btnReverse.IsEnabled = true;
                    btnLeft.IsEnabled = false;
                    btnRight.IsEnabled = false;
                }
            }
            catch (Exception) { }

            // Update the speed in the debug label 

            //lblRotationValue.Content = TugRotationSetting.ToString();

            if (Math.Abs(TugRotationSetting) < 1)
            {
                circLeftLow.Fill = new SolidColorBrush(Colors.Black);
                circLeftLow.Stroke = new SolidColorBrush(Colors.DarkGray);
                circLeftMedium.Fill = new SolidColorBrush(Colors.Black);
                circLeftMedium.Stroke = new SolidColorBrush(Colors.DarkGray);
                circLeftHigh.Fill = new SolidColorBrush(Colors.Black);
                circLeftHigh.Stroke = new SolidColorBrush(Colors.DarkGray);

                circRightLow.Fill = new SolidColorBrush(Colors.Black);
                circRightLow.Stroke = new SolidColorBrush(Colors.DarkGray);
                circRightMedium.Fill = new SolidColorBrush(Colors.Black);
                circRightMedium.Stroke = new SolidColorBrush(Colors.DarkGray);
                circRightHigh.Fill = new SolidColorBrush(Colors.Black);
                circRightHigh.Stroke = new SolidColorBrush(Colors.DarkGray);
                btnRight.IsEnabled = true;
                lblRight.Foreground = new SolidColorBrush(Colors.LightGray);

            }

            if (TugRotationSetting > 0)
            {
                // Right (positive)
                if (TugRotationSetting > 2)
                {
                    circRightLow.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circRightLow.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circRightMedium.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circRightMedium.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circRightHigh.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circRightHigh.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    btnRight.IsEnabled = false;
                    lblRight.Foreground = new SolidColorBrush(Colors.DarkGray);
                }
                else if (TugRotationSetting > 1)
                {
                    circRightLow.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circRightLow.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circRightMedium.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circRightMedium.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circRightHigh.Fill = new SolidColorBrush(Colors.Black);
                    circRightHigh.Stroke = new SolidColorBrush(Colors.DarkGray);
                }
                else if (TugRotationSetting > 0)
                {
                    circRightLow.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circRightLow.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circRightMedium.Fill = new SolidColorBrush(Colors.Black);
                    circRightMedium.Stroke = new SolidColorBrush(Colors.DarkGray);
                    circRightHigh.Fill = new SolidColorBrush(Colors.Black);
                    circRightHigh.Stroke = new SolidColorBrush(Colors.DarkGray);
                }

            }
            else
            {
                // Left (negative)
                if (TugRotationSetting < -2)
                {
                    circLeftLow.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circLeftLow.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circLeftMedium.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circLeftMedium.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circLeftHigh.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circLeftHigh.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    btnLeft.IsEnabled = false;
                    lblLeft.Foreground = new SolidColorBrush(Colors.DarkGray);
                }
                else if (TugRotationSetting < -1)
                {
                    circLeftLow.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circLeftLow.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circLeftMedium.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circLeftMedium.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circLeftHigh.Fill = new SolidColorBrush(Colors.Black);
                    circLeftHigh.Stroke = new SolidColorBrush(Colors.DarkGray);
                }
                else if (TugRotationSetting < 0)
                {
                    circLeftLow.Fill = new SolidColorBrush(Colors.GreenYellow);
                    circLeftLow.Stroke = new SolidColorBrush(Colors.GreenYellow);
                    circLeftMedium.Fill = new SolidColorBrush(Colors.Black);
                    circLeftMedium.Stroke = new SolidColorBrush(Colors.DarkGray);
                    circLeftHigh.Fill = new SolidColorBrush(Colors.Black);
                    circLeftHigh.Stroke = new SolidColorBrush(Colors.DarkGray);
                }
            }
        }

        private void ExitManager_ExitEvent(ExitManager.ExitType Exit, bool ExitIsOpen)
        {
            btnOpenMainDoor.IsEnabled = exitManager.MainExitEnabled ?? false;
            btnOpenCargoDoor.IsEnabled = exitManager.CargoExitEnabled ?? false;
            btnOpenEmergencyDoor.IsEnabled = exitManager.EmergencyExitEnabled ?? false;

            switch (Exit)
            {
                case ExitManager.ExitType.Main:
                    lblMainDoor.Foreground = ExitIsOpen ? new SolidColorBrush(Colors.GreenYellow) : new SolidColorBrush(Colors.LightGray);
                    break;
                case ExitManager.ExitType.Cargo:
                    lblCargoDoor.Foreground = ExitIsOpen ? new SolidColorBrush(Colors.GreenYellow) : new SolidColorBrush(Colors.LightGray);
                    break;
                case ExitManager.ExitType.Emergency:
                    lblEmergencyDoor.Foreground = ExitIsOpen ? new SolidColorBrush(Colors.GreenYellow) : new SolidColorBrush(Colors.LightGray);
                    break;
            }
        }

        private void SimLocalService_ParkingBrakeStateChanged(object sender, ParkingBrakeStateChangedEventArgs e)
        {
            SetParkBrake(e.Active);
        }

        private void ServicesManager_ParkingBrakeEvent(bool value)
        {
            SetParkBrake(value);
        }

        private void BtnJetway_Click(object sender, RoutedEventArgs e)
        {
            servicesManager.ToggleJetway();
        }

        private void BtnFuel_Click(object sender, RoutedEventArgs e)
        {
            servicesManager.ToggleFuel();
        }

        private void BtnTug_Click(object sender, RoutedEventArgs e)
        {
            if (tugManager.TugActive)
                tugManager.Disable();
            else
                tugManager.Enable();
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            tugManager.Forward();
        }

        private void BtnReverse_Click(object sender, RoutedEventArgs e)
        {
            tugManager.Reverse();
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            tugManager.Right();
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            tugManager.Left();
        }

        private void BtnAircraftDoorMain_Click(object sender, RoutedEventArgs e)
        {
            exitManager.ToggleExit(ExitManager.ExitType.Main);
        }

        private void BtnAircraftDoorEmergency_Click(object sender, RoutedEventArgs e)
        {
            exitManager.ToggleExit(ExitManager.ExitType.Emergency);
        }

        private void BtnAircraftDoorCargo_Click(object sender, RoutedEventArgs e)
        {
            exitManager.ToggleExit(ExitManager.ExitType.Cargo);
        }

        private void BtnLuggage_Click(object sender, RoutedEventArgs e)
        {
            servicesManager.RequestLuggage();
        }

        private void BtnPowerSupply_Click(object sender, RoutedEventArgs e)
        {
            servicesManager.RequestPowerSupply();
        }

        private void BtnCatering_Click(object sender, RoutedEventArgs e)
        {
            servicesManager.RequestCatering();
        }

        private void BtnRampTruck_Click(object sender, RoutedEventArgs e)
        {
            servicesManager.ToggleRampTruck();
        }

        private void BtnParkingBrake_Click(object sender, RoutedEventArgs e)
        {
            servicesManager.ToggleParkingBrake();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tugManager.SetSpeed((uint)e.NewValue);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                base.OnSourceInitialized(e);

                //Set the window style to noactivate.
                var handle = new WindowInteropHelper(this).Handle;
                SetWindowLong(handle, -20, GetWindowLong(handle, -20) | 0x08000000);
            }
            catch
            { }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }
}
