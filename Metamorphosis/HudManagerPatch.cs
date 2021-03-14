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
            MorphButton.Visible = false;
            MorphButton.Clickable = false;
            MorphButton.EffectDuration = 20.0f;
            MorphButton.CooldownDuration = 5.0f;//10.0f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix2(HudManager __instance)
        {
            if (PlayerControlPatch.Metamorphs != null)
            {
                foreach (Metamorph metamorph in PlayerControlPatch.Metamorphs)
                {
                    metamorph.Update();
                    if (metamorph.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        MorphButton.Update();
                }

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HudManager.SetHudActive))]
        private static void HudManagerSetHudActive([HarmonyArgument(0)] bool isActive)
        {
            MorphButton.HudVisible = isActive; // Show/hide all buttons, as the game does.
        }
    }
}
