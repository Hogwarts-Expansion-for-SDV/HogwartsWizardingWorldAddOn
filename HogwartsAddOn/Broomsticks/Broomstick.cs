using HogwartsAddOn.Patches;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;

namespace HogwartsAddOn.Broomsticks
{
    internal class Broomstick : BroomstickModDataReader<Horse>
    {
        internal float SpeedMultiplier => float.TryParse(GetValue(Speed), out float multiplier) ? multiplier : 1f;
        internal BroomstickStable? ThisStable => BroomstickPatches.GetBroomstickStable(This.TryFindStable());
        internal Broomstick(Horse horse, string modId) : base(horse, modId)
        {
            FailedToUpdateData();
        }

        protected void UpdateAllModData(string speed, string colorVariant)
        {
            UpdateModData(Speed, speed, "1");
            UpdateModData(Variant, colorVariant, "Default");
        }

        protected bool FailedToUpdateData()
        {
            if (IsInvalid() && ThisStable is BroomstickStable stable) UpdateAllModData(stable.MovementSpeed, stable.ColorVariant);
            return IsInvalid();
        }

        internal void MakeBroomstick(long ownerId, string speed, string colorVariant)
        {
            This.Name = "";
            This.ateCarrotToday = true;
            This.ownerId.Value = ownerId;
            UpdateAllModData(speed, colorVariant);
            This.ChooseAppearance();
            Logger.Debug($"Turned {ownerId}'s horse {This.HorseId} into a {GetValue(Variant)} broomstick");
        }

        internal void ChangeSprite()
        {
            if (FailedToUpdateData()) return;
            if (GetValue(Variant) is string texture && This.Sprite.textureName.Value != texture)
                This.Sprite = new(texture, 0, 32, 32) { loop = true };
        }

        internal void StartRiding(Farmer farmer, GameLocation location)
        {
            if (FailedToUpdateData()) return;
            This.rider = farmer;
            This.rider.freezePause = 5000;
            This.rider.synchronizedJump(6f);
            This.rider.Halt();
            if (This.rider.Position.X < This.Position.X) This.rider.faceDirection(1);
            location.playSound("dwop");
            This.mounting.Value = true;
            This.rider.isAnimatingMount = true;
            This.rider.completelyStopAnimatingOrDoingAction();
            This.rider.faceGeneralDirection(Utility.PointToVector2(This.StandingPixel), 0, opposite: false, useTileCalculations: false);
        }

        internal bool ShouldRunHorseAction(Farmer player, GameLocation location)
        {
            if (This.rider != null || FailedToUpdateData()) return true;
            This.mutex.RequestLock(delegate
            {
                UpdateOwner(player);
                StartRiding(player, location);
            });
            return false;
        }

        internal void UpdateOwner(Farmer player)
        {
            if (FailedToUpdateData() || This.getOwner() != Game1.player || ThisStable is not BroomstickStable stable) return;
            UpdateAllModData(stable.MovementSpeed, stable.ColorVariant);
            stable.UpdateOwnership(player.UniqueMultiplayerID);
        }

        internal Rectangle? GetBoundingBox(Rectangle originalBoundingBox)
        {
            if (FailedToUpdateData()) return null;
            if (This.rider == null) return null;
            if (This.mounting.Value) return new Rectangle(originalBoundingBox.X, originalBoundingBox.Y, 48, 32);
            int x = (int)This.rider.Position.X + 32;
            int y = (int)This.rider.Position.Y + This.rider.Sprite.getHeight() - 48;
            return new Rectangle(x, y, 48, 32);
        }
    }
}
