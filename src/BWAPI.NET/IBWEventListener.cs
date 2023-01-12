namespace BWAPI.NET
{
    public interface IBWEventListener
    {
        void OnStart();

        void OnEnd(bool isWinner);

        void OnFrame();

        void OnSendText(string text);

        void OnReceiveText(Player player, string text);

        void OnPlayerLeft(Player player);

        void OnNukeDetect(Position target);

        void OnUnitDiscover(Unit unit);

        void OnUnitEvade(Unit unit);

        void OnUnitShow(Unit unit);

        void OnUnitHide(Unit unit);

        void OnUnitCreate(Unit unit);

        void OnUnitDestroy(Unit unit);

        void OnUnitMorph(Unit unit);

        void OnUnitRenegade(Unit unit);

        void OnSaveGame(string gameName);

        void OnUnitComplete(Unit unit);

        void OnPlayerDropped(Player player);
    }
}