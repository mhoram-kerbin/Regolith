using System;
using System.Collections.Generic;
using Regolith.Scenario;
using UnityEngine;

namespace Regolith.Common
{
    public static class Utilities
    {
        public const double FLOAT_TOLERANCE = 0.000000001d;

        public static Vector2 LatLonToCart(double lat, double lon, int precision = 10)
        {
            var x = (float)Math.Round(lon * Math.Cos(lat), precision);
            var y = (float)Math.Round(lat, precision);
            var cart = new Vector2(x, y);
            return cart;
        }

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
            //Default to one hour
            return 3600;
        }

        public static double GetECDeltaTime()
        {
            //Default to 30 seconds
            return 30;
        }

        public static double GetAltitude(Vessel v)
        {
            v.GetHeightFromTerrain();
            double alt = v.heightFromTerrain;
            if (alt < 0)
            {
                alt = v.mainBody.GetAltitude(v.CoM);
            }
            return alt;
        }
    }
}