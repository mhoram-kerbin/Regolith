using Regolith.Scenario;

namespace Regolith.Common
{
    public class ResourceData 
    {
        public string ResourceName { get; set; }
        public string PlanetName { get; set; }
        public string BiomeName { get; set; }

        public int ResourceType { get; set; } 
        public DistributionData Distribution { get; set; }
    }
}