namespace BWAPI.NET
{
    public class UnitSelf
    {
        public readonly OrderCache order = new OrderCache();
        public readonly IntegerCache targetPositionX = new IntegerCache();
        public readonly IntegerCache targetPositionY = new IntegerCache();
        public readonly IntegerCache orderTargetPositionX = new IntegerCache();
        public readonly IntegerCache orderTargetPositionY = new IntegerCache();
        public readonly IntegerCache target = new IntegerCache();
        public readonly BooleanCache isConstructing = new BooleanCache();
        public readonly BooleanCache isIdle = new BooleanCache();
        public readonly UnitTypeCache buildType = new UnitTypeCache();
        public readonly OrderCache secondaryOrder = new OrderCache();
        public readonly IntegerCache remainingBuildTime = new IntegerCache();
        public readonly IntegerCache buildUnit = new IntegerCache();
        public readonly UnitTypeCache type = new UnitTypeCache();
        public readonly BooleanCache isMorphing = new BooleanCache();
        public readonly BooleanCache isCompleted = new BooleanCache();
        public readonly IntegerCache remainingResearchTime = new IntegerCache();
        public readonly TechTypeCache tech = new TechTypeCache();
        public readonly BooleanCache isTraining = new BooleanCache();
        public readonly IntegerCache remainingTrainTime = new IntegerCache();
        public readonly UpgradeTypeCache upgrade = new UpgradeTypeCache();
        public readonly IntegerCache remainingUpgradeTime = new IntegerCache();
        public readonly BooleanCache isMoving = new BooleanCache();
        public readonly BooleanCache isGathering = new BooleanCache();
        public readonly IntegerCache rallyPositionX = new IntegerCache();
        public readonly IntegerCache rallyPositionY = new IntegerCache();
        public readonly IntegerCache rallyUnit = new IntegerCache();
        public readonly IntegerCache stimTimer = new IntegerCache();
        public readonly IntegerCache orderTarget = new IntegerCache();
        public readonly UnitTypeCache[] trainingQueue = new UnitTypeCache[5];
        public readonly IntegerCache hitPoints = new IntegerCache();
        public readonly IntegerCache trainingQueueCount = new IntegerCache();
        public readonly IntegerCache energy = new IntegerCache();

        public UnitSelf()
        {
            for (int i = 0; i < trainingQueue.Length; i++)
            {
                trainingQueue[i] = new UnitTypeCache();
            }
        }
    }
}