using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using FinePrint;
using Regolith.Asteroids;
using Regolith.Scenario;
using UnityEngine;

namespace Regolith.Common
{
    public class REGO_ModuleResourceHarvester : ResourceModule
    {
        [KSPField]
        public float Efficiency = .1f;

        [KSPField] 
        public int HarvesterType = 0;

        [KSPField]
        public string RecipeInputs = "";

        [KSPField]
        public string ResourceName = "";

        [KSPField] 
        public float DepletionRate = 0f;

        [KSPField] 
        public bool CausesDepletion = false;

        [KSPField(guiActive = true, guiName = "", guiActiveEditor = false)]
        public string ResourceStatus = "Unknown"; 
        
        private double _resFlow = 0;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            Fields["ResourceStatus"].guiName = ResourceName + " rate";
        }

        protected override ConversionRecipe PrepareRecipe(double deltaTime)
        {
            try
            {
                if (!HighLogic.LoadedSceneIsFlight)
                    return null;

                if (HarvesterType == 0 && !vessel.Landed)
                {
                    status = "must land first";
                    print("[REGO] Shutting down because vehicle is not landed");
                    IsActivated = false;
                    return null;
                }
                if (HarvesterType == 1 && !vessel.Splashed)
                {
                    status = "must be splashed down";
                    print("[REGO] Shutting down because vehicle is not splashed");
                    IsActivated = false;
                    return null;
                }

                //Handle state change
                UpdateConverterStatus();
                if (!IsActivated)
                    return null;

           
                var abundance = RegolithResourceMap
                    .GetAbundance(vessel.latitude, vessel.longitude, ResourceName,
                        FlightGlobals.currentMainBody.flightGlobalsIndex,HarvesterType,vessel.altitude);
                var rate = abundance * Efficiency;
                if (HarvesterType == 2) //Account for altitude and airspeed
                {
                    double atmDensity = vessel.atmDensity;
                    double airSpeed = part.vessel.srf_velocity.magnitude + 40.0;
                    double totalIntake = airSpeed * atmDensity;
                    rate *= (float)totalIntake;
                 }
                _resFlow = rate;

                //Setup our recipe
                var recipe = LoadRecipe(rate);
                return recipe;
            }
            catch (Exception e)
            {
                print("[REGO] - Error in - REGO_ModuleCrustalHarvester_ConversionRecipe - " + e.Message);
                return null;
            }
        }

        private ConversionRecipe LoadRecipe(double harvestRate)
        {
            var r = new ConversionRecipe();
            try
            {
                bool dumpExcess = HarvesterType == 2;
                var inputs = RecipeInputs.Split(',');
                for (int ip = 0; ip < inputs.Count(); ip += 2)
                {
                    //print(String.Format("[REGOLITH] - INPUT {0} {1}", inputs[ip], inputs[ip + 1]));
                    r.Inputs.Add(new ResourceRatio
                    {
                        ResourceName = inputs[ip].Trim(),
                        Ratio = Convert.ToDouble(inputs[ip + 1])
                    });
                }

                //print(String.Format("[REGOLITH] - OUTPUT RESOURCE {0} {1}", ResourceName,Efficiency));
                r.Outputs.Add(new ResourceRatio
                {
                    ResourceName = ResourceName,
                    Ratio = harvestRate,
                    DumpExcess = dumpExcess
                });
            }
            catch (Exception)
            {
                print(String.Format("[REGOLITH] Error performing coversion for {0} - {1}", RecipeInputs, ResourceName));
            }
            return r;
        }

        protected override void PostProcess(double result, double deltaTime)
        {
            ResourceStatus = String.Format("{0:0.000000}/sec", _resFlow);
            if (CausesDepletion)
            {
                //Depletion time. This is a function of a few things:
                // - the overall rate - this is capped at 100% (Anything beyond that reflects mining gear that is
                //   more efficient and less likely to cause depletion.  
                //   with large delta time swings (like insta-strip mining).  I'm ok with 
                // - the depletion rate
                // - the current depletion level at this node
                var flow =(float) Math.Min(1,result/deltaTime);
                var depNode = RegolithResourceMap.GetDepletionNode(FlightGlobals.ship_latitude,
                    FlightGlobals.ship_latitude);
                float curDep =
                    RegolithScenario.Instance.gameSettings.GetDepletionNodeValue(vessel.mainBody.flightGlobalsIndex,
                        ResourceName, (int)depNode.x, (int)depNode.y);
                float netDepRate = DepletionRate*flow;
                float newDep = curDep - (curDep*netDepRate);

                RegolithScenario.Instance.gameSettings.SetDepletionNodeValue(vessel.mainBody.flightGlobalsIndex,
                        ResourceName, (int)depNode.x, (int)depNode.y,newDep);

            }
            base.PostProcess(result, deltaTime);
        }
    }
}
