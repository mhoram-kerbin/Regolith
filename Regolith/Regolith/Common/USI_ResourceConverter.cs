using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regolith.Common
{
    public class USI_ResourceConverter
    {
        private IResourceBroker _broker;
        public USI_ResourceConverter(IResourceBroker broker)
        {
            _broker = broker;
        }

        public USI_ResourceConverter() : this(new ResourceBroker())
        { }


        public void ProcessRecipe(double deltaTime)
        {
        }
    }
}
