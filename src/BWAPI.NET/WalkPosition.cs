namespace BWAPI.NET
{
    public class WalkPosition : Point<WalkPosition>
    {
        public static readonly int SIZE_IN_PIXELS = 8;
        public static readonly WalkPosition Invalid = new WalkPosition(32000 / SIZE_IN_PIXELS, 32000 / SIZE_IN_PIXELS);
        public static readonly WalkPosition None = new WalkPosition(32000 / SIZE_IN_PIXELS, 32032 / SIZE_IN_PIXELS);
        public static readonly WalkPosition Unknown = new WalkPosition(32000 / SIZE_IN_PIXELS, 32064 / SIZE_IN_PIXELS);
        public static readonly WalkPosition Origin = new WalkPosition(0, 0);

        public WalkPosition(int x, int y) : base(x, y, SIZE_IN_PIXELS)
        {
        }

        public WalkPosition(Position p) : this(p.x / SIZE_IN_PIXELS, p.y / SIZE_IN_PIXELS)
        {
        }

        public WalkPosition(WalkPosition wp) : this(wp.x, wp.y)
        {
        }

        public WalkPosition(TilePosition tp) : this(tp.x * TILE_WALK_FACTOR, tp.y * TILE_WALK_FACTOR)
        {
        }

        public virtual Position ToPosition()
        {
            return new Position(this);
        }

        public virtual TilePosition ToTilePosition()
        {
            return new TilePosition(this);
        }

        public override WalkPosition Subtract(WalkPosition other)
        {
            return new WalkPosition(x - other.x, y - other.y);
        }

        public override WalkPosition Add(WalkPosition other)
        {
            return new WalkPosition(x + other.x, y + other.y);
        }

        public override WalkPosition Divide(int divisor)
        {
            return new WalkPosition(x / divisor, y / divisor);
        }

        public override WalkPosition Multiply(int multiplier)
        {
            return new WalkPosition(x * multiplier, y * multiplier);
        }
    }
}