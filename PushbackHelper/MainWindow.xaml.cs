using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
        public MainWindow()
        {
            InitializeComponent();

            var top = Properties.Settings.Default.WindowTop;
            var left = Properties.Settings.Default.WindowLeft;
            var height = Properties.Settings.Default.WindowHeight;

            // Restore window location
            if (top != 0 || left != 0 || height != 440)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Top = top;
                Left = left;
                SetHeight(height);
            }

            listener = new LowLevelKeyboardListener();
            listener.OnKeyPressed += Listener_OnKeyPressed;
            listener.HookKeyboard();
            simConnectManager = new SimConnectManager();
            simConnectManager.ConnectStatusEvent += SimConnectManager_ConnectStatusEvent;
            tugManager = new TugManager(simConnectManager);
            tugManager.TugStatusEvent += TugManager_TugStatusEvent;
            exitManager = new ExitManager(simConnectManager);
            exitManager.ExitEvent += ExitManager_ExitEvent;
            servicesManager = new ServicesManager(simConnectManager);
            servicesManager.ParkingBrakeEvent += ServicesManager_ParkingBrakeEvent;
            speedSlider.Value = Properties.Settings.Default.TugSpeed;
            speedSlider.Minimum = 5;
            speedSlider.Maximum = 50;
            tugManager.SetSpeed(Properties.Settings.Default.TugSpeed);
            btnOpenMainDoor.IsEnabled = false;
            btnOpenCargoDoor.IsEnabled = false;
            btnOpenEmergencyDoor.IsEnabled = false;

            simConnectManager.Start();
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            SetHeight(sizeInfo.NewSize.Height);
        }
        private void SetHeight(double height)
        {
            if (height < 110)
            {
                Width = 75;
                Height = 110;
            }
            else if (height > 880)
            {
                Width = 600;
                Height = 880;
            }
            else
            {
                Height = height;
                Width = height * 15/22;
            }
        }
        private void SimConnectManager_ConnectStatusEvent(bool Connected)
        {
            if (Connected)
            {
                lblSimStatus.Content = "CONNECTED";
                lblSimStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
            }
            else
            {
                lblSimStatus.Content = "DISCONNECTED";
                lblSimStatus.Foreground = new SolidColorBrush(Colors.Red);
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
            try
            {
                // Save position
                Properties.Settings.Default.WindowTop = Top;
                Properties.Settings.Default.WindowLeft = Left;
                Properties.Settings.Default.WindowHeight = Height;
                Properties.Settings.Default.TugSpeed = tugManager.SpeedFactor;
                Properties.Settings.Default.Save();
                // Stop
                tugManager.Disable();
                simConnectManager.Stop();
                listener.UnHookKeyboard();
            }
            catch (Exception)
            {

            }
            Close();
        }
        private void TugManager_TugStatusEvent(TugManager.TugStatus Status)
        {
            try
            {
                if (Status == TugManager.TugStatus.Disabled)
                {
                    lblPushbackStatus.Content = "DISABLED";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.Gray);
                    lblTug.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblForward.Content = "FORWARD";
                    lblForward.Foreground = new SolidColorBrush(Colors.LightGray);
                    lblReverse.Content = "REVERSE";
                    lblReverse.Foreground = new SolidColorBrush(Colors.LightGray);
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
                }
                else if (Status == TugManager.TugStatus.Forward)
                {
                    lblPushbackStatus.Content = "ACTIVE";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblTug.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblReverse.Content = "STOP";
                    lblReverse.Foreground = new SolidColorBrush(Colors.Red);
                    lblForward.Content = "FORWARD";
                    lblForward.Foreground = new SolidColorBrush(Colors.LightGray);
                }
                else if (Status == TugManager.TugStatus.Reverse)
                {
                    lblPushbackStatus.Content = "ACTIVE";
                    lblPushbackStatus.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblTug.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    lblForward.Content = "STOP";
                    lblForward.Foreground = new SolidColorBrush(Colors.Red);
                    lblReverse.Content = "REVERSE";
                    lblReverse.Foreground = new SolidColorBrush(Colors.LightGray);
                }
            }
            catch (Exception) { }
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
        private void ServicesManager_ParkingBrakeEvent(bool value)
        {
            if(value)
                lblParkingBrake.Foreground = new SolidColorBrush(Colors.Red);
            else
                lblParkingBrake.Foreground = new SolidColorBrush(Colors.LightGray);
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
    }
}
