using System;

namespace BWAPI.NET
{
    /// <summary>
    /// An object representing a bullet or missile spawned from an attack.
    /// <p>
    /// The Bullet interface allows you to detect bullets, missiles, and other types
    /// of non-melee attacks or special abilities that would normally be visible through
    /// human eyes (A lurker spike or a Queen's flying parasite), allowing quicker reaction
    /// to unavoidable consequences.
    /// <p>
    /// For example, ordering medics to restore units that are about to receive a lockdown
    /// to compensate for latency and minimize its effects. You can't know entirely which unit
    /// will be receiving a lockdown unless you can detect the lockdown missile using the
    /// {@link Bullet} class.
    /// <p>
    /// {@link Bullet} objects are re-used after they are destroyed, however their ID is updated when it
    /// represents a new Bullet.
    /// <p>
    /// If {@link Flag#CompleteMapInformation} is disabled, then a {@link Bullet} is accessible if and only if
    /// it is visible. Otherwise if {@link Flag#CompleteMapInformation} is enabled, then all Bullets
    /// in the game are accessible.
    /// </summary>
    /// <remarks>
    /// @seeGame#getBullets
    /// @seeBullet#exists
    /// </remarks>
    public sealed class Bullet : IEquatable<Bullet>, IComparable<Bullet>
    {
        private readonly int _id;
        private readonly ClientData.BulletData _bulletData;
        private readonly Game _game;

        public Bullet(int id, ClientData.BulletData bulletData, Game game)
        {
            _bulletData = bulletData;
            _id = id;
            _game = game;
        }

        /// <summary>
        /// Retrieves a unique identifier for the current {@link Bullet}.
        /// </summary>
        /// <returns>An integer value containing the identifier.</returns>
        public int GetID()
        {
            return _id;
        }

        /// <summary>
        /// Checks if the {@link Bullet} exists in the view of the BWAPI player.
        /// <p>
        /// If {@link Flag#CompleteMapInformation} is disabled, and a {@link Bullet} is not visible, then the
        /// return value will be false regardless of the Bullet's true existence. This is because
        /// absolutely no state information on invisible enemy bullets is made available to the AI.
        /// <p>
        /// If {@link Flag#CompleteMapInformation} is enabled, then this function is accurate for all
        /// {@link Bullet} information.
        /// </summary>
        /// <returns>true if the bullet exists or is visible, false if the bullet was destroyed or has gone out of scope.</returns>
        /// <remarks>
        /// @see#isVisible
        /// @seeUnit#exists
        /// </remarks>
        public bool Exists()
        {
            return _bulletData.GetExists();
        }

        /// <summary>
        /// Retrieves the {@link Player} interface that owns the Bullet.
        /// </summary>
        /// <returns>The owning {@link Player} object. Returns null if the {@link Player} object for this {@link Bullet} is inaccessible.</returns>
        public Player GetPlayer()
        {
            return _game.GetPlayer(_bulletData.GetPlayer());
        }

        /// <summary>
        /// Retrieves the type of this {@link Bullet}.
        /// </summary>
        /// <returns>A {@link BulletType} representing the Bullet's type. Returns {@link BulletType#Unknown} if the {@link Bullet} is inaccessible.</returns>
        public BulletType GetBulletType()
        {
            return _bulletData.GetBulletType();
        }

        /// <summary>
        /// Retrieves the {@link Unit} that the {@link Bullet} spawned from.
        /// </summary>
        /// <returns>The owning {@link Unit} object. Returns null if the source can not be identified or is inaccessible.</returns>
        /// <remarks>@see#getTarget</remarks>
        public Unit GetSource()
        {
            return _game.GetUnit(_bulletData.GetSource());
        }

        /// <summary>
        /// Retrieves the Bullet's current position.
        /// </summary>
        /// <returns>A {@link Position} containing the Bullet's current coordinates. Returns {@link Position#Unknown} if the {@link Bullet} is inaccessible.</returns>
        /// <remarks>@see#getTargetPosition</remarks>
        public Position GetPosition()
        {
            return new Position(_bulletData.GetPositionX(), _bulletData.GetPositionY());
        }

