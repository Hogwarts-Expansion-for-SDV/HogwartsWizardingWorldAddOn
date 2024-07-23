using StardewValley;

namespace HogwartsAddOn.Broomsticks
{
    internal abstract class BroomstickModDataReader<T> where T : IHaveModData
    {
        protected const string Speed = "BroomstickSpeed";
        protected const string Variant = "BroomstickColor";
        protected const string StableStatus = "BroomstickStable";
        protected const string HideStable = "HIDE";
        protected const string BroomTexture = "BroomTexture";
        protected readonly T This;
        protected readonly string ModId;
        internal string? GetValue(string key) => This.modData.TryGetValue($"{ModId}_{key}", out string result) ? result : null;

        internal virtual bool IsInvalid()
        {
            return string.IsNullOrWhiteSpace(GetValue(Speed));
        }

        protected BroomstickModDataReader(T originalObject, string modId)
        {
            This = originalObject;
            ModId = modId;
        }

        internal bool UpdateModData(string key, string? value, string? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (IsInvalid() && key != Speed) return false;
            string newValue = string.IsNullOrWhiteSpace(value) ? defaultValue ?? string.Empty : value;
            This.modData[$"{ModId}_{key}"] = newValue;
            return true;
        }
    }
}
