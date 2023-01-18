using System;

namespace BWAPI.NET
{
    public readonly struct WalkPosition : IPoint<WalkPosition>
    {
        /// <summary>
        /// The scale of a <see cref="WalkPosition"/>. Each walk position corresponds to an 8x8 pixel area.
        /// </summary>
        public const int Scale = PointHelper.WalkPositionScale;

        public static readonly WalkPosition Invalid = new WalkPosition(32000 / PointHelper.WalkPositionScale, 32000 / PointHelper.WalkPositionScale);
        public static readonly WalkPosition None = new WalkPosition(32000 / PointHelper.WalkPositionScale, 32032 / PointHelper.WalkPositionScale);
        public static readonly WalkPosition Unknown = new WalkPosition(32000 / PointHelper.WalkPositionScale, 32064 / PointHelper.WalkPositionScale);
        public static readonly WalkPosition Origin = new WalkPosition(0, 0);

        public readonly int x;
        public readonly int y;

        public WalkPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public WalkPosition(Position p)
            : this(p.x / PointHelper.WalkPositionScale, p.y / PointHelper.WalkPositionScale)
        {
        }

        public WalkPosition(TilePosition tp)
            : this(tp.x * PointHelper.TileWalkFactor, tp.y * PointHelper.TileWalkFactor)
        {
        }

        public WalkPosition Add(WalkPosition other)
        {
            return this + other;
        }

        public WalkPosition Add(int value)
        {
            return this + new WalkPosition(value, value);
        }

        public WalkPosition Subtract(WalkPosition other)
        {
            return this - other;
        }

        public WalkPosition Subtract(int value)
        {
            return this - new WalkPosition(value, value);
        }

        public WalkPosition Multiply(int multiplier)
        {
            return this * multiplier;
        }

        public WalkPosition Divide(int divisor)
        {
            return this / divisor;
        }

        public static bool operator ==(WalkPosition p1, WalkPosition p2)
        {
            return p1.x == p2.x && p1.y == p2.y;
        }

        public static bool operator !=(WalkPosition p1, WalkPosition p2)
        {
            return !(p1 == p2);
        }

        public static bool operator <(WalkPosition left, WalkPosition right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(WalkPosition left, WalkPosition right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(WalkPosition left, WalkPosition right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(WalkPosition left, WalkPosition right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static WalkPosition operator +(WalkPosition p1, WalkPosition p2)
        {
            return new WalkPosition(p1.x + p2.x, p1.y + p2.y);
        }

        public static WalkPosition operator +(WalkPosition p1, int value)
        {
            return new WalkPosition(p1.x + value, p1.y + value);
        }

        public static WalkPosition operator +(int value, WalkPosition p1)
        {
            return new WalkPosition(p1.x + value, p1.y + value);
        }

        public static WalkPosition operator -(WalkPosition p1, WalkPosition p2)
        {
            return new WalkPosition(p1.x - p2.x, p1.y - p2.y);
        }

        public static WalkPosition operator -(WalkPosition p1, int value)
        {
            return new WalkPosition(p1.x - value, p1.y - value);
        }

        public static WalkPosition operator -(int value, WalkPosition p1)
        {
            return new WalkPosition(value - p1.x, value - p1.y);
        }

        public static WalkPosition operator *(WalkPosition p1, int multiplier)
        {
            return new WalkPosition(p1.x * multiplier, p1.y * multiplier);
        }

        public static WalkPosition operator /(WalkPosition p1, int divisor)
        {
            return new WalkPosition(p1.x / divisor, p1.y / divisor);
        }

        public int CompareTo(WalkPosition other)
        {
            return x == other.x ? y.CompareTo(other.y) : x.CompareTo(other.x);
        }

        public int GetApproxDistance(WalkPosition point)
        {
            return PointHelper.GetApproxDistance(x, y, point.x, point.y);
        }

        public double GetDistance(WalkPosition point)
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

        public WalkPosition MakeValid(Game game)
        {
            PointHelper.MakeValid(x, y, PointHelper.PositionScale, game, out int newx, out int newy);
            return new WalkPosition(newx, newy);
        }

        public WalkPosition SetMax(WalkPosition p)
        {
            return new WalkPosition(Math.Max(x, p.x), Math.Max(y, p.y));
        }

        public WalkPosition SetMin(WalkPosition p)
        {
            return new WalkPosition(Math.Min(x, p.x), Math.Min(y, p.y));
        }

        public bool Equals(WalkPosition other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is WalkPosition p && Equals(p);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }

        public Position ToPosition()
        {
            return new Position(this);
        }

        public TilePosition ToTilePosition()
        {
            return new TilePosition(this);
        }

        public int X => x;

        public int Y => y;
    }
}