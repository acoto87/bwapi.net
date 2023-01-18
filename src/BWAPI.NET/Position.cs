using System;

namespace BWAPI.NET
{
    public readonly struct Position : IPoint<Position>
    {
        /// <summary>
        /// The scale of a <see cref="Position"/>. Each position corresponds to a 1x1 pixel area.
        /// </summary>
        public const int Scale = PointHelper.PositionScale;

        public static readonly Position Invalid = new Position(32000 / PointHelper.PositionScale, 32000 / PointHelper.PositionScale);
        public static readonly Position None = new Position(32000 / PointHelper.PositionScale, 32032 / PointHelper.PositionScale);
        public static readonly Position Unknown = new Position(32000 / PointHelper.PositionScale, 32064 / PointHelper.PositionScale);
        public static readonly Position Origin = new Position(0, 0);

        public readonly int x;
        public readonly int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Position(WalkPosition wp)
            : this(wp.x * PointHelper.WalkPositionScale, wp.y * PointHelper.WalkPositionScale)
        {
        }

        public Position(TilePosition tp)
            : this(tp.x * PointHelper.TilePositionScale, tp.y * PointHelper.TilePositionScale)
        {
        }

        public Position(ClientData.Position position)
            : this(position.GetX(), position.GetY())
        {
        }

        public Position Add(Position other)
        {
            return this + other;
        }

        public Position Add(int value)
        {
            return this + new Position(value, value);
        }

        public Position Subtract(Position other)
        {
            return this - other;
        }

        public Position Subtract(int value)
        {
            return this - new Position(value, value);
        }

        public Position Multiply(int multiplier)
        {
            return this * multiplier;
        }

        public Position Divide(int divisor)
        {
            return this / divisor;
        }

        public static bool operator ==(Position p1, Position p2)
        {
            return p1.x == p2.x && p1.y == p2.y;
        }

        public static bool operator !=(Position p1, Position p2)
        {
            return !(p1 == p2);
        }

        public static bool operator <(Position left, Position right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Position left, Position right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Position left, Position right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Position left, Position right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static Position operator +(Position p1, Position p2)
        {
            return new Position(p1.x + p2.x, p1.y + p2.y);
        }

        public static Position operator +(Position p1, int value)
        {
            return new Position(p1.x + value, p1.y + value);
        }

        public static Position operator +(int value, Position p1)
        {
            return new Position(p1.x + value, p1.y + value);
        }

        public static Position operator -(Position p1, Position p2)
        {
            return new Position(p1.x - p2.x, p1.y - p2.y);
        }

        public static Position operator -(Position p1, int value)
        {
            return new Position(p1.x - value, p1.y - value);
        }

        public static Position operator -(int value, Position p1)
        {
            return new Position(value - p1.x, value - p1.y);
        }

        public static Position operator *(Position p1, int multiplier)
        {
            return new Position(p1.x * multiplier, p1.y * multiplier);
        }

        public static Position operator /(Position p1, int divisor)
        {
            return new Position(p1.x / divisor, p1.y / divisor);
        }

        public int CompareTo(Position other)
        {
            return x == other.x ? y.CompareTo(other.y) : x.CompareTo(other.x);
        }

        public int GetApproxDistance(Position point)
        {
            return PointHelper.GetApproxDistance(x, y, point.x, point.y);
        }

        public double GetDistance(Position point)
        {
            return this.Subtract(point).GetLength();
        }

        public double GetLength()
        {
            return PointHelper.GetLength(x, y);
        }

        public bool IsValid(Game game)
        {
            return PointHelper.IsValid(x, y, PointHelper.PositionScale, game);
        }

        public Position MakeValid(Game game)
        {
            PointHelper.MakeValid(x, y, PointHelper.PositionScale, game, out int newx, out int newy);
            return new Position(newx, newy);
        }

        public Position SetMax(Position p)
        {
            return new Position(Math.Max(x, p.x), Math.Max(y, p.y));
        }

        public Position SetMin(Position p)
        {
            return new Position(Math.Min(x, p.x), Math.Min(y, p.y));
        }

        public bool Equals(Position other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Position p && Equals(p);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }

        public TilePosition ToTilePosition()
        {
            return new TilePosition(this);
        }

        public WalkPosition ToWalkPosition()
        {
            return new WalkPosition(this);
        }

        public int X => x;

        public int Y => y;
    }
}