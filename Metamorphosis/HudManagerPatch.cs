using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(HudManager))]
    public class HudManagerPatch
    {
        public static MorphButton MorphButton;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HudManager.Start))]
        public static void Postfix1(HudManager __instance)
        {
            MorphButton = new MorphButton(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix2(HudManager __instance)
        {
            if (Metamorph.LocalMetamorph != null)
            {
				Metamorph.LocalMetamorph.Update();
            }

            if (MorphButton != null)
            {
                MorphButton.Update();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HudManager.SetHudActive))]
        public static void Postfix(bool LKKLMJJOFAK)
        {
            MorphButton.HudVisible = LKKLMJJOFAK; // Show/hide all buttons, as the game does.
        }
    }
}
