using System;

namespace Regolith.Common
{


    public class REGO_ModuleBiomeScanner : PartModule
    {
        [KSPField(guiActive = true, guiName = "Body", guiActiveEditor = false)]
        public string body = "???";
        [KSPField(guiActive = true, guiName = "Lat", guiActiveEditor = false)]
        public string lat = "???";
        [KSPField(guiActive = true, guiName = "Lon", guiActiveEditor = false)]
        public string lon = "???";
        [KSPField(guiActive = true, guiName = "Biome", guiActiveEditor = false)]
        public string bioName = "???";

        public override void OnUpdate()
        {
            body = FlightGlobals.currentMainBody.bodyName;
            lat = FlightGlobals.ship_latitude.ToString();
            lon = FlightGlobals.ship_longitude.ToString();
            var bmap = FlightGlobals.currentMainBody.BiomeMap;
            var bdata = bmap.GetAtt(FlightGlobals.ship_latitude, FlightGlobals.ship_longitude);
            bioName = bdata.name;
        }
    }
}