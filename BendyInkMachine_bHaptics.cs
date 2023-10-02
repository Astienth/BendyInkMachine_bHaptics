using System;
using System.Threading;
using System.Threading.Tasks;
using BendyVR_5.Player.Patches;
using BepInEx;
using BepInEx.Logging;
using DG.Tweening;
using HarmonyLib;
using MyBhapticsTactsuit;
using S13Audio;
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
        public static bool handSet = false;
        public static string handString = "R";
        public static bool isGrounded = true;
        public delegate void MyMethod();

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
            Plugin.tactsuitVr.PlaybackHaptics("JumpScareLight_Vest");
            Plugin.tactsuitVr.PlaybackHaptics("JumpScare_Left_Arms", true, 0.4f);
            Plugin.tactsuitVr.PlaybackHaptics("JumpScare_Right_Arms", true, 0.4f);
            Plugin.tactsuitVr.PlaybackHaptics("ShotVisor", true, 0.4f);
        }
        public static void PlayJumpScareStrong()
        {
            Plugin.tactsuitVr.PlaybackHaptics("JumpScare_Vest");
            Plugin.tactsuitVr.PlaybackHaptics("JumpScare_Left_Arms");
            Plugin.tactsuitVr.PlaybackHaptics("JumpScare_Right_Arms");
            Plugin.tactsuitVr.PlaybackHaptics("ShotVisor");
            Plugin.tactsuitVr.PlayHapticsWithDelay("HeartBeatFast", 400);
            Plugin.tactsuitVr.PlayHapticsWithDelay("HeartBeatFast", 1400);
        }

        public static void RumbleOnce(float rumbleIntensity = 1.0f, bool withDelay = false, int delay = 0)
        {
            if(withDelay)
            {
                Thread thread = new Thread(() =>
                {
                    Thread.Sleep(delay);
                    Plugin.tactsuitVr.PlaybackHaptics("Rumble_Head", true, rumbleIntensity);
                    Plugin.tactsuitVr.PlaybackHaptics("Rumble_Left_Arms", true, rumbleIntensity);
                    Plugin.tactsuitVr.PlaybackHaptics("Rumble_Right_Arms", true, rumbleIntensity);
                    Plugin.tactsuitVr.PlaybackHaptics("Rumble_Vest", true, rumbleIntensity);
                });
                thread.Start();
            }
            else
            {
                Plugin.tactsuitVr.PlaybackHaptics("Rumble_Head", true, rumbleIntensity);
                Plugin.tactsuitVr.PlaybackHaptics("Rumble_Left_Arms", true, rumbleIntensity);
                Plugin.tactsuitVr.PlaybackHaptics("Rumble_Right_Arms", true, rumbleIntensity);
                Plugin.tactsuitVr.PlaybackHaptics("Rumble_Vest", true, rumbleIntensity);
            }
        }

        public static void RunFunctionWithDelay(MyMethod method, int delay)
        {
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(delay);
                method.Invoke();
            });
            thread.Start();
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
            Plugin.tactsuitVr.StopThreads();
        }
    }
    
    [HarmonyPatch(typeof(PlayerController), "PlayRespawnEffects")]
    public class bhaptics_OnPlayRespawnEffects
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            Plugin.tactsuitVr.StopThreads();
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
            Plugin.tactsuitVr.StopThreads();
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
            Plugin.tactsuitVr.StopThreads();
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
    
    [HarmonyPatch(typeof(CH2ClosingSequenceController), "HandleFinalTriggerOnEnter")]
    public class bhaptics_OnHandleFinalTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    #endregion

    #region CH3 JUMP SCARES

    [HarmonyPatch(typeof(CH3AliceRevealController), "HandleAliceIntroMusicOnComplete")]
    public class bhaptics_OnHandleAliceIntroMusicOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
            Plugin.RunFunctionWithDelay(Plugin.PlayJumpScareStrong, 3500);
        }
    }

    [HarmonyPatch(typeof(CH3BorisJumpscareController), "HandleJumpscareTriggerOnEnter")]
    public class bhaptics_OnHandleJumpscareTriggerOnEnterBoris
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH3LiftEntranceController), "HandlePiperTriggerOnEnter")]
    public class bhaptics_OnHandlePiperTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
        }
    }
    
    [HarmonyPatch(typeof(CH3DarkHallwayController), "HandleDuctCrawlingTriggerOnEnter")]
    public class bhaptics_OnHandleDuctCrawlingTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RunFunctionWithDelay(Plugin.PlayJumpScareLight, 600);
        }
    }
    
    [HarmonyPatch(typeof(CH3AliceLairController), "HandleAliceControllerOnPressed")]
    public class bhaptics_OnClearLair
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(1.0f, true, 200);
            Plugin.RumbleOnce(1.0f, true, 400);
        }
    }
    
    [HarmonyPatch(typeof(CH3InkPressurePuzzle), "HandleCollectableOnInteracted")]
    public class bhaptics_OnHandleCollectableOnInteracted
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.PlaybackHaptics("Rumble_Left_Arms");
            Plugin.tactsuitVr.PlayHapticsWithDelay("RecoilVest_L", 100);
        }
    }

    [HarmonyPatch(typeof(CH3BendyController), "HandleBendyOnSpotted")]
    public class bhaptics_OnHandleBendyOnSpotted
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
            Plugin.tactsuitVr.StartHeartBeat(true);
        }
    }
    
    [HarmonyPatch(typeof(CH3BendyController), "HandleBendyOnTrackingLost")]
    public class bhaptics_OnHandleBendyOnTrackingLost
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.Log.LogWarning("STOP CHASE 1");
            Plugin.tactsuitVr.StopHeartBeat();
            Plugin.tactsuitVr.PlaybackHaptics("HeartBeat");
            Plugin.tactsuitVr.PlayHapticsWithDelay("HeartBeat", 1000);
        }
    }
    
    [HarmonyPatch(typeof(CH3BendyController), "HandleBendyDespawnerOnEnter")]
    public class bhaptics_OnHandleBendyDespawnerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.Log.LogWarning("STOP CHASE 2");
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }
    
    [HarmonyPatch(typeof(CH3BendyController), "StopChaseMusic")]
    public class bhaptics_OnStopChaseMusic
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.Log.LogWarning("STOP CHASE 3");
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    [HarmonyPatch(typeof(CH3BendyController), "HandleBendyOnWaypointComplete")]
    public class bhaptics_OnHandleBendyOnWaypointComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.Log.LogWarning("STOP CHASE 4");
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    [HarmonyPatch(typeof(CH3ProjectionistTaskController), "HandleFirstHeartOnInteracted")]
    public class bhaptics_OnHandleFirstHeartOnInteracted
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
        }
    }

    [HarmonyPatch(typeof(CH3ProjectionistTaskController), "HandlePlayerOnSpotted")]
    public class bhaptics_OnHandlePlayerOnSpotted
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
            Plugin.tactsuitVr.StartHeartBeat(true);
        }
    }
    
    [HarmonyPatch(typeof(CH3ProjectionistTaskController), "HandleProjectionistOnRetreat")]
    public class bhaptics_OnHandleProjectionistOnRetreat
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
            Plugin.tactsuitVr.PlaybackHaptics("HeartBeat");
            Plugin.tactsuitVr.PlayHapticsWithDelay("HeartBeat", 1000);
        }
    }
    
    [HarmonyPatch(typeof(CH3ProjectionistTaskController), "HandleProjectionistOnDeath")]
    public class bhaptics_OnHandleProjectionistOnDeath
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
            Plugin.tactsuitVr.PlaybackHaptics("HeartBeat");
            Plugin.tactsuitVr.PlayHapticsWithDelay("HeartBeat", 1000);
        }
    }


    // LIFT MOVEMENTS RUMBLES

    [HarmonyPatch(typeof(CH3LiftController), "DOLiftMove")]
    public class bhaptics_OnDOLiftMove
    {
        [HarmonyPostfix]
        public static void Postfix(CH3LiftController __instance)
        {
            if(!__instance.IsInCart)
            {
                return;
            }
            Plugin.tactsuitVr.StartRumble(0.2f);
        }
    }

    [HarmonyPatch(typeof(CH3LiftController), "HandleMoveOnComplete")]
    public class bhaptics_OnHandleMoveOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix(CH3LiftController __instance)
        {
            if (!__instance.IsInCart)
            {
                return;
            }

            Plugin.tactsuitVr.StopRumble();
        }
    }
        
    [HarmonyPatch(typeof(CH3ClosingSequenceController), "DOLiftDrop")]
    public class bhaptics_OnDOLiftDrop
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopRumble();
            Plugin.tactsuitVr.StartRumble(1.5f);
        }
    }

    [HarmonyPatch(typeof(CH3ClosingSequenceController), "DOLiftAscend")]
    public class bhaptics_OnDOLiftAscend
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StartRumble(0.2f);
        }
    }
    
    [HarmonyPatch(typeof(CH3ClosingSequenceController), "LiftDropOnComplete")]
    public class bhaptics_OnLiftDropOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopRumble();
        }
    }
    
    [HarmonyPatch(typeof(CH3ClosingSequenceController), "HandleAliceOnWaypointComplete")]
    public class bhaptics_OnHandleAliceOnWaypointComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH3ClosingSequenceController), "DOBorisPull")]
    public class bhaptics_OnDOBorisPull
    {
        [HarmonyPostfix]
        public static void Postfix(Sequence __result)
        {
            __result.OnStart(() => { Plugin.PlayJumpScareStrong(); });            
        }
    }

    #endregion

    #region CH4 JUMPSCARES
    
    [HarmonyPatch(typeof(CH4AccountingController), "HandleVisionEffectOnStart")]
    public class bhaptics_OnHandleVisionEffectOnStart
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
        }
    }
    [HarmonyPatch(typeof(CH4AccountingController), "HandleVisionEffectOnStop")]
    public class bhaptics_OnHandleVisionEffectOnStop
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH4AbyssController), "PipeLeverOnComplete")]
    public class bhaptics_OnPipeLeverOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StartRumble(0.1f);
        }
    }
    [HarmonyPatch(typeof(CH4AbyssController), "PipeRiseOnComplete")]
    public class bhaptics_OnPipeRiseOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopRumble();
        }
    }
    
    [HarmonyPatch(typeof(CH4BridgeMachine), "HandleCartOnBreakdown")]
    public class bhaptics_OnHandleCartOnBreakdown
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
            Plugin.tactsuitVr.StartHeartBeat();
            Plugin.RunFunctionWithDelay(Plugin.tactsuitVr.StopHeartBeat, 15000);
        }
    }    

    [HarmonyPatch(typeof(CH4StairwellController), "HandleVisionEffectOnStart")]
    public class bhaptics_OnHandleVisionEffectOnStartCH4
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
            Plugin.tactsuitVr.StartHeartBeat();
        }
    }

    [HarmonyPatch(typeof(CH4StairwellController), "HandleVisionEffectOnStop")]
    public class bhaptics_OnHandleVisionEffectOnStopCH4
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }
    
    [HarmonyPatch(typeof(CH4StairwellController), "HandleOnDoorOpen")]
    public class bhaptics_OnHandleOnDoorOpen
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(CH4BendyVent), "Activate")]
    public class bhaptics_OnActivateBendyVent
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
            Plugin.tactsuitVr.StartHeartBeat(true);
            Plugin.RunFunctionWithDelay(Plugin.tactsuitVr.StopHeartBeat, 8000);
        }
    }
    
    [HarmonyPatch(typeof(CH4WarehouseController), "HandleEntranceTriggerOnEnter")]
    public class bhaptics_OnHandleEntranceTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }

    // Bertrum battle system
    
    [HarmonyPatch(typeof(CH4BertrumController), "DestroyWorkbench")]
    public class bhaptics_OnDestroyWorkbench
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(1f);
        }
    }

    [HarmonyPatch(typeof(CH4BertrumController), "DOSpinSequence")]
    public class bhaptics_OnDOSpinSequence
    {
        public static bool isActive = true;
        [HarmonyPostfix]
        public static void Postfix(CH4BertrumController __instance)
        {
            if(isActive)
            {
                Plugin.tactsuitVr.StartRumble(0.1f);
            }
        }
    }
    [HarmonyPatch(typeof(CH4BertrumController), "DOSpinInteruption")]
    public class bhaptics_OnDOSpinInteruption
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopRumble();
        }
    }
    [HarmonyPatch(typeof(CH4BertrumController), "DOAttack")]
    public class bhaptics_OnDOAttack
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(1f, true, 1000);
        }
    }
    [HarmonyPatch(typeof(CH4BertrumController), "DODeath")]
    public class bhaptics_OnDODeath
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            bhaptics_OnDOSpinSequence.isActive = false;
            Plugin.tactsuitVr.StopRumble();
            Plugin.RumbleOnce(0.5f, true, 2000);
            Plugin.RumbleOnce(0.5f, true, 3500);
            Plugin.RumbleOnce(0.5f, true, 5000);
            Plugin.RumbleOnce(0.5f, true, 6500);
            Plugin.RumbleOnce(1f, true, 8000);
            Plugin.RumbleOnce(1f, true, 9000);
            Plugin.RumbleOnce(1f, true, 10000);
            Plugin.RumbleOnce(0.2f, true, 11500);
        }
    }

    // Bertrum battle system END

    [HarmonyPatch(typeof(CH4MaintenanceController), "HandleProjectionstOnSpotted")]
    public class bhaptics_HandleProjectionstOnSpotted
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
            Plugin.tactsuitVr.StartHeartBeat(true);
        }
    }

    [HarmonyPatch(typeof(CH4MaintenanceController), "HandleProjectionistOnRetreat")]
    public class bhaptics_OnHandleProjectionistOnRetreatCH4
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
            Plugin.tactsuitVr.PlaybackHaptics("HeartBeat");
            Plugin.tactsuitVr.PlayHapticsWithDelay("HeartBeat", 1000);
        }
    }

    [HarmonyPatch(typeof(CH4MaintenanceController), "HandleProjectionistOnDeath")]
    public class bhaptics_OnHandleProjectionistOnDeathCH4
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
            Plugin.tactsuitVr.PlaybackHaptics("HeartBeat");
            Plugin.tactsuitVr.PlayHapticsWithDelay("HeartBeat", 1000);
        }
    }

    [HarmonyPatch(typeof(CH4ProjectionistBendyFight), "Activate")]
    public class bhaptics_OnHandleCH4ProjectionistBendyFight
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StartHeartBeat();
            Plugin.RunFunctionWithDelay(bhaptics_OnHandleCH4ProjectionistBendyFight.battleEvents, 7000);
            Plugin.RunFunctionWithDelay(Plugin.PlayJumpScareLight, 7000);
            Plugin.RunFunctionWithDelay(Plugin.PlayJumpScareLight, 16000);
        }

        public static void battleEvents()
        {
            Plugin.tactsuitVr.StopHeartBeat();
            Plugin.tactsuitVr.StartHeartBeat(true);
        }
    }
    
    [HarmonyPatch(typeof(CH4MaintenanceController), "HandleBendyProjFightOnComplete")]
    public class bhaptics_OnHandleBendyProjFightOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }
    
    [HarmonyPatch(typeof(CH4HauntedHouseController), "HandleCartOnEnter")]
    public class bhaptics_OnHandleCartOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StartRumble(0.2f);
        }
    }
    [HarmonyPatch(typeof(CH4HauntedHouseController), "CartRideOnComplete")]
    public class bhaptics_OnCartRideOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopRumble();
        }
    }
    
    [HarmonyPatch(typeof(CH4HauntedHouseController), "HandlePopupTriggerOnEnter")]
    public class bhaptics_OnHandlePopupTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }

    [HarmonyPatch(typeof(BruteBorisAnimationEvents), "RevealGrabCart")]
    public class bhaptics_OnRevealGrabCart
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }
    
    [HarmonyPatch(typeof(BruteBorisAi), "PickupCart")]
    public class bhaptics_OnPickupCart
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(0.2f);
            Plugin.RumbleOnce(0.2f, true, 10000);
            Plugin.RumbleOnce(0.2f, true, 12000);
            Plugin.RumbleOnce(0.5f, true, 14000);
            Plugin.RunFunctionWithDelay(Plugin.PlayJumpScareStrong, 15500);
        }
    }
    
    [HarmonyPatch(typeof(CH4ClosingSequenceController), "AliceHit")]
    public class bhaptics_OnAliceHit
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
        }
    }

    #endregion

    #region CH5 JUMPSCARES
    
    [HarmonyPatch(typeof(CH5Administration), "HandleInkMakerOnComplete")]
    public class bhaptics_OnHandleInkMakerOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix(CH5Administration __instance)
        {
            if(!Traverse.Create(__instance).Field("m_HasUsedMachine").GetValue<bool>())
            {
                Plugin.PlayJumpScareLight();
            }
        }
    }

    [HarmonyPatch(typeof(CH5LostHarborSammyController), "ActivateSammy")]
    public class bhaptics_OnActivateSammy
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareStrong();
        }
    }
    
    [HarmonyPatch(typeof(CH5BendyScare), "SpawnBendy")]
    public class bhaptics_OnSpawnBendy
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.PlayJumpScareLight();
            Plugin.tactsuitVr.StartHeartBeat();
        }
    }    

    [HarmonyPatch(typeof(CH5BendyScare), "HandleBendyOnWaypointComplete")]
    public class bhaptics_OnHandleBendyOnWaypointCompleteCH5
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    [HarmonyPatch(typeof(S13AudioManager), "PlayAudio")]
    public class bhaptics_OnHandleBendyThroneAppears
    {
        [HarmonyPostfix]
        public static void Postfix(S13AudioManager __instance, string soundId)
        {
            if(soundId == "sfx_beast_reveal_anim")
            {
                Plugin.PlayJumpScareStrong();
                Plugin.tactsuitVr.StartHeartBeat();
            }
        }
    }
    
    [HarmonyPatch(typeof(CH5ThroneRoom), "RevealOnComplete")]
    public class bhaptics_OnRevealOnComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(1f);
            Plugin.tactsuitVr.StartHeartBeat();
        }
    }

    [HarmonyPatch(typeof(CH5BendyArena), "HandleMazeExitTriggerOnEnter")]
    public class bhaptics_OnHandleMazeExitTriggerOnEnter
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    [HarmonyPatch(typeof(CH5BendyArena), "HandleOnStartUpComplete")]
    public class bhaptics_OnHandleOnStartUpComplete
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StartHeartBeat();
        }
    }
    
    [HarmonyPatch(typeof(CH5BendyArena), "HandlePillarOnHit")]
    public class bhaptics_OnHandlePillarOnHit
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(1f);
        }
    }
    
    [HarmonyPatch(typeof(CH5BendyArena), "HandlePowerStationOnPowerShutDown")]
    public class bhaptics_OnHandlePowerStationOnPowerShutDown
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.StopHeartBeat();
        }
    }

    [HarmonyPatch(typeof(CH5TheEnd), "TheEnd1")]
    public class bhaptics_OnTheEnd1
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(0.2f);
        }
    }
    [HarmonyPatch(typeof(CH5TheEnd), "TheEnd2")]
    public class bhaptics_OnTheEnd2
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(0.2f);
        }
    }
    
    [HarmonyPatch(typeof(CH5TheEnd), "TheEndRest")]
    public class bhaptics_OnTheEndRest
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(0.5f);
        }
    }

    [HarmonyPatch(typeof(CH5TheEnd), "THE_END")]
    public class bhaptics_OnTHE_END
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.RumbleOnce(0.8f);
            Plugin.RumbleOnce(1f, true, 1000);
        }
    }

    #endregion
}

