﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Regolith.Common
{
    public struct ConverterResults
    {
        public string Status;
        public double TimeFactor;
    }

    public class ResourceConverter : MonoBehaviour
    {
        private IResourceBroker _broker;

        public ResourceConverter(IResourceBroker broker)
        {
            _broker = broker;
        }

        public ResourceConverter() : this(new ResourceBroker())
        { }

        public class ResourceTransferParameters
        {
            public class ResourceTransfer
            {
                public bool input;
                public Part part;
                public string resourceName;
                public double amount;
            }
            public List<ResourceTransfer> rtl;
            public double timeFactor;
            public double bonus;
        }
        public ResourceTransferParameters GetTransferAmounts(double deltaTime, ConversionRecipe recipe, Part resPart, float efficiencyBonus)
        {
            ResourceTransferParameters rtp = new ResourceTransferParameters();
            rtp.rtl = new List<ResourceTransferParameters.ResourceTransfer>();
//            var result = new ConverterResults();
//            result.Status = "Idle";
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
            {
                throw new RecepieException("Missing resources");
//                result.Status = "Missing resources";
//                result.TimeFactor = 0;
//                return result;
            }

            //We test for availability of all inputs
            double timeFactor = deltaTime;
            foreach (var r in recipe.Inputs.Where(r=>r.ResourceName != "ElectricCharge"))
            {
                // 10 seconds with a ratio of 2/sec and a bonus of 1.5 means we need 30.
                //Include our take amount multiplier to fool us into having less capacity than we
                //really have.
                var avail = _broker.AmountAvailable(resPart, r.ResourceName) * recipe.TakeAmount;
                //If we only have 15...
                //15 < (2*10*1.5)[30]
                if (avail < r.Ratio * timeFactor * bonus)
                {
                    //Then we reduce our timeFactor to 5.
                    // 10 * (15 / (2*10*1.5)[30]) = .5
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
                //10 seconds of operation - which seems pretty reasonable.
                if (avail < ecWarp)
                    ecWarp = Utilities.GetMaxECDeltaTime();
                
                //The only time this should affect our overall conversion rate
                //is if we can't even meet this basic requirement.  i.e. we can't run stuff on empty.
                //In that case, the whole shebang shuts down to be in line with the EC number.
                var ecGoal = Math.Min(ecRes.Ratio*timeFactor*bonus, ecWarp);

                if (avail < ecGoal)
                {
                    //We currently have a TimeFactor of 10.
                    //Our EC goal is 2.  But we only have 1.
                    //Hence, or TimeFactor should now be 5.
                    
                    // 10 * ( 1 / 2 ) = 5
                    var ectimeFactor = timeFactor*(avail/ecGoal);
                    timeFactor = ectimeFactor;
                }
            }

            if (timeFactor <= Utilities.FLOAT_TOLERANCE)
            {
                throw new RecepieException("Missing inputs");
//                result.Status = "Missing inputs";
//                result.TimeFactor = 0;
//                return result;
            }


            //test for space of all outputs.  Ignore ones where it's ok to dump them
            //Also:  We do use the FillAmount, this effectively fools the system into thinking there is 
            //less space available than there really is.  The use case would be things where we want a slow charge,
            //such as battery charging, etc.
            foreach (var r in recipe.Outputs.Where(ro=>!ro.DumpExcess))
            {
                var space = (_broker.StorageAvailable(resPart, r.ResourceName))* recipe.FillAmount;
                
                if (space < r.Ratio * timeFactor * bonus)
                {
                    timeFactor = timeFactor * (space / (r.Ratio * timeFactor * bonus));
                }
            }

            if (timeFactor <= Utilities.FLOAT_TOLERANCE)
            {
                throw new RecepieException("no space");
//                result.Status = "no space";
//                result.TimeFactor = 0;
//                return result;
            }

            //Pull inputs
            foreach (var res in recipe.Inputs)
            {
                if (res.ResourceName == "ElectricCharge")
                {
                    var ecWarp = TimeWarp.CurrentRate*Utilities.GetSecondsPerTick();
                    var avail = _broker.AmountAvailable(resPart, "ElectricCharge");
                    if (avail < ecWarp)
                        ecWarp = Utilities.GetMaxECDeltaTime();
                    rtp.rtl.Add(new ResourceTransferParameters.ResourceTransfer { amount = res.Ratio * bonus * Math.Min(timeFactor, ecWarp), input = true, part = resPart, resourceName = res.ResourceName });
                }
                else
                {
                    rtp.rtl.Add(new ResourceTransferParameters.ResourceTransfer { amount = res.Ratio * timeFactor * bonus, input = true, part = resPart, resourceName = res.ResourceName });
                }
            }
            
            //Store outputs
            foreach (var res in recipe.Outputs)
            {
                rtp.rtl.Add(new ResourceTransferParameters.ResourceTransfer { amount = res.Ratio * timeFactor * bonus, input = false, part = resPart, resourceName = res.ResourceName });
            }
            rtp.bonus = bonus;
            rtp.timeFactor = timeFactor;
            return rtp;
        }

        public ConverterResults ProcessRecipe(double deltaTime, ConversionRecipe recipe, Part resPart, float efficiencyBonus)
        {
            var result = new ConverterResults();
            result.Status = "Idle";
            ResourceTransferParameters rtp = new ResourceTransferParameters();
            try {
                rtp = GetTransferAmounts(deltaTime, recipe, resPart, efficiencyBonus);
            }
            catch (RecepieException e) {
                result.Status = e.problem;
                result.TimeFactor = 0;
                return result;
            }

            foreach (ResourceTransferParameters.ResourceTransfer rt in rtp.rtl)
            {
                if (rt.input)
                {
                    _broker.RequestResource(rt.part, rt.resourceName,rt.amount);
                }
                else
                {
                    _broker.StoreResource(rt.part, rt.resourceName, rt.amount);
                }
            }

            //Work in bonus so our efficiency value is correct.
            result.TimeFactor = rtp.timeFactor * rtp.bonus;
            return result;
        }

        private class RecepieException : Exception
        {
            public string problem;
            public RecepieException(string p) { problem = p; }
        }

    }
}
