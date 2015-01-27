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
        public List<ResourceRatio> Requirements { get; private set; }
        public float FillAmount { get; set; }
        public float TakeAmount { get; set; }
        public ConversionRecipe()
        {
            Inputs = new List<ResourceRatio>();
            Outputs = new List<ResourceRatio>();
            Requirements = new List<ResourceRatio>();
            FillAmount = 1f;
            TakeAmount = 1f;
        }
    }
}
