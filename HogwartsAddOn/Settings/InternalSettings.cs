using HogwartsAddOn.Models;
using StardewModdingAPI;
using Microsoft.Xna.Framework;

namespace HogwartsAddOn.Settings
{
    internal static class InternalSettings
    {
        internal static CPFile? Settings { get; set; }
        internal static CategoryInfo? Category { get; set; }
        internal static Dictionary<string, WarpTotem> WarpTotems { get; set; } = new();
        internal static string Path { get; set; } = string.Empty;
        internal static IReflectionHelper? Reflection { get; set; }

        internal static void Initialize(string modId, IReflectionHelper reflection)
        {
            Path = $"Mods/{modId}/InternalSettings";
            Reflection = reflection;
        }

        internal static void UpdateSettings(CPFile? settings)
        {
            if (settings is null || settings.ModId is null || settings.ModId == string.Empty)
            {
                if (Context.IsWorldReady) Logger.LogOnce("Couldn't find a valid settings file from the CP mod. The mod is disabled until this is fixed.", LogLevel.Trace);
                return;
            };
            Settings = settings;
            Logger.Trace("Successfully loaded settings from {0}", Settings.ModId);
            if (Settings.FeatureToggles.CategoryRecolor == true || Settings.FeatureToggles.CategoryRenaming == true)
            {
                if (Settings.EditCategories is not null) Category = new(Settings.ModId, Settings.EditCategories);
            }
            if (Settings.FeatureToggles.WarpTotems == true && Settings.WarpTotems is not null)
            {
                WarpTotems = new();
                foreach (KeyValuePair<string, WarpTotemInfo> keyValuePair in Settings.WarpTotems)
                {
                    if (keyValuePair.Value.MapName is null) continue;
                    WarpTotem totem = new WarpTotem(keyValuePair.Value.MapName, keyValuePair.Value.X, keyValuePair.Value.Y, GetColorFromList(keyValuePair.Value.Color));
                    WarpTotems.Add(keyValuePair.Key, totem);
                }
            }
        }

        internal static Color GetColorFromList(List<int>? list)
        {
            if (list is not null && IsValidRGBValue(list)) return new Color(list[0], list[1], list[2]);
            return Color.Red;
        }

        private static bool IsValidRGBValue(List<int>? colorValueList)
        {
            if (colorValueList is null || colorValueList.Count == 0) return false;
            if (colorValueList.Count == 1) colorValueList = new List<int> { colorValueList[0], colorValueList[0], colorValueList[0] };
            if (colorValueList.Count != 3)
            {
                Logger.LogOnce($"[{string.Join(", ", colorValueList)}] is not a valid color value list. They should have either 1 (greyscale) or 3 values (RGB).", LogLevel.Trace);
                return false;
            }
            foreach (int colorValue in colorValueList)
                if (!Enumerable.Range(0, 256).Contains(colorValue))
                {
                    Logger.LogOnce($"{colorValue} (from [{string.Join(", ", colorValueList)}]) is not a valid color value. Color values can't be smaller than 0 or bigger than 255.", LogLevel.Trace);
                    return false;
                }
            return true;
        }
    }
}
