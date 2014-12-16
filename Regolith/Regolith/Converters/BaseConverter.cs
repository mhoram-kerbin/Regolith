using System;
using System.Text;
using Regolith.Common;
using UnityEngine;

namespace Regolith.Asteroids
{
    public interface IGenericConverter
    {
        
    }
    public abstract class BaseConverter : PartModule, IAnimatedModule
    {
        [KSPField(isPersistant = true)]
        public float EfficiencBonus = 1;
        
        [KSPField(isPersistant = true)]
        public bool IsActivated = false;

        [KSPField(isPersistant = true)]
        public bool DirtyFlag = true;
        
        [KSPField(isPersistant = false)]
        public string StartActionName;

        [KSPField(isPersistant = false)]
        public string StopActionName;

        [KSPField(isPersistant = false)]
        public string ConverterName = "";

        [KSPField(guiActive = true, guiName = "", guiActiveEditor = false)]
        public string status = "Unknown";

        [KSPEvent(guiActive = true, guiName = "Start Converter", active = false)]
        public void StartResourceConverter()
        {
            IsActivated = true;
        }

        [KSPEvent(guiActive = true, guiName = "Stop Converter", active = false)]
        public void StopResourceConverter()
        {
            IsActivated = false;
        }

        [KSPAction("Stop Converter")]
        public void StopResourceConverterAction(KSPActionParam param)
        {
            StopResourceConverter();
        }

        [KSPAction("Start Converter")]
        public void StartResourceConverterAction(KSPActionParam param)
        {
            StartResourceConverter();
        }

        protected double lastUpdateTime;
        protected IResourceBroker _broker;
        protected ResourceConverter _converter;


        protected BaseConverter()
        {
            _broker = new ResourceBroker();
            _converter = new ResourceConverter(_broker);
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

        protected double GetDeltaTime()
        {
            try
            {
                if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
                {
                    return -1;
                }

                if (Math.Abs(lastUpdateTime) < Utilities.FLOAT_TOLERANCE)
                {
                    // Just started running
                    lastUpdateTime = Planetarium.GetUniversalTime();
                    return -1;
                }

                var deltaTime = Math.Min(Planetarium.GetUniversalTime() - lastUpdateTime, Utilities.GetMaxDeltaTime());
                lastUpdateTime += deltaTime;
                return deltaTime;
            }
            catch (Exception e)
            {
                print("[REGO] - Error in - BaseConverter_GetDeltaTime - " + e.Message);
                return 0;
            }
        }

        protected void UpdateConverterStatus()
        {
            if (DirtyFlag != IsActivated)
            {
                DirtyFlag = IsActivated;
                Events["StartResourceConverter"].active = !IsActivated;
                Events["StopResourceConverter"].active = IsActivated;
                status = "Operational";
                MonoUtilities.RefreshContextWindows(part);
            }            
        }

        public override void OnLoad(ConfigNode node)
        {
            try
            {
                base.OnLoad(node);
                lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", lastUpdateTime);
                part.force_activate();

                Events["StartResourceConverter"].guiName = StartActionName;
                Events["StopResourceConverter"].guiName = StopActionName;
                Actions["StartResourceConverterAction"].guiName = StartActionName;
                Actions["StopResourceConverterAction"].guiName = StopActionName;
                Fields["status"].guiName = ConverterName;

                //Check for presence of an Animation Group.  If not present, enable the module.
                if (!part.Modules.Contains("USI_ModuleAnimationGroup"))
                    EnableModule();

                MonoUtilities.RefreshContextWindows(part);
            }
            catch (Exception e)
            {
                print("[REGO] - Error in - BaseConverter_OnLoad - " + e.Message); 
            }
        }
        
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            node.AddValue("lastUpdateTime", lastUpdateTime);
        }

        public override void OnFixedUpdate()
        {
            try
            {
                //Check our time
                var deltaTime = GetDeltaTime();
                if (deltaTime < 0) return;

                var recipe = PrepareRecipe(deltaTime);


                if (recipe != null)
                {
                    var result = _converter.ProcessRecipe(deltaTime, recipe, part, EfficiencBonus);
                    PostProcess(result,deltaTime);
                }
            }
            catch (Exception e)
            {
                print("[REGO] - Error in - BaseConverter_OnFixedUpdate - " + e.Message); 
            }
        }

        protected virtual void PostProcess(double result, double deltaTime)
        {
            status = String.Format("{0:0.00}% load", result/deltaTime*100);           
        }

        protected virtual ConversionRecipe PrepareRecipe(double deltatime)
        {
            print("[REGOLITH] No Implementation of PrepareRecipe in derived class");
            return null;
        }




    }
}