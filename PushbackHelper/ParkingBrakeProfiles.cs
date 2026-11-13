using System;
using System.Linq;

namespace PushbackHelper
{
    /// <summary>
    /// An aircraft whose parking brake is not the standard BRAKE PARKING POSITION but a local variable of its own.
    /// Aircraft that match no profile use the standard variable and the PARKING_BRAKES event.
    /// </summary>
    class ParkingBrakeProfile
    {
        public string Name { get; set; }
        /// <summary>Case-insensitive fragments of the aircraft's TITLE.</summary>
        public string[] TitleFragments { get; set; } = new string[0];
        /// <summary>Case-insensitive fragments of the loaded aircraft's package path.</summary>
        public string[] PathFragments { get; set; } = new string[0];
        /// <summary>The local variable, without the "L:" prefix.</summary>
        public string LVar { get; set; }
        public Func<double, bool> IsSet { get; set; }
        /// <summary>
        /// True when the brake is set and released by writing the local variable. When false the
        /// PARKING_BRAKES event is used, as for any other aircraft.
        /// </summary>
        public bool WriteLVar { get; set; }
        public double SetValue { get; set; } = 1;
        public double ReleaseValue { get; set; } = 0;

        public bool Matches(string title, string path)
        {
            return Contains(title, TitleFragments) || Contains(path, PathFragments);
        }

        private static bool Contains(string value, string[] fragments)
        {
            return !string.IsNullOrEmpty(value)
                && fragments.Any(fragment => value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    static class ParkingBrakeProfiles
    {
        private static readonly ParkingBrakeProfile[] Profiles =
        {
            // Lever: 0 off, 1 set. Since the aircraft's update of 22 October 2026 BRAKE PARKING POSITION no longer
            // follows it, and the PARKING_BRAKES event does not move it either.
            new ParkingBrakeProfile
            {
                Name = "Fenix A319/A320/A321",
                TitleFragments = new[] { "FenixA3" },
                PathFragments = new[] { "FNX_32X" },
                LVar = "S_MIP_PARKING_BRAKE",
                IsSet = value => value >= 1,
                WriteLVar = true
            },
            new ParkingBrakeProfile
            {
                Name = "FlyByWire A32NX",
                TitleFragments = new[] { "A320 Neo FlyByWire" },
                PathFragments = new[] { "FlyByWire_A320_NEO" },
                LVar = "A32NX_PARK_BRAKE_LEVER_POS",
                IsSet = value => value >= 0.5
            },
            new ParkingBrakeProfile
            {
                Name = "Headwind A330",
                TitleFragments = new[] { "Headwind Simulations ACJ330" },
                PathFragments = new[] { "Headwind_A330", "Headwind_ACJ330" },
                LVar = "A32NX_PARK_BRAKE_LEVER_POS",
                IsSet = value => value >= 0.5
            },
            // The variable the aircraft gives its own ground services to tell that it is braked.
            new ParkingBrakeProfile
            {
                Name = "FSLabs A32x",
                TitleFragments = new[] { "FSLabs A32" },
                PathFragments = new[] { "fsl-a32x", "FSLabs A32" },
                LVar = "FSLA320_ParkBrake",
                IsSet = value => value >= 1
            }
        };

        /// <summary>The profile for the aircraft, or null to use the standard variable.</summary>
        public static ParkingBrakeProfile Find(string title, string path)
        {
            return Profiles.FirstOrDefault(profile => profile.Matches(title, path));
        }
    }
}
