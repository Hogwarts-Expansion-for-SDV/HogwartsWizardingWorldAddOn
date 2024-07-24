using StardewValley;

namespace HogwartsAddOn.Integrations
{
    public interface IUnlockableBundlesApi
    {
        /// <summary>Fires once for every player when a bundle has been purchased before the ShopEvent</summary>
        event BundlesPurchasedDelegate BundlePurchasedEvent;

        /// <summary> Returns the wallet currency value of a player </summary>
        /// <param name="currencyId"></param>
        /// <param name="who">The players unique multiplayer id</param>
#pragma warning disable IDE1006 // Naming Styles
        int getWalletCurrency(string currencyId, long who);
#pragma warning restore IDE1006 // Naming Styles

        public delegate void BundlesPurchasedDelegate(object sender, IBundlePurchasedEventArgs e);
    }

    public interface IBundle
    {
        public string Key { get; }
        public string Location { get; }
        public string LocationOrUnique { get; }
        public IDictionary<string, int> Price { get; }
        public IDictionary<string, int> AlreadyPaid { get; }
        public bool Purchased { get; }
        public int DaysSincePurchase { get; }
        public bool AssetLoaded { get; }
        public bool Discovered { get; }
    }
    public interface IBundlePurchasedEventArgs
    {
        public Farmer Who { get; }
        public string Location { get; }
        public string LocationOrUnique { get; }
        public IBundle Bundle { get; }
        public bool IsBuyer { get; }
    }
}
