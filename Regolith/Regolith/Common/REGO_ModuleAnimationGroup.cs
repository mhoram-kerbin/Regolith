using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Regolith.Asteroids;
using UnityEngine;

namespace Regolith.Common 
{
    public class REGO_ModuleAnimationGroup : PartModule
    {
        [KSPField]
        public string deployAnimationName = "Deploy";

        [KSPField]
        public string activeAnimationName = "";

        [KSPField]
        public string deactivateAnimationName = "";

        [KSPField] 
        public string deployActionName = "Deploy";

        [KSPField]
        public string retractActionName = "Retract";


        [KSPField(isPersistant = true)]
        public bool isDeployed = false;

        [KSPField(isPersistant = false)]
        public bool alwaysActive = false;

        [KSPField(isPersistant = true)]
        public bool _isActive;

        [KSPField(isPersistant = false)]
        public string moduleType = "Module";

        [KSPEvent(guiName = "Deploy", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void DeployModule()
        {
            SetDeployedState(1);
        }

        [KSPEvent(guiName = "Retract", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void RetractModule()
        {
            SetRetractedState(-1);
        }

        [KSPAction("Deploy")]
        public void DeployModuleAction(KSPActionParam param)
        {
            if (!isDeployed)
            {
                DeployModule();
            }
        }

        [KSPAction("Retract")]
        public void RetractModuleAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                RetractModule();
            }
        }


        [KSPAction("Toggle")]
        public void ToggleModuleAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                RetractModule();
            }
            else
            {
                DeployModule();
            }
        }

        private List<IAnimatedModule> _Modules;

        public Animation DeployAnimation
        {
            get
            {
                if (deployAnimationName == "") return null;
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }
        public Animation ActiveAnimation
        {
            get
            {
                if (activeAnimationName == "") return null;
                return part.FindModelAnimators(activeAnimationName)[0];
            }
        }

        public Animation DeactivateAnimation
        {
            get
            {
                if (deactivateAnimationName == "") return null;
                return part.FindModelAnimators(deactivateAnimationName)[0];
            }
        }

        public override void OnStart(StartState state)
        {
            FindModules();
            StopAnimations();
            CheckAnimationState();

            if (deployAnimationName != "")
            {
                DeployAnimation[deployAnimationName].layer = 3;
            }
            else
            {
                //Auto deploy if it has no animation
                Events["DeployModule"].active = false;
                Events["RetractModule"].active = false;
                isDeployed = true;
            }
            if (activeAnimationName != "")
            {
                ActiveAnimation[activeAnimationName].layer = 4;
            }
            if (deactivateAnimationName != "")
            {
                DeactivateAnimation[deactivateAnimationName].layer = 4;
            }
            Setup();
        }

        private void StopAnimations()
        {
            var anims = part.FindModelAnimators().ToList();
            foreach (var a in anims)
            {
                a.Stop();
            }
        }

        private void Setup()
        {
            Events["DeployModule"].guiName = deployActionName + " " + moduleType;
            Events["RetractModule"].guiName = retractActionName + " " + moduleType;
            Actions["DeployModuleAction"].guiName = deployActionName + " " + moduleType;
            Actions["RetractModuleAction"].guiName = retractActionName + " " + moduleType;
            Actions["ToggleModuleAction"].guiName = "Toggle " + moduleType;
            MonoUtilities.RefreshContextWindows(part);
        }

        public override void OnLoad(ConfigNode node)
        {
            FindModules();
            CheckAnimationState();
        }

        public override void OnUpdate()
        {
            try
            {
                if (vessel != null)
                {
                    if (!isDeployed)
                    {
                        DisableModules();
                    }
                    else
                    {
                        CheckForActivity();
                    }
                }
                base.OnUpdate();
            }
            catch (Exception)
            {
                print("[Regolith] - ERROR IN OnUpdate of Regolith_ModuleAnimationGroup");
            }
        }


        private void CheckAnimationState()
        {
            if (isDeployed)
            {
                SetDeployedState(1000);
            }
            else
            {
                SetRetractedState(-1000);
            }
            ToggleEmmitters(false);
        }
        private void FindModules()
        {
            if (vessel != null)
            {
                if (part.Modules.OfType<IAnimatedModule>().Any())
                {
                    _Modules = part.Modules.OfType<IAnimatedModule>().ToList();
                }
            }
        }

        private void CheckForActivity()
        {
            if ((_Modules.Any(e => e.ModuleIsActive() || alwaysActive) && isDeployed))
            {
                ToggleActiveState(1, true);
            }
            else
            {
                ToggleActiveState(-1,false);
            }
        }

        private void ToggleActiveState(int speed, bool state)
        {
            try
            {
                if (activeAnimationName != "" && !ActiveAnimation.isPlaying && state == true)
                {
                    ActiveAnimation[activeAnimationName].speed = speed;
                    ActiveAnimation.Play(activeAnimationName);
                }
                if (deactivateAnimationName != "" && !DeactivateAnimation.isPlaying && state == false )
                {
                    DeactivateAnimation[deactivateAnimationName].speed = speed;
                    DeactivateAnimation.Play(deactivateAnimationName);
                }
                ToggleEmmitters(state);
            }
            catch (Exception)
            {
                print("[REGOLITH] ERROR in ToggleActiveState of Regolith_ModuleAnimationGroup");
            }
        }

        private void ToggleEmmitters(bool state)
        {
            var eList = part.GetComponentsInChildren<KSPParticleEmitter>();
            foreach (var e in eList)
            {
                e.emit = state;
                e.enabled = state;
            }
        }


        private void SetRetractedState(int speed)
        {
            isDeployed = false;
            Events["RetractModule"].active = false;
            Events["DeployModule"].active = true;
            PlayDeployAnimation(speed);
            DisableModules();
            MonoUtilities.RefreshContextWindows(part);
        }

        private void SetDeployedState(int speed)
        {
            isDeployed = true;
            Events["DeployModule"].active = false;
            Events["RetractModule"].active = true;
            PlayDeployAnimation(speed);
            EnableModules();
            MonoUtilities.RefreshContextWindows(part);
        }

        private void PlayDeployAnimation(int speed)
        {
            if (speed < 0)
            {
                if (activeAnimationName != "")
                {
                    ActiveAnimation.Stop(activeAnimationName);
                }
                if (deactivateAnimationName != "")
                {
                    DeactivateAnimation.Stop(deactivateAnimationName);
                }
                if (deployAnimationName != "")
                {
                    DeployAnimation[deployAnimationName].time = DeployAnimation[deployAnimationName].length;
                }
            }
            if (deployAnimationName != "")
            {
                DeployAnimation[deployAnimationName].speed = speed;
                DeployAnimation.Play(deployAnimationName);
            }
        }

        private void DisableModules()
        {
            if (vessel == null || _Modules == null) return;
            foreach (var e in _Modules)
            {
                e.DisableModule();
            }
            _isActive = false;
        }

        private void EnableModules()
        {
            if (vessel == null || _Modules == null) return;
            foreach (var e in _Modules)
            {
                e.EnableModule();
            }
        }

    }
}
