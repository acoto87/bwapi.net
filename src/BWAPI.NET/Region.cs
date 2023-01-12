using System;
using System.Collections.Generic;

namespace BWAPI.NET
{
    /// <summary>
    /// Region objects are created by Starcraft: Broodwar to contain several tiles with the same
    /// properties, and create a node in pathfinding and other algorithms. Regions may not contain
    /// detailed information, but have a sufficient amount of data to identify general chokepoints,
    /// accessibility to neighboring terrain, be used in general pathing algorithms, and used as
    /// nodes to rally units to.
    /// <p>
    /// Most parameters that are available are explicitly assigned by Broodwar itself.
    /// </summary>
    /// <remarks>
    /// @seeGame#getAllRegions
    /// @seeGame#getRegionAt
    /// @seeUnit#getRegion
    /// </remarks>
    public sealed class Region : IEquatable<Region>, IComparable<Region>
    {
        private readonly int _id;
        private readonly int _regionGroupID;
        private readonly ClientData.RegionData _regionData;
        private readonly Game _game;
        private readonly Position _center;
        private readonly bool _higherGround;
        private readonly int _defensePriority;
        private readonly bool _accessible;
        private readonly int _boundsLeft;
        private readonly int _boundsTop;
        private readonly int _boundsRight;
        private readonly int _boundsBottom;
        private Region _closestAccessibleRegion;
        private Region _closestInaccessibleRegion;
        private List<Region> _neighbours;

        public Region(ClientData.RegionData regionData, Game game)
        {
            _regionData = regionData;
            _game = game;
            _id = regionData.GetId();
            _regionGroupID = regionData.IslandID();
            _center = new Position(regionData.GetCenter_x(), regionData.GetCenter_y());
            _higherGround = regionData.IsHigherGround();
            _defensePriority = regionData.GetPriority();
            _accessible = regionData.IsAccessible();
            _boundsLeft = regionData.GetLeftMost();
            _boundsTop = regionData.GetTopMost();
            _boundsRight = regionData.GetRightMost();
            _boundsBottom = regionData.GetBottomMost();
        }

        public void UpdateNeighbours()
        {
            _neighbours = new List<Region>();

            var accessibleBestDist = int.MaxValue;
            var inaccessibleBestDist = int.MaxValue;

            for (var i = 0; i < _regionData.GetNeighborCount(); i++)
            {
                var region = _game.GetRegion(_regionData.GetNeighbors(i));
                _neighbours.Add(region);
                var d = GetDistance(region);
                if (region.IsAccessible())
                {
                    if (d < accessibleBestDist)
                    {
                        _closestAccessibleRegion = region;
                        accessibleBestDist = d;
                    }
                }
                else if (d < inaccessibleBestDist)
                {
                    _closestInaccessibleRegion = region;
                    inaccessibleBestDist = d;
                }
            }
        }

        /// <summary>
        /// Retrieves a unique identifier for this region.
        /// <p>
        /// This identifier is explicitly assigned by Broodwar.
        /// </summary>
        /// <returns>An integer that represents this region.</returns>
        /// <remarks>@seeGame#getRegion</remarks>
        public int GetID()
        {
            return _id;
        }

        /// <summary>
        /// Retrieves a unique identifier for a group of regions that are all connected and
        /// accessible by each other. That is, all accessible regions will have the same
        /// group ID. This function is generally used to check if a path is available between two
        /// points in constant time.
        /// <p>
        /// This identifier is explicitly assigned by Broodwar.
        /// </summary>
        /// <returns>An integer that represents the group of regions that this one is attached to.</returns>
        public int GetRegionGroupID()
        {
            return _regionGroupID;
        }

        /// <summary>
        /// Retrieves the center of the region. This position is used as the node
        /// of the region.
        /// </summary>
        /// <returns>A {@link Position} indicating the center location of the Region, in pixels.</returns>
        public Position GetCenter()
        {
            return _center;
        }

        /// <summary>
        /// Checks if this region is part of higher ground. Higher ground may be
        /// used in strategic placement of units and structures.
        /// </summary>
        /// <returns>true if this region is part of strategic higher ground, and false otherwise.</returns>
        public bool IsHigherGround()
        {
            return _higherGround;
        }

