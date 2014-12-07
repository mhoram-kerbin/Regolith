using System;
using System.Collections.Generic;
using Regolith.Common;

namespace Regolith.Scenario
{
    public class RegolithScenario : ScenarioModule
    {
        public static RegolithScenario Instance { get; private set; }
        public RegolithGameSettings gameSettings { get; private set; }

        public RegolithScenario()
        {
            Instance = this;
            gameSettings = new RegolithGameSettings();
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            gameSettings.Load(gameNode);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            gameSettings.Save(gameNode);
        }

    }
}