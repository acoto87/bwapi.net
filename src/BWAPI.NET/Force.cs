using System;
using System.Collections.Generic;
using System.Linq;

namespace BWAPI.NET
{
    /// <summary>
    /// The {@link Force} class is used to get information about each force in a match.
    /// Normally this is considered a team.
    /// <p>
    /// It is not called a team because players on the same force do not necessarily need
    /// to be allied at the beginning of a match.
    /// </summary>
    public sealed class Force : IEquatable<Force>, IComparable<Force>
    {
        private readonly int _id;
        private readonly string _name;
        private readonly Game _game;

        public Force(int id, ClientData.ForceData forceData, Game game)
        {
            _game = game;
            _id = id;
            _name = forceData.GetName();
        }

        /// <summary>
        /// Retrieves the unique ID that represents this {@link Force}.
        /// </summary>
        /// <returns>An integer containing the ID for the {@link Force}.</returns>
        public int GetID()
        {
            return _id;
        }

        /// <summary>
        /// Retrieves the name of the {@link Force}.
        /// </summary>
        /// <returns>A String object containing the name of the force.</returns>
        public string GetName()
        {
            return _name;
        }

        /// <summary>
        /// Retrieves the set of players that belong to this {@link Force}.
        /// </summary>
        /// <returns>A List<Player> object containing the players that are part of this {@link Force}.</returns>
        public List<Player> GetPlayers()
        {
            return _game.GetPlayers().Where(x => Equals(x.GetForce())).ToList();
        }

        public override bool Equals(object obj)
        {
            return obj is Force other && Equals(other);
        }

        public bool Equals(Force other)
        {
            return _id == other._id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public int CompareTo(Force other)
        {
            return _id - other._id;
        }
    }
}