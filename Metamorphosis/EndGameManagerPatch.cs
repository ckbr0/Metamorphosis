using HarmonyLib;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(EndGameManager))]
    public static class EndGameManagerPatch
    {
        [HarmonyPostfix]
        //[HarmonyPatch(nameof(EndGameManager.BPHOLDGAGHI))]
        [HarmonyPatch(nameof(EndGameManager.DHLNMMLDGIO))]
        public static void Postfix(EndGameManager __instance)
        {
            if (PlayerControlPatch.Metamorphs != null)
            {
                PlayerControlPatch.Metamorphs.Clear();
                PlayerControlPatch.Metamorphs = null;
            }

            if (HudManagerPatch.MorphButton != null)
            {
                HudManagerPatch.MorphButton.Visible = false;
                HudManagerPatch.MorphButton.HideButton();
            }
        }
    }
}