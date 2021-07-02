using System.Collections.Generic;

namespace PushbackHelper.MSFSLocalService
{
    public static class SwitchDefinitions
    {
        // Assignment lvar to switch types
        private static readonly Dictionary<string, SwitchType> _switchDefinitions = new Dictionary<string, SwitchType> {
            { "A32NX_PARK_BRAKE_LEVER_POS", SwitchType.ParkingBrake }
        };

        public static bool Exists(string name)
        {
            return _switchDefinitions.ContainsKey(name);
        }

        public static SwitchType GetSwitchType(string name)
        {
            if (!Exists(name))
            {
                return SwitchType.Undefined;
            }

            return _switchDefinitions[name];
        }
    }
}
