using HarmonyLib;
using System;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

namespace Metamorphosis
{
    [HarmonyPatch(typeof(GameOptionsMenu))]
    public static class GameOptionMenuPatch
    {
        public static NumberOption MorphDuration;
        public static NumberOption MorphCooldown;
        public static GameOptionsMenu Instance;
        public static int CurrentCount = 0;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameOptionsMenu.Start))]
        public static void Postfix1(GameOptionsMenu __instance)
        {
            Instance = __instance;
            CustomPlayerMenuPatch.AddOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameOptionsMenu.Update))]
        public static void Postfix2(GameOptionsMenu __instance)
        {
            __instance.GetComponentInParent<Scroller>().YBounds.max = (__instance.MCAHCPOHNFI.Count - 7) * 0.5F + 0.13F;
            OptionBehaviour option = __instance.MCAHCPOHNFI[CurrentCount - 1];

            if (MorphDuration != null & MorphCooldown != null)
            {
                MorphDuration.transform.position = option.transform.position - new Vector3(0, 0.5f, 0);
                MorphCooldown.transform.position = option.transform.position - new Vector3(0, 1f, 0);
            }

        }
    }

    [HarmonyPatch(typeof(NumberOption))]
    public static class NumberOptionPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NumberOption.Increase))]
        public static bool Prefix1(NumberOption __instance)
        {
            if (__instance.TitleText.text == "Morph Duration")
            {
                CustomGameOptions.MorphDuration = Math.Min(CustomGameOptions.MorphDuration + 2.5f, 60.0f);
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                GameOptionMenuPatch.MorphDuration.LCDAKOCANPH = CustomGameOptions.MorphDuration;
                GameOptionMenuPatch.MorphDuration.Value = CustomGameOptions.MorphDuration;
                GameOptionMenuPatch.MorphDuration.ValueText.text = CustomGameOptions.MorphDuration.ToString();
                return false;
            }
            else if (__instance.TitleText.text == "Morph Cooldown")
            {
                CustomGameOptions.MorphCooldown = Math.Min(CustomGameOptions.MorphCooldown + 2.5f, 60.0f);
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                GameOptionMenuPatch.MorphCooldown.LCDAKOCANPH = CustomGameOptions.MorphCooldown;
                GameOptionMenuPatch.MorphCooldown.Value = CustomGameOptions.MorphCooldown;
                GameOptionMenuPatch.MorphCooldown.ValueText.text = CustomGameOptions.MorphCooldown.ToString();
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(NumberOption.Decrease))]
        public static bool Prefix2(NumberOption __instance)
        {

            if (__instance.TitleText.text == "Morph Duration")
            {
                CustomGameOptions.MorphDuration = Math.Max(CustomGameOptions.MorphDuration - 2.5f, 10.0f);
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                GameOptionMenuPatch.MorphDuration.LCDAKOCANPH = CustomGameOptions.MorphDuration;
                GameOptionMenuPatch.MorphDuration.Value = CustomGameOptions.MorphDuration;
                GameOptionMenuPatch.MorphDuration.ValueText.text = CustomGameOptions.MorphDuration.ToString();
                return false;
            }
            else if (__instance.TitleText.text == "Morph Cooldown")
            {
                CustomGameOptions.MorphCooldown = Math.Max(CustomGameOptions.MorphCooldown - 2.5f, 10.0f);
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                GameOptionMenuPatch.MorphCooldown.LCDAKOCANPH = CustomGameOptions.MorphCooldown;
                GameOptionMenuPatch.MorphCooldown.Value = CustomGameOptions.MorphCooldown;
                GameOptionMenuPatch.MorphCooldown.ValueText.text = CustomGameOptions.MorphCooldown.ToString();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CustomPlayerMenu))]
    public class CustomPlayerMenuPatch
    {
        public static void DeleteOptions(bool destroy)
        {
            if (GameOptionMenuPatch.MorphDuration != null && GameOptionMenuPatch.MorphCooldown != null)
            {
                GameOptionMenuPatch.MorphDuration.gameObject.SetActive(false);
                GameOptionMenuPatch.MorphCooldown.gameObject.SetActive(false);
                if (destroy) 
                { 
                  GameObject.Destroy(GameOptionMenuPatch.MorphDuration);
                  GameObject.Destroy(GameOptionMenuPatch.MorphCooldown);
                  GameOptionMenuPatch.MorphDuration = null;
                  GameOptionMenuPatch.MorphCooldown = null;
                }
            }

        }
        public static void AddOptions()
        {
            if (GameOptionMenuPatch.MorphDuration == null | GameOptionMenuPatch.MorphCooldown == null)
            {
                var killcd = GameObject.FindObjectsOfType<NumberOption>().ToList().Where(x => x.TitleText.text == "Kill Cooldown").First();
                GameOptionMenuPatch.MorphDuration = GameObject.Instantiate(killcd);
                GameOptionMenuPatch.MorphCooldown = GameObject.Instantiate(killcd);

                OptionBehaviour[] options = new OptionBehaviour[GameOptionMenuPatch.Instance.MCAHCPOHNFI.Count + 2];
                GameOptionMenuPatch.Instance.MCAHCPOHNFI.ToArray().CopyTo(options, 0);
                options[options.Length - 2] = GameOptionMenuPatch.MorphDuration;
                options[options.Length - 1] = GameOptionMenuPatch.MorphCooldown;
                GameOptionMenuPatch.Instance.MCAHCPOHNFI = new Il2CppReferenceArray<OptionBehaviour>(options);

                GameOptionMenuPatch.CurrentCount = options.Length - 2;
            }
            else
            {
                GameOptionMenuPatch.MorphDuration.gameObject.SetActive(true);
                GameOptionMenuPatch.MorphCooldown.gameObject.SetActive(true);
            }

            GameOptionMenuPatch.MorphDuration.TitleText.text = "Morph Duration";
            GameOptionMenuPatch.MorphDuration.Value = CustomGameOptions.MorphDuration;
            GameOptionMenuPatch.MorphDuration.ValueText.text = CustomGameOptions.MorphDuration.ToString();

            GameOptionMenuPatch.MorphCooldown.TitleText.text = "Morph Cooldown";
            GameOptionMenuPatch.MorphCooldown.Value = CustomGameOptions.MorphCooldown;
            GameOptionMenuPatch.MorphCooldown.ValueText.text = CustomGameOptions.MorphCooldown.ToString();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CustomPlayerMenu.Close))]
        public static void Postfix1(CustomPlayerMenu __instance, bool DOGNCNIKKIP)
        {
            DeleteOptions(true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CustomPlayerMenu.OpenTab))]
        public static void Prefix1(GameObject NCDEAICDCNC)
        {
            
            if (NCDEAICDCNC.name == "GameGroup" && GameOptionMenuPatch.Instance != null)
            {
                AddOptions();
            }
            else 
            {
                DeleteOptions(false);
            }
            
        }
    }
}