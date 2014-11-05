using System.Collections.Generic;

namespace Regolith.Scenario
{
    public class PlanetaryResourceData
    {
        public string PlanetName { get; set; }
        public DistributionData Distribution { get; set; }

        public List<PlanetaryResourceData> BiomeOverrides { get; private set; }
    }
}