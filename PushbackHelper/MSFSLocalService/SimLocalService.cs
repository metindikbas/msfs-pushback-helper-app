using FSUIPC;
using System;
using System.Windows;
using System.Windows.Interop;

namespace PushbackHelper.MSFSLocalService
{
    public class SimLocalService : IDisposable
    {
        private bool disposedValue;

        private bool connected;

        private MSFSVariableServices msfsVariableService;

        public event EventHandler<bool> ConnectionStateChanged;

        public event EventHandler<ParkingBrakeStateChangedEventArgs> ParkingBrakeStateChanged;

        public event EventHandler<LocalVariableChangedEventArgs> LocalVariableChanged;

        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                if (connected != value)
                {
                    connected = value;
                    OnConnectionStateChanged(value);
                }
            }
        }

        public SimLocalService()
        {
            msfsVariableService = new MSFSVariableServices();
        }

        public void Connect()
        {
            var lHwnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;

            msfsVariableService.OnVariablesReadyChanged += MSFSVariableService_OnVariablesReadyChanged;
            msfsVariableService.OnValuesChanged += MSFSVariableService_OnValuesChanged;

            msfsVariableService.Init(lHwnd);
            msfsVariableService.LVARUpdateFrequency = 10;

            msfsVariableService.Start();
        }

        private void MSFSVariableService_OnVariablesReadyChanged(object sender, EventArgs e)
        {
            Connected = msfsVariableService.VariablesReady;
        }

        private void MSFSVariableService_OnValuesChanged(object sender, EventArgs e)
        {
            foreach (FsLVar lvar in msfsVariableService.LVarsChanged)
            {
                var switchType = SwitchDefinitions.GetSwitchType(lvar.Name);
                switch (switchType)
                {
                    case SwitchType.ParkingBrake:
                        OnParkingBrakeStateChanged(lvar);
                        break;
                    
                    default:
                        OnLocalVariableChanged(lvar);
                        break;
                }
            }
        }

        protected virtual void OnConnectionStateChanged(bool state)
        {
            var eventArgs = new ConnectionStateChangedEventArgs
            {
                Connected = state
            };

            var eventHandler = ConnectionStateChanged;
            eventHandler?.Raise(this, eventArgs);
        }

        protected virtual void OnParkingBrakeStateChanged(FsLVar lvar)
        {
            var eventArgs = new ParkingBrakeStateChangedEventArgs
            {
                Id = lvar.ID,
                Name = lvar.Name,
                Active = lvar.Value > 0
            };

            var eventHandler = ParkingBrakeStateChanged;
            eventHandler?.Raise(this, eventArgs);
        }

        protected virtual void OnLocalVariableChanged(FsLVar lVar)
        {
            var eventArgs = new LocalVariableChangedEventArgs
            {
                Id = lVar.ID,
                Name = lVar.Name,
                Value = lVar.Value
            };

            var eventHandler = LocalVariableChanged;
            eventHandler?.Raise(this, eventArgs);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    msfsVariableService?.Stop();
                    msfsVariableService = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
