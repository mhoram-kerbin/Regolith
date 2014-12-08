using System;

namespace Regolith.Common
{


    public class REGO_ModuleBiomeScanner : PartModule
    {
        [KSPField(guiActive = true, guiName = "Body", guiActiveEditor = false)]
        public string body = "???";
        [KSPField(guiActive = true, guiName = "Biome", guiActiveEditor = false)]
        public string bioName = "???";
        [KSPField(guiActive = true, guiName = "Lat", guiActiveEditor = false)]
        public string lat = "???";
        [KSPField(guiActive = true, guiName = "Lon", guiActiveEditor = false)]
        public string lon = "???";
        [KSPField(guiActive = true, guiName = "DepNode", guiActiveEditor = false)]
        public string depNode = "???";

        public override void OnUpdate()
        {
            try
            {
                body = FlightGlobals.currentMainBody.bodyName;
                lat = FlightGlobals.ship_latitude.ToString();
                lon = FlightGlobals.ship_longitude.ToString();
                var dn = RegolithResourceMap.GetDepletionNode(FlightGlobals.ship_latitude, FlightGlobals.ship_longitude);
                depNode = string.Format("x:{0}/y:{1}", dn.x, dn.y);
                var bmap = FlightGlobals.currentMainBody.BiomeMap;
                if (bmap != null)
                {
                    var bdata = bmap.GetAtt(FlightGlobals.ship_latitude, FlightGlobals.ship_longitude);
                    bioName = bdata.name;
                }
            }
            catch (Exception e)
            {
                //Swallow - probably a biome issue
            }
        }
    }
}