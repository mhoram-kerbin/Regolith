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
            var conv = new ResourceConverter(null);
            Assert.IsNotNull(conv);
        }

        [TestMethod]
        public void Converters_Require_a_broker()
        {
            var broker = new FakeResourceBroker();
            var c = new ResourceConverter(broker);
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

        /*  How it works:
         *  Converters are already tied to a broker.
         *  The recipe needs three things:  A part, a recipe, and delta time.
         *  From there, it can return a list of net changes (note the changes already took place).
         */

        [TestMethod]
        public void Should_be_able_to_perform_a_single_resource_conversion()
        {
            var recipe = GetSimpleRecipe();
            var results = GetSimpleOutput();
            var conv = new ResourceConverter(new FakeResourceBroker());
            //We'll start with a delta time of 1 second - the default.
            var actual = conv.ProcessRecipe(1,recipe, null,1);
            var result = comp.Compare(results, actual);
            Assert.IsTrue(result.AreEqual);
        }

        [TestMethod]
        public void Should_be_able_to_perform_a_complex_multi_resource_conversion()
        {
            var recipe = GetComplexRecipe();
            var results = GetComplexOutput();
            var conv = new ResourceConverter(new FakeResourceBroker());
            //We'll start with a delta time of 1 second - the default.
            var actual = conv.ProcessRecipe(1, recipe, null,1);
            var result = comp.Compare(results, actual);
            Assert.IsTrue(result.AreEqual);
        }

        [TestMethod]
        public void Should_be_able_to_account_for_delta_time()
        {
            var recipe = GetComplexRecipe();
            var results = GetComplexOutputWithDelta();
            var conv = new ResourceConverter(new FakeResourceBroker());
            var actual = conv.ProcessRecipe(0.5, recipe, null,1);
            var result = comp.Compare(results, actual);
            Assert.IsTrue(result.AreEqual);
        }


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
            results.Add(new ResourceRatio { ResourceName = "Hydrogen", Ratio = -2 });
            results.Add(new ResourceRatio { ResourceName = "Oxygen", Ratio = -2 });
            results.Add(new ResourceRatio { ResourceName = "ElectricCharge", Ratio = -10 });
            results.Add(new ResourceRatio { ResourceName = "LiquidFuel", Ratio = 0.9f });
            results.Add(new ResourceRatio { ResourceName = "Oxidizer", Ratio = 1.1f });
            return results;
        }

        private List<ResourceRatio> GetComplexOutputWithDelta()
        {
            var results = new List<ResourceRatio>();
            results.Add(new ResourceRatio { ResourceName = "Hydrogen", Ratio = -1 });
            results.Add(new ResourceRatio { ResourceName = "Oxygen", Ratio = -1 });
            results.Add(new ResourceRatio { ResourceName = "ElectricCharge", Ratio = -5 });
            results.Add(new ResourceRatio { ResourceName = "LiquidFuel", Ratio = 0.45f });
            results.Add(new ResourceRatio { ResourceName = "Oxidizer", Ratio = 0.55f });
            return results;
        }

        private List<ResourceRatio> GetSimpleOutput()
        {
            var results = new List<ResourceRatio>();
            results.Add(new ResourceRatio { ResourceName = "Ore", Ratio = -1 });
            results.Add(new ResourceRatio { ResourceName = "Metal", Ratio = 1 });
            return results;
        }
    }
}
