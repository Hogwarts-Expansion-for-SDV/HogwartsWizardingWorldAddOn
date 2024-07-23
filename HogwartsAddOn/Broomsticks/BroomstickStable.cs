using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;

namespace HogwartsAddOn.Broomsticks
{
    internal class BroomstickStable : BroomstickModDataReader<Stable>
    {
        internal string MovementSpeed => GetValue(Speed) is string speed && !string.IsNullOrWhiteSpace(speed) ? speed : "1";
        internal bool ShouldHide => GetValue(StableStatus) == HideStable;
        internal string ColorVariant => GetValue(BroomTexture) is string texture && !string.IsNullOrWhiteSpace(texture) ? texture : "Hogwarts\\Broomsticks\\Default";
        internal Horse? SpawnedHorse => Utility.findHorse(This.HorseId);
        internal BroomstickStable(Stable stable, string modId) : base(stable, modId) { }

        internal override bool IsInvalid()
        {
            return base.IsInvalid() || This.daysOfConstructionLeft.Value > 0;
        }

        internal void UpdateOwnership(long ownerId)
        {
            This.owner.Value = ownerId;
            This.updateHorseOwnership();
        }

        internal void PerformActionOnConstruction()
        {
            Logger.Warn(This.buildingType.Value);
            if (!IsInvalid()) UpdateStable();
        }

        internal bool ShouldRunOriginalGrabHorse()
        {
            if (IsInvalid() || SpawnedHorse != null) return true;
            Horse horse = new(This.HorseId, This.tileX.Value + 1, This.tileY.Value + 1);
            Broomstick broomstick = new(horse, ModId);
            broomstick.MakeBroomstick(This.owner.Value, MovementSpeed, ColorVariant);
            This.GetParentLocation().characters.Add(horse);
            return false;
        }

        internal void UpdateBroomColor()
        {
            if (IsInvalid() || SpawnedHorse is not Horse horse) return;
            Broomstick broomstick = new(horse, ModId);
            if (broomstick.IsInvalid()) return;
            broomstick.UpdateModData(Variant, ColorVariant, "Default");
            broomstick.ChangeSprite();
        }

        internal bool ShouldRunOriginalUpdateHorseOwnership()
        {
            if (IsInvalid() || SpawnedHorse is not Horse horse) return true;
            horse.ownerId.Value = This.owner.Value;
            return false;
        }

        internal void ToggleHideStatus(bool shouldHide = false)
        {
            if (UpdateModData(StableStatus, shouldHide ? HideStable : "show") && shouldHide) UpdateStable();
        }

        protected void UpdateStable()
        {
            if (IsInvalid()) return;
            This.grabHorse();
            This.updateHorseOwnership();
        }
    }
}
