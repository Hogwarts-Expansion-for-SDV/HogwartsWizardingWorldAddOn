using HogwartsAddOn.Settings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace HogwartsAddOn
{
    public class WarpTotem
    {
        internal string MapName { get; set; }
        internal int XCoordinate { get; set; }
        internal int YCoordinate { get; set; }
        internal Color WarpColor { get; set; }

        internal StardewValley.Object? Object { get; set; }

        internal WarpTotem(string mapName, int x, int y, Color? color)
        {
            MapName = mapName;
            XCoordinate = x;
            YCoordinate = y;
            WarpColor = color ?? Color.LimeGreen;
        }

        internal bool Use(GameLocation location)
        {
            if (Object == null || !CanWarp(Object)) return false;
            Logger.Trace("Recognized object {0} as a valid warp totem", Object.ItemId);
            PrepareToWarp(location);
            AnimateWarpOnUse(Object, location, TryToWarp);
            return true;
        }

        public void TryToWarp(Farmer who)
        {
            if (InternalSettings.Reflection is null || Object is null)
            {
                // Object nullability was checked in the method called before this one but VS doesn't get it
                Logger.LogOnce("Couldn't get Reflection to work! Warp Totems are disabled", LogLevel.Warn);
                return;
            }
            Logger.Trace("Trying to warp after using object {0}", Object.ItemId);
            InternalSettings.Reflection.GetMethod(Object, "totemWarp").Invoke(who);
        }

        private static bool CanWarp(StardewValley.Object? obj)
        {
            if (InternalSettings.Settings is null || InternalSettings.Settings.ModId == string.Empty || InternalSettings.Settings.WarpTotems is null) return false;
            if (obj is null || obj.isTemporarilyInvisible || obj.Category != 0) return false;
            if (!obj.ItemId.StartsWith(InternalSettings.Settings.ModId) || !obj.HasContextTag("totem_item")) return false;
            if (Game1.eventUp || Game1.isFestival() || Game1.fadeToBlack) return false;
            if (!Game1.player.canMove || Game1.player.swimming.Value || Game1.player.bathingClothes.Value || Game1.player.onBridge.Value) return false;
            return true;
        }

        private static void PrepareToWarp(GameLocation location)
        {
            Game1.player.jitterStrength = 1f;
            location.playSound("warrior");
            Game1.player.faceDirection(2);
            Game1.player.CanMove = false;
            Game1.player.temporarilyInvincible = true;
            Game1.player.temporaryInvincibilityTimer = -4000;
            Game1.changeMusicTrack("silence");
        }

        private static TemporaryAnimatedSprite CreateSprite(int index, string itemId)
        {
            float x;
            if (index == 0) x = 0f;
            else x = index == 1 ? -64f : 64f;
            float multiplier = index == 0 ? 1f : 0.5f;
            TemporaryAnimatedSprite sprite = new(0, 9999f, 1, 999, Game1.player.Position + new Vector2(x, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
            {
                motion = new Vector2(0f, multiplier * -1f),
                scale = multiplier,
                scaleChange = multiplier * 0.01f,
                alpha = 1f,
                alphaFade = 0.0075f,
                shakeIntensity = 1f,
                delayBeforeAnimationStart = index == 0 ? 0 : 10,
                initialPosition = Game1.player.Position + new Vector2(index == 0 ? 0f : -64f, -96f),
                xPeriodic = true,
                xPeriodicLoopTime = 1000f,
                xPeriodicRange = 4f,
                layerDepth = index == 0 ? 1f : 0.9999f
            };
            sprite.CopyAppearanceFromItemId(itemId);
            return sprite;
        }

        private void AnimateWarpOnUse(StardewValley.Object obj, GameLocation location, AnimatedSprite.endOfAnimationBehavior endOfAnimationBehavior)
        {
            PrepareToWarp(location);
            Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
            {
                        new(57, 2000, secondaryArm: false, flip: false),

                        new((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, endOfAnimationBehavior, behaviorAtEndOfFrame: true)
            });
            AnimateStartOfWarp(location, obj.QualifiedItemId);
        }

        private void AnimateStartOfWarp(GameLocation location, string itemId)
        {
            Game1.Multiplayer.broadcastSprites(location, CreateSprite(0, itemId));
            Game1.Multiplayer.broadcastSprites(location, CreateSprite(1, itemId));
            Game1.Multiplayer.broadcastSprites(location, CreateSprite(2, itemId));
            Game1.screenGlowOnce(WarpColor, hold: false);
            Utility.addSprinklesToLocation(location, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);
        }

        public void Warp()
        {
            Game1.warpFarmer(MapName, XCoordinate, YCoordinate, flip: false);
            AnimateEndOfWarp();
        }

        private static void AnimateEndOfWarp()
        {
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }
    }
}