        /// <summary>
        /// Retrieve's the direction the {@link Bullet} is facing. If the angle is 0, then
        /// the {@link Bullet} is facing right.
        /// </summary>
        /// <returns>A double representing the direction the {@link Bullet} is facing. Returns 0.0 if the bullet is inaccessible.</returns>
        public double GetAngle()
        {
            return _bulletData.GetAngle();
        }

        /// <summary>
        /// Retrieves the X component of the Bullet's velocity, measured in pixels per frame.
        /// </summary>
        /// <returns>A double representing the number of pixels moved on the X axis per frame. Returns 0.0 if the {@link Bullet} is inaccessible.</returns>
        /// <remarks>
        /// @see#getVelocityY
        /// @see#getAngle
        /// </remarks>
        public double GetVelocityX()
        {
            return _bulletData.GetVelocityX();
        }

        /// <summary>
        /// Retrieves the Y component of the Bullet's velocity, measured in pixels per frame.
        /// </summary>
        /// <returns>A double representing the number of pixels moved on the Y axis per frame. Returns 0.0 if the {@link Bullet} is inaccessible.</returns>
        /// <remarks>
        /// @see#getVelocityX
        /// @see#getAngle
        /// </remarks>
        public double GetVelocityY()
        {
            return _bulletData.GetVelocityY();
        }

        /// <summary>
        /// Retrieves the Unit interface that the {@link Bullet} is heading to.
        /// </summary>
        /// <returns>The target Unit object, if one exists. Returns null if the Bullet's target {@link Unit} is inaccessible, the {@link Bullet} is targetting the ground, or if the {@link Bullet} itself is inaccessible.</returns>
        /// <remarks>
        /// @see#getTargetPosition
        /// @see#getSource
        /// </remarks>
        public Unit GetTarget()
        {
            return _game.GetUnit(_bulletData.GetTarget());
        }

        /// <summary>
        /// Retrieves the target position that the {@link Bullet} is heading to.
        /// </summary>
        /// <returns>A {@link Position} indicating where the {@link Bullet} is headed. Returns {@link Position#Unknown} if the bullet is inaccessible.</returns>
        /// <remarks>
        /// @see#getTarget
        /// @see#getPosition
        /// </remarks>
        public Position GetTargetPosition()
        {
            return new Position(_bulletData.GetTargetPositionX(), _bulletData.GetTargetPositionY());
        }

        /// <summary>
        /// Retrieves the timer that indicates the Bullet's life span.
        /// <p>
        /// Bullets are not permanent objects, so they will often have a limited life span.
        /// This life span is measured in frames. Normally a Bullet will reach its target
        /// before being removed.
        /// </summary>
        /// <returns>An integer representing the remaining number of frames until the {@link Bullet} self-destructs. Returns 0 if the {@link Bullet} is inaccessible.</returns>
        public int GetRemoveTimer()
        {
            return _bulletData.GetRemoveTimer();
        }

        public bool IsVisible()
        {
            return IsVisible(_game.Self());
        }

        /// <summary>
        /// Retrieves the visibility state of the Bullet.
        /// </summary>
        /// <param name="player">If this parameter is specified, then the Bullet's visibility to the given player is checked. If this parameter is omitted, then a default value of null is used, which will check if the BWAPI player has vision of the {@link Bullet}.</param>
        /// <returns>true if the {@link Bullet} is visible to the specified player, false if the {@link Bullet} is not visible to the specified player.</returns>
        public bool IsVisible(Player player)
        {
            return player == null ? IsVisible() : _bulletData.IsVisible(player.GetID());
        }

        public bool Equals(Bullet other)
        {
            return _id == other._id;
        }

        public override bool Equals(object o)
        {
            return o is Bullet other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public int CompareTo(Bullet other)
        {
            return _id - other._id;
        }
    }
}