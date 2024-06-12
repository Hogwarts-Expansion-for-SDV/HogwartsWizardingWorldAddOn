using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using HogwartsAddOn.Models;
using HogwartsAddOn.Patches;
using HogwartsAddOn.Settings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace HogwartsAddOn
{
    public class ModEntry : Mod
    {
        bool IsDebugMode = false;
        public override void Entry(IModHelper helper)
        {
            InternalSettings.Initialize(this.ModManifest.UniqueID, this.Helper.Reflection);
            Logger.Initialize(Monitor, IsDebugMode);

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetReady += this.OnAssetReady;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Content.LocaleChanged += this.OnLocaleChanged;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            Harmony harmony = new(this.ModManifest.UniqueID);
            // Category editing
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Object), name: nameof(StardewValley.Object.getCategoryName)),
                postfix: new HarmonyMethod(typeof(ObjectCategoryPatches), nameof(ObjectCategoryPatches.GetCategoryName_PostFix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Object), name: nameof(StardewValley.Object.getCategoryColor)),
                postfix: new HarmonyMethod(typeof(ObjectCategoryPatches), nameof(ObjectCategoryPatches.GetCategoryColor_PostFix))
                );
            // Melee weapons need to be patched separately but it works the same way
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Tools.MeleeWeapon), name: nameof(StardewValley.Tools.MeleeWeapon.getCategoryName)),
                postfix: new HarmonyMethod(typeof(ObjectCategoryPatches), nameof(ObjectCategoryPatches.GetCategoryName_PostFix))
                );
            // Warp totems
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Object), name: nameof(StardewValley.Object.performUseAction)),
                postfix: new HarmonyMethod(typeof(WarpTotemPatches), nameof(WarpTotemPatches.PerformUseAction_PostFix))
                );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StardewValley.Object), name: "totemWarpForReal"),
                postfix: new HarmonyMethod(typeof(WarpTotemPatches), nameof(WarpTotemPatches.TotemWarpForReal_PostFix))
                );
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
            } catch (Exception ex)
            {
                Logger.Error("Internal error: Couldn't update internal settings. Add-on won't work unless Content Patcher updates them. Please report this bug to the Hogwarts Expansion Team! Exception:\n{0}", ex.Message);
            }
        }
    }
}
