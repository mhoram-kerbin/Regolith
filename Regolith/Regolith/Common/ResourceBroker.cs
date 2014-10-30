namespace Regolith.Common
{
    public class ResourceBroker : IResourceBroker
    {
        public virtual double AmountAvailable(string resName)
        {
            throw new System.NotImplementedException();
        }

        public virtual double RequestResource(Part part, string resName, double resAmount)
        {
            throw new System.NotImplementedException();
        }
    }
}