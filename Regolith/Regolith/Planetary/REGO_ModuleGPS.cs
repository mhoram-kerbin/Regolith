using System;

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
                var thisLat = Utilities.Deg2Rad(Utilities.fixLat(vessel.latitude));
                var thisLon = Utilities.Deg2Rad(Utilities.fixLong(vessel.longitude));
                var biome = Utilities.GetBiome(thisLat, thisLon, FlightGlobals.currentMainBody);

                body = thisBody.bodyName;
                lat = String.Format("{0:0.000} [{1:0.000}N]", Utilities.fixLat(vessel.latitude), thisLat);
                lon = String.Format("{0:0.000} [{1:0.000}E]", Utilities.fixLong(vessel.longitude), thisLon);
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
}