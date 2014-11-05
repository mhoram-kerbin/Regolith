using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Regolith.Common
{
    public class USI_ResourceConverter
    {
        private IResourceBroker _broker;
        public USI_ResourceConverter(IResourceBroker broker)
        {
            _broker = broker;
        }

        public USI_ResourceConverter() : this(new ResourceBroker())
        { }


        public List<ResourceRatio> ProcessRecipe(double deltaTime, ConversionRecipe recipe, Part part)
        {
            //How recipes work:
            var results = new List<ResourceRatio>();

            //We test for availability of all inputs
            var ratio = deltaTime;
            foreach (var r in recipe.Inputs)
            {
                var avail = _broker.AmountAvailable(part, r.ResourceName);
                if (avail < r.Ratio * ratio)
                {
                    ratio = ratio*(avail/(r.Ratio*ratio));
                }
            }
            //test for space of all outputs
            foreach (var r in recipe.Outputs)
            {
                var space = _broker.StorageAvailable(part, r.ResourceName);
                if (space < r.Ratio * ratio)
                {
                    ratio = ratio * (space / (r.Ratio * ratio));
                }
            }


            //Pull inputs
            foreach (var r in recipe.Inputs)
            {
                var result = new ResourceRatio {Ratio = r.Ratio*ratio, ResourceName = r.ResourceName};
                results.Add(result);
            }
            //Pull outputs
            foreach (var r in recipe.Outputs)
            {
                var result = new ResourceRatio {Ratio = r.Ratio*ratio*-1, ResourceName = r.ResourceName};
                results.Add(result);
            }

            return results;
        }
    }
}
