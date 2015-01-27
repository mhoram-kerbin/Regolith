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

        [Obsolete("Keeping in for SCANSat compatibility, use an AbundanceRequest instead")]
        public static float GetAbundance(double latitude, double longitude, string resourceName, int bodyId,
            HarvestTypes resourceType = HarvestTypes.Planetary, double altitude = 0)
        {
            var abRequest = new AbundanceRequest
                            {
                                Latitude = latitude,
                                Longitude = longitude,
                                BodyId = bodyId,
                                ResourceName = resourceName,
                                ResourceType = resourceType,
                                Altitude = altitude,
                                CheckForLock = true
                            };
            return GetAbundance(abRequest);
        }

        public static int GenerateAbundanceSeed(AbundanceRequest request, CBAttributeMapSO.MapAttribute biome, CelestialBody body)
        {
            var seed = RegolithScenario.Instance.gameSettings.Seed;
            seed *= (request.BodyId + 1);
            seed += request.ResourceName.Length * request.ResourceName.Substring(1).ToCharArray().First();
            seed += body.bodyName.Length * body.bodyName.Substring(1).ToCharArray().First();

            if (biome != null)
            {
                seed += Convert.ToInt32(biome.mapColor.grayscale * 4096) * ((int)request.ResourceType + 1);
            }
            return seed;
        }

        public static float GetAbundance(AbundanceRequest request)
        {
            try
            {
                var northing = Utilities.Deg2Rad(request.Latitude);
                var easting = Utilities.Deg2Rad(request.Longitude);
                var body = FlightGlobals.Bodies.FirstOrDefault(b => b.flightGlobalsIndex == request.BodyId);
                var biome = Utilities.GetBiome(northing, easting, body);
                var seed = GenerateAbundanceSeed(request, biome, body);
                var biomeName = GetDefaultSituation(request.ResourceType);
                if (biome != null)
                {
                    biomeName = biome.name;
                }

                //Is the biome even locked?  If not, just return a negative one
                //so the consuming application can act accordingly.
                if (RegolithScenario.Instance.gameSettings.IsBiomeUnlocked(request.BodyId, biomeName))
                    return -1;

                //We need to determine our data set for randomization.
                //Is there biome data?
                DistributionData distro = null;
                var biomeConfigs = BiomeResources.Where(
                        r => r.PlanetName == body.bodyName
                             && r.BiomeName == biomeName
                             && r.ResourceName == request.ResourceName
                             && r.ResourceType == (int)request.ResourceType).ToList();
                var planetConfigs =
                    PlanetaryResources.Where(
                        r => r.PlanetName == body.bodyName
                             && r.ResourceName == request.ResourceName
                             && r.ResourceType == (int)request.ResourceType).ToList();
                var globalConfigs =
                    GlobalResources.Where(
                        r => r.ResourceName == request.ResourceName
                             && r.ResourceType == (int)request.ResourceType).ToList();
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
                for (var ns = 0; ns < 8; ns++)
                {
                    noiseSeed[ns] = rand.Next();
                }
                var spx = new NoiseGenerator(noiseSeed);
                var noiseX = (float) northing * distro.Variance / 10f;
                var noiseY = (float)easting * distro.Variance / 10f;
                var noiseZ = (rand.Next(100)) / 100f;
                var noise = spx.noise(noiseX, noiseY, noiseZ);
                
                var presenceRoll = rand.Next(100);
                var isPresent = (presenceRoll <= distro.PresenceChance);
                if (!isPresent)
                    return 0f;
                
                //Abundance begins with a starting range.
                var min = (int)(distro.MinAbundance * 1000);
                var max = (int)(distro.MaxAbundance * 1000);
                //In case someone is silly
                if (min > max)
                    max = min + 1;
                var abundance = (rand.Next(min, max))/1000f;

                //Applies to all but interplanetary
                if (request.ResourceType != HarvestTypes.Interplanetary)
                {
                    //Lets add some noise...
                    float swing = abundance*(distro.Variance/100f);
                    abundance = abundance - swing + (swing*noise*2);
                    //You should only be able to hit zero if someohe sets their
                    //variance >= 100
                    if (abundance < 0)
                        abundance = 0;
                }
                //Altitude band - only applies to atmospheric and interplanetary
                if (
                    (request.ResourceType == HarvestTypes.Atmospheric || request.ResourceType == HarvestTypes.Atmospheric) 
                    && distro.HasVariableAltitude())
                {
                    var rad = body.Radius;
                    var ideal = ((rad*distro.MinAltitude) + (rad*distro.MaxAltitude))/2;
                    //print("REGO: IDEAL = " + ideal);
                    var range = rand.Next((int)(rad * distro.MinRange), (int)(rad * distro.MaxRange));
                    var diff = Math.Abs(ideal - request.Altitude);
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

        private static string GetDefaultSituation(HarvestTypes type)
        {
            switch (type)
            {
                case HarvestTypes.Atmospheric:
                    return Vessel.Situations.FLYING.ToString();
                case HarvestTypes.Oceanic:
                    return Vessel.Situations.SPLASHED.ToString();
                case HarvestTypes.Planetary:
                    return Vessel.Situations.LANDED.ToString();
                default:
                    return Vessel.Situations.ORBITING.ToString();
            }
        }

        public static Vector2 GetDepletionNode(double latitude, double longitude)
        {
            //For precision, we'll be rounding.
            //This gives us 64K potential drill sites.
            var adjLat = Utilities.Rad2Lat(latitude);
            var adjLon = Utilities.Rad2Lon(longitude);
            var x = Math.Round(adjLat, 0);
            var y = Math.Round(adjLon, 0);
            return new Vector2((float)x,(float)y);
        }
    }
}