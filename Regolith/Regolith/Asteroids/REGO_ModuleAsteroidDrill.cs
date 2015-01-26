using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Regolith.Common;
using Regolith.Converters;
using UnityEngine;

namespace Regolith.Asteroids
{
    public class REGO_ModuleAsteroidDrill : BaseConverter
    {

        [KSPField(isPersistant = false)] 
        public float PowerConsumption = 1;

        [KSPField(isPersistant = false)]
        public double Efficiency = .1;

        [KSPField(isPersistant = true)]
        public string ImpactTransform = "";

        [KSPField(isPersistant = false)]
        public float ImpactRange = 5f;

        [KSPField(isPersistant = true)]
        public bool DirectAttach = false;

        [KSPField] 
        public bool RockOnly = false;


        protected override ConversionRecipe PrepareRecipe(double deltaTime)
        {
            //Determine if we are in fact latched to an asteroid
            var potato = GetAttachedPotato();
            if (potato == null)
            {
                status = "No asteroid detected";
                IsActivated = false;
                return null;
            }

            if (DirectAttach && !(part.children.Contains(potato) || part.parent == potato))
            {
                status = "Not directly attached to asteroid";
                IsActivated = false;
                return null;
            }

            if (!RockOnly && !potato.Modules.Contains("REGO_ModuleAsteroidResource"))
            {
                status = "No resource data";
                IsActivated = false;
                return null;
            }
            var resourceList = potato.FindModulesImplementing<REGO_ModuleAsteroidResource>();

            if (!CheckForImpact())
            {
                status = "No surface impact";
                IsActivated = false;
                return null;               
            }

            var info = potato.FindModuleImplementing<REGO_ModuleAsteroidInfo>();
            if (info == null)
            {
                status = "No info";
                IsActivated = false;
                return null;
            }

            if (info.massThreshold >= potato.mass)
            {
                status = "Resources Depleted";
                IsActivated = false;
                return null;
            }

            var ec = _broker.AmountAvailable(part, "ElectricCharge");
            if (ec <= deltaTime * PowerConsumption)
            {
                status = "Insufficient Power";
                IsActivated = false;
                return null;
            }

            //Handle state change
            UpdateConverterStatus();
            //If we're enabled"
            if (!IsActivated)
                return null;

            
            //Fetch our recipe
            var recipe = new ConversionRecipe();

            //If ANY resources have storage space, we attempt to do mass conversion.
            //If there is NO storage space available, there is no output and EC is dumped.
            bool spaceAvailable = resourceList.Any(res => _broker.StorageAvailable(part, res.resourceName) > 0);

            if (spaceAvailable)
            {
                recipe.Inputs.Add(new ResourceRatio {ResourceName = "ElectricCharge", Ratio = PowerConsumption});
                foreach (var ar in resourceList)
                {
                    if (ar.abundance <= Utilities.FLOAT_TOLERANCE)
                        continue;
                    var resInfo = PartResourceLibrary.Instance.GetDefinition(ar.resourceName);
                    //Make sure we have enough mass
                    var desiredUnits = deltaTime*ar.abundance*Efficiency;
                    var slackMass = potato.mass - info.massThreshold;
                    var maxUnits = slackMass / resInfo.density;
                    var unitsToAdd = Math.Min(desiredUnits, maxUnits);
                    var newMass = potato.mass - ((float)(resInfo.density * unitsToAdd));
                    potato.mass = newMass;
                    var outRes = new ResourceRatio { ResourceName = ar.resourceName, Ratio = unitsToAdd, DumpExcess = true};
                    recipe.Outputs.Add(outRes);
                }
            }
            else
            {
                status = "No Storage Space";
            }

            return recipe;
        }

        private bool CheckForImpact()
        {
            if (ImpactTransform == "") 
                return true;
            var t = part.FindModelTransform(ImpactTransform);
            var targetType = "PotatoRoid";
            var pos = t.position;
            RaycastHit hitInfo;
            var ray = new Ray(pos, t.forward);
            Physics.Raycast(ray, out hitInfo, ImpactRange);
            if (hitInfo.collider != null)
            {
                var colType =   hitInfo.collider.attachedRigidbody.gameObject.name;
                return (colType.StartsWith(targetType));
            }
            return false;
        }

        private Part GetAttachedPotato()
        {
            var potatoes = vessel.Parts.Where(p => p.Modules.Contains("ModuleAsteroid"));
            if (potatoes.Any())
            {
                return potatoes.FirstOrDefault();
            }
            return null;
        }
    }
}
