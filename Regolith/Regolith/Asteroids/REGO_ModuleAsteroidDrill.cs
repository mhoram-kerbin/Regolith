using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Regolith.Common;
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
            recipe.Inputs.Add(new ResourceRatio {ResourceName = "ElectricCharge", Ratio = PowerConsumption});


            if (RockOnly)
            {
                resourceList.Clear();
                resourceList.Add(new REGO_ModuleAsteroidResource {abundance = 1, resourceName = "Rock"});
            }
            else
            {
                //Setup rock
                var purity = resourceList.Sum(ar => ar.abundance);
                var rockAmt = 1 - purity;
                resourceList.Add(new REGO_ModuleAsteroidResource {abundance = rockAmt, resourceName = "Rock"});
            }

            foreach (var ar in resourceList)
            {
                if (!(ar.abundance > Utilities.FLOAT_TOLERANCE))
                    continue;
                var res = potato.Resources[ar.resourceName];
                var resInfo = PartResourceLibrary.Instance.GetDefinition(res.resourceName);
                var outRes = new ResourceRatio {ResourceName = ar.resourceName, Ratio = ar.abundance*Efficiency};
                //Make sure we have enough free space
                var spaceNeeded = deltaTime*ar.abundance*Efficiency;
                var spaceAvailable = res.maxAmount - res.amount;
                if (spaceAvailable < spaceNeeded)
                {
                    var slackMass = potato.mass - info.massThreshold;
                    var maxSpace = slackMass/resInfo.density;
                    var unitsToAdd = Math.Min(maxSpace, (spaceNeeded - spaceAvailable));
                    var newMass = potato.mass - ((float) (resInfo.density*unitsToAdd));
                    res.maxAmount += unitsToAdd;
                    potato.mass = newMass;

                }
                recipe.Outputs.Add(outRes);
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
            Physics.Raycast(ray, out hitInfo, 5f);
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
