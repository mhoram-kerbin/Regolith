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

        private double lastUpdateTime = 0.0f;

        public override void OnFixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
            {
                return;
            }

            if (lastUpdateTime == 0.0f)
            {
                // Just started running
                lastUpdateTime = Planetarium.GetUniversalTime();
                return;
            }

            double deltaTime = Math.Min(Planetarium.GetUniversalTime() - lastUpdateTime, Utilities.GetMaxDeltaTime());
            lastUpdateTime += deltaTime;


        }


        public void ResetLastUpdateTime()
        {
            this.lastUpdateTime = 0f;
        }


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", lastUpdateTime);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            node.AddValue("lastUpdateTime", lastUpdateTime);
        }


        public void EnableModule()
        {
 	        throw new NotImplementedException();
        }

        public void DisableModule()
        {
 	        throw new NotImplementedException();
        }

        public bool ModuleIsActive()
        {
 	        throw new NotImplementedException();
        }
    }
}
