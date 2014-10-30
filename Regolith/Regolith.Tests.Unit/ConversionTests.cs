using System;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Regolith.Common;
using UnityEngine;

namespace Regolith.Tests.Unit
{
    [TestClass]
    public class When_performing_conversions
    {
        private CompareLogic comp = new CompareLogic();

        [TestMethod]
        public void Should_be_able_to_establish_a_converter()
        {
            var conv = new USI_ResourceConverter(null);
            Assert.IsNotNull(conv);
        }

        [TestMethod]
        public void Converters_Require_a_Conversion_Recipe()
        {
            var conData = new ConversionRecipe();
            var c = new USI_ResourceConverter(conData);
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public void a_conversion_recipe_has_a_list_of_inputs()
        {
            var c = new ConversionRecipe();
            c.Inputs.Add(new ResourceRatio());
            Assert.IsNotNull(c.Inputs);
        }

        [TestMethod]
        public void a_conversion_recipe_has_a_list_of_outputs()
        {
            var c = new ConversionRecipe();
            c.Outputs.Add(new ResourceRatio());
            Assert.IsNotNull(c.Outputs);
        }

        /* So for this, we should have the concept of getting a resource list from a vessel, or from a part.  We can pass something in (take a parm) but 
         * not instantiate one due to some unity stuff.  We can then make sure bits are moving around via the broker (which we assume just works - we'll need 
         * to integration test it. */


        //[TestMethod]
        //public void Should_be_able_to_perform_a_single_resource_conversion()
        //{
        //    var recipe = GetSimpleRecipe();
        //    var results = GetSimpleOutput();
        //    var conv = new USI_ResourceConverter(recipe);
        //    //We'll start with a delta time of 1 second - the default.
        //    conv.ProcessRecipe(1);
        //    var result = comp.Compare(results, actual);
        //    Assert.IsTrue(result.AreEqual);
        //}

        //[TestMethod]
        //public void Should_be_able_to_perform_a_complex_multi_resource_conversion()
        //{
        //    var recipe = GetComplexRecipe();
        //    var results = GetComplexOutput();
        //    var conv = new USI_ResourceConverter(recipe);
        //    //We'll start with a delta time of 1 second - the default.
        //    var actual = conv.ProcessRecipe(1);
        //    var result = comp.Compare(results, actual);
        //    Assert.IsTrue(result.AreEqual);
        //}

        //[TestMethod]
        //public void Should_be_able_to_account_for_delta_time()
        //{
        //    var recipe = GetComplexRecipe();
        //    var results = GetComplexOutputWithDelta();
        //    var conv = new USI_ResourceConverter(recipe);
        //    var actual = conv.ProcessRecipe(0.5);
        //    var result = comp.Compare(results, actual);
        //    Assert.IsTrue(result.AreEqual);
        //}



        private ConversionRecipe GetSimpleRecipe()
        {
            var recipe = new ConversionRecipe();
            recipe.Inputs.Add(new ResourceRatio { ResourceName = "Ore", Ratio = 1 });
            recipe.Outputs.Add(new ResourceRatio { ResourceName = "Metal", Ratio = 1 });
            return recipe;
        }

        private ConversionRecipe GetComplexRecipe()
        {
            var recipe = new ConversionRecipe();
            recipe.Inputs.Add(new ResourceRatio { ResourceName = "Hydrogen", Ratio = 2 });
            recipe.Inputs.Add(new ResourceRatio { ResourceName = "Oxygen", Ratio = 2 });
            recipe.Inputs.Add(new ResourceRatio { ResourceName = "ElectricCharge", Ratio = 10 });
            recipe.Outputs.Add(new ResourceRatio { ResourceName = "LiquidFuel", Ratio = 0.9f });
            recipe.Outputs.Add(new ResourceRatio { ResourceName = "Oxidizer", Ratio = 1.1f });
            return recipe;
        }

        private List<ResourceRatio> GetComplexOutput()
        {
            var results = new List<ResourceRatio>();
            results.Add(new ResourceRatio { ResourceName = "LiquidFuel", Ratio = 0.9f });
            results.Add(new ResourceRatio { ResourceName = "Oxidizer", Ratio = 1.1f });
            return results;
        }

        private List<ResourceRatio> GetComplexOutputWithDelta()
        {
            var results = new List<ResourceRatio>();
            results.Add(new ResourceRatio { ResourceName = "LiquidFuel", Ratio = 0.45f });
            results.Add(new ResourceRatio { ResourceName = "Oxidizer", Ratio = 0.55f });
            return results;
        }


        private List<ResourceRatio> GetSimpleOutput()
        {
            var results = new List<ResourceRatio>();
            results.Add(new ResourceRatio { ResourceName = "Metal", Ratio = 1});
            return results;
        }
    }
}
