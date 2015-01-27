using System.Linq;
using Regolith.Common;

namespace Regolith.Asteroids
{
    public class REGO_ModuleAsteroidAnalysis : PartModule
    {

        private Part _potato;

        [KSPField(guiActive = true, guiName = "", guiActiveEditor = false)]
        public string status = "No asteroid detected";

        public override void OnUpdate()
        {
            if (vessel != null)
            {
                if (_potato == null) FindAsteroidResources();
            }
        }

        private void FindAsteroidResources()
        {
            {
                var potatoes = vessel.Parts.Where(p => p.Modules.Contains("ModuleAsteroid"));
                if (potatoes.Any())
                {
                    if (_potato == null)
                    {
                        _potato = potatoes.FirstOrDefault();
                        var resources = _potato.FindModulesImplementing<REGO_ModuleAsteroidResource>();
                        foreach (var res in resources)
                        {
                            var analysis =
                                part.FindModulesImplementing<REGO_ModuleAnalysisResource>().FirstOrDefault(r => r.resourceName == res.resourceName);
                            if (analysis != null)
                            {
                                analysis.abundance = res.abundance;
                            }
                        }
                        Fields["status"].guiActive = false;
                    }
                    return;
                }
            }
            _potato = null;
        }
    }
}
