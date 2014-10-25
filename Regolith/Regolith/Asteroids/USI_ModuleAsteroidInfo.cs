using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Regolith.Asteroids;
using UnityEngine;
using Random = System.Random;

namespace DynamicTanks
{
    public class USI_ModuleAsteroidInfo : PartModule
    {
        public override void OnStart(StartState state)
        {
            if (part.Resources.Count == 0)
            {
                print("[REGOLITH] Setting up resources");
                SetupAsteroidResources();
            }
        }

        private class ResourceData
        {
            public float Weight;
            public string Name;
        }

        private void SetupAsteroidResources()
        {
            var r = new Random();
            double purity = r.Next(10, 90) / 100f;
            var resources = new List<ResourceData>();

            var resInfoList = part.Modules.OfType<USI_ModuleAsteroidResource>();
            foreach (var resInfo in resInfoList)
            {
                //Let's see if we even have it.
                if (r.Next(100) < resInfo.presenceChance)
                {
                    var res = new ResourceData();
                    res.Name = resInfo.resourceName;
                    res.Weight = r.Next(resInfo.lowRange, resInfo.highRange);
                    resources.Add(res);
                    print("[REGOLITH] found " + res.Weight + " " + res.Name );
                }
            }

            var totalWeight = resources.Sum(rd => rd.Weight);

            foreach (var resInfo in resInfoList)
            {
                if(resources.Any(rs=>rs.Name == resInfo.resourceName))
                {
                    var resData = resources.First(rd => rd.Name == resInfo.resourceName);
                    double resWeight = resData.Weight / totalWeight * purity;
                    var resNode = new ConfigNode("RESOURCE");
                    resNode.AddValue("name", resData.Name);
                    resNode.AddValue("amount", 0.0001);
                    resNode.AddValue("maxAmount", 0);
                    resInfo.abundance = 0.001 + resWeight;
                    print("[REGOLITH] adding " + resInfo.abundance + " " + resData.Name + " weight " + resData.Weight); 
                    part.AddResource(resNode);
                }
            }
            //And rock 
            var resRock = new ConfigNode("RESOURCE");
            resRock.AddValue("name", "Rock");
            resRock.AddValue("amount", 0.0001);
            resRock.AddValue("maxAmount", 0);
            part.AddResource(resRock);
        }

    }
}