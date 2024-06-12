using StardewModdingAPI;
using HogwartsAddOn.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Tools;

namespace HogwartsAddOn.Settings
{
    internal class CategoryInfo
    {
        internal string CPModId;
        internal Dictionary<string, CategoryEditingRules>? categoryRules;
        internal Dictionary<string, CategoryEditingRules>? contextTagRules;

        internal CategoryInfo(string contentPatcherModId, CategoryNamingTypes? rules)
        {
            CPModId = contentPatcherModId;
            if (rules != null) UpdateCategoryRules(rules);
        }

        internal void UpdateCategoryRules(CategoryNamingTypes rules)
        {
            categoryRules = rules.ByCategory;
            contextTagRules = rules.ByContextTag;
        }

        internal string? GetCategoryName(StardewValley.Object instance)
        {
            return GetCategoryInfo(instance)?.Name;
        }

        internal Color? GetCategoryColor(StardewValley.Object instance)
        {
            if (GetCategoryInfo(instance) is not CategoryEditingRules info) return null;
            if (info.Color is null || info.Color.Count == 0 || info.Color[0] == -1) return null;
            return InternalSettings.GetColorFromList(info.Color);
        }

        private CategoryEditingRules? GetCategoryInfo(StardewValley.Object instance)
        {
            if (instance == null || instance.ItemId == null || instance.ItemId.Length == 0) return null;
            if (!instance.ItemId.StartsWith(CPModId)) return null;
            return GetInfoFromCustomFields(instance.ItemId) ?? GetInfoByContextTags(instance) ?? GetInfoByCategory(instance.Category);
        }

        private CategoryEditingRules? GetInfoByCategory(int category)
        {
            if (categoryRules == null) return null;
            categoryRules.TryGetValue(category.ToString(), out CategoryEditingRules? info);
            return info;
        }

        private CategoryEditingRules? GetInfoByContextTags(StardewValley.Object instance)
        {
            if (contextTagRules == null) return null;
            CategoryEditingRules? info = null;
            foreach (string contextTag in instance.GetContextTags())
            {
                if (contextTag != null)
                {
                    contextTagRules.TryGetValue(contextTag, out CategoryEditingRules? foundInfo);
                    if (foundInfo != null)
                    {
                        if (info == null) info = foundInfo;
                        else if (info.Priority <= foundInfo.Priority)
                        {
                            if (foundInfo.Name is not null && foundInfo.Name != string.Empty) info.Name = foundInfo.Name;
                            if (foundInfo.Color is not null && foundInfo.Color.Count > 0) info.Color = foundInfo.Color;
                        }
                    }
                }
            }
            return info;
        }

        private CategoryEditingRules? GetInfoFromCustomFields(string objectId)
        {
            Game1.objectData.TryGetValue(objectId, out ObjectData? data);
            if (data == null || data.CustomFields == null) return null;
            CategoryEditingRules? info = new();
            data.CustomFields.TryGetValue(CPModId + "/CategoryName", out string? categoryName);
            data.CustomFields.TryGetValue(CPModId + "/CategoryColor", out string? categoryColor);
            info.Name = categoryName ?? string.Empty;
            info.Color = categoryColor?.Split(",")?.Select(int.Parse)?.ToList();
            return info;
        }
    }
}
