namespace BWAPI.NET
{
    public sealed class UnitSelf
    {
        public readonly OrderCache Order = new OrderCache();
        public readonly IntegerCache TargetPositionX = new IntegerCache();
        public readonly IntegerCache TargetPositionY = new IntegerCache();
        public readonly IntegerCache OrderTargetPositionX = new IntegerCache();
        public readonly IntegerCache OrderTargetPositionY = new IntegerCache();
        public readonly IntegerCache Target = new IntegerCache();
        public readonly BooleanCache IsConstructing = new BooleanCache();
        public readonly BooleanCache IsIdle = new BooleanCache();
        public readonly UnitTypeCache BuildType = new UnitTypeCache();
        public readonly OrderCache SecondaryOrder = new OrderCache();
        public readonly IntegerCache RemainingBuildTime = new IntegerCache();
        public readonly IntegerCache BuildUnit = new IntegerCache();
        public readonly UnitTypeCache Type = new UnitTypeCache();
        public readonly BooleanCache IsMorphing = new BooleanCache();
        public readonly BooleanCache IsCompleted = new BooleanCache();
        public readonly IntegerCache RemainingResearchTime = new IntegerCache();
        public readonly TechTypeCache Tech = new TechTypeCache();
        public readonly BooleanCache IsTraining = new BooleanCache();
        public readonly IntegerCache RemainingTrainTime = new IntegerCache();
        public readonly UpgradeTypeCache Upgrade = new UpgradeTypeCache();
        public readonly IntegerCache RemainingUpgradeTime = new IntegerCache();
        public readonly BooleanCache IsMoving = new BooleanCache();
        public readonly BooleanCache IsGathering = new BooleanCache();
        public readonly IntegerCache RallyPositionX = new IntegerCache();
        public readonly IntegerCache RallyPositionY = new IntegerCache();
        public readonly IntegerCache RallyUnit = new IntegerCache();
        public readonly IntegerCache StimTimer = new IntegerCache();
        public readonly IntegerCache OrderTarget = new IntegerCache();
        public readonly UnitTypeCache[] TrainingQueue = new UnitTypeCache[5];
        public readonly IntegerCache HitPoints = new IntegerCache();
        public readonly IntegerCache TrainingQueueCount = new IntegerCache();
        public readonly IntegerCache Energy = new IntegerCache();

        public UnitSelf()
        {
            for (int i = 0; i < TrainingQueue.Length; i++)
            {
                TrainingQueue[i] = new UnitTypeCache();
            }
        }
    }
}