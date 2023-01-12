using System;

namespace BWAPI.NET
{
    public abstract class Point<T> : IComparable<Point<T>>
        where T : Point<T>
    {
        public const int TILE_WALK_FACTOR = 4; // 32 / 8

        public readonly int x;
        public readonly int y;
        private readonly int scalar;

        protected Point(int x, int y, int type)
        {
            this.x = x;
            this.y = y;
            scalar = type;
        }

        public virtual int GetX()
        {
            return x;
        }

        public virtual int GetY()
        {
            return y;
        }

        public virtual double GetLength()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public virtual double GetDistance(T point)
        {
            int dx = point.x - x;
            int dy = point.y - y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static int GetApproxDistance(int x1, int y1, int x2, int y2)
        {
            var max = Math.Abs(x1 - x2);
            var min = Math.Abs(y1 - y2);
            if (max < min)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            if (min <= (max >> 2))
            {
                return max;
            }

            var minCalc = (3 * min) >> 3;
            return (minCalc >> 5) + minCalc + max - (max >> 4) - (max >> 6);
        }

        public virtual int GetApproxDistance(T point)
        {
            return GetApproxDistance(x, y, point.x, point.y);
        }

        public abstract T Subtract(T other);
        public abstract T Add(T other);
        public abstract T Divide(int divisor);
        public abstract T Multiply(int multiplier);
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            var point = (Point<T>)o;
            return x == point.x && y == point.y;
        }

        /// <summary>
        /// Check if the current point is a valid point for the current game
        /// </summary>
        public virtual bool IsValid(Game game)
        {
            return x >= 0 && y >= 0 && scalar * x < game.MapPixelWidth() && scalar * y < game.MapPixelHeight();
        }

        public override int GetHashCode()
        {
            return (x << 16) ^ y;
        }

        public int CompareTo(Point<T> o)
        {
            if (scalar == o.scalar)
            {
                return GetHashCode() - o.GetHashCode();
            }

            return scalar - o.scalar;
        }

        public override string ToString()
        {
            return "[" + x + ", " + y + "]";
        }
    }
}