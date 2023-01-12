using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BWAPI.NET
{
    public sealed class Position : Point<Position>
    {
        public static readonly int SIZE_IN_PIXELS = 1;
        public static readonly Position Invalid = new Position(32000 / SIZE_IN_PIXELS, 32000 / SIZE_IN_PIXELS);
        public static readonly Position None = new Position(32000 / SIZE_IN_PIXELS, 32032 / SIZE_IN_PIXELS);
        public static readonly Position Unknown = new Position(32000 / SIZE_IN_PIXELS, 32064 / SIZE_IN_PIXELS);
        public static readonly Position Origin = new Position(0, 0);
        public Position(int x, int y) : base(x, y, SIZE_IN_PIXELS)
        {
        }

        public Position(Position p)
            : this(p.x, p.y)
        {
        }

        public Position(WalkPosition wp)
            : this(wp.x * WalkPosition.SIZE_IN_PIXELS, wp.y * WalkPosition.SIZE_IN_PIXELS)
        {
        }

        public Position(TilePosition tp)
            : this(tp.x * TilePosition.SIZE_IN_PIXELS, tp.y * TilePosition.SIZE_IN_PIXELS)
        {
        }

        public Position(ClientData.Position position)
            : this(position.GetX(), position.GetY())
        {
        }

        public TilePosition ToTilePosition()
        {
            return new TilePosition(this);
        }

        public WalkPosition ToWalkPosition()
        {
            return new WalkPosition(this);
        }

        public override Position Subtract(Position other)
        {
            return new Position(x - other.x, y - other.y);
        }

        public override Position Add(Position other)
        {
            return new Position(x + other.x, y + other.y);
        }

        public override Position Divide(int divisor)
        {
            return new Position(x / divisor, y / divisor);
        }

        public override Position Multiply(int multiplier)
        {
            return new Position(x * multiplier, y * multiplier);
        }
    }
}