using HogwartsAddOn.Settings;
using StardewModdingAPI;
using StardewValley;

namespace HogwartsAddOn.Patches
{
    internal class WarpTotemPatches
    {
        internal static void PerformUseAction_PostFix(StardewValley.Object __instance, GameLocation location, ref bool __result)
        {
            try
            {
                if (FindTotem(__instance) is not WarpTotem totem) return;
                totem.Object = __instance;
                if (totem.Use(location)) __result = true;
            }
            catch (Exception ex)
            {
                Logger.LogOnce("Failed to patch performUseAction for object ID {0}:\n{1}", LogLevel.Warn, __instance?.ItemId ?? "unknown", ex);
            }
        }

        internal static void TotemWarpForReal_PostFix(StardewValley.Object __instance)
        {
            try
            {
                if (FindTotem(__instance) is not WarpTotem totem) return;
                totem.Warp();
            }
            catch (Exception ex)
            {
                Logger.LogOnce("Failed to patch totemWarpForReal for object ID {0}:\n{1}", LogLevel.Warn, __instance?.ItemId ?? "unknown", ex);
            }
        }

        private static WarpTotem? FindTotem(StardewValley.Object obj)
        {
            if (InternalSettings.Settings == null) return null;
            if (InternalSettings.Settings.FeatureToggles == null || !InternalSettings.Settings.FeatureToggles.WarpTotems) return null;
            if (InternalSettings.WarpTotems is null || InternalSettings.Reflection is null) return null;
            InternalSettings.WarpTotems.TryGetValue(obj.ItemId, out WarpTotem? totem);
            return totem;
        }
    }
}
