using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Regolith.Scenario;
using UnityEngine;

namespace Regolith.Common
{
    public static class Utilities
    {
        public const double FLOAT_TOLERANCE = 0.000000001d;

        public static T LoadNodeProperties<T>(ConfigNode node)
            where T : new()
        {
            var nodeType = typeof (T);
            var newNode = new T();
            //Iterate our properties and set values
            foreach (var val in node.values)
            {
                var cval = (ConfigNode.Value) val;
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

        public static List<DepletionData> ImportDepletionNodeList(ConfigNode[] nodes)
        {
            var depDataList = new List<DepletionData>();
            foreach (var node in nodes)
            {
                var depData = LoadNodeProperties<DepletionData>(node);
                var depNodeList = node.GetNodes("DEPLETION_NODE");
                if (depNodeList != null)
                {
                    foreach (var dnode in depNodeList)
                    {
                        var depNode = LoadNodeProperties<DepletionNode>(dnode);
                        depData.DepletionNodes.Add(depNode);
                    }                   
                }
                depDataList.Add(depData);
            }
            return depDataList;
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
            //Default to 24h
            return 86400;
        }

        public static double GetSecondsPerTick()
        {
            //Default to 1/50th of a second
            return 0.02f;
        }

        public static double GetMaxECDeltaTime()
        {           
            return 1;
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

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T) formatter.Deserialize(ms);
            }
        }


        public static double Deg2Rad(double degrees)
        {
            return degrees*Math.PI/180;
        }

        public static double Rad2Lat(double radians)
        {
            var rad = radians%(Math.PI*2);
            if (rad < 0)
                rad = 2*Math.PI + rad;
            
            var radLat = rad%(Math.PI);
            if (radLat > Math.PI/2)
                radLat = Math.PI - radLat;
            if (rad > Math.PI)
                rad = -radLat;
            else
                rad = radLat;
            return (rad/Math.PI*180);
        }

        public static double Rad2Lon(double radians)
        {
            var rad = radians%(Math.PI*2);
            if (rad < 0)
                rad = 2*Math.PI + rad;
            
            var radLon = rad%(Math.PI*2);
            if (radLon > Math.PI)
                radLon = Math.PI*2 - radLon;
            if (rad > Math.PI)
                rad = -radLon;
            else
                rad = radLon;
            return (rad/Math.PI*180);
        }
    }
}