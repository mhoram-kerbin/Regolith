using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Regolith.Scenario;
using SimplexNoise;
using UnityEngine;
using Random = System.Random;

namespace Regolith.Common
{
    public abstract class RegolithResourceMap : MonoBehaviour
    {
        private static List<ResourceData> _globalResources;
        private static List<ResourceData> _planetaryResources;
        private static List<ResourceData> _biomeResources;

        public static List<ResourceData> BiomeResources
        {
            get { return _biomeResources ?? (_biomeResources = LoadResourceInfo("REGOLITH_BIOME_RESOURCE")); }
        }
        public static List<ResourceData> GlobalResources
        {
            get { return _globalResources ?? (_globalResources = LoadResourceInfo("REGOLITH_GLOBAL_RESOURCE")); }
        }
        public static List<ResourceData> PlanetaryResources
        {
            get { return _planetaryResources ?? (_planetaryResources = LoadResourceInfo("REGOLITH_PLANETARY_RESOURCE")); }
        }
        protected static List<ResourceData> LoadResourceInfo(string node)
        {
            var resList = GameDatabase.Instance.GetConfigNodes(node);
            return Utilities.ImportConfigNodeList(resList);
        }
        protected static DistributionData GetBestResourceData(List<ResourceData> configs)
        {
            try
            {
                var distro = new DistributionData
                             {
                                 Variance = configs.First().Distribution.Variance,
                                 PresenceChance = configs.First().Distribution.PresenceChance,
                                 MinAbundance = configs.First().Distribution.MinAbundance,
                                 MaxAbundance = configs.First().Distribution.MaxAbundance
                             };


                foreach (var config in configs)
                {
                    var cd = config.Distribution;
                    if (cd.PresenceChance > 0 && cd.PresenceChance > distro.PresenceChance)
                        distro.PresenceChance = cd.PresenceChance;
                    if (cd.MinAbundance > 0 && cd.MinAbundance < distro.MinAbundance)
                        distro.MinAbundance = cd.MinAbundance;
                    if (cd.MaxAbundance > 0 && cd.MaxAbundance > distro.MaxAbundance)
                        distro.MaxAbundance = cd.MaxAbundance;
                    if (cd.Variance > 0 && cd.Variance > distro.Variance)
                        distro.Variance = cd.Variance;

                    if (cd.MinAltitude > 0 && cd.MinAltitude < distro.MinAltitude)
                        distro.MinAltitude = cd.MinAltitude;
                    if (cd.MaxAltitude > 0 && cd.MaxAltitude > distro.MaxAltitude)
                        distro.MaxAltitude = cd.MaxAltitude;
                    if (cd.MinRange > 0 && cd.MinRange < distro.MinRange)
                        distro.MinRange = cd.MinRange;
                    if (cd.MaxRange > 0 && cd.MaxRange > distro.MaxRange)
                        distro.MaxRange = cd.MaxRange;
                
                }
                return distro;
            }
            catch (Exception e)
            {
                print("[REGO] - Error in - RegolithResourceMap_GetBestResourceData - " + e.Message);
                return null;
            }
        }

        public static float GetAbundance(double lat, double lon, string resourceName, int bodyId, int resourceType = 0, double altitude = 0)
        {
            try
            {
                var body = FlightGlobals.Bodies[bodyId];
                var biome = body.BiomeMap.GetAtt(lat, lon);
                var seed = RegolithScenario.Instance.gameSettings.seed;
                DistributionData distro = null;
                seed *= (bodyId + 1);
                seed += resourceName.Length * resourceName.Substring(1).ToCharArray().First();
                seed += Convert.ToInt32(biome.mapColor.grayscale * 4096);
                //First - we need to determine our data set for randomization.
                //Is there biome data?
                var biomeConfigs = BiomeResources.Where(
                    r => r.PlanetName == body.bodyName
                         && r.BiomeName == biome.name
                         && r.ResourceName == resourceName
                         && r.ResourceType == resourceType).ToList();
                var planetConfigs =
                    PlanetaryResources.Where(
                        r => r.PlanetName == body.bodyName
                             && r.ResourceName == resourceName
                             && r.ResourceType == resourceType).ToList();
                var globalConfigs =
                    GlobalResources.Where(
                        r => r.ResourceName == resourceName
                             && r.ResourceType == resourceType).ToList();
                //Extrapolate based on matching overrides
                if (biomeConfigs.Any())
                {
                    distro = GetBestResourceData(biomeConfigs);
                    seed *= 2;
                }
                else if (planetConfigs.Any())
                {
                    distro = GetBestResourceData(planetConfigs);
                    seed *= 3;
                }
                else if (globalConfigs.Any())
                {
                    distro = GetBestResourceData(globalConfigs);
                    seed *= 4;
                }
                else
                {
                    return 0f;
                }

                var rand = new Random(seed);
                //Our Simplex noise:
                var noiseSeed = new int[8];
                for (int ns = 0; ns < 8; ns++)
                {
                    noiseSeed[ns] = rand.Next();
                }
                var spx = new SimplexNoiseGenerator(noiseSeed);
                var noise = spx.noise((float)(lat + 180)/2, (float)(lon + 180)/2, rand.Next());
                var presenceRoll = rand.Next(100);
                var isPresent = (presenceRoll <= distro.PresenceChance);
                if (!isPresent)
                    return 0f;

                //Basic abundance 
                var min = distro.MinAbundance * 1000;
                var max = distro.MaxAbundance * 1000;
                //In case someone is silly
                if (min > max)
                    max = min + 1;
                float abundance = rand.Next((int)min, (int)max) / 1000;

                //Variance and noise.  20% minimum.
                //Only applies to all but interplanetary
                if (resourceType <= 2)
                {
                    abundance += (abundance*noise*distro.Variance/100);
                }

                //Altitude band - only applies to atmospheric and interplanetary
                if (resourceType >= 2 && distro.HasVariableAltitude())
                {
                    var rad = body.Radius;
                    var ideal = ((rad*distro.MinAltitude) + (rad*distro.MaxAltitude))/2;
                    var range = rand.Next((int)(rad * distro.MinRange), (int)(rad * distro.MaxRange));
                    var diff = Math.Abs(ideal - altitude);
                    var rangePerc = diff / range;
                    var modifier = 1d - rangePerc;
                    abundance *= (float)modifier;
                }

                
                
                if (abundance <= Utilities.FLOAT_TOLERANCE)
                    return 0f;

                //Return it as a float not a percent
                return abundance / 100;
            }
            catch (Exception e)
            {
                print("[REGO] - Error in - RegolithResourceMap_GetAbundance - " + e.Message);
                return 0f;
            }
        }
    }
}