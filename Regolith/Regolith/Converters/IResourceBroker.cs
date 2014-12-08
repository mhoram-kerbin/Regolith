using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regolith.Common
{
    public interface IResourceBroker
    {
        double AmountAvailable(Part part, string resName);
        double RequestResource(Part part, string resName, double resAmount);
        double StorageAvailable(Part part, string resName);
        double StoreResource(Part part, string resName, double resAmount);
    }
}
