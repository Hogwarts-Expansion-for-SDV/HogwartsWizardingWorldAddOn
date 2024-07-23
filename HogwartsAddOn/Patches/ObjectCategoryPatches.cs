using HarmonyLib;
using HogwartsAddOn.Settings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace HogwartsAddOn.Patches
{
    internal static class ObjectCategoryPatches
    {
        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Object), name: nameof(StardewValley.Object.getCategoryName)),
                postfix: new HarmonyMethod(typeof(ObjectCategoryPatches), nameof(ObjectCategoryPatches.GetCategoryName_PostFix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Object), name: nameof(StardewValley.Object.getCategoryColor)),
                postfix: new HarmonyMethod(typeof(ObjectCategoryPatches), nameof(ObjectCategoryPatches.GetCategoryColor_PostFix))
                );
            // Melee weapons need to be patched separately but it works the same way
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Tools.MeleeWeapon), name: nameof(StardewValley.Tools.MeleeWeapon.getCategoryName)),
                postfix: new HarmonyMethod(typeof(ObjectCategoryPatches), nameof(ObjectCategoryPatches.GetCategoryName_PostFix))
                );
        }
        internal static void GetCategoryName_PostFix(StardewValley.Object __instance, ref string __result)
        {
            try
            {
                if (InternalSettings.Settings?.FeatureToggles == null || !InternalSettings.Settings.FeatureToggles.CategoryRenaming) return;
                if (InternalSettings.Category is null) return;
                if (InternalSettings.Category.GetCategoryName(__instance) is not string newCategoryName || newCategoryName == string.Empty) return;
                Logger.LogOnce($"Patching category name for object {__instance.ItemId}", LogLevel.Debug);
                __result = newCategoryName;
            }
            catch (Exception ex)
            {
                Logger.LogOnce("Failed to patch getCategoryName for object ID {0}:\n{1}", LogLevel.Warn, __instance?.ItemId ?? "unknown", ex);
            }
        }

        internal static void GetCategoryColor_PostFix(StardewValley.Object __instance, ref Color __result)
        {
            try
            {
                if (InternalSettings.Settings?.FeatureToggles == null || !InternalSettings.Settings.FeatureToggles.CategoryRecolor) return;
                if (InternalSettings.Category is null) return;
                if (InternalSettings.Category.GetCategoryColor(__instance) is not Color newCategoryColor) return;
                Logger.LogOnce($"Patching category color for object {__instance.ItemId}", LogLevel.Debug);
                __result = newCategoryColor;
            }
            catch (Exception ex)
            {
                Logger.LogOnce("Failed to patch getCategoryColor for object ID {0}:\n{1}", LogLevel.Warn, __instance?.ItemId ?? "unknown", ex);
            }
        }
    }
}
