using HarmonyLib;
using GameOptionsData = CEIOGGEDKAN;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(GameOptionsData))]
    public class GameOptionsDataPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameOptionsData.ONCLFHFDADB))]
        public static void Postix(GameOptionsData __instance, ref string __result, int JFGKGCCMCNK)
        {
            __result += $"Morph Duration: {CustomGameOptions.MorphDuration.ToString()}s\n";
            __result += $"Morph Cooldown: {CustomGameOptions.MorphCooldown.ToString()}s\n";
        }
    }
}
