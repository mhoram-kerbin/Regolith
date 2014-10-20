using System.Collections.Generic;
using Regolith.Common;

namespace Regolith.Scenario
{
    public class RegolithScenario : ScenarioModule
    {
        public static RegolithScenario Instance { get; private set; }
        public RegolithGameSettings gameSettings { get; private set; }

        private readonly List<UnityEngine.Component> children = new List<UnityEngine.Component>();

        public RegolithScenario()
        {
            print("Constructor");
            Instance = this;
            gameSettings = new RegolithGameSettings();
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            gameSettings.Load(gameNode);
            UpdatePlanetaryResourceData();
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            gameSettings.Save(gameNode);
        }

        void OnDestroy()
        {
            foreach (UnityEngine.Component c in children)
            {
                Destroy(c);
            }
            children.Clear();
        }

        private void UpdatePlanetaryResourceData()
        {
            var resList = GameDatabase.Instance.GetConfigNodes("Regolith_Resource");
            var allResData = Utilities.ImportConfigNodeList(resList);
        }

        private ResourceData LoadResource(ConfigNode resNode)
        {
            var res = new ResourceData();
            return res;
        }


    }
}