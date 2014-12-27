using System.Collections.Generic;

namespace Regolith.Scenario
{
    public class DepletionData
    {
        public int PlanetId { get; set; }
        public string ResourceName { get; set; }
        public List<DepletionNode> DepletionNodes { get; private set; }

        public DepletionData()
        {
            DepletionNodes = new List<DepletionNode>();
        }
    }
}