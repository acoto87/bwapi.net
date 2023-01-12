namespace BWAPI.NET
{
    public sealed class TilePosition : Point<TilePosition>
    {
        public static readonly int SIZE_IN_PIXELS = 32;
        public static readonly TilePosition Invalid = new TilePosition(32000 / SIZE_IN_PIXELS, 32000 / SIZE_IN_PIXELS);
        public static readonly TilePosition None = new TilePosition(32000 / SIZE_IN_PIXELS, 32032 / SIZE_IN_PIXELS);
        public static readonly TilePosition Unknown = new TilePosition(32000 / SIZE_IN_PIXELS, 32064 / SIZE_IN_PIXELS);
        public static readonly TilePosition Origin = new TilePosition(0, 0);

        public TilePosition(int x, int y)
            : base(x, y, SIZE_IN_PIXELS)
        {
        }

        public TilePosition(Position p)
            : this(p.x / SIZE_IN_PIXELS, p.y / SIZE_IN_PIXELS)
        {
        }

        public TilePosition(WalkPosition wp)
            : this(wp.x / WalkPosition.TILE_WALK_FACTOR, wp.y / WalkPosition.TILE_WALK_FACTOR)
        {
        }

        public TilePosition(TilePosition tp)
            : this(tp.x, tp.y)
        {
        }

        public TilePosition(ClientData.Position position)
            : this(position.GetX(), position.GetY())
        {
        }

        public Position ToPosition()
        {
            return new Position(this);
        }

        public WalkPosition ToWalkPosition()
        {
            return new WalkPosition(this);
        }

        public override TilePosition Subtract(TilePosition other)
        {
            return new TilePosition(x - other.x, y - other.y);
        }

        public override TilePosition Add(TilePosition other)
        {
            return new TilePosition(x + other.x, y + other.y);
        }

        public override TilePosition Divide(int divisor)
        {
            return new TilePosition(x / divisor, y / divisor);
        }

        public override TilePosition Multiply(int multiplier)
        {
            return new TilePosition(x * multiplier, y * multiplier);
        }
    }
}