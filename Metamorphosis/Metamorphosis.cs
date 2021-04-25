using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using System.Net;


namespace Metamorphosis
{
    [BepInPlugin("org.bepinex.plugins.Metamorphosis", "Metamorphosis", "1.1.0.0")]
    [BepInProcess("Among Us.exe")]
    public class Metamorphosis : BasePlugin
    {
        public static BepInEx.Logging.ManualLogSource Logger;
        private readonly Harmony harmony;

        public Metamorphosis()
        {
            Logger = Log;
            this.harmony = new Harmony("Metamorphosis");
        }
        
        public override void Load()
        {
            Logger.LogMessage("Metamorphosis loaded");

            this.harmony.PatchAll();
        }
    }
}