        /// <summary>
        /// Retrieves a value that represents the strategic advantage of this region relative
        /// to other regions. A value of 2 may indicate a possible choke point, and a value
        /// of 3 indicates a signficant strategic position.
        /// <p>
        /// This value is explicitly assigned by Broodwar.
        /// </summary>
        /// <returns>An integer indicating this region's strategic potential.</returns>
        public int GetDefensePriority()
        {
            return _defensePriority;
        }

        /// <summary>
        /// Retrieves the state of accessibility of the region. The region is
        /// considered accessible if it can be accessed by ground units.
        /// </summary>
        /// <returns>true if ground units can traverse this region, and false if the tiles in this
        /// region are inaccessible or unwalkable.</returns>
        public bool IsAccessible()
        {
            return _accessible;
        }

        /// <summary>
        /// Retrieves the set of neighbor Regions that this one is connected to.
        /// </summary>
        /// <returns>A reference to a List<Region> containing the neighboring Regions.</returns>
        public List<Region> GetNeighbors()
        {
            return _neighbours;
        }

        /// <summary>
        /// Retrieves the approximate left boundary of the region.
        /// </summary>
        /// <returns>The x coordinate, in pixels, of the approximate left boundary of the region.</returns>
        public int GetBoundsLeft()
        {
            return _boundsLeft;
        }

        /// <summary>
        /// Retrieves the approximate top boundary of the region.
        /// </summary>
        /// <returns>The y coordinate, in pixels, of the approximate top boundary of the region.</returns>
        public int GetBoundsTop()
        {
            return _boundsTop;
        }

        /// <summary>
        /// Retrieves the approximate right boundary of the region.
        /// </summary>
        /// <returns>The x coordinate, in pixels, of the approximate right boundary of the region.</returns>
        public int GetBoundsRight()
        {
            return _boundsRight;
        }

        /// <summary>
        /// Retrieves the approximate bottom boundary of the region.
        /// </summary>
        /// <returns>The y coordinate, in pixels, of the approximate bottom boundary of the region.</returns>
        public int GetBoundsBottom()
        {
            return _boundsBottom;
        }

        /// <summary>
        /// Retrieves the closest accessible neighbor region.
        /// </summary>
        /// <returns>The closest {@link Region} that is accessible.</returns>
        public Region GetClosestAccessibleRegion()
        {
            return _closestAccessibleRegion;
        }

        /// <summary>
        /// Retrieves the closest inaccessible neighbor region.
        /// </summary>
        /// <returns>The closest {@link Region} that is inaccessible.</returns>
        public Region GetClosestInaccessibleRegion()
        {
            return _closestInaccessibleRegion;
        }

        /// <summary>
        /// Retrieves the center-to-center distance between two regions.
        /// <p>
        /// Ignores all collisions.
        /// </summary>
        /// <param name="other">The target {@link Region} to calculate distance to.</param>
        /// <returns>The integer distance from this Region to other.</returns>
        public int GetDistance(Region other)
        {
            return GetCenter().GetApproxDistance(other.GetCenter());
        }

        public List<Unit> GetUnits()
        {
            return GetUnits((u) => true);
        }

        /// <summary>
        /// Retrieves a List<Unit> containing all the units that are in this region.
        /// Also has the ability to filter the units before the creation of the List<Unit>.
        /// </summary>
        /// <param name="pred">If this parameter is used, it is a UnitFilter or function predicate that will retrieve only the units whose attributes match the given criteria. If omitted, then a default value of null is used, in which case there is no filter.</param>
        /// <returns>A List<Unit> containing all units in this region that have met the requirements
        /// of pred.</returns>
        /// <remarks>@seeUnitFilter</remarks>
        public List<Unit> GetUnits(UnitFilter pred)
        {
            return _game.GetUnitsInRectangle(GetBoundsLeft(), GetBoundsTop(), GetBoundsRight(), GetBoundsBottom(), (u) => Equals(u.GetRegion()) && pred(u));
        }

        public bool Equals(Region other)
        {
            return _id == other._id;
        }

        public override bool Equals(object o)
        {
            return o is Region other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public int CompareTo(Region other)
        {
            return _id - other._id;
        }
    }
}