using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using FinePrint;
using Regolith.Asteroids;
using Regolith.Converters;
using Regolith.Scenario;
using UnityEngine;

namespace Regolith.Common
{
    public class REGO_ModuleResourceHarvester : BaseConverter
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
        public float HarvestThreshold = 0f;
        
        [KSPField]
        public bool CausesDepletion = false;

        [KSPField(guiActive = true, guiName = "", guiActiveEditor = false)]
        public string ResourceStatus = "n/a";

        private double _resFlow = 0;

        private string GetLocationString()
        {
            return ((HarvestTypes) HarvesterType).ToString();
        }

        public override string GetInfo()
        {
            var sb = new StringBuilder();
            var recipe = LoadRecipe(1);
            sb.Append(".");
            sb.Append("\n");
            sb.Append(ConverterName);
            sb.Append("\n");
            sb.Append("<color=#BADA55>(" + GetLocationString() + " use)</color>");
            sb.Append("\n\n<color=#99FF00>Max inputs:</color>");
            foreach (var input in recipe.Inputs)
            {
                sb.Append("\n - ")
                    .Append(input.ResourceName)
                    .Append(": ");
                if (input.Ratio < 0.0001)
                {
                    sb.Append(String.Format("{0:0.00}", input.Ratio * 21600)).Append("/6h");
                }
                else if (input.Ratio < 0.01)
                {
                    sb.Append(String.Format("{0:0.00}", input.Ratio * 3600)).Append("/hr");
                }
                else
                {
                    sb.Append(String.Format("{0:0.00}", input.Ratio)).Append("/sec");
                }

            }
            sb.Append("\n<color=#99FF00>Max outputs:</color>");
            foreach (var output in recipe.Outputs)
            {
                sb.Append("\n - ")
                    .Append(output.ResourceName)
                    .Append(": ");
                if (output.Ratio < 0.0001)
                {
                    sb.Append(String.Format("{0:0.00}", output.Ratio * 21600)).Append("/6h");
                }
                else if (output.Ratio < 0.01)
                {
                    sb.Append(String.Format("{0:0.00}", output.Ratio * 3600)).Append("/hr");
                }
                else
                {
                    sb.Append(String.Format("{0:0.00}", output.Ratio)).Append("/sec");
                }
            }
            if (recipe.Requirements.Any())
            {
                sb.Append("\n<color=#99FF00>Requirements:</color>");
                foreach (var output in recipe.Requirements)
                {
                    sb.Append("\n - ")
                        .Append(output.ResourceName)
                        .Append(": ");
                    sb.Append(String.Format("{0:0.00}", output.Ratio));
                }
            }
            sb.Append("\n");
            return sb.ToString();
        }

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
                vessel.checkLanded();
                vessel.checkSplashed();

                if (HarvesterType == 0 && !vessel.Landed)
                {
                    //Let's do an altitude sanity check just in case
                    var alt = vessel.GetHeightFromTerrain();
                    if (alt > 500)
                    {
                        status = "must land first";
                        IsActivated = false;
                        return null;
                    }
                }
                if (HarvesterType == 1 && !vessel.Splashed)
                {
                    status = "must be splashed down";
                    IsActivated = false;
                    return null;
                }

                var abRequest = new AbundanceRequest
                                {
                                    Altitude = vessel.altitude,
                                    BodyId = FlightGlobals.currentMainBody.flightGlobalsIndex,
                                    CheckForLock = false,
                                    Latitude = vessel.latitude,
                                    Longitude = vessel.longitude,
                                    ResourceType = (HarvestTypes)HarvesterType,
                                    ResourceName = ResourceName
                                };
                var abundance = RegolithResourceMap.GetAbundance(abRequest);
                
                //Harvesting thresholds, if used.
                if (abundance < HarvestThreshold || abundance < Utilities.FLOAT_TOLERANCE)
                {
                    status = "nothing to harvest";
                    IsActivated = false;
                    return null;
                }

                if (!IsActivated)
                {
                    status = "Inactive";
                    return null;
                }

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

        protected override void PostUpdateCleanup()
        {
            if (IsActivated)
            {
                ResourceStatus = String.Format("{0:0.000000}/sec", _resFlow);
            }
            else
            {
                ResourceStatus = "n/a";
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
                    r.Inputs.Add(new ResourceRatio
                    {
                        ResourceName = inputs[ip].Trim(),
                        Ratio = Convert.ToDouble(inputs[ip + 1])
                    });
                }

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

        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
            if (CausesDepletion)
            {
                //Depletion time. This is a function of a few things:
                // - the overall rate - this is capped at 100% (Anything beyond that reflects mining gear that is
                //   more efficient and less likely to cause depletion.  
                //   with large delta time swings (like insta-strip mining).  I'm ok with 
                // - the depletion rate
                // - the current depletion level at this node
                var flow = (float)Math.Min(1, result.TimeFactor / deltaTime);
                var depNode = RegolithResourceMap.GetDepletionNode(vessel.latitude,
                    vessel.longitude);
                float curDep =
                    RegolithScenario.Instance.gameSettings.GetDepletionNodeValue(vessel.mainBody.flightGlobalsIndex,
                        ResourceName, (int)depNode.x, (int)depNode.y);
                float netDepRate = DepletionRate * flow;
                float newDep = curDep - (curDep * netDepRate);

                RegolithScenario.Instance.gameSettings.SetDepletionNodeValue(vessel.mainBody.flightGlobalsIndex,
                        ResourceName, (int)depNode.x, (int)depNode.y, newDep);

            }
            base.PostProcess(result, deltaTime);
        }

    }
}
