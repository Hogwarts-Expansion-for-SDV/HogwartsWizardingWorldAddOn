using HogwartsAddOn.Models;
using HogwartsAddOn.Settings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogwartsAddOn.Patches
{
    internal class ObjectCategoryPatches
    {
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
            catch (Exception ex) {
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
