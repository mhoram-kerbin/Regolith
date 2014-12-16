using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Regolith.Scenario;
using UnityEngine;
using Random = System.Random;

namespace Regolith.Common
{
    public class RegolithResourceMap : MonoBehaviour
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
        private static List<ResourceData> LoadResourceInfo(string node)
        {
            var resList = GameDatabase.Instance.GetConfigNodes(node);
            return Utilities.ImportConfigNodeList(resList);
        }
        private static DistributionData GetBestResourceData(List<ResourceData> configs)
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


        private static CBAttributeMapSO.MapAttribute GetBiome(double lat, double lon, CelestialBody body)
        {
            try
            {
                var biome =  body.BiomeMap.GetAtt(lat, lon);
                return biome;
            }
            catch (Exception)
            {
                //Just means we have no biome.                    
                return null;
            }
        }

        public static float GetAbundance(double lat, double lon, string resourceName, int bodyId, int resourceType = 0, double altitude = 0)
        {
            try
            {
                var body = FlightGlobals.Bodies.FirstOrDefault(b => b.flightGlobalsIndex == bodyId);
                var biome = GetBiome(lat, lon, body);
                var seed = RegolithScenario.Instance.gameSettings.seed;
                seed *= (bodyId + 1);
                seed += resourceName.Length * resourceName.Substring(1).ToCharArray().First();
                seed += body.bodyName.Length * body.bodyName.Substring(1).ToCharArray().First();
                if (biome != null)
                    seed += Convert.ToInt32(biome.mapColor.grayscale*4096)*(resourceType + 1);
                //First - we need to determine our data set for randomization.
                //Is there biome data?
                DistributionData distro = null;
                var biomeName = "UNKNOWN";
                if (biome != null)
                {
                    biomeName = biome.name;
                }
                var biomeConfigs = BiomeResources.Where(
                        r => r.PlanetName == body.bodyName
                             && r.BiomeName == biomeName
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
                var spx = new NoiseGenerator(noiseSeed);
                var noiseX = (float) lat;
                var noiseY = (float) lon;
                var noiseZ = (float) rand.Next(100) / 100f;
                //print("[REGO] NX: " + noiseX);
                //print("[REGO] NY: " + noiseY);
                //print("[REGO] NZ: " + noiseZ);
                var noise = spx.noise(noiseX, noiseY, noiseZ);
                //print("[REGO] NOISE: " + noise);
                var presenceRoll = rand.Next(100);
                var isPresent = (presenceRoll <= distro.PresenceChance);
                if (!isPresent)
                    return 0f;
                //Basic abundance 
                int min = (int)(distro.MinAbundance * 1000);
                int max = (int)(distro.MaxAbundance * 1000);
                //In case someone is silly
                if (min > max)
                    max = min + 1;
                var ab = rand.Next(min, max);
                float abundance = ab / 1000f;

                //print("[REGO] ABUNDANCE-1: " + abundance);

                //Variance and noise.  20% minimum.
                //Applies to all but interplanetary
                if (resourceType <= 2)
                {
                    abundance += (abundance*noise*distro.Variance/100);
                }
                //print("[REGO] ABUNDANCE-2: " + abundance);

                //Altitude band - only applies to atmospheric and interplanetary
                if (resourceType >= 2 && distro.HasVariableAltitude())
                {
                    var rad = body.Radius;
                    var ideal = ((rad*distro.MinAltitude) + (rad*distro.MaxAltitude))/2;
                    //print("REGO: IDEAL = " + ideal);
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

        public static Vector2 GetDepletionNode(double lat, double lon)
        {
            //For precision, we'll be rounding.
            //This gives us 65K potential drill sites.
            var x = Math.Round(lat, 0);
            var y = Math.Round(lon, 0);
            return new Vector2((float)x,(float)y);
        }
    }
}