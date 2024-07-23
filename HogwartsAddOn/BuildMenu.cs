using HarmonyLib;
using HogwartsAddOn.Broomsticks;
using HogwartsAddOn.Patches;
using HogwartsAddOn.Settings;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace HogwartsAddOn
{
    internal static class BuildMenu
    {
        internal static void Patch(Harmony harmony)
        {
            Type[] performActionParameters = { typeof(string[]), typeof(Farmer), typeof(xTile.Dimensions.Location) };
            harmony.Patch(
                original: AccessTools.Method(type: typeof(GameLocation), name: nameof(GameLocation.performAction), parameters: performActionParameters),
                postfix: new HarmonyMethod(typeof(BuildMenu), nameof(GameLocationPerformAction_PostFix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Building), name: nameof(Building.performActionOnConstruction)),
                postfix: new HarmonyMethod(typeof(BuildMenu), nameof(PerformActionOnConstruction_PostFix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(CarpenterMenu), name: nameof(CarpenterMenu.robinConstructionMessage)),
                prefix: new HarmonyMethod(typeof(BuildMenu), nameof(RobinConstructionMessage_PreFix))
                );
        }

        internal static void PerformActionOnConstruction_PostFix(Building __instance)
        {
            if (BroomstickPatches.GetBroomstickStable(__instance) is BroomstickStable stable) stable.PerformActionOnConstruction();
        }

        internal static void GameLocationPerformAction_PostFix(GameLocation __instance, ref bool __result, string[] action, Farmer who, xTile.Dimensions.Location tileLocation)
        {
            if (InternalSettings.ModId == null || __instance.ShouldIgnoreAction(action, who, tileLocation) || !who.IsLocalPlayer || action.Length != 2) return;
            if (!ArgUtility.TryGet(action, 0, out string? actionType, out _) || actionType != InternalSettings.ModId + "_OpenBuildMenu") return;
            __instance.ShowConstructOptions(action[1]);
            __result = true;
        }

        internal static bool RobinConstructionMessage_PreFix(CarpenterMenu __instance)
        {
            if (__instance.Blueprint.MagicalConstruction || InternalSettings.ModId is not string modId) return true;
            // Our current mod id starts with "Hogwarts" so at the moment this check is redundant, but it shouldn't be removed in case the mod id ever changes.
            if (!__instance.Builder.StartsWith("Hogwarts") && !__instance.Builder.StartsWith(modId)) return true;
            if (Game1.getCharacterFromName(__instance.Builder) is not NPC builder) return true;
            __instance.exitThisMenu();
            Game1.player.forceCanMove();
            string displayName = __instance.Blueprint.DisplayName;
            string dialoguePath = GetConstructionDialoguePath(__instance.Blueprint.BuildDays, __instance.Builder, __instance.upgrading);
            if (Game1.content.LoadString(dialoguePath, displayName) == dialoguePath) Game1.drawDialogueBox(dialoguePath.Replace(__instance.Builder, modId + "_Default"));
            else Game1.DrawDialogue(builder, dialoguePath, displayName);
            return false;
        }

        internal static string GetConstructionDialoguePath(int buildDays, string builderName, bool isUpgrade)
        {
            if (buildDays <= 0) return $"Data\\ExtraDialogue:{builderName}_Instant";
            string upgrade = isUpgrade ? "Upgrade" : "New";
            string festival = Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season) ? "_Festival" : "";
            return $"Data\\ExtraDialogue:{builderName}_{upgrade}Construction{festival}";
        }
    }
}
