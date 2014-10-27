using System;
using System.Collections.Generic;
using Regolith.Scenario;

namespace Regolith.Common
{
    public static class Utilities
    {
        public static T LoadNodeProperties<T>(ConfigNode node)
            where T : new()
        {
            var nodeType = typeof (T);
            var newNode = new T();
            //Iterate our properties and set values
            foreach (var val in node.values)
            {
                var cval = (ConfigNode.Value)val;
                var propInfo = nodeType.GetProperty(cval.name);
                if (propInfo != null)
                {
                    propInfo.SetValue(newNode, Convert.ChangeType(cval.value, propInfo.PropertyType), null);
                }
            }
            return newNode;
        }

        public static List<ResourceData> ImportConfigNodeList(ConfigNode[] nodes)
        {
            var resList = new List<ResourceData>();
            foreach (var node in nodes)
            {
                var res = LoadNodeProperties<ResourceData>(node);
                var distNode = node.GetNode("Distribution");
                if (distNode != null)
                {
                    var dist = LoadNodeProperties<DistributionData>(distNode);
                    res.Distribution = dist;
                }

                resList.Add(res);
            }
            return resList;
        }

        public static double GetValue(ConfigNode node, string name, double curVal)
        {
            double newVal;
            if (node.HasValue(name) && double.TryParse(node.GetValue(name), out newVal))
            {
                return newVal;
            }
            else
            {
                return curVal;
            }
        }

        public static double GetMaxDeltaTime()
        {
            //Default to one Kerbin day
            return 21600f;
        }

        public static double GetECDeltaTime()
        {
            //Default to one second
            return 1;
        }
    }
}