namespace Regolith.Common
{
    public class REGO_ModuleAnalysisResource : PartModule
    {
        [KSPField(isPersistant = true)] 
        public string resourceName = "";

        [KSPField(isPersistant = true)]
        public float abundance = 0;
       
        [KSPField(guiActive = false, guiName = "", guiActiveEditor = false)]
        public string status = "Unknown";


        public override void OnUpdate()
        {
            SetupAnalysis();
        }

        public void SetupAnalysis()
        {
            if (abundance > 0 && Fields["status"].guiActive == false)
            {
                Fields["status"].guiActive = true;
                Fields["status"].guiName = resourceName;
                status = string.Format("{0:0.0000}%",abundance * 100);
            }
        }
    }
}