using Regolith.Common;

namespace Regolith.Scenario
{
    public class DistributionData
    {
        public float PresenceChance { get; set; }
        public float MinAbundance { get; set; }
        public float MaxAbundance { get; set; }
        public float Variance { get; set; }
        public float MinAltitude { get; set; }
        public float MaxAltitude { get; set; }

        public float MinRange { get; set; }
        public float MaxRange { get; set; }
       
        public bool HasVariableAltitude()
        {
            return (MinAltitude > Utilities.FLOAT_TOLERANCE || MaxAltitude > Utilities.FLOAT_TOLERANCE);
        }
    }
}