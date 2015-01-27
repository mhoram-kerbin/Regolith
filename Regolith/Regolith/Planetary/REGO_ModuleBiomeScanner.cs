using System;
using System.Runtime.InteropServices;
using Regolith.Asteroids;
using Regolith.Scenario;

namespace Regolith.Common
{
    public class REGO_ModuleGPS : PartModule
    {
        [KSPField(guiActive = true, guiName = "Body", guiActiveEditor = false)]
        public string body = "???";
        [KSPField(guiActive = true, guiName = "Biome", guiActiveEditor = false)]
        public string bioName = "???";
        [KSPField(guiActive = true, guiName = "Lat", guiActiveEditor = false)]
        public string lat = "???";
        [KSPField(guiActive = true, guiName = "Lon", guiActiveEditor = false)]
        public string lon = "???";

        public override void OnUpdate()
        {
            try
            {
                var thisBody = FlightGlobals.currentMainBody;
                var thisLat = Utilities.Rad2Lat(FlightGlobals.ship_latitude);
                var thisLon = Utilities.Rad2Lon(FlightGlobals.ship_longitude);
                var biome = Utilities.GetBiome(thisLat, thisLon, FlightGlobals.currentMainBody);

                body = thisBody.bodyName;
                lat = thisLat.ToString();
                lon = thisLon.ToString();
                if (biome != null)
                {
                    bioName = biome.name;
                }
                else
                {
                    bioName = vessel.situation.ToString();
                }
            }
            catch (Exception)
            {
                print("[REGO] Error updating ModuleGPS");
            }
        }
    }

    public class REGO_ModuleBiomeScanner : PartModule
    {
        [KSPEvent(guiName = "Run Analysis", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true,
            active = false, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public
        void RunAnalysis()
        {
            var thisBody = FlightGlobals.currentMainBody;
            var thisLat = Utilities.Rad2Lat(FlightGlobals.ship_latitude);
            var thisLon = Utilities.Rad2Lon(FlightGlobals.ship_longitude);
            var biome = Utilities.GetBiome(thisLat, thisLon, FlightGlobals.currentMainBody);
            var biomeName = vessel.situation.ToString();
            if (biome != null)
            {
                biomeName = biome.name;
            }
            string msg = string.Format("Resource analysis performed for {0} {1}", thisBody.name, biomeName);
            ScreenMessages.PostScreenMessage(msg, 5f, ScreenMessageStyle.UPPER_CENTER);
            RegolithScenario.Instance.gameSettings.UnlockBiome(thisBody.flightGlobalsIndex, biomeName);
        }

        public override void OnStart(StartState state)
        {
            part.force_activate();
        }

        public override void OnFixedUpdate()
        {
            vessel.checkLanded();
            vessel.checkSplashed();

            var isLanded = vessel.Landed || vessel.Splashed;

            if (Events["RunAnalysis"].active != isLanded)
            {
                Events["RunAnalysis"].active = isLanded;
                MonoUtilities.RefreshContextWindows(part);
            }
        }
    }
}