using System;
using System.Security.Permissions;

namespace Regolith.Asteroids
{
    public class REGO_ModuleAsteroidResource : PartModule
    {
            [KSPField(isPersistant = true)]
            public string resourceName;

            [KSPField]
            public int presenceChance;

            [KSPField]
            public int lowRange;

            [KSPField]
            public int highRange;

            [KSPField(isPersistant = true)]
            public float abundance = 0;

    }
}