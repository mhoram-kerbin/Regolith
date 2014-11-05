using System;
using System.Collections.Generic;
using System.Linq;
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
            double available = broker.AmountAvailable(null, "EnrichedUranium");
            Assert.IsNotNull(available);
        }

        [TestMethod]
        public void Should_be_able_to_request_a_Resource()
        {
            IResourceBroker broker = new FakeResourceBroker();
            double takeAmt = broker.RequestResource(null, "EnrichedUranium", 0f);
            Assert.IsNotNull(takeAmt);
        }

        [TestMethod]
        public void Should_be_able_to_get_available_storage()
        {
            IResourceBroker broker = new FakeResourceBroker();
            double space = broker.StorageAvailable(null, "EnrichedUranium");
            Assert.IsNotNull(space);
        }

        [TestMethod]
        public void Should_be_able_to_store_a_Resource()
        {
            IResourceBroker broker = new FakeResourceBroker();
            double storeAmt = broker.StoreResource(null, "EnrichedUranium", 0f);
            Assert.IsNotNull(storeAmt);
        }
    
    }

    public class FakeResourceBroker : ResourceBroker
    {
        private List<FakeResourceInfo> _partResources;
        private List<FakeResourceInfo> _shipResources;
        public FakeResourceBroker()
        {
            _partResources = new List<FakeResourceInfo>
                         {
                             new FakeResourceInfo {ResourceName = "Ore", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Metal", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Hydrogen", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Oxygen", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "ElectricCharge", Amount = 1000, MaxAmount = 2000, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "LiquidFuel", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Oxidizer", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "EnrichedUranium", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.NONE},
                         };
            _shipResources = new List<FakeResourceInfo>
                         {
                             new FakeResourceInfo {ResourceName = "Ore", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Metal", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Hydrogen", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Oxygen", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "ElectricCharge", Amount = 1000, MaxAmount = 2000, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "LiquidFuel", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "Oxidizer", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.PUMP},
                             new FakeResourceInfo {ResourceName = "EnrichedUranium", Amount = 100, MaxAmount = 200, TransferMode = ResourceTransferMode.NONE},
                         };

        }

        public override double AmountAvailable(Part part, string resName)
        {
            var res = _partResources.First(r => r.ResourceName == resName);
            return res.Amount;
        }

        public override double RequestResource(Part part, string resName, double resAmount)
        {
            return resAmount;
        }

        public override double StorageAvailable(Part part, string resName)
        {
            var res = _partResources.First(r => r.ResourceName == resName);
            return (res.MaxAmount - res.Amount);

        }

        public override double StoreResource(Part part, string resName, double resAmount)
        {
            return resAmount;
        }


        public class FakeResourceInfo
        {
            public string ResourceName { get; set; }
            public double Amount { get; set; }
            public double MaxAmount { get; set; }

            public ResourceTransferMode TransferMode { get; set; }
        }
    }


}
