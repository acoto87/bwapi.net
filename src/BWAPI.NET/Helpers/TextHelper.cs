namespace BWAPI.NET
{
    public static class TextHelper
    {
        /// <summary>
        /// Format text with a textcolor to display on broodwar
        /// </summary>
        public static string FormatText(string text, Text format)
        {
            return Game.FormatString("%c" + text, format);
        }

        /// <summary>
        /// Checks if the given character is a color-changing control code.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// true if <paramref name="text"/> is a regular color, not <see cref="Text.Previous"/>,
        /// <see cref="Text.Invisible"/>, <see cref="Text.Invisible2"/>, <see cref="Text.Align_Right"/> or
        /// <see cref="Text.Align_Center"/>
        /// </returns>
        /// <remarks>
        /// Since 4.2.0
        /// </remarks>
        public static bool IsColor(this Text text)
        {
            var c = (int)text;
            return (2 <= c && c <= 8) || (14 <= c && c <= 17) || (21 <= c && c <= 31);
        }
    }
}