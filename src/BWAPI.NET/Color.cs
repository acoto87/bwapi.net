using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BWAPI.NET
{
    /// <summary>
    /// The Color object is used in drawing routines to specify the color to use.
    /// <p>
    /// Starcraft uses a 256 color palette for rendering. Thus, the colors available are
    /// limited to this palette.
    /// </summary>
    public sealed class Color
    {
        /// <summary>
        /// The default color for Player 1.
        /// </summary>
        public static readonly Color Red = new Color(111);
        /// <summary>
        /// The default color for Player 2.
        /// </summary>
        public static readonly Color Blue = new Color(165);
        /// <summary>
        /// The default color for Player 3.
        /// </summary>
        public static readonly Color Teal = new Color(159);
        /// <summary>
        /// The default color for Player 4.
        /// </summary>
        public static readonly Color Purple = new Color(164);
        /// <summary>
        /// The default color for Player 5.
        /// </summary>
        public static readonly Color Orange = new Color(156);
        /// <summary>
        /// The default color for Player 6.
        /// </summary>
        public static readonly Color Brown = new Color(19);
        /// <summary>
        /// A bright white. Note that this is lighter than Player 7's white.
        /// </summary>
        public static readonly Color White = new Color(255);
        /// <summary>
        /// The default color for Player 8.
        /// </summary>
        public static readonly Color Yellow = new Color(135);
        /// <summary>
        /// The alternate color for Player 7 on Ice tilesets.
        /// </summary>
        public static readonly Color Green = new Color(117);
        /// <summary>
        /// The default color for Neutral (Player 12).
        /// </summary>
        public static readonly Color Cyan = new Color(128);
        /// <summary>
        /// The color black.
        /// </summary>
        public static readonly Color Black = new Color(0);
        /// <summary>
        /// The color grey.
        /// </summary>
        public static readonly Color Grey = new Color(74);

        private static Dictionary<int, string> defaultColors = null;
        private static readonly RGBQUAD RGBRESERVE = new RGBQUAD(0, 0, 0, 0xFF);
        private static readonly RGBQUAD[] defaultPalette = new[] { new RGBQUAD(0, 0, 0), RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, new RGBQUAD(24, 36, 44), new RGBQUAD(72, 36, 20), new RGBQUAD(92, 44, 20), new RGBQUAD(112, 48, 20), new RGBQUAD(104, 60, 36), new RGBQUAD(124, 64, 24), new RGBQUAD(120, 76, 44), new RGBQUAD(168, 8, 8), new RGBQUAD(140, 84, 48), new RGBQUAD(132, 96, 68), new RGBQUAD(160, 84, 28), new RGBQUAD(196, 76, 24), new RGBQUAD(188, 104, 36), new RGBQUAD(180, 112, 60), new RGBQUAD(208, 100, 32), new RGBQUAD(220, 148, 52), new RGBQUAD(224, 148, 84), new RGBQUAD(236, 196, 84), new RGBQUAD(52, 68, 40), new RGBQUAD(64, 108, 60), new RGBQUAD(72, 108, 80), new RGBQUAD(76, 128, 80), new RGBQUAD(80, 140, 92), new RGBQUAD(92, 160, 120), new RGBQUAD(0, 0, 24), new RGBQUAD(0, 16, 52), new RGBQUAD(0, 8, 80), new RGBQUAD(36, 52, 72), new RGBQUAD(48, 64, 84), new RGBQUAD(20, 52, 124), new RGBQUAD(52, 76, 108), new RGBQUAD(64, 88, 116), new RGBQUAD(72, 104, 140), new RGBQUAD(0, 112, 156), new RGBQUAD(88, 128, 164), new RGBQUAD(64, 104, 212), new RGBQUAD(24, 172, 184), new RGBQUAD(36, 36, 252), new RGBQUAD(100, 148, 188), new RGBQUAD(112, 168, 204), new RGBQUAD(140, 192, 216), new RGBQUAD(148, 220, 244), new RGBQUAD(172, 220, 232), new RGBQUAD(172, 252, 252), new RGBQUAD(204, 248, 248), new RGBQUAD(252, 252, 0), new RGBQUAD(244, 228, 144), new RGBQUAD(252, 252, 192), new RGBQUAD(12, 12, 12), new RGBQUAD(24, 20, 16), new RGBQUAD(28, 28, 32), new RGBQUAD(40, 40, 48), new RGBQUAD(56, 48, 36), new RGBQUAD(56, 60, 68), new RGBQUAD(76, 64, 48), new RGBQUAD(76, 76, 76), new RGBQUAD(92, 80, 64), new RGBQUAD(88, 88, 88), new RGBQUAD(104, 104, 104), new RGBQUAD(120, 132, 108), new RGBQUAD(104, 148, 108), new RGBQUAD(116, 164, 124), new RGBQUAD(152, 148, 140), new RGBQUAD(144, 184, 148), new RGBQUAD(152, 196, 168), new RGBQUAD(176, 176, 176), new RGBQUAD(172, 204, 176), new RGBQUAD(196, 192, 188), new RGBQUAD(204, 224, 208), new RGBQUAD(240, 240, 240), new RGBQUAD(28, 16, 8), new RGBQUAD(40, 24, 12), new RGBQUAD(52, 16, 8), new RGBQUAD(52, 32, 12), new RGBQUAD(56, 16, 32), new RGBQUAD(52, 40, 32), new RGBQUAD(68, 52, 8), new RGBQUAD(72, 48, 24), new RGBQUAD(96, 0, 0), new RGBQUAD(84, 40, 32), new RGBQUAD(80, 64, 20), new RGBQUAD(92, 84, 20), new RGBQUAD(132, 4, 4), new RGBQUAD(104, 76, 52), new RGBQUAD(124, 56, 48), new RGBQUAD(112, 100, 32), new RGBQUAD(124, 80, 80), new RGBQUAD(164, 52, 28), new RGBQUAD(148, 108, 0), new RGBQUAD(152, 92, 64), new RGBQUAD(140, 128, 52), new RGBQUAD(152, 116, 84), new RGBQUAD(184, 84, 68), new RGBQUAD(176, 144, 24), new RGBQUAD(176, 116, 92), new RGBQUAD(244, 4, 4), new RGBQUAD(200, 120, 84), new RGBQUAD(252, 104, 84), new RGBQUAD(224, 164, 132), new RGBQUAD(252, 148, 104), new RGBQUAD(252, 204, 44), new RGBQUAD(16, 252, 24), new RGBQUAD(12, 0, 32), new RGBQUAD(28, 28, 44), new RGBQUAD(36, 36, 76), new RGBQUAD(40, 44, 104), new RGBQUAD(44, 48, 132), new RGBQUAD(32, 24, 184), new RGBQUAD(52, 60, 172), new RGBQUAD(104, 104, 148), new RGBQUAD(100, 144, 252), new RGBQUAD(124, 172, 252), new RGBQUAD(0, 228, 252), new RGBQUAD(156, 144, 64), new RGBQUAD(168, 148, 84), new RGBQUAD(188, 164, 92), new RGBQUAD(204, 184, 96), new RGBQUAD(232, 216, 128), new RGBQUAD(236, 196, 176), new RGBQUAD(252, 252, 56), new RGBQUAD(252, 252, 124), new RGBQUAD(252, 252, 164), new RGBQUAD(8, 8, 8), new RGBQUAD(16, 16, 16), new RGBQUAD(24, 24, 24), new RGBQUAD(40, 40, 40), new RGBQUAD(52, 52, 52), new RGBQUAD(76, 60, 56), new RGBQUAD(68, 68, 68), new RGBQUAD(72, 72, 88), new RGBQUAD(88, 88, 104), new RGBQUAD(116, 104, 56), new RGBQUAD(120, 100, 92), new RGBQUAD(96, 96, 124), new RGBQUAD(132, 116, 116), new RGBQUAD(132, 132, 156), new RGBQUAD(172, 140, 124), new RGBQUAD(172, 152, 148), new RGBQUAD(144, 144, 184), new RGBQUAD(184, 184, 232), new RGBQUAD(248, 140, 20), new RGBQUAD(16, 84, 60), new RGBQUAD(32, 144, 112), new RGBQUAD(44, 180, 148), new RGBQUAD(4, 32, 100), new RGBQUAD(72, 28, 80), new RGBQUAD(8, 52, 152), new RGBQUAD(104, 48, 120), new RGBQUAD(136, 64, 156), new RGBQUAD(12, 72, 204), new RGBQUAD(188, 184, 52), new RGBQUAD(220, 220, 60), new RGBQUAD(16, 0, 0), new RGBQUAD(36, 0, 0), new RGBQUAD(52, 0, 0), new RGBQUAD(72, 0, 0), new RGBQUAD(96, 24, 4), new RGBQUAD(140, 40, 8), new RGBQUAD(200, 24, 24), new RGBQUAD(224, 44, 44), new RGBQUAD(232, 32, 32), new RGBQUAD(232, 80, 20), new RGBQUAD(252, 32, 32), new RGBQUAD(232, 120, 36), new RGBQUAD(248, 172, 60), new RGBQUAD(0, 20, 0), new RGBQUAD(0, 40, 0), new RGBQUAD(0, 68, 0), new RGBQUAD(0, 100, 0), new RGBQUAD(8, 128, 8), new RGBQUAD(36, 152, 36), new RGBQUAD(60, 156, 60), new RGBQUAD(88, 176, 88), new RGBQUAD(104, 184, 104), new RGBQUAD(128, 196, 128), new RGBQUAD(148, 212, 148), new RGBQUAD(12, 20, 36), new RGBQUAD(36, 60, 100), new RGBQUAD(48, 80, 132), new RGBQUAD(56, 92, 148), new RGBQUAD(72, 116, 180), new RGBQUAD(84, 132, 196), new RGBQUAD(96, 148, 212), new RGBQUAD(120, 180, 236), RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, RGBRESERVE, new RGBQUAD(255, 255, 255) };
        private static byte[][][] closestColor = null;

        public readonly int id;

        /// <summary>
        /// A constructor that uses the color index in the palette that is closest to the
        /// given rgb values. On its first call, the colors in the palette will be sorted for fast indexing.
        /// <p>
        /// This function computes the distance of the RGB values and may not be accurate.
        /// </summary>
        /// <param name="red">The amount of red.</param>
        /// <param name="green">The amount of green.</param>
        /// <param name="blue">The amount of blue.</param>
        public Color(int red, int green, int blue)
        {
            id = GetRGBIndex(red, green, blue);
        }

        /// <summary>
        /// A constructor that uses the color at the specified palette index.
        /// </summary>
        /// <param name="id">The index of the color in the 256-color palette.</param>
        public Color(int id)
        {

            // The id is set to 255 if the id is invalid, as in the official BWAPI sources:
            // https://github.com/bwapi/bwapi/blob/3438abd8e0222f37934ba62b2130c3933b067678/bwapi/include/BWAPI/Color.h#L13
            // https://github.com/bwapi/bwapi/blob/3438abd8e0222f37934ba62b2130c3933b067678/bwapi/include/BWAPI/Type.h#L66
            this.id = id < 0 || id > 255 ? 255 : id;
        }

        private static int GetBestIdFor(int red, int green, int blue)
        {
            int min_dist = 3 * 256 * 256;
            int best_id = 0;
            for (int id = 0; id < 255; ++id)
            {
                RGBQUAD p = defaultPalette[id];
                if (p.rgbReserved != 0)
                {
                    continue;
                }

                int r = red - (p.rgbRed & 0xFF);
                int g = green - (p.rgbGreen & 0xFF);
                int b = blue - (p.rgbBlue & 0xFF);
                int distance = r * r + g * g + b * b;
                if (distance < min_dist)
                {
                    min_dist = distance;
                    best_id = id;
                    if (distance == 0)
                    {
                        break;
                    }
                }
            }

            return best_id;
        }

        private static int GetRGBIndex(int red, int green, int blue)
        {
            if (closestColor == null)
            {
                closestColor = new byte[64][][];
                for (int r = 0; r < 64; ++r)
                {
                    closestColor[r] = new byte[64][];
                    for (int g = 0; g < 64; ++g)
                    {
                        closestColor[r][g] = new byte[64];
                        for (int b = 0; b < 64; ++b)
                        {
                            closestColor[r][g][b] = (byte)GetBestIdFor(r << 2, g << 2, b << 2);
                        }
                    }
                }
            }

            return closestColor[red >> 2][green >> 2][blue >> 2] & 0xFF;
        }

        public int RedChannel()
        {
            return defaultPalette[id].rgbRed & 0xFF;
        }

        public int GreenChannel()
        {
            return defaultPalette[id].rgbGreen & 0xFF;
        }

        public int BlueChannel()
        {
            return defaultPalette[id].rgbBlue & 0xFF;
        }

        public override bool Equals(object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            Color color = (Color)o;
            return id == color.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            string defaultColor = GetDefaultColor(id);
            if (defaultColor != null)
            {
                return "Color." + defaultColor;
            }

            return "Color{" + "red=" + RedChannel() + ", green=" + GreenChannel() + ", blue=" + BlueChannel() + "}";
        }

        /// BROODWAR COLOR IMPLEMENTATION
        private class RGBQUAD
        {
            public readonly byte rgbRed;
            public readonly byte rgbGreen;
            public readonly byte rgbBlue;
            public readonly byte rgbReserved;

            public RGBQUAD(int rgbRed, int rgbGreen, int rgbBlue)
                : this(rgbRed, rgbGreen, rgbBlue, 0)
            {
            }

            public RGBQUAD(int rgbRed, int rgbGreen, int rgbBlue, int rgbReserved)
            {
                this.rgbRed = (byte)rgbRed;
                this.rgbGreen = (byte)rgbGreen;
                this.rgbBlue = (byte)rgbBlue;
                this.rgbReserved = (byte)rgbReserved;
            }
        }

        private static string GetDefaultColor(int id)
        {
            if (defaultColors == null)
            {
                defaultColors = new Dictionary<int, string>();
                defaultColors.Add(Color.Red.id, "Red");
                defaultColors.Add(Color.Blue.id, "Blue");
                defaultColors.Add(Color.Teal.id, "Teal");
                defaultColors.Add(Color.Purple.id, "Purple");
                defaultColors.Add(Color.Orange.id, "Orange");
                defaultColors.Add(Color.Brown.id, "Brown");
                defaultColors.Add(Color.White.id, "White");
                defaultColors.Add(Color.Yellow.id, "Yellow");
                defaultColors.Add(Color.Green.id, "Green");
                defaultColors.Add(Color.Cyan.id, "Cyan");
                defaultColors.Add(Color.Black.id, "Black");
                defaultColors.Add(Color.Grey.id, "Grey");
            }

            return defaultColors.GetValueOrDefault(id, null);
        }
    }
}