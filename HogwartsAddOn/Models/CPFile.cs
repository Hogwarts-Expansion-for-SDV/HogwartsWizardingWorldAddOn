using System.ComponentModel.DataAnnotations;

namespace HogwartsAddOn.Models
{
    public sealed class CPFile
    {
        [Required]
        public string ModId { get; set; } = string.Empty;
        public Toggles FeatureToggles { get; set; } = new();
        public CategoryNamingTypes EditCategories { get; set; } = new();
        public Dictionary<string, WarpTotemInfo> WarpTotems { get; set; } = new();
    }

    public sealed class Toggles
    {
        public bool CategoryRenaming { get; set; } = false;
        public bool CategoryRecolor { get; set; } = false;
        public bool WarpTotems { get; set; } = false;
    }
    public sealed class CategoryNamingTypes
    {
        public Dictionary<string, CategoryEditingRules> ByCategory { get; set; } = new();
        public Dictionary<string, CategoryEditingRules> ByContextTag { get; set; } = new();
    }
    public sealed class CategoryEditingRules
    {
        public int Priority { get; set; } = 0;
        public string? Name { get; set; }
        public List<int>? Color { get; set; }
    }
    public sealed class WarpTotemInfo
    {
        [Required]
        public string? MapName { get; set; }
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public List<int>? Color { get; set; }
    }
}
