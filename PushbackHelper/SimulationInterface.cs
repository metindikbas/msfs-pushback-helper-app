using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PushbackHelper
{
    public class SimulationInterface
    {
        private bool _connectionStatus;
        private IntPtr IntPtr = new IntPtr(1);
        private Timer _timer;
        private Microsoft.FlightSimulator.SimConnect.SimConnect SimConnectClient;
        public event EventHandler SimulationConnected;
        public event EventHandler SimulationDisconnected;

        public SimulationInterface()
        {

        }

        private void TickTimer(object state)
        {
            if (!_connectionStatus)
            {
                StopTimer();
            }
        }

        public void Connect()
        {
            try
            {
                SimConnectClient = new Microsoft.FlightSimulator.SimConnect.SimConnect("Managed Data Request", IntPtr, 0x402, null, 0);
                _connectionStatus = true;
                SimulationConnected?.Invoke(this, new EventArgs());
                _timer = new Timer(new TimerCallback(TickTimer), null, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(500));
            }
            catch (Exception ex)
            {
                _connectionStatus = false;
            }
        }

        private void StopTimer()
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.Zero);
            _timer.Dispose();
        }

        public void Disconnect()
        {
            try
            {
                StopTimer();
                SimConnectClient.Dispose();
                _connectionStatus = false;
                SimulationDisconnected?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                _connectionStatus = false;
            }
        }
    }
}
