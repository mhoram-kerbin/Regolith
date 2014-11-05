using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regolith.Common
{
    public class ConversionRecipe
    {
        public List<ResourceRatio> Inputs { get; private set; }
        public List<ResourceRatio> Outputs { get; private set; }

        public ConversionRecipe()
        {
            Inputs = new List<ResourceRatio>();
            Outputs = new List<ResourceRatio>();
        }
    }
}
