namespace BWAPI.NET
{
    public static class EventHandler
    {
        public static void Operation(IBWEventListener eventListener, Game game, ClientData.Event @event)
        {
            Unit u;
            int frames = game.GetFrameCount();
            switch (@event.GetEventType())
            {
                case EventType.MatchStart:
                    game.Init();
                    eventListener.OnStart();
                    break;
                case EventType.MatchEnd:
                    eventListener.OnEnd(@event.GetV1() != 0);
                    break;
                case EventType.MatchFrame:
                    game.OnFrame(frames);
                    eventListener.OnFrame();
                    break;
                case EventType.SendText:
                    eventListener.OnSendText(game.ClientData.GameData.GetEventStrings(@event.GetV1()));
                    break;
                case EventType.ReceiveText:
                    eventListener.OnReceiveText(game.GetPlayer(@event.GetV1()), game.ClientData.GameData.GetEventStrings(@event.GetV2()));
                    break;
                case EventType.PlayerLeft:
                    eventListener.OnPlayerLeft(game.GetPlayer(@event.GetV1()));
                    break;
                case EventType.NukeDetect:
                    eventListener.OnNukeDetect(new Position(@event.GetV1(), @event.GetV2()));
                    break;
                case EventType.SaveGame:
                    eventListener.OnSaveGame(game.ClientData.GameData.GetEventStrings(@event.GetV1()));
                    break;
                case EventType.UnitDiscover:
                    game.UnitCreate(@event.GetV1());
                    u = game.GetUnit(@event.GetV1());
                    u.UpdatePosition(frames);
                    eventListener.OnUnitDiscover(u);
                    break;
                case EventType.UnitEvade:
                    u = game.GetUnit(@event.GetV1());
                    u.UpdatePosition(frames);
                    eventListener.OnUnitEvade(u);
                    break;
                case EventType.UnitShow:
                    game.UnitShow(@event.GetV1());
                    u = game.GetUnit(@event.GetV1());
                    u.UpdatePosition(frames);
                    eventListener.OnUnitShow(u);
                    break;
                case EventType.UnitHide:
                    game.UnitHide(@event.GetV1());
                    u = game.GetUnit(@event.GetV1());
                    eventListener.OnUnitHide(u);
                    break;
                case EventType.UnitCreate:
                    game.UnitCreate(@event.GetV1());
                    u = game.GetUnit(@event.GetV1());
                    u.UpdatePosition(frames);
                    eventListener.OnUnitCreate(u);
                    break;
                case EventType.UnitDestroy:
                    game.UnitHide(@event.GetV1());
                    u = game.GetUnit(@event.GetV1());
                    eventListener.OnUnitDestroy(u);
                    break;
                case EventType.UnitMorph:
                    u = game.GetUnit(@event.GetV1());
                    u.UpdatePosition(frames);
                    eventListener.OnUnitMorph(u);
                    break;
                case EventType.UnitRenegade:
                    u = game.GetUnit(@event.GetV1());
                    eventListener.OnUnitRenegade(u);
                    break;
                case EventType.UnitComplete:
                    game.UnitCreate(@event.GetV1());
                    u = game.GetUnit(@event.GetV1());
                    eventListener.OnUnitComplete(u);
                    break;
            }
        }
    }
}