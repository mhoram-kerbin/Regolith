using System;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Regolith.Common;
using Regolith.Scenario;

namespace Regolith.Tests.Unit
{
    [TestClass]
    public class When_importing_ConfigNodes_into_an_object_tree 
    {
        private CompareLogic comp = new CompareLogic();
        
        [TestMethod]
        public void Should_be_able_to_import_a_simple_root_value()
        {
            var nodes = GetSimpleNodes();
            var expected = GetSimpleResults();
            var actual = Utilities.ImportConfigNodeList(nodes);
            var result = comp.Compare(expected, actual);
            Assert.IsTrue(result.AreEqual);
        }

        [TestMethod]
        public void Should_ignore_bad_config_values()
        {
            var nodes = GetInvalidPropertyNodes();
            var expected = GetInvalidPropertyResults();
            var actual = Utilities.ImportConfigNodeList(nodes);
            var result = comp.Compare(expected, actual);
            Assert.IsTrue(result.AreEqual);
        }

        [TestMethod]
        public void Should_be_able_to_retrieve_complex_properties()
        {
            var nodes = GetComplexNodes();
            var expected = GetComplexResults();
            var actual = Utilities.ImportConfigNodeList(nodes);
            var result = comp.Compare(expected, actual);
            Assert.IsTrue(result.AreEqual);
        }

        #region TEST DATA
        private ConfigNode[] GetSimpleNodes()
        {
            var nodes = new List<ConfigNode>();
            var n1 = new ConfigNode("Regolith_Resource");
            n1.AddValue("ResourceName", "Karbonite");
            nodes.Add(n1);
            return nodes.ToArray();
        }

        private List<ResourceData> GetSimpleResults()
        {
            var resList = new List<ResourceData>();
            resList.Add(new ResourceData { ResourceName = "Karbonite"});
            return resList;
        }

        private ConfigNode[] GetInvalidPropertyNodes()
        {
            var nodes = new List<ConfigNode>();
            var n1 = new ConfigNode("Regolith_Resource");
            n1.AddValue("InvalidKey", "UnicornHooves");
            nodes.Add(n1);
            nodes.AddRange(GetSimpleNodes());
            return nodes.ToArray();
        }


        private List<ResourceData> GetInvalidPropertyResults()
        {
            var resList = new List<ResourceData>();
            resList.Add(new ResourceData());
            resList.AddRange(GetSimpleResults());
            return resList;
        }


        private ConfigNode[] GetComplexNodes()
        {
            var nodes = new List<ConfigNode>();
            var n1 = new ConfigNode("Regolith_Resource");
            n1.AddValue("ResourceName", "LiquidHydrogen");

            var d1 = new ConfigNode("Distribution");
            d1.AddValue("MinAtmosphericPresence", 0);
            d1.AddValue("MaxAtmosphericPresence", 30);

            n1.AddNode(d1);

            nodes.Add(n1);
            nodes.AddRange(GetSimpleNodes());
            return nodes.ToArray();
        }


        private List<ResourceData> GetComplexResults()
        {
            var resList = new List<ResourceData>();
            resList.Add(new ResourceData
                        {
                            Distribution = new DistributionData { MinAtmosphericPresence = 0, MaxAtmosphericPresence = 30 },
                            ResourceName = "LiquidHydrogen"
                        });
            resList.AddRange(GetSimpleResults());
            return resList;
        }

        #endregion

    }
}
