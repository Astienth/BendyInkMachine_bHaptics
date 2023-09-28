using System;
using System.Runtime.CompilerServices;
using BendyVR_5.Player.Patches;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBhapticsTactsuit;
using TMG.Controls;
using UnityEngine;

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
        public static bool handSet = false;
        public static string handString = "R";
        public static bool isGrounded = true;

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

        public static void setHand()
        {
            if(!handSet)
            {
                handString = (BendyVR_5.Settings.VrSettings.LeftHandedMode.Value) ? "L" : "R";
                handSet = true;
            }
        }

        public static void PlayJumpScareLight()
        {
            tactsuitVr.PlaybackHaptics("JumpScareLight_Vest");
            tactsuitVr.PlaybackHaptics("JumpScare_Left_Arms", true, 0.4f);
            tactsuitVr.PlaybackHaptics("JumpScare_Right_Arms", true, 0.4f);
        }
        public static void PlayJumpScareStrong()
        {
            tactsuitVr.PlaybackHaptics("JumpScare_Vest");
            tactsuitVr.PlaybackHaptics("JumpScare_Left_Arms");
            tactsuitVr.PlaybackHaptics("JumpScare_Right_Arms");
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

    [HarmonyPatch(typeof(PlayerController), "FixedUpdate")]
    public class bhaptics_OnJumpLand
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            bool isGrounded = Traverse.Create(__instance).Field("m_CharacterController").GetValue<CharacterController>().isGrounded;
            if (!Plugin.isGrounded && isGrounded)
            {
                Plugin.tactsuitVr.PlaybackHaptics("LandAfterJump");
            }
            Plugin.isGrounded = isGrounded;
        }
    }

    [HarmonyPatch(typeof(PlayerController), "EquipWeapon")]
    public class bhaptics_OnEquipWeapon
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            Plugin.setHand();
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_" + Plugin.handString, true, 0.5f, 1f);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_" + Plugin.handString, true, 0.5f, 1f);
        }
    }

    [HarmonyPatch(typeof(PlayerController), "UnEquipWeapon")]
    public class bhaptics_OnUnEquipWeapon
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_" + Plugin.handString, true, 0.5f, 1f);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_" + Plugin.handString, true, 0.5f, 1f);
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
    
    [HarmonyPatch(typeof(HoldableItemPatches), "VRReload")]
    public class bhaptics_OnGunWeaponReloaded
    {
        [HarmonyPostfix]
        public static void Postfix(HoldableItemPatches __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_" + Plugin.handString, true, 0.2f, 1f);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_" + Plugin.handString, true, 0.2f, 1f);
        }
    }
    
    [HarmonyPatch(typeof(HoldableItemPatches), "GunAttack")]
    public class bhaptics_OnGunAttack
    {
        [HarmonyPostfix]
        public static void Postfix(HoldableItemPatches __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_" + Plugin.handString);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_" + Plugin.handString);
        }
    }

    [HarmonyPatch(typeof(HoldableItemPatches), "RevisedMeleeAttack")]
    public class bhaptics_OnRevisedMeleeAttack
    {
        [HarmonyPostfix]
        public static void Postfix(HoldableItemPatches __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_" + Plugin.handString);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_" + Plugin.handString);
        }
    }

    [HarmonyPatch(typeof(ThrowWeapon), "OnAttack")]
    public class bhaptics_OnThrowWeapon
    {
        [HarmonyPostfix]
        public static void Postfix(ThrowWeapon __instance)
        {
            Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_" + Plugin.handString);
            Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_" + Plugin.handString);
        }
    }

    #endregion

    #region CH1 JUMP SCARES

    [HarmonyPatch(typeof(CH1InkMachineRevealController), "HandleLeverOnComplete")]
    public class bhaptics_OnHandleLeverOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StartRumble(0.2f);
        }
    }
    [HarmonyPatch(typeof(CH1InkMachineRevealController), "OnMachineRevealComplete")]
    public class bhaptics_OnOnMachineRevealComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopRumble();
        }
    }
    [HarmonyPatch(typeof(CH1JumpScareController), "DOPlankScare")]
    public class bhaptics_OnPlanckJumpScare
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.PlayHapticsWithDelay("JumpScareLight_Vest", 400);
        }
    }
    [HarmonyPatch(typeof(CH1JumpScareController), "HandleJumpScareTriggerOnEnter")]
    public class bhaptics_OnHandleJumpScareTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }    
    [HarmonyPatch(typeof(CH1JumpScareController), "DOBendyDoor")]
    public class bhaptics_OnDOBendyDoor
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }    
    [HarmonyPatch(typeof(CH1TheatreController), "HandleTheatreEnterOnEnter")]
    public class bhaptics_OnHandleTheatreEnterOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }    
    [HarmonyPatch(typeof(CH1JumpScareController), "HandleBendyDoorOnInteract")]
    public class bhaptics_OnHandleBendyDoorOnInteract
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    [HarmonyPatch(typeof(CH1BendyFinaleController), "ActualActivate")]
    public class bhaptics_OnActualActivate
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
            Plugin.tactsuitVr.PlaybackHaptics("HeartBeatFast");
            Plugin.tactsuitVr.StartHeartBeat(true);
        }
    }
    [HarmonyPatch(typeof(CH1BendyFinaleController), "ShakeCamera")]
    public class bhaptics_OnShakeCamera
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    [HarmonyPatch(typeof(CH1BendyFinaleController), "BreakFloor")]
    public class bhaptics_OnBreakFloor
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
        }
    }
    [HarmonyPatch(typeof(CH1BendyFinaleController), "HandleExitEventTriggerOnEnter")]
    public class bhaptics_OnHandleExitEventTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }
    [HarmonyPatch(typeof(CH1ClosingSequenceController), "HandleFinaleEventTriggerOnEnter")]
    public class bhaptics_OnHandleFinaleEventTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
            Plugin.tactsuitVr.StartHeartBeat(true);
        }
    }
    [HarmonyPatch(typeof(CH1ClosingSequenceController), "SequenceOnComplete")]
    public class bhaptics_OnSequenceOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    #endregion

    #region CH2 JUMP SCARES

    [HarmonyPatch(typeof(CH2RitualRoomController), "HandleScareTriggerOnEnter")]
    public class bhaptics_OnHandleScareTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH2MusicDepartmentController), "HandleInitialSearcherOnActivate")]
    public class bhaptics_OnHandleInitialSearcherOnActivate
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH2SanctuaryController), "HandleJumpscareTriggerOnEnter")]
    public class bhaptics_OnHandleJumpscareTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH2SearcherBattleController), "HandleFinaleTriggerOnEnter")]
    public class bhaptics_OnHandleFinaleTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH2SammyOfficeController), "HandleKnockoutEventTriggerOnEnter")]
    public class bhaptics_OnHandleKnockoutEventTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
            Plugin.tactsuitVr.PlaybackHaptics("ShotVisor", true, 2f);
            Plugin.tactsuitVr.StartHeartBeat();
        }
    }
    
    [HarmonyPatch(typeof(CH2SammyOfficeController), "OnDisposed")]
    public class bhaptics_OnDisposed
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopAllHapticFeedback();
        }
    }
        
    [HarmonyPatch(typeof(CH2SacrificeController), "HandleSpeakersOnComplete")]
    public class bhaptics_OnHandleSpeakersOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StartHeartBeat();
        }
    }
    
    [HarmonyPatch(typeof(CH2SacrificeController), "HandleSpeakerMonologueOnComplete")]
    public class bhaptics_OnHandleSpeakerMonologueOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }
    
    [HarmonyPatch(typeof(CH2BendyChaseController), "BendyReveal")]
    public class bhaptics_OnBendyReveal
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
            Plugin.tactsuitVr.StartHeartBeat(true);
        }
    }
    
    [HarmonyPatch(typeof(CH2BendyChaseController), "HandleDoorCloseOnComplete")]
    public class bhaptics_OnHandleDoorCloseOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    [HarmonyPatch(typeof(CH2RecordingStudioController), "HandlePianoJumpscareOnEnter")]
    public class bhaptics_OnHandlePianoJumpscareOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    #endregion
}

