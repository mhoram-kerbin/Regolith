using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Regolith.Common;
using UnityEngine;

namespace Regolith.Asteroids
{
    public class USI_ModuleAsteroidDrill : PartModule, IAnimatedModule
    {
        [KSPField(isPersistant = true)]
        public bool IsActivated = false;

        [KSPField(isPersistant = false)] 
        public float PowerConsumption;

        [KSPField(isPersistant = false)]
        public string StartActionName;

        [KSPField(isPersistant = false)]
        public string StopActionName;

        [KSPField(isPersistant = false)]
        public double ExtractionRate;

        [KSPField(isPersistant = false)]
        public double Efficiency = .1;

        [KSPField(isPersistant = false)]
        public double DumpExcess;

        [KSPField(isPersistant = true)]
        public string ImpactTransform = "";

        [KSPField(guiActive = true, guiName = "", guiActiveEditor = false)]
        public string status = "Unknown";

        [KSPEvent(guiActive = true, guiName = "Start Extraction", active = false)]
        public void startResourceExtraction()
        {
            IsActivated = true;
        }

        [KSPEvent(guiActive = true, guiName = "Stop Extraction", active = false)]
        public void stopResourceExtraction()
        {
            IsActivated = false;
        }

        [KSPAction("Stop Extraction")]
        public void stopResourceExtractionAction(KSPActionParam param)
        {
            stopResourceExtraction();
        }

        [KSPAction("Start Extraction")]
        public void startResourceExtractionAction(KSPActionParam param)
        {
            startResourceExtraction();
        }

        private double lastUpdateTime = 0.0f;
        private ResourceConverter converter = new ResourceConverter();

        public override void OnFixedUpdate()
        {
            //Check our time
            var deltaTime = GetDeltaTime();
                if (deltaTime < 0) return;

            //Determine if we are in fact latched to an asteroid
            var potato = GetAttachedPotato();
            if (potato == null)
            {
                status = "No asteroid detected";
                IsActivated = false;
                return;
            }

            if (!potato.Modules.Contains("USI_ModuleAsteroidResource"))
            {
                status = "No resource data";
                IsActivated = false;
                return;
            }
            var resourceList = potato.FindModulesImplementing<USI_ModuleAsteroidResource>();

            if (!CheckForImpact())
            {
                status = "No surface impact";
                IsActivated = false;
                return;               
            }

            var info = potato.FindModuleImplementing<USI_ModuleAsteroidInfo>();
            if (info == null)
            {
                status = "No info";
                IsActivated = false;
                return;
            }

            if (info.massThreshold >= potato.mass)
            {
                status = "Resources Depleted";
                IsActivated = false;
                return;
            }

            //Handle state change
            UpdateDrillingStatus();
            //If we're enabled"
            if (IsActivated)
            {
                //Fetch our recipe
                var recipe = new ConversionRecipe();
                recipe.Inputs.Add(new ResourceRatio {ResourceName = "ElectricCharge", Ratio = 1});
                
                //Setup rock
                var purity = resourceList.Sum(ar => ar.abundance);
                var rockAmt = 1 - purity;
                resourceList.Add(new USI_ModuleAsteroidResource {abundance = rockAmt, resourceName = "Rock"});

                foreach (var ar in resourceList)
                {
                    purity += ar.abundance;
                    if (ar.abundance > Utilities.FLOAT_TOLERANCE)
                    {
                        var res = potato.Resources[ar.resourceName];
                        var resInfo = PartResourceLibrary.Instance.GetDefinition(res.resourceName);
                        var outRes = new ResourceRatio {ResourceName = ar.resourceName, Ratio = ar.abundance * Efficiency};
                        //Make sure we have enough free space
                        var spaceNeeded = deltaTime*ar.abundance*Efficiency;
                        var spaceAvailable = res.maxAmount - res.amount;
                        if (spaceAvailable < spaceNeeded)
                        {
                            var slackMass = potato.mass - info.massThreshold;
                            var maxSpace = slackMass/resInfo.density; 
                            var unitsToAdd = Math.Min(maxSpace, (spaceNeeded - spaceAvailable));
                            var newMass = potato.mass - ((float)(resInfo.density * unitsToAdd));
                            res.maxAmount += unitsToAdd;
                            potato.mass = newMass;
                            
                        }
                        recipe.Outputs.Add(outRes);
                    }
                }
                //Process the recipe
                converter.ProcessRecipe(deltaTime, recipe, part);

            }
        }


        private bool CheckForImpact()
        {
            var t = part.FindModelTransform(ImpactTransform);
            var targetType = "PotatoRoid";
            var pos = t.position;
            RaycastHit hitInfo;
            var ray = new Ray(pos, t.forward);
            Physics.Raycast(ray, out hitInfo, 5f);
            if (hitInfo.collider != null)
            {
                var colType =   hitInfo.collider.attachedRigidbody.gameObject.name;
                return (colType.StartsWith(targetType));
            }
            return false;
        }

        private void UpdateDrillingStatus()
        {
            Events["startResourceExtraction"].active = !IsActivated;
            Events["stopResourceExtraction"].active = IsActivated;
            status = "Operational";
        }

        private Part GetAttachedPotato()
        {
            var potatoes = vessel.Parts.Where(p => p.Modules.Contains("ModuleAsteroid"));
            if (potatoes.Any())
            {
                return potatoes.FirstOrDefault();
            }
            return null;
        }

        private double GetDeltaTime()
        {
            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
            {
                return -1;
            }

            if (lastUpdateTime == 0.0f)
            {
                // Just started running
                lastUpdateTime = Planetarium.GetUniversalTime();
                return -1;
            }

            var deltaTime = Math.Min(Planetarium.GetUniversalTime() - lastUpdateTime, Utilities.GetMaxDeltaTime());
            lastUpdateTime += deltaTime;
            return deltaTime;
        }

        public void ResetLastUpdateTime()
        {
            this.lastUpdateTime = 0f;
        }


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", lastUpdateTime);
            part.force_activate();
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            node.AddValue("lastUpdateTime", lastUpdateTime);
        }


        public void EnableModule()
        {
            isEnabled = true;
        }

        public void DisableModule()
        {
            isEnabled = false;
        }

        public bool ModuleIsActive()
        {
            return IsActivated;
        }
    }
}
