using System;
using System.Runtime.InteropServices;
using Regolith.Asteroids;
using Regolith.Scenario;

namespace Regolith.Common
{
    public class REGO_ModuleBiomeScanner : PartModule
    {
        [KSPEvent(guiName = "Run Analysis", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true,
            active = false, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public
        void RunAnalysis()
        {
            var thisBody = vessel.mainBody;
            var thisLat = Utilities.Deg2Rad(vessel.latitude);
            var thisLon = Utilities.Deg2Rad(vessel.longitude);
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

            var isEnabled = vessel.Landed || vessel.Splashed;
            if (isEnabled)
            {
                var thisBody = vessel.mainBody;
                var thisLat = Utilities.Deg2Rad(vessel.latitude);
                var thisLon = Utilities.Deg2Rad(vessel.longitude);
                var biome = Utilities.GetBiome(thisLat, thisLon, FlightGlobals.currentMainBody);
                var biomeName = vessel.situation.ToString();
                if (biome != null)
                {
                    biomeName = biome.name;
                }
                if(RegolithScenario.Instance.gameSettings.IsBiomeUnlocked(thisBody.flightGlobalsIndex, biomeName))
                    isEnabled = false;
            }

            if (Events["RunAnalysis"].active != isEnabled)
            {
                Events["RunAnalysis"].active = isEnabled;
                MonoUtilities.RefreshContextWindows(part);
            }
        }
    }
}