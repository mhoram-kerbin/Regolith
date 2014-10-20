using System;

namespace Regolith.Scenario
{
    public class RegolithGameSettings
    {
        public int seed { get; set; }

        public void Load(ConfigNode node)
        {
            if (node.HasNode("RegolithGameSettings"))
            {
                ConfigNode settingsNode = node.GetNode("RegolithGameSettings");
                seed = GetValue(settingsNode, "GameSeed", seed);
            }
            else
            {
                //Set our seed
                var r = new System.Random();
                seed = r.Next(1, Int32.MaxValue);
            }
        }

        public void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode("RegolithGameSettings"))
            {
                settingsNode = node.GetNode("RegolithGameSettings");
            }
            else
            {
                settingsNode = node.AddNode("RegolithGameSettings");
            }

            settingsNode.AddValue("GameSeed", seed);
        }

        public static int GetValue(ConfigNode config, string name, int currentValue)
        {
            int newValue;
            if (config.HasValue(name) && int.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }
    }
}