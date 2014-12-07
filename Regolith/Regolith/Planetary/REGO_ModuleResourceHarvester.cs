using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Regolith.Asteroids;
using Regolith.Scenario;
using UnityEngine;

namespace Regolith.Common
{
    public class REGO_ModuleCrustalHarvester : BaseConverter
    {
        [KSPField(isPersistant = false)]
        public double Efficiency = .1;

        [KSPField] 
        public int HarvesterType = 0;

        [KSPField]
        public string RecipeInputs = "";

        [KSPField]
        public string ResourceName = "";

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
                        FlightGlobals.currentMainBody.flightGlobalsIndex,0);
                var rate = abundance * Efficiency;
                if (HarvesterType == 2) //Account for density
                {
                    rate *= vessel.altitude/FlightGlobals.currentMainBody.maxAtmosphereAltitude;
                }

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
                    DumpExcess = false
                });
            }
            catch (Exception)
            {
                print(String.Format("[REGOLITH] Error performing coversion for {0} - {1}", RecipeInputs, ResourceName));
            }
            return r;
        }
    }
}
