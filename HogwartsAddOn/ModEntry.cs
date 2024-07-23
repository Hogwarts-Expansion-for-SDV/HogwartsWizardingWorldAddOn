using HarmonyLib;
using HogwartsAddOn.Models;
using HogwartsAddOn.Patches;
using HogwartsAddOn.Settings;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HogwartsAddOn
{
    public class ModEntry : Mod
    {
        private readonly bool IsDebugMode = false;
        public override void Entry(IModHelper helper)
        {
            InternalSettings.Initialize(this.ModManifest.UniqueID, this.Helper.Reflection);
            Logger.Initialize(Monitor, IsDebugMode);
            Listen(helper.Events);
            Patch();
        }

        private void Listen(IModEvents events)
        {
            events.Content.AssetRequested += this.OnAssetRequested;
            events.Content.AssetReady += this.OnAssetReady;
            events.Content.LocaleChanged += this.OnLocaleChanged;
            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        private void Patch()
        {
            Harmony harmony = new(this.ModManifest.UniqueID);
            ObjectCategoryPatches.Patch(harmony);
            WarpTotemPatches.Patch(harmony);
            BroomstickPatches.Patch(harmony);
            BuildMenu.Patch(harmony);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            ReloadInternalSettings();
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            ReloadInternalSettings();
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(InternalSettings.Path)) e.LoadFrom(() => new CPFile(), AssetLoadPriority.Medium);
        }

        private void OnAssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (e.Name.IsEquivalentTo(InternalSettings.Path))
            {
                ReloadInternalSettings();
            }
        }

        private void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
        {
            ReloadInternalSettings();
        }

        private static void ReloadInternalSettings()
        {
            try
            {
                InternalSettings.UpdateSettings(Game1.content.Load<CPFile>(InternalSettings.Path));
            }
            catch (Exception ex)
            {
                Logger.Error("Internal error: Couldn't update internal settings. Add-on won't work unless Content Patcher updates them. Please report this bug to the Hogwarts Expansion Team! Exception:\n{0}", ex.Message);
            }
        }
    }
}
