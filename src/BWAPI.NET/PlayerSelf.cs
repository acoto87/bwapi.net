namespace BWAPI.NET
{
    public class PlayerSelf
    {
        public readonly IntegerCache minerals = new IntegerCache();
        public readonly IntegerCache gas = new IntegerCache();
        public readonly IntegerCache[] supplyUsed = new IntegerCache[3];
        public readonly BooleanCache[] isResearching = new BooleanCache[(int)TechType.Unknown];
        public readonly BooleanCache[] isUpgrading = new BooleanCache[(int)UpgradeType.Unknown];

        public PlayerSelf()
        {
            for (var i = 0; i < supplyUsed.Length; i++)
            {
                supplyUsed[i] = new IntegerCache();
            }

            for (var i = 0; i < isResearching.Length; i++)
            {
                isResearching[i] = new BooleanCache();
            }

            for (var i = 0; i < isUpgrading.Length; i++)
            {
                isUpgrading[i] = new BooleanCache();
            }
        }
    }
}