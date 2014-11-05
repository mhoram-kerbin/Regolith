namespace Regolith.Common
{
    public class ResourceBroker : IResourceBroker
    {
        public virtual double AmountAvailable(Part part, string resName)
        {
            throw new System.NotImplementedException();
        }

        public virtual double RequestResource(Part part, string resName, double resAmount)
        {
            throw new System.NotImplementedException();
        }


        public virtual double StorageAvailable(Part part, string resName)
        {
            throw new System.NotImplementedException();
        }

        public virtual double StoreResource(Part part, string resName, double resAmount)
        {
            throw new System.NotImplementedException();
        }
    }
}