using System;
using System.Collections.Generic;
using System.Linq;
using Regolith.Common;
using Random = System.Random;

namespace Regolith.Asteroids
{
    public class REGO_ModuleAsteroidInfo : PartModule
    {
        [KSPField(guiActive = true, guiName = "Mass", guiActiveEditor = false)]
        public string mass = "???";

        [KSPField(guiActive = true, guiName = "Resources", guiActiveEditor = false)]
        public string resources = "???";

        [KSPField(isPersistant = true)] 
        public float massThreshold = 0f;
        public override void OnStart(StartState state)
        {
            if (massThreshold <= Utilities.FLOAT_TOLERANCE)
            {
                //Setup time!
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
            var minMass = r.Next(10, 50) / 100f;
            massThreshold = part.mass * minMass;
            var resources = new List<ResourceData>();

            var resInfoList = part.Modules.OfType<REGO_ModuleAsteroidResource>();
            foreach (var resInfo in resInfoList)
            {
                //Let's see if we even have it.
                if (r.Next(100) < resInfo.presenceChance)
                {
                    var res = new ResourceData
                              {
                                  Name = resInfo.resourceName,
                                  Weight = r.Next(resInfo.lowRange, resInfo.highRange)
                              };
                    resources.Add(res);
                }
            }

            var totalWeight = resources.Sum(rd => rd.Weight);

            foreach (var resInfo in resInfoList)
            {
                if(resources.Any(rs=>rs.Name == resInfo.resourceName))
                {
                    var resData = resources.First(rd => rd.Name == resInfo.resourceName);
                    float resWeight = resData.Weight / totalWeight;
                    resInfo.abundance = 0.01f + resWeight;
                }
            }
        }

        public override void OnUpdate()
        {
            mass = String.Format("{0:0.00}t", part.mass);
            resources = String.Format("{0:0.00}t ({1:0})%", part.mass - massThreshold, (part.mass - massThreshold) / part.mass * 100);
        }
    }
}