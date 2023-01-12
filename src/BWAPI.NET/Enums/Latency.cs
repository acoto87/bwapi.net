namespace BWAPI.NET
{
    /// <summary>
    /// Contains enumeration of known latency values.
    /// </summary>
    /// <remarks>
    /// <see cref="Game.GetLatency"/>.
    /// </remarks>
    public enum Latency
    {
        SinglePlayer = 2,
        LanLow = 5,
        LanMedium = 7,
        LanHigh = 9,
        BattlenetLow = 14,
        BattlenetMedium = 19,
        BattlenetHigh = 24
    }
}