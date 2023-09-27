using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBhapticsTactsuit;
using TMG.Controls;

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

    #region Player Physical Events

    [HarmonyPatch(typeof(PlayerController), "Die")]
    public class bhaptics_OnDeath
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("Death");
            Plugin.tactsuitVr.StopAllHapticFeedback();
        }
    }

    [HarmonyPatch(typeof(CharacterFootsteps), "PlayJumpAudio")]
    public class bhaptics_OnJump
    {
        [HarmonyPostfix]
        public static void Postfix(CharacterFootsteps __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("OnJump");
        }
    }

    [HarmonyPatch(typeof(CharacterFootsteps), "PlayLandAudio")]
    public class bhaptics_OnJumpLand
    {
        [HarmonyPostfix]
        public static void Postfix(CharacterFootsteps __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("LandAfterJump");
        }
    }

    [HarmonyPatch(typeof(PlayerController), "EquipWeapon")]
    public class bhaptics_OnEquipWeapon
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_R", true, 0.5f, 1f);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_R", true, 0.5f, 1f);
        }
    }

    [HarmonyPatch(typeof(PlayerController), "UnEquipWeapon")]
    public class bhaptics_OnUnEquipWeapon
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_R", true, 0.5f, 1f);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_R", true, 0.5f, 1f);
        }
    }    

    [HarmonyPatch(typeof(GameManager), "Heal")]
    public class bhaptics_OnHeal
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.PlaybackHaptics("Heal");
        }
    }

    [HarmonyPatch(typeof(HurtBordersController), "ShowBorder")]
    public class bhaptics_OnShowBorderAndHitDamage
    {
        [HarmonyPostfix]
        public static void Postfix(HurtBordersController __instance)
        {
            if (Traverse.Create(__instance).Field("m_HitCount").GetValue<int>() <
                Traverse.Create(__instance).Field("m_HitMax").GetValue<int>())
            {
                Plugin.tactsuitVr.StartHeartBeat();
                Plugin.tactsuitVr.PlaybackHaptics("Impact");
                Plugin.tactsuitVr.PlaybackHaptics("ShotVisor");
            }
        }
    }

    [HarmonyPatch(typeof(HurtBordersController), "HideBorder")]
    public class bhaptics_OnHideBorder
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    [HarmonyPatch(typeof(BaseWeapon), "Attack")]
    public class bhaptics_OnAttackWeapon
    {
        [HarmonyPostfix]
        public static void Postfix(BaseWeapon __instance)
        {
            if (!Traverse.Create(__instance).Field("m_CanAttack").GetValue<bool>() ||
                !Traverse.Create(__instance).Field("m_IsEquipped").GetValue<bool>() ||
                GameManager.Instance.Player.isLocked ||
                GameManager.Instance.isPaused ||
                !(!Traverse.Create(__instance).Field("m_IsHoldToAttack").GetValue<bool>() ? PlayerInput.Attack() : PlayerInput.AttackHold()))
            {
                return;
            }
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_R");
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_R");
        }
    }

    #endregion

    #region JumpScare Events

    #endregion
}

