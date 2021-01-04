namespace PushbackHelper
{
    enum RequestsEnum
    {
        RefreshDataRequest,
        StartTugMode,
        ExitTypeRequest,
        ExitOpenRequest
    }
    enum DefinitionsEnum
    {
        RefreshDataStruct,
        ExitTypeStruct,
        ExitOpenStruct,
        PushbackWait,
        VelocityX,
        VelocityY,
        VelocityZ,
        RotationX,
        RotationY,
        RotationZ
    }
    enum EventsEnum
    {
        KEY_PUSHBACK_SET,
        KEY_TUG_HEADING,
        KEY_TOGGLE_JETWAY,
        KEY_REQUEST_FUEL,
        KEY_TOGGLE_AIRCRAFT_EXIT
    }
    enum NotificationGroupsEnum
    {
        Group0
    }
    public struct RefreshDataStruct
    {
        public double trueHeading;
        public uint pushbackState;
        public bool parkingBrakeState;
    }
    public struct ExitDataStruct
    {
        public double exitdata1;
        public double exitdata2;
        public double exitdata3;
        public double exitdata4;
        public double exitdata5;
        public double exitdata6;
        public double exitdata7;
        public double exitdata8;
        public double exitdata9;
        public double exitdata10;
        public double exitdata11;
        public double exitdata12;
        public double exitdata13;
        public double exitdata14;
        public double exitdata15;
        public double exitdata16;
        public double exitdata17;
        public double exitdata18;
        public double exitdata19;
        public double exitdata20;
        public double exitdata21;
        public double exitdata22;
        public double exitdata23;
        public double exitdata24;
        public double exitdata25;
        public double exitdata26;
        public double exitdata27;
        public double exitdata28;
        public double exitdata29;
        public double exitdata30;
        public double exitdata31;
        public double exitdata32;
        public double exitdata33;
        public double exitdata34;
        public double exitdata35;
        public double exitdata36;
        public double exitdata37;
        public double exitdata38;
        public double exitdata39;
        public double exitdata40;
        public double exitdata41;
        public double exitdata42;
        public double exitdata43;
        public double exitdata44;
        public double exitdata45;
        public double exitdata46;
        public double exitdata47;
        public double exitdata48;
        public double exitdata49;
        public double exitdata50;
    }
}
