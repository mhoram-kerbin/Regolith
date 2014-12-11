using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Regolith.Common
{
    public class ResourceConverter
    {
        private IResourceBroker _broker;
        public ResourceConverter(IResourceBroker broker)
        {
            _broker = broker;
        }

        public ResourceConverter() : this(new ResourceBroker())
        { }


        public double ProcessRecipe(double deltaTime, ConversionRecipe recipe, Part resPart)
        {
            //We test for availability of all inputs
            var timeFactor = deltaTime;
            foreach (var r in recipe.Inputs.Where(r=>r.ResourceName != "ElectricCharge"))
            {
                var avail = _broker.AmountAvailable(resPart, r.ResourceName);
                if (avail < r.Ratio * timeFactor)
                {
                    timeFactor = timeFactor * (avail / (r.Ratio * timeFactor));
                }
            }
            //EC is a separate case.  
            if (recipe.Inputs.Any(r => r.ResourceName == "ElectricCharge"))
            {
                var ecRes = recipe.Inputs.First(r => r.ResourceName == "ElectricCharge");
                var avail = _broker.AmountAvailable(resPart, ecRes.ResourceName);
                if (avail < Math.Min(ecRes.Ratio * timeFactor, Utilities.GetECDeltaTime()))
                {
                    timeFactor = timeFactor*Math.Min((avail / (ecRes.Ratio*timeFactor)), Utilities.GetECDeltaTime());
                }
            }


            //test for space of all outputs.  Ignore ones where it's ok to dump them
            foreach (var r in recipe.Outputs.Where(ro=>!ro.DumpExcess))
            {
                var space = _broker.StorageAvailable(resPart, r.ResourceName);
                if (space < r.Ratio * timeFactor)
                {
                    timeFactor = timeFactor * (space / (r.Ratio * timeFactor));
                }
            }


            //Pull inputs
            foreach (var res in recipe.Inputs)
            {
                double input;
                if (res.ResourceName == "ElectricCharge")
                {
                    var ecTime = 0.02d;  //One tick
                    //Exception for stupidly large warps:
                    if (deltaTime < Utilities.GetMaxDeltaTime())
                    {
                        ecTime = Utilities.GetECDeltaTime();
                    }
                        input = _broker.RequestResource(resPart, res.ResourceName,
                            res.Ratio*Math.Min(timeFactor, ecTime));
                }
                else
                {
                    input = _broker.RequestResource(resPart, res.ResourceName, res.Ratio * timeFactor);
                }
            }
            
            //Store outputs
            foreach (var res in recipe.Outputs)
            {

                var output = _broker.StoreResource(resPart, res.ResourceName, res.Ratio * timeFactor);
            }

            return timeFactor;
        }
    }
}
