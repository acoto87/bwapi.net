namespace BWAPI.NET
{
    /// <summary>
    /// The coordinate type enumeration for relative drawing positions.
    /// </summary>
    public enum CoordinateType
    {
        /// <summary>
        /// A default value for uninitialized coordinate types.
        /// </summary>
        None = 0,
        /// <summary>
        /// <see cref="Position.Origin"/> (0,0) corresponds to the top left corner of the <b>screen</b>
        /// </summary>
        Screen = 1,
        /// <summary>
        /// <see cref="Position.Origin"/> (0,0) corresponds to the top left corner of the <b>map</b>
        /// </summary>
        Map = 2,
        /// <summary>
        /// <see cref="Position.Origin"/> (0,0) corresponds to the top left corner of the <b>mouse cursor</b>
        /// </summary>
        Mouse = 3
    }
}