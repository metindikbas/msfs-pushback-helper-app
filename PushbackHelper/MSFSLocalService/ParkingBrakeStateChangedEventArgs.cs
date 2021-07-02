using System;

namespace PushbackHelper.MSFSLocalService
{
    public class ParkingBrakeStateChangedEventArgs : EventArgs
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }
    }
}
