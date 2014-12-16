using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Regolith.Common
{
    public class ResourceConverter : MonoBehaviour
    {
        private IResourceBroker _broker;
        public ResourceConverter(IResourceBroker broker)
        {
            _broker = broker;
        }

        public ResourceConverter() : this(new ResourceBroker())
        { }


        public double ProcessRecipe(double deltaTime, ConversionRecipe recipe, Part resPart, float efficiencyBonus)
        {
            //Efficiency bonus is comprised of two things.
            //The bonus passed in, and the presence of all required components.
            var bonus = 1d; //We start at 100%.
            foreach (var r in recipe.Requirements)
            {
                var res = resPart.Resources[r.ResourceName];
                var thisRatio = res.amount/Math.Abs(r.Ratio);

                if(r.Ratio < 0) //bad things we need to clean out
                {
                    thisRatio = (1 - thisRatio);
                }
                if (thisRatio < bonus) //It's worse...
                {
                    bonus = thisRatio;
                }
            }
            bonus *= efficiencyBonus;
            //It may be that we're at zero!
            if (bonus <= Utilities.FLOAT_TOLERANCE)
                return 0f;

            //We test for availability of all inputs
            var timeFactor = deltaTime;
            foreach (var r in recipe.Inputs.Where(r=>r.ResourceName != "ElectricCharge"))
            {
                var avail = _broker.AmountAvailable(resPart, r.ResourceName);
                if (avail < r.Ratio * timeFactor * bonus)
                {
                    timeFactor = timeFactor * (avail / (r.Ratio * timeFactor * bonus));
                }
            }
            //EC is a separate case.  
            if (recipe.Inputs.Any(r => r.ResourceName == "ElectricCharge"))
            {
                //In theory warping should just work.  The exception would be catching up
                //when we're hitting max delta time.
                var ecWarp = TimeWarp.CurrentRate*Utilities.GetSecondsPerTick(); // default is 50 physics frames per second.
                var ecRes = recipe.Inputs.First(r => r.ResourceName == "ElectricCharge");
                var avail = _broker.AmountAvailable(resPart, ecRes.ResourceName);
                //If for some reason we don't have all of the EC we need, fall back to max EC time,
                //which defaults to about a minute.  The reason is that if the resource is there (i.e. batteries)
                //then we want to take it.  But if not, then let's make sure they have enough battery power to cover
                //6o seconds of operation - which seems pretty reasonable.
                if (avail < ecWarp)
                    ecWarp = Utilities.GetMaxECDeltaTime();
                
                if (avail < Math.Min(ecRes.Ratio * timeFactor * bonus, ecWarp))
                {
                    timeFactor = timeFactor*Math.Min((avail / (ecRes.Ratio* timeFactor * bonus)), ecWarp);
                }
            }


            //test for space of all outputs.  Ignore ones where it's ok to dump them
            foreach (var r in recipe.Outputs.Where(ro=>!ro.DumpExcess))
            {
                var space = _broker.StorageAvailable(resPart, r.ResourceName);
                if (space < r.Ratio * timeFactor * bonus)
                {
                    timeFactor = timeFactor * (space / (r.Ratio * timeFactor * bonus));
                }
            }


            //Pull inputs
            foreach (var res in recipe.Inputs)
            {
                double input;
                if (res.ResourceName == "ElectricCharge")
                {
                    var ecWarp = TimeWarp.CurrentRate*Utilities.GetSecondsPerTick();
                    var avail = _broker.AmountAvailable(resPart, "ElectricCharge");
                    if (avail < ecWarp)
                        ecWarp = Utilities.GetMaxECDeltaTime();
                    input = _broker.RequestResource(resPart, res.ResourceName,
                        res.Ratio * bonus * Math.Min(timeFactor, ecWarp));
                }
                else
                {
                    input = _broker.RequestResource(resPart, res.ResourceName, res.Ratio * timeFactor * bonus);
                }
            }
            
            //Store outputs
            foreach (var res in recipe.Outputs)
            {

                var output = _broker.StoreResource(resPart, res.ResourceName, res.Ratio * timeFactor * bonus);
            }

            //Work in bonus so our efficiency value is correct.
            return timeFactor * bonus;
        }
    }
}
