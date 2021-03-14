using HarmonyLib;
using GameOptionsData = PAMOPBEDCNI;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(GameOptionsData))]
    public class GameOptionsDataPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameOptionsData.NHJLMAAHKJF))]
        public static void Postix(GameOptionsData __instance, ref string __result, int MKGPLPMAKLO)
        {
            __result += $"Morph Duration: {CustomGameOptions.MorphDuration.ToString()}s\n";
            __result += $"Morph Cooldown: {CustomGameOptions.MorphCooldown.ToString()}s\n";
        }
    }
}