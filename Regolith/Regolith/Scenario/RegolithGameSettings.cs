using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Regolith.Common;
using UnityEngine;

namespace Regolith.Scenario
{
    public class RegolithGameSettings
    {
        public int Seed { get; set; }
        public List<DepletionData> DepletionInfo { get; private set; }
        public ConfigNode SettingsNode { get; private set; }


        public void Load(ConfigNode node)
        {
            if (node.HasNode("RegolithGameSettings"))
            {
                SettingsNode = node.GetNode("RegolithGameSettings");
                Seed = GetValue(SettingsNode, "GameSeed", Seed);
                DepletionInfo = SetupDepletionInfo();
            }
            else
            {
                //Set our seed
                var r = new System.Random();
                Seed = r.Next(1, Int32.MaxValue);
                DepletionInfo = new List<DepletionData>();
            }
        }


        private List<DepletionData> SetupDepletionInfo()
        {
            var depletionNodes = SettingsNode.GetNodes("DEPLETION_DATA");
            return Utilities.ImportDepletionNodeList(depletionNodes);
        }

        public void Save(ConfigNode node)
        {
            if (node.HasNode("RegolithGameSettings"))
            {
                SettingsNode = node.GetNode("RegolithGameSettings");
            }
            else
            {
                SettingsNode = node.AddNode("RegolithGameSettings");
            }

            SettingsNode.AddValue("GameSeed", Seed);
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

        /*DepletionNodeStorage
         * 
         *      DEPLETION_DATA
         *      {
         *          PLANET_ID = 1
         *          RESOURCE_NAME = Karbonite
         *          DEPLETION_NODE
         *          {
         *              x = 10
         *              y = 20
         *              value = 0.1
         *              lastUpdate = 1234567890
         *          }
         *      }
         */

        private DepletionNode GetDepletionNode(int planetId, string resource, int x, int y)
        {
            //Does a node exist?
            var planetInfo = DepletionInfo.FirstOrDefault(n => n.PlanetId == planetId && n.ResourceName == resource);
            if (planetInfo == null)
                return null;  //Default is 100%

            var node = planetInfo.DepletionNodes.FirstOrDefault(d => d.X == x && d.Y == y);
            return node;
        }

        public float GetDepletionNodeValue(int planetId, string resource, int x, int y)
        {
            var node = GetDepletionNode(planetId, resource, x, y);
            if (node == null)
                return 1f;
            return node.Value;
        }

        public void SetDepletionNodeValue(int planetId, string resource, int x, int y, float value)
        {
            var planetInfo = DepletionInfo.FirstOrDefault(n => n.PlanetId == planetId && n.ResourceName == resource);
            if (planetInfo == null)
            {
                planetInfo = new DepletionData();
                planetInfo.PlanetId = planetId;
                planetInfo.ResourceName = resource;
                DepletionInfo.Add(planetInfo);
            }

            var node = GetDepletionNode(planetId, resource, x, y);
            if (node == null)
            {
                node = new DepletionNode();
                node.X = x;
                node.Y = y;
                planetInfo.DepletionNodes.Add(node);
            }
            node.Value = value;
            node.LastUpdate = Planetarium.GetUniversalTime();
        }

    }
}