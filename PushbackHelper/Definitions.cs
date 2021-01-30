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
        TOGGLE_PUSHBACK,
        TOGGLE_JETWAY,
        TOGGLE_AIRCRAFT_EXIT,
        TOGGLE_PARKING_BRAKES,
        TOGGLE_RAMPTRUCK,
        SET_TUG_HEADING,
        REQUEST_FUEL,
        REQUEST_LUGGAGE,
        REQUEST_POWER_SUPPLY,
        REQUEST_CATERING
    }
    enum NotificationGroupsEnum
    {
        Group0
    }
    public struct RefreshDataStruct
    {
        public double trueHeading;
        public uint pushbackState;
        public bool pushbackAttached;
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
    }
}
