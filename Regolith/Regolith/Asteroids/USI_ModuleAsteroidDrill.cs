using UnityEngine;

namespace Regolith.Asteroids
{
    public class USI_ModuleAsteroidDrill : PartModule
    {
        [KSPField]
        public string deployAnimationName = "Deploy";

        [KSPField]
        public string activeAnimationName = "Mine";

        public Animation DeployAnimation
        {
            get
            {
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }

        public Animation ActiveAnimation
        {
            get
            {
                return part.FindModelAnimators(activeAnimationName)[0];
            }
        }

        public override void OnUpdate()
        {
            if (!ActiveAnimation.IsPlaying(activeAnimationName))
            {
                ActiveAnimation[activeAnimationName].speed = 1;
                ActiveAnimation.Play(activeAnimationName);
            }

            var eList = part.GetComponentsInChildren<KSPParticleEmitter>();
            foreach (var e in eList)
            {
                e.emit = true;
                e.enabled = true;
            }
        }
    }
}