namespace BWAPI.NET
{
    /// <summary>
    /// Helper methods over <see cref="PlayerType"/>.
    /// </summary>
    public static class PlayerTypeHelper
    {
        /// <summary>
        /// Identifies whether or not this type is used for the pre-game lobby.
        /// A type such as <see cref="PlayerType.ComputerLeft" would only appear in-game when a computer player is defeated.
        /// </summary>
        /// <param name="playerType">The player type.</param>
        /// <returns>true if this type can appear in the pre-game lobby, false otherwise.</returns>
        public static bool IsLobbyType(this PlayerType playerType)
        {
            return playerType == PlayerType.EitherPreferComputer || playerType == PlayerType.EitherPreferHuman || IsRescueNeutralType(playerType);
        }

        /// <summary>
        /// Identifies whether or not this type is used in-game. A type such as
        /// <see cref="PlayerType.Closed"/> would not be a valid in-game type.
        /// </summary>
        /// <param name="playerType">The player type.</param>
        /// <returns>true if the type can appear in-game, false otherwise.</returns>
        /// <remarks>
        /// <seealso cref="IsLobbyType"/>.
        /// </remarks>
        public static bool IsGameType(this PlayerType playerType)
        {
            return playerType == PlayerType.Player || playerType == PlayerType.Computer || IsRescueNeutralType(playerType);
        }

        /// <summary>
        /// Identifies whether or not this type is a rescue-neutral.
        /// </summary>
        /// <param name="playerType">The player type.</param>
        /// <returns>true if the type is rescue-neutral.</returns>
        public static bool IsRescueNeutralType(PlayerType playerType)
        {
            return playerType == PlayerType.RescuePassive || playerType == PlayerType.RescueActive || playerType == PlayerType.Neutral;
        }
    }
}