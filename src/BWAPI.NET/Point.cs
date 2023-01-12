using System;

namespace BWAPI.NET
{
    public interface IPoint<T> : IEquatable<T>, IComparable<T>
        where T : IPoint<T>
    {
        int X { get; }

        int Y { get; }

        double GetLength();

        double GetDistance(T point);

        int GetApproxDistance(T point);

        T Add(T other);

        T Subtract(T other);

        T Multiply(int multiplier);

        T Divide(int divisor);

        T SetMax(T p);

        T SetMin(T p);

        bool IsValid(Game game);

        T MakeValid(Game game);
    }

    public static class PointHelper
    {
        /// <summary>
        /// The scale of a <see cref="Position"/>. Each position corresponds to a 1x1 pixel area.
        /// </summary>
        public const int PositionScale = 1;

        /// <summary>
        /// The scale of a <see cref="WalkPosition"/>. Each walk position corresponds to an 8x8 pixel area.
        /// </summary>
        public const int WalkPositionScale = 8;

        /// <summary>
        /// The scale of a <see cref="TilePosition"/>. Each tile position corresponds to a 32x32 pixel area.
        /// </summary>
        public const int TilePositionScale = 32;

        public const int TileWalkFactor = TilePositionScale / WalkPositionScale;

        public static double GetLength(int x, int y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static int GetApproxDistance(int x1, int y1, int x2, int y2)
        {
            var max = Math.Abs(x1 - x2);
            var min = Math.Abs(y1 - y2);
            if (max < min)
            {
                (max, min) = (min, max);
            }

            if (min <= (max >> 2))
            {
                return max;
            }

            var minCalc = (3 * min) >> 3;
            return (minCalc >> 5) + minCalc + max - (max >> 4) - (max >> 6);
        }

        public static bool IsValid(int x, int y, int scale, Game game)
        {
            // Not valid if < 0
            if (x < 0 || y < 0 )
            {
                return false;
            }

            // If Broodwar pointer is not initialized, just assume maximum map size
            if (game == null)
            {
                return x * scale < (256 * 32) && y * scale < (256 * 32);
            }

            // If BW ptr exists then compare with actual map size
            return x * scale < game.MapPixelWidth() &&  y * scale < game.MapPixelHeight();
        }

        public static void MakeValid(int x, int y, int scale, Game game, out int xout, out int yout)
        {
            // Set x/y to 0 if less than 0
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            // If broodwar ptr doesn't exist, set to below max size
            if (game == null)
            {
                var max = (256 * 32) / scale - 1;
                if (x > max) x = max;
                if (y > max) y = max;
            }
            else
            {
                var wid = game.MapPixelWidth() / scale - 1;
                var hgt = game.MapPixelHeight() / scale - 1;

                if (x > wid) x = wid;
                if (y > hgt) y = hgt;
            }

            xout = x;
            yout = y;
            return;
        }
    }
}