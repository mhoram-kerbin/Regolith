namespace Regolith.Asteroids
{
    //Used in the definition of a probe to determine what resources
    //may or may not be present on asteroids
    public class USI_ModuleProbeData : PartModule
    {
        [KSPField]
        public string resourceName;

        [KSPField]
        public int presenceChance;

        [KSPField]
        public int lowRange;

        [KSPField]
        public int highRange;
    }
}