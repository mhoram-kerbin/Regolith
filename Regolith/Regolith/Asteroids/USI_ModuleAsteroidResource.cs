using System;
using System.Security.Permissions;

namespace Regolith.Asteroids
{
    public class USI_ModuleAsteroidResource : PartModule
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
            public double abundance = 0;

    }
}