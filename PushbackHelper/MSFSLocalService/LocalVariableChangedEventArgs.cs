using System;

namespace PushbackHelper.MSFSLocalService
{
    public class LocalVariableChangedEventArgs : EventArgs
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Value { get; set; }
    }
}
