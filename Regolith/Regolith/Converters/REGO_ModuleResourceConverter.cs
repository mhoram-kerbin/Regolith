using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Regolith.Asteroids;

namespace Regolith.Common
{
    public class REGO_ModuleResourceConverter : BaseConverter
    {
        [KSPField] 
        public string RecipeInputs = "";

        [KSPField]
        public string RecipeOutputs = "";

        private ConversionRecipe _recipe;
        protected override ConversionRecipe PrepareRecipe(double deltatime)
        {

            if (_recipe == null)
                _recipe = LoadRecipe();
            UpdateConverterStatus();
            if (!IsActivated)
                return null;
            return _recipe;
        }

        private ConversionRecipe LoadRecipe()
        {
            var r = new ConversionRecipe();
            try
            {
                var inputs = RecipeInputs.Split(',');
                var outputs = RecipeOutputs.Split(',');
                for (int ip = 0; ip < inputs.Count(); ip += 2)
                {
                    print(String.Format("[REGOLITH] - INPUT {0} {1}", inputs[ip], inputs[ip + 1]));
                    r.Inputs.Add(new ResourceRatio
                                 {
                                     ResourceName = inputs[ip].Trim(),
                                     Ratio = Convert.ToDouble(inputs[ip + 1])
                                 });
                }
                for (int op = 0; op < outputs.Count(); op += 3)
                {
                    print(String.Format("[REGOLITH] - OUTPUTS {0} {1} {2}", outputs[op], outputs[op + 1], outputs[op+2]));
                    r.Outputs.Add(new ResourceRatio
                    {
                        ResourceName = outputs[op].Trim(),
                        Ratio = Convert.ToDouble(outputs[op+1]),
                        DumpExcess = Convert.ToBoolean(outputs[op+2].Trim())
                    });
                }
            }
            catch (Exception)
            {
                print(String.Format("[REGOLITH] Error performing coversion for {0} - {1}", RecipeInputs, RecipeOutputs));
            }
            return r;
        }

    }
}
