namespace BWAPI.NET
{
    /// <summary>
    /// Enum containing text formatting codes.
    /// Such codes are used in calls to <see cref="Game.DrawText"/>, <see cref="Game.Printf"/>
    /// </summary>
    public enum Text
    {
        /// <summary>
        /// Uses the previous color that was specified before the current one.
        /// </summary>
        Previous = 1,
        /// <summary>
        /// Uses the default blueish color. This color is used in standard game messages.
        /// </summary>
        Default = 2,
        /// <summary>
        /// A solid yellow. This yellow is used in notifications and is also the default color when printing text to Broodwar.
        /// </summary>
        Yellow = 3,
        /// <summary>
        /// A bright white. This is used for timers.
        /// </summary>
        White = 4,
        /// <summary>
        /// A dark grey. This color code will override all color formatting that follows.
        /// </summary>
        Grey = 5,
        /// <summary>
        /// A deep red. This color code is used for error messages.
        /// </summary>
        Red = 6,
        /// <summary>
        /// A solid green. This color is used for sent messages and resource counters.
        /// </summary>
        Green = 7,
        /// <summary>
        /// A type of red. This color is used to color the name of the red player.
        /// </summary>
        BrightRed = 8,
        /// <summary>
        /// This code hides all text and formatting that follows.
        /// </summary>
        Invisible = 11,
        /// <summary>
        /// A deep blue. This color is used to color the name of the blue player.
        /// </summary>
        Blue = 14,
        /// <summary>
        /// A teal color. This color is used to color the name of the teal player.
        /// </summary>
        Teal = 15,
        /// <summary>
        /// A deep purple. This color is used to color the name of the purple player.
        /// </summary>
        Purple = 16,
        /// <summary>
        /// A solid orange. This color is used to color the name of the orange player.
        /// </summary>
        Orange = 17,
        /// <summary>
        /// An alignment directive that aligns the text to the right side of the screen.
        /// </summary>
        Align_Right = 18,
        /// <summary>
        /// An alignment directive that aligns the text to the center of the screen.
        /// </summary>
        Align_Center = 19,
        /// <summary>
        /// This code hides all text and formatting that follows.
        /// </summary>
        Invisible2 = 20,
        /// <summary>
        /// A dark brown. This color is used to color the name of the brown player.
        /// </summary>
        Brown = 21,
        /// <summary>
        /// A dirty white. This color is used to color the name of the white player.
        /// </summary>
        PlayerWhite = 22,
        /// <summary>
        /// A deep yellow. This color is used to color the name of the yellow player.
        /// </summary>
        PlayerYellow = 23,
        /// <summary>
        /// A dark green. This color is used to color the name of the green player.
        /// </summary>
        DarkGreen = 24,
        /// <summary>
        /// A bright yellow.
        /// </summary>
        LightYellow = 25,
        /// <summary>
        /// A cyan color. Similar to Default.
        /// </summary>
        Cyan = 26,
        /// <summary>
        /// A tan color.
        /// </summary>
        Tan = 27,
        /// <summary>
        /// A dark blueish color.
        /// </summary>
        GreyBlue = 28,
        /// <summary>
        /// A type of Green.
        /// </summary>
        GreyGreen = 29,
        /// <summary>
        /// A different type of Cyan.
        /// </summary>
        GreyCyan = 30,
        /// <summary>
        /// A bright blue color.
        /// </summary>
        Turquoise = 31
    }
}