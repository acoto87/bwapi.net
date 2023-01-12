namespace BWAPI.NET
{
    public sealed class PlayerSelf
    {
        public readonly IntegerCache Minerals = new IntegerCache();
        public readonly IntegerCache Gas = new IntegerCache();
        public readonly IntegerCache[] SupplyUsed = new IntegerCache[3];
        public readonly BooleanCache[] IsResearching = new BooleanCache[(int)TechType.Unknown];
        public readonly BooleanCache[] IsUpgrading = new BooleanCache[(int)UpgradeType.Unknown];

        public PlayerSelf()
        {
            for (var i = 0; i < SupplyUsed.Length; i++)
            {
                SupplyUsed[i] = new IntegerCache();
            }

            for (var i = 0; i < IsResearching.Length; i++)
            {
                IsResearching[i] = new BooleanCache();
            }

            for (var i = 0; i < IsUpgrading.Length; i++)
            {
                IsUpgrading[i] = new BooleanCache();
            }
        }
    }
}