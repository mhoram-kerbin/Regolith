using System.Collections.Generic;

namespace Regolith.Scenario
{
    public class ResourceData
    {
        public string ResourceName { get; set; }
        public DistributionData Distribution { get; set; }

        public List<PlanetaryResourceData> PlanetaryOverrides { get; private set; }
    }
}