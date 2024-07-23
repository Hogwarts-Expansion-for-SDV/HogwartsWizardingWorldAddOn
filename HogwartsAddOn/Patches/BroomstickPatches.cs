using HarmonyLib;
using HogwartsAddOn.Broomsticks;
using HogwartsAddOn.Settings;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;

namespace HogwartsAddOn.Patches
{
    public static class BroomstickPatches
    {
        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Horse), name: nameof(Horse.checkAction)),
                prefix: new HarmonyMethod(typeof(BroomstickPatches), nameof(CheckAction_Prefix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Horse), name: nameof(Horse.PerformDefaultHorseFootstep)),
                prefix: new HarmonyMethod(typeof(BroomstickPatches), nameof(PerformDefaultHorseFootstep_Prefix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Horse), name: nameof(Horse.dayUpdate)),
                postfix: new HarmonyMethod(typeof(BroomstickPatches), nameof(DayUpdate_Postfix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Horse), name: nameof(Horse.ChooseAppearance)),
                postfix: new HarmonyMethod(typeof(BroomstickPatches), nameof(ChooseAppearance_Postfix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Horse), name: nameof(Horse.GetBoundingBox)),
                postfix: new HarmonyMethod(typeof(BroomstickPatches), nameof(GetBoundingBox_Postfix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Farmer), name: nameof(Farmer.getMovementSpeed)),
                postfix: new HarmonyMethod(typeof(BroomstickPatches), nameof(FarmerGetMovementSpeed_Postfix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Stable), name: nameof(Stable.getStableHorse)),
                postfix: new HarmonyMethod(typeof(BroomstickPatches), nameof(GetStableHorse_PostFix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Stable), name: nameof(Stable.grabHorse)),
                prefix: new HarmonyMethod(typeof(BroomstickPatches), nameof(GrabHorse_Prefix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Stable), name: nameof(Stable.updateHorseOwnership)),
                prefix: new HarmonyMethod(typeof(BroomstickPatches), nameof(UpdateHorseOwnership_PreFix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Game1), name: nameof(Game1.UpdateHorseOwnership)),
                prefix: new HarmonyMethod(typeof(BroomstickPatches), nameof(Game1UpdateHorseOwnership_PreFix)),
                postfix: new HarmonyMethod(typeof(BroomstickPatches), nameof(Game1UpdateHorseOwnership_PostFix))
                );
        }

        internal static Broomstick? GetBroomstick(Horse? horse)
        {
            if (horse == null || string.IsNullOrEmpty(InternalSettings.ModId)) return null;
            Broomstick broomstick = new(horse, InternalSettings.ModId);
            return broomstick.IsInvalid() ? null : broomstick;
        }
        internal static BroomstickStable? GetBroomstickStable(Stable? stable)
        {
            if (stable == null || string.IsNullOrEmpty(InternalSettings.ModId)) return null;
            BroomstickStable broomstickStable = new(stable, InternalSettings.ModId);
            return broomstickStable.IsInvalid() ? null : broomstickStable;
        }

        internal static BroomstickStable? GetBroomstickStable(Building? building)
        {
            return (building is Stable stable) ? GetBroomstickStable(stable) : null;
        }

        internal static bool CheckAction_Prefix(Horse __instance, ref bool __result, Farmer who, GameLocation l)
        {
            if (GetBroomstick(__instance)?.ShouldRunHorseAction(who, l) == true) return true;
            __result = true;
            return false;
        }

        internal static void GetBoundingBox_Postfix(Horse __instance, ref Rectangle __result)
        {
            if (GetBroomstick(__instance)?.GetBoundingBox(__result) is Rectangle newRectangle) __result = newRectangle;
        }


        internal static bool PerformDefaultHorseFootstep_Prefix(Horse __instance)
        {
            return GetBroomstick(__instance) == null;
        }

        internal static void DayUpdate_Postfix(Horse __instance)
        {
            if (GetBroomstick(__instance) != null) __instance.ateCarrotToday = true;
        }

        internal static void ChooseAppearance_Postfix(Horse __instance)
        {
            GetBroomstick(__instance)?.ChangeSprite();
        }

        internal static void GetStableHorse_PostFix(Stable __instance, ref Horse? __result)
        {
            if (__result != null && GetBroomstickStable(__instance)?.ShouldHide == true) __result = null;
        }

        internal static bool GrabHorse_Prefix(Stable __instance)
        {
            return GetBroomstickStable(__instance)?.ShouldRunOriginalGrabHorse() != false;
        }

        internal static bool UpdateHorseOwnership_PreFix(Stable __instance)
        {
            return GetBroomstickStable(__instance)?.ShouldRunOriginalUpdateHorseOwnership() != false;
        }

        internal static bool Game1UpdateHorseOwnership_PreFix()
        {
            return UpdateStableOwnership(true);
        }

        internal static void Game1UpdateHorseOwnership_PostFix()
        {
            UpdateStableOwnership(false);
        }

        internal static bool UpdateStableOwnership(bool shouldHide)
        {
            Utility.ForEachBuilding(delegate (Stable stable)
            {
                GetBroomstickStable(stable)?.ToggleHideStatus(shouldHide);
                return true;
            });
            return true;
        }

        internal static void FarmerGetMovementSpeed_Postfix(Farmer __instance, ref float __result)
        {
            if (!__instance.isRidingHorse() || GetBroomstick(__instance.mount) is not Broomstick broomstick) return;
            if (Game1.CurrentEvent == null && __instance.hasBuff("19")) return;
            if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence) __result = MovementSpeedWithoutBook(__instance);
            __result *= broomstick.SpeedMultiplier;
        }

        internal static float MovementSpeedWithoutBook(Farmer player)
        {
            float eventUpSpeed = Game1.eventUp ? player.speed : player.speed + player.addedSpeed + 4.6f;
            float movementSpeed = Math.Max(1f, eventUpSpeed * 0.066f * Game1.currentGameTime.ElapsedGameTime.Milliseconds);
            if (player.movementDirections.Count > 1) movementSpeed *= 0.707f;
            return movementSpeed;
        }
    }
}
