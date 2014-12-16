using UnityEngine;

namespace Regolith.Asteroids
{
    public class MonoUtilities : MonoBehaviour
    {
        public static void RefreshContextWindows(Part part)
        {
            foreach (UIPartActionWindow window in FindObjectsOfType(typeof(UIPartActionWindow)))
            {
                if (window.part == part)
                {
                    window.displayDirty = true;
                }
            }
        }
    }
}