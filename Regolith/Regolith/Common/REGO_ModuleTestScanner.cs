using System;

namespace Regolith.Common
{


    public class REGO_ModuleTestScanner : PartModule
    {
        [KSPField(guiActive = true, guiName = "Lat", guiActiveEditor = false)]
        public string lat = "???";
        [KSPField(guiActive = true, guiName = "Lon", guiActiveEditor = false)]
        public string lon = "???";
        [KSPField(guiActive = true, guiName = "Biome", guiActiveEditor = false)]
        public string bioName = "???";
        [KSPField(guiActive = true, guiName = "Abundance", guiActiveEditor = false)]
        public string ab = "???";
        [KSPField(guiActive = true, guiName = "CheckTime", guiActiveEditor = false)]
        public string time = "???";

        public override void OnUpdate()
        {
            var start = DateTime.Now;
            lat = FlightGlobals.ship_latitude.ToString();
            lon = FlightGlobals.ship_longitude.ToString();
            var bmap = FlightGlobals.currentMainBody.BiomeMap;
            var bdata = bmap.GetAtt(FlightGlobals.ship_latitude, FlightGlobals.ship_longitude);
            bioName = bdata.name;
            ab = RegolithResourceMap.GetAbundance(FlightGlobals.ship_latitude, FlightGlobals.ship_longitude, "Karbonite",
                0).ToString();
            var ms = (DateTime.Now - start).TotalMilliseconds;
            time = ms.ToString();
        }
    }
}