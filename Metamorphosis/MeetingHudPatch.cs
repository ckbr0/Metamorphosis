using HarmonyLib;

using PlayerInfo = GameData.GOOIGLGKMCE;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(MeetingHud))]
    public class MeetingHudPatch 
    {
        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(MeetingHud), "BGADFCJCOAA")]
        public static void Prefix1(PlayerInfo EKCPHFOGJPA)
        {
            Metamorphosis.Logger.LogMessage($"BGADFCJCOAA: {EKCPHFOGJPA.FMAAJCIEMEH}");
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MeetingHud.EEFGLJPBAHF))]
        public static void Prefix(PlayerInfo EKCPHFOGJPA)
        {
            if (PlayerControlPatch.Metamorphs != null)
            {
                foreach (Metamorph metamorph in PlayerControlPatch.Metamorphs)
                {
                    if (metamorph.PlayerId == EKCPHFOGJPA.FMAAJCIEMEH)
                    {
                        MorphInfo original = metamorph.OrignalInfo;
                        EKCPHFOGJPA.LNFMCJAPLBH = original.Name;
                        EKCPHFOGJPA.ACBLKMFEPKC = original.ColorId;
                        EKCPHFOGJPA.FHNDEEGICJP = original.SkinId;
                        EKCPHFOGJPA.KCILOGLJODF = original.HatId;
                        EKCPHFOGJPA.HIJJGKGBKOJ = original.PetId;
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MeetingHud.Close))]
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControlPatch.Metamorphs != null)
            {
                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                if (PlayerControlPatch.IsMetamorph(localPlayer))
                {
                    HudManagerPatch.MorphButton.StartCooldown(HudManagerPatch.MorphButton.CooldownDuration+8.0f);
                }
            }
        }

        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(MeetingHud), "JKIFEGOFNDP")]
        public static void Prefix3(PlayerInfo EKCPHFOGJPA)
        {
            Metamorphosis.Logger.LogMessage($"JKIFEGOFNDP: {EKCPHFOGJPA.FMAAJCIEMEH}");
        }*/
    }
}