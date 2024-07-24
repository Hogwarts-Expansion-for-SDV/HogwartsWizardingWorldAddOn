using StardewValley;
using StardewValley.Tools;

namespace HogwartsAddOn
{
    internal class FarmerWandColor
    {
        private const string spriteFlagKey = "HOGWARTSWANDSPRITE";
        private const string statsFlagKey = "HOGWARTSWANDSTATS";
        private const string wandOwnerIdKey = "WandOwner";
        private const string wandOwnerNameKey = "WandOwnerName";
        private const string wandColorKey = "LastKnownColor";
        private readonly string ModId;
        private readonly MeleeWeapon Wand;
        private Farmer? Owner;
        internal bool IsWand;

        private static string? GetInfoFromMailData(Farmer? player, string flagKey) => player?.mailReceived.FirstOrDefault(mail => mail.StartsWith(flagKey))?.Replace(flagKey, "");

        internal FarmerWandColor(string modId, MeleeWeapon wand, Farmer? player = null)
        {
            ModId = modId;
            Wand = wand;
            IsWand = Wand.ItemId.StartsWith($"{ModId}_Wand");
            Owner = player ?? FindOwner(modId, wand);
        }

        private static Farmer? FindOwner(string modId, MeleeWeapon wand)
        {
            if (!wand.modData.TryGetValue($"{modId}_{wandOwnerIdKey}", out string ownerIdString)) return LastHolderIfWasOwner(wand);
            if (!long.TryParse(ownerIdString, out long ownerId)) return null;
            return Game1.getFarmerMaybeOffline(ownerId);
        }

        private Farmer? LastHolderIfWasOwner(MeleeWeapon wand)
        {
            if (wand.getLastFarmerToUse() is Farmer player && PlayerSharesStats(player)) return player;
            return null;
        }

        internal bool PlayerSharesStats(Farmer player)
        {
            return Wand.ItemId.Split("_")[^1] == GetInfoFromMailData(player, statsFlagKey);
        }

        private int GetSpriteIndex()
        {
            if (Wand.modData.TryGetValue(wandColorKey, out string color) && int.TryParse(color, out int spriteIndex)) return spriteIndex;
            return int.TryParse(GetInfoFromMailData(Owner, spriteFlagKey), out int mailSpriteIndex) ? mailSpriteIndex : 0;
        }

        private void UpdateWandOwnerInfo()
        {
            UpdateModData(wandOwnerIdKey, Owner!.UniqueMultiplayerID.ToString());
            UpdateModData(wandOwnerNameKey, Owner.displayName);
            UpdateModData(wandColorKey, GetSpriteIndex().ToString());
        }

        private void UpdateModData(string key, string value)
        {
            Wand.modData[$"{ModId}_{key}"] = value;
        }

        private bool IsOwnerInvalid()
        {
            if (Owner != null) return false;
            if (FindOwner(ModId, Wand) is not Farmer owner) return true;
            Owner = owner;
            return false;
        }

        internal void UpdateWand(Farmer? newOwner = null)
        {
            if (newOwner != null) Owner = newOwner;
            else if (IsOwnerInvalid()) return;
            UpdateWandOwnerInfo();
            Wand.SetSpriteIndex(GetSpriteIndex());
            Wand.description = Wand.Description.Replace("{0}", Owner!.displayName);
        }
    }
}
