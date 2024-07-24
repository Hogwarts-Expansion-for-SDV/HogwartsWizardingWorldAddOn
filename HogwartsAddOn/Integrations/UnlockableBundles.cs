using StardewValley;
using StardewValley.Tools;

namespace HogwartsAddOn.Integrations
{
    internal class UnlockableBundles
    {
        IUnlockableBundlesApi UnlockableBundlesApi { get; set; }
        string CPModId { get; set; }
        internal UnlockableBundles(IUnlockableBundlesApi api, string cpModId)
        {
            UnlockableBundlesApi = api;
            CPModId = cpModId;
            RegisterEventHandlers();
        }

        internal void RegisterEventHandlers()
        {
            UnlockableBundlesApi.BundlePurchasedEvent += OnBundlePurchased;
        }

        protected void OnBundlePurchased(object sender, IBundlePurchasedEventArgs e)
        {
            if (e.Who == null || !e.Bundle.Purchased) return;
            if (e.Bundle.Key.StartsWith($"{CPModId}_Wand") && GetPlayerWand(e.Who) is FarmerWandColor wand) wand.UpdateWand();
        }

        protected FarmerWandColor? GetPlayerWand(Farmer player)
        {
            if (player.mostRecentlyGrabbedItem is MeleeWeapon maybeWand)
            {
                FarmerWandColor wand = new(CPModId, maybeWand, player);
                if (wand.IsWand && wand.PlayerSharesStats(player)) return wand;
            }
            return player.Items
                .Where(item => item is MeleeWeapon)
                .Cast<MeleeWeapon>()
                .Select(weapon => new FarmerWandColor(CPModId, weapon, player))
                .First(wand => wand.IsWand && wand.PlayerSharesStats(player));
        }

        internal int GetHousePoints(string house)
        {
            return UnlockableBundlesApi.getWalletCurrency($"{CPModId}_{house}", Game1.player.UniqueMultiplayerID);
        }
    }
}
