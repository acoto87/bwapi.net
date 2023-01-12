namespace BWAPI.NET
{
    /// <summary>
    /// Represents the type of controller for the player slot (i.e. human, computer).
    /// </summary>
    public enum PlayerType
    {
        None = 0,
        Computer = 1,
        Player = 2,
        RescuePassive = 3,
        RescueActive = 4,
        EitherPreferComputer = 5,
        EitherPreferHuman = 6,
        Neutral = 7,
        Closed = 8,
        Observer = 9,
        PlayerLeft = 10,
        ComputerLeft = 11,
        Unknown = 12
    }
}