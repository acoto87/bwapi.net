using System;

namespace BWAPI.NET
{
    /// <summary>
    /// A side effect is an interaction that a bot attempts to have with the game.
    /// This entails sending a game or unit command, or drawing a shape.
    /// </summary>
    public class SideEffect
    {
        public static SideEffect AddUnitCommand(UnitCommandType type, int unit, int target, int x, int y, int extra)
        {
            var output = new SideEffect
            {
                application = (ClientData.TGameData gameData) =>
                {
                    var unitCommand = GameDataHelper.AddUnitCommand(gameData);
                    unitCommand.SetTid(type);
                    unitCommand.SetUnitIndex(unit);
                    unitCommand.SetTargetIndex(target);
                    unitCommand.SetX(x);
                    unitCommand.SetY(y);
                    unitCommand.SetExtra(extra);
                }
            };
            return output;
        }

        public static SideEffect AddCommand(CommandType type, int value1, int value2)
        {
            var output = new SideEffect
            {
                application = (ClientData.TGameData gameData) =>
                {
                    var command = GameDataHelper.AddCommand(gameData);
                    command.SetCommandType(type);
                    command.SetValue1(value1);
                    command.SetValue2(value2);
                }
            };
            return output;
        }

        public static SideEffect AddCommand(CommandType type, string text, int value2)
        {
            var output = new SideEffect
            {
                application = (ClientData.TGameData gameData) =>
                {
                    var command = GameDataHelper.AddCommand(gameData);
                    command.SetCommandType(type);
                    command.SetValue1(GameDataHelper.AddString(gameData, text));
                    command.SetValue2(value2);
                }
            };
            return output;
        }

        public static SideEffect AddShape(ShapeType type, CoordinateType coordType, int x1, int y1, int x2, int y2, int extra1, int extra2, int color, bool isSolid)
        {
            var output = new SideEffect
            {
                application = (ClientData.TGameData gameData) =>
                {
                    var shape = GameDataHelper.AddShape(gameData);
                    shape.SetShapeType(type);
                    shape.SetCtype(coordType);
                    shape.SetX1(x1);
                    shape.SetY1(y1);
                    shape.SetX2(x2);
                    shape.SetY2(y2);
                    shape.SetExtra1(extra1);
                    shape.SetExtra2(extra2);
                    shape.SetColor(color);
                    shape.SetIsSolid(isSolid);
                }
            };
            return output;
        }

        public static SideEffect AddShape(ShapeType type, CoordinateType coordType, int x1, int y1, int x2, int y2, string text, int extra2, int color, bool isSolid)
        {
            var output = new SideEffect
            {
                application = (ClientData.TGameData gameData) =>
                {
                    var shape = GameDataHelper.AddShape(gameData);
                    shape.SetShapeType(type);
                    shape.SetCtype(coordType);
                    shape.SetX1(x1);
                    shape.SetY1(y1);
                    shape.SetX2(x2);
                    shape.SetY2(y2);
                    shape.SetExtra1(GameDataHelper.AddString(gameData, text));
                    shape.SetExtra2(extra2);
                    shape.SetColor(color);
                    shape.SetIsSolid(isSolid);
                }
            };
            return output;
        }

        private Action<ClientData.TGameData> application;

        private SideEffect()
        {
        }

        public void Apply(ClientData.TGameData gameData)
        {
            application(gameData);
        }
    }
}