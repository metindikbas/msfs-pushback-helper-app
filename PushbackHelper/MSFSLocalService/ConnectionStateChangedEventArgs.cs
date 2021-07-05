using System;

namespace PushbackHelper.MSFSLocalService
{
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        public bool Connected { get; set; }
    }
}
