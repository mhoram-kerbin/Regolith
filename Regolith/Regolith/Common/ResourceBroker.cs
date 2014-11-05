using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Regolith.Common;

namespace Regolith.Common
{}
    public class ResourceBroker : IResourceBroker
    {
        //For our first implementation, we just care about transfer mode - i.e. the converters are greedy,
        //They will try to work with the current part, and if that fails, will switch to an even distribution of the remainder
        //for all parts.

        public const double FLOAT_TOLERANCE = 0.000000001d; 

        public virtual double AmountAvailable(Part part, string resName)
        {
            var res = PartResourceLibrary.Instance.GetDefinition(resName);
            var resList = new List<PartResource>();
            part.GetConnectedResources(res.id, res.resourceFlowMode, resList);
            return resList.Sum(r => r.amount);
        }

        public virtual double RequestResource(Part part, string resName, double resAmount)
        {
            var res = PartResourceLibrary.Instance.GetDefinition(resName);
            var resList = new List<PartResource>();
            part.GetConnectedResources(res.id, res.resourceFlowMode, resList);
            var demandLeft = resAmount;
            var amountTaken = 0d;
            foreach (var r in resList)
            {
                if (r.amount >= demandLeft)
                {
                    amountTaken += demandLeft;
                    r.amount -= demandLeft;
                    demandLeft = 0;
                }
                else
                {
                    amountTaken += r.amount;
                    demandLeft -= r.amount;
                    r.amount = 0;
                }

                if (Math.Abs(demandLeft) < FLOAT_TOLERANCE) continue;
            }
            
            //This should generally be demand unless weird stuff happened.
            return amountTaken;
        }

        public virtual double StorageAvailable(Part part, string resName)
        {
            var res = PartResourceLibrary.Instance.GetDefinition(resName);
            var resList = new List<PartResource>();
            part.GetConnectedResources(res.id, res.resourceFlowMode, resList);
            return resList.Sum(r => r.maxAmount - r.amount);
        }

        public virtual double StoreResource(Part part, string resName, double resAmount)
        {
            var res = PartResourceLibrary.Instance.GetDefinition(resName);
            var resList = new List<PartResource>();
            part.GetConnectedResources(res.id, res.resourceFlowMode, resList);
            var stuffLeft = resAmount;
            var amountStored = 0d;
            foreach (var r in resList)
            {
                var spaceAvailable = r.maxAmount - r.amount;
                if (spaceAvailable >= stuffLeft)
                {
                    amountStored += stuffLeft;
                    r.amount += stuffLeft;
                    stuffLeft = 0;
                }
                else
                {
                    amountStored += spaceAvailable;
                    stuffLeft -= spaceAvailable;
                    r.amount += spaceAvailable;
                }

                if (Math.Abs(stuffLeft) < FLOAT_TOLERANCE) continue;
            }
            
            //This should generally be demand unless weird stuff happened.
            return amountStored;        
        }
    }
