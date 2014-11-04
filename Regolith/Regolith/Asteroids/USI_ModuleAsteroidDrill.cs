using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Regolith.Annotations;
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
        public double Efficiency;

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
        private USI_ResourceConverter converter = new USI_ResourceConverter();

        public override void OnFixedUpdate()
        {
            //Check our time
            print("[REGOLITH] Checking Time");
            var deltaTime = GetDeltaTime();
                if (deltaTime < 0) return;

            //Determine if we are in fact latched to an asteroid
            print("[REGOLITH] Checking Asteroid");
            var potato = GetAttachedPotato();
            if (potato == null)
            {
                status = "No asteroid detected";
                IsActivated = false;
                return;
            }


            if (!CheckForImpact(new Vector3(1, 0, 0)))
            {
                status = "No surface impact";
                IsActivated = false;
                return;               
            }
            //Handle state change
            UpdateDrillingStatus();
            //If we're enabled:
            if (IsActivated)
            {
                print("[REGOLITH] Drilling!");
                //Determine our input
                //Do we have enough input?
                //Determine our output - let's start with rock.
                //Do we have enough SPACE for output?
                //Remove the inputs
                //Add the outputs
            }
        }


        private bool CheckForImpact(Vector3 v)
        {
            var t = part.FindModelTransform(ImpactTransform);
            var targetType = "PotatoRoid";
            var pos = t.position;
            RaycastHit hitInfo;
            var ray = new Ray(pos, v);
            Physics.Raycast(ray, out hitInfo, 5f);
            if (hitInfo.collider != null)
            {
                print(String.Format("Vector {0},{1},{2}", v.x,v.y,v.z));
                print(hitInfo.collider.gameObject.name);
                var colType =   hitInfo.collider.attachedRigidbody.gameObject.name;
                print(hitInfo.collider.attachedRigidbody.gameObject.name);
                return (targetType == colType);
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
