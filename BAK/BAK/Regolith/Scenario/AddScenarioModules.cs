using System.Linq;
using Regolith.Annotations;
using UnityEngine;

namespace Regolith.Scenario
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddScenarioModules : MonoBehaviour
    {
        void Start()
        {
            var game = HighLogic.CurrentGame;

            var psm = game.scenarios.Find(s => s.moduleName == typeof(RegolithScenario).Name);
            if (psm == null)
            {               
                game.AddProtoScenarioModule(typeof(RegolithScenario), GameScenes.SPACECENTER,
                    GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.SPH);
            }
            else
            {
                if (psm.targetScenes.All(s => s != GameScenes.SPACECENTER))
                {
                    psm.targetScenes.Add(GameScenes.SPACECENTER);
                }
                if (psm.targetScenes.All(s => s != GameScenes.FLIGHT))
                {
                    psm.targetScenes.Add(GameScenes.FLIGHT);
                }
                if (psm.targetScenes.All(s => s != GameScenes.EDITOR))
                {
                    psm.targetScenes.Add(GameScenes.EDITOR);
                }
                if (psm.targetScenes.All(s => s != GameScenes.SPH))
                {
                    psm.targetScenes.Add(GameScenes.SPH);
                }
            }
        }
    }
}