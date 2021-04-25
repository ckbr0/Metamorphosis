using HarmonyLib;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(VersionShower), "Start")]
    class VersionShowerPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.text += "\nloaded Metamorphosis v1.1 by ckbr0 ";
        }
    }
}
