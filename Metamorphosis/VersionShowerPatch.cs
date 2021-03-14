using HarmonyLib;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(VersionShower), "Start")]
    class VersionShowerPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.Text += "\nloaded Metamorphosis v1.0 Early Access by ckbr0 ";
        }
    }
}
