namespace BWAPI.NET
{
    /// <summary>
    /// Contains flag enumerations for BWAPI.
    /// </summary>
    /// <remarks>
    /// <seealso cref="Game.EnableFlag(Flag)"/>
    /// <seealso cref="Game.IsFlagEnabled(Flag)"/>
    /// </remarks>
    public enum Flag
    {
        /// <summary>
        /// Enable to get information about all units on the map, not just the visible units.
        /// </summary>
        CompleteMapInformation = 0,
        /// <summary>
        /// Enable to get information from the user (what units are selected, chat messages the user enters, etc)
        /// </summary>
        UserInput = 1
    }
}