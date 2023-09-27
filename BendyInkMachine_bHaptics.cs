using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using MyBhapticsTactsuit;

namespace BendyInkMachine_bHaptics
{
    [BepInPlugin("org.bepinex.plugins.BendyInkMachine_bHaptics", "BendyInkMachine_bHaptics integration", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
#pragma warning disable CS0109 // Remove unnecessary warning
        internal static new ManualLogSource Log;
#pragma warning restore CS0109
        public static TactsuitVR tactsuitVr;
        public static bool startedHeart = false;

        private void Awake()
        {
            // Make my own logger so it can be accessed from the Tactsuit class
            Log = base.Logger;
            // Plugin startup logic
            Logger.LogMessage("Plugin BendyInkMachine_bHaptics is loaded!");
            tactsuitVr = new TactsuitVR();
            // one startup heartbeat so you know the vest works correctly
            tactsuitVr.PlaybackHaptics("HeartBeat");
            // patch all functions
            var harmony = new Harmony("bhaptics.patch.BendyInkMachine_bHaptics");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(EntityPlayerLocal), "LateUpdate")]
    public class bhaptics_OnUpdate
    {
        [HarmonyPostfix]
        public static void Postfix(EntityPlayerLocal __instance)
        {
            Plugin.currentHealth = Traverse.Create(__instance).Field("oldHealth").GetValue<float>();
        }
    }
}

