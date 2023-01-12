namespace BWAPI.NET
{
    /// <summary>
    /// Convenience class that extends all methods in {@link BWEventListener}.
    /// Not all of the methods need an implementation.
    /// </summary>
    public class DefaultBWListener : IBWEventListener
    {
        public virtual void OnStart()
        {
        }

        public virtual void OnEnd(bool isWinner)
        {
        }

        public virtual void OnFrame()
        {
        }

        public virtual void OnSendText(string text)
        {
        }

        public virtual void OnReceiveText(Player player, string text)
        {
        }

        public virtual void OnPlayerLeft(Player player)
        {
        }

        public virtual void OnNukeDetect(Position position)
        {
        }

        public virtual void OnUnitDiscover(Unit unit)
        {
        }

        public virtual void OnUnitEvade(Unit unit)
        {
        }

        public virtual void OnUnitShow(Unit unit)
        {
        }

        public virtual void OnUnitHide(Unit unit)
        {
        }

        public virtual void OnUnitCreate(Unit unit)
        {
        }

        public virtual void OnUnitDestroy(Unit unit)
        {
        }

        public virtual void OnUnitMorph(Unit unit)
        {
        }

        public virtual void OnUnitRenegade(Unit unit)
        {
        }

        public virtual void OnSaveGame(string text)
        {
        }

        public virtual void OnUnitComplete(Unit unit)
        {
        }

        public virtual void OnPlayerDropped(Player player)
        {
        }
    }
}