using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Regolith.Common;

namespace Regolith.Tests.Unit
{
    [TestClass]
    public class When_working_with_Resources
    {
        [TestMethod]
        public void Should_be_able_to_get_available_quantity()
        {
            IResourceBroker broker = new FakeResourceBroker();
            double available = broker.AmountAvailable("EnrichedUranium");
            Assert.IsNotNull(available);
        }

        [TestMethod]
        public void Should_be_able_to_request_a_Resource()
        {
            IResourceBroker broker = new FakeResourceBroker();
            double takeAmt = broker.RequestResource(null, "EnrichedUranium", 0f);
            Assert.IsNotNull(takeAmt);
        }
    }

    public class FakeResourceBroker : ResourceBroker 
    {
        public override double AmountAvailable(string resName)
        {
            return 0f;
        }

        public override double RequestResource(Part part, string resName, double resAmount)
        {
            return 0f;
        }
    }


}
