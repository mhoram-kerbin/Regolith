using System;
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

        [KSPField]
        public string RequiredResources = "";


        public ConversionRecipe Recipe
        {
            get { return _recipe ?? (_recipe = LoadRecipe()); }
        }

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

                if (!String.IsNullOrEmpty(RecipeInputs))
                {
                    var inputs = RecipeInputs.Split(',');
                    for (int ip = 0; ip < inputs.Count(); ip += 2)
                    {
                        print(String.Format("[REGOLITH] - INPUT {0} {1}", inputs[ip], inputs[ip + 1]));
                        r.Inputs.Add(new ResourceRatio
                                     {
                                         ResourceName = inputs[ip].Trim(),
                                         Ratio = Convert.ToDouble(inputs[ip + 1].Trim())
                                     });
                    }
                }

                if (!String.IsNullOrEmpty(RecipeOutputs))
                {
                    var outputs = RecipeOutputs.Split(',');
                    for (int op = 0; op < outputs.Count(); op += 3)
                    {
                        print(String.Format("[REGOLITH] - OUTPUTS {0} {1} {2}", outputs[op], outputs[op + 1],
                            outputs[op + 2]));
                        r.Outputs.Add(new ResourceRatio
                                      {
                                          ResourceName = outputs[op].Trim(),
                                          Ratio = Convert.ToDouble(outputs[op + 1].Trim()),
                                          DumpExcess = Convert.ToBoolean(outputs[op + 2].Trim())
                                      });
                    }
                }

                if (!String.IsNullOrEmpty(RequiredResources))
                {
                    var requirements = RequiredResources.Split(',');
                    for (int rr = 0; rr < requirements.Count(); rr += 2)
                    {
                        print(String.Format("[REGOLITH] - REQUIREMENTS {0} {1}", requirements[rr], requirements[rr + 1]));
                        r.Requirements.Add(new ResourceRatio
                                           {
                                               ResourceName = requirements[rr].Trim(),
                                               Ratio = Convert.ToDouble(requirements[rr + 1].Trim()),
                                           });
                    }
                }
            }
            catch (Exception)
            {
                print(String.Format("[REGOLITH] Error performing conversion for '{0}' - '{1}' - '{2}'", RecipeInputs, RecipeOutputs, RequiredResources));
            }
            return r;
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            var recipe = LoadRecipe();
            sb.Append(".");
            sb.Append("\n");
            sb.Append(ConverterName);
            sb.Append("\n\nInputs:");
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
            sb.Append("\n\nOutputs: ");
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
            sb.Append("\n\nRequirements: ");
            foreach (var output in recipe.Requirements)
            {
                sb.Append("\n - ")
                    .Append(output.ResourceName)
                    .Append(": ");
                    sb.Append(String.Format("{0:0.00}", output.Ratio));
            }
            sb.Append("\n");
            return sb.ToString();
        }

    }
}
