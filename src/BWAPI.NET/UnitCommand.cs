using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BWAPI.NET
{
    public sealed class UnitCommand : IEquatable<UnitCommand>
    {
        internal Unit _unit;
        internal UnitCommandType _type;
        internal Unit _target;
        internal int _x = Position.None.x;
        internal int _y = Position.None.y;
        internal int _extra = 0;

        private UnitCommand(Unit unit, UnitCommandType type)
        {
            _unit = unit;
            _type = type;
        }

        private void AssignTarget<T>(Point<T> target)
            where T : Point<T>
        {
            _x = target.x;
            _y = target.y;
        }

        public static UnitCommand Attack(Unit unit, Position target)
        {
            return Attack(unit, target, false);
        }

        public static UnitCommand Attack(Unit unit, Unit target)
        {
            return Attack(unit, target, false);
        }

        public static UnitCommand Attack(Unit unit, Position target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Attack_Move);
            c.AssignTarget(target);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Attack(Unit unit, Unit target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Attack_Unit);
            c._target = target;
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Build(Unit unit, TilePosition target, UnitType type)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Build);
            c.AssignTarget(target);
            c._extra = (int)type;
            return c;
        }

        public static UnitCommand BuildAddon(Unit unit, UnitType type)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Build_Addon);
            c._extra = (int)type;
            return c;
        }

        public static UnitCommand Train(Unit unit, UnitType type)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Train);
            c._extra = (int)type;
            return c;
        }

        public static UnitCommand Morph(Unit unit, UnitType type)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Morph);
            c._extra = (int)type;
            return c;
        }

        public static UnitCommand Research(Unit unit, TechType tech)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Research);
            c._extra = (int)tech;
            return c;
        }

        public static UnitCommand Upgrade(Unit unit, UpgradeType upgrade)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Upgrade);
            c._extra = (int)upgrade;
            return c;
        }

        public static UnitCommand SetRallyPoint(Unit unit, Position target)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Set_Rally_Position);
            c.AssignTarget(target);
            return c;
        }

        public static UnitCommand SetRallyPoint(Unit unit, Unit target)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Set_Rally_Unit);
            c._target = target;
            return c;
        }

        public static UnitCommand Move(Unit unit, Position target)
        {
            return Move(unit, target, false);
        }

        public static UnitCommand Move(Unit unit, Position target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Move);
            c.AssignTarget(target);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Patrol(Unit unit, Position target)
        {
            return Patrol(unit, target, false);
        }

        public static UnitCommand Patrol(Unit unit, Position target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Patrol);
            c.AssignTarget(target);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand HoldPosition(Unit unit)
        {
            return HoldPosition(unit, false);
        }

        public static UnitCommand HoldPosition(Unit unit, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Hold_Position);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Stop(Unit unit)
        {
            return Stop(unit, false);
        }

        public static UnitCommand Stop(Unit unit, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Stop);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Follow(Unit unit, Unit target)
        {
            return Follow(unit, target, false);
        }

        public static UnitCommand Follow(Unit unit, Unit target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Follow);
            c._target = target;
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Gather(Unit unit, Unit target)
        {
            return Gather(unit, target, false);
        }

        public static UnitCommand Gather(Unit unit, Unit target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Gather);
            c._target = target;
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand ReturnCargo(Unit unit)
        {
            return ReturnCargo(unit, false);
        }

        public static UnitCommand ReturnCargo(Unit unit, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Return_Cargo);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Repair(Unit unit, Unit target)
        {
            return Repair(unit, target, false);
        }

        public static UnitCommand Repair(Unit unit, Unit target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Repair);
            c._target = target;
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Burrow(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Burrow);
        }

        public static UnitCommand Unburrow(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Unburrow);
        }

        public static UnitCommand Cloak(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Cloak);
        }

        public static UnitCommand Decloak(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Decloak);
        }

        public static UnitCommand Siege(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Siege);
        }

        public static UnitCommand Unsiege(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Unsiege);
        }

        public static UnitCommand Lift(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Lift);
        }

        public static UnitCommand Land(Unit unit, TilePosition target)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Land);
            c.AssignTarget(target);
            return c;
        }

        public static UnitCommand Load(Unit unit, Unit target)
        {
            return Load(unit, target, false);
        }

        public static UnitCommand Load(Unit unit, Unit target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Load);
            c._target = target;
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand Unload(Unit unit, Unit target)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Unload);
            c._target = target;
            return c;
        }

        public static UnitCommand UnloadAll(Unit unit)
        {
            return UnloadAll(unit, false);
        }

        public static UnitCommand UnloadAll(Unit unit, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Unload_All);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand UnloadAll(Unit unit, Position target)
        {
            return UnloadAll(unit, target, false);
        }

        public static UnitCommand UnloadAll(Unit unit, Position target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Unload_All_Position);
            c.AssignTarget(target);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand RightClick(Unit unit, Position target)
        {
            return RightClick(unit, target, false);
        }

        public static UnitCommand RightClick(Unit unit, Unit target)
        {
            return RightClick(unit, target, false);
        }

        public static UnitCommand RightClick(Unit unit, Position target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Right_Click_Position);
            c.AssignTarget(target);
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand RightClick(Unit unit, Unit target, bool shiftQueueCommand)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Right_Click_Unit);
            c._target = target;
            c._extra = shiftQueueCommand ? 1 : 0;
            return c;
        }

        public static UnitCommand HaltConstruction(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Halt_Construction);
        }

        public static UnitCommand CancelConstruction(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Cancel_Construction);
        }

        public static UnitCommand CancelAddon(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Cancel_Addon);
        }

        public static UnitCommand CancelTrain(Unit unit)
        {
            return CancelTrain(unit, -2);
        }

        public static UnitCommand CancelTrain(Unit unit, int slot)
        {
            UnitCommand c = new UnitCommand(unit, slot >= 0 ? UnitCommandType.Cancel_Train_Slot : UnitCommandType.Cancel_Train);
            c._extra = slot;
            return c;
        }

        public static UnitCommand CancelMorph(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Cancel_Morph);
        }

        public static UnitCommand CancelResearch(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Cancel_Research);
        }

        public static UnitCommand CancelUpgrade(Unit unit)
        {
            return new UnitCommand(unit, UnitCommandType.Cancel_Upgrade);
        }

        public static UnitCommand UseTech(Unit unit, TechType tech)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Use_Tech);
            c._extra = (int)tech;
            if (tech == TechType.Burrowing)
            {
                c._type = unit.IsBurrowed() ? UnitCommandType.Unburrow : UnitCommandType.Burrow;
            }
            else if (tech == TechType.Cloaking_Field || tech == TechType.Personnel_Cloaking)
            {
                c._type = unit.IsCloaked() ? UnitCommandType.Decloak : UnitCommandType.Cloak;
            }
            else if (tech == TechType.Tank_Siege_Mode)
            {
                c._type = unit.IsSieged() ? UnitCommandType.Unsiege : UnitCommandType.Siege;
            }

            return c;
        }

        public static UnitCommand UseTech(Unit unit, TechType tech, Position target)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Use_Tech_Position);
            c.AssignTarget(target);
            c._extra = (int)tech;
            return c;
        }

        public static UnitCommand UseTech(Unit unit, TechType tech, Unit target)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Use_Tech_Unit);
            c._target = target;
            c._extra = (int)tech;
            return c;
        }

        public static UnitCommand PlaceCOP(Unit unit, TilePosition target)
        {
            UnitCommand c = new UnitCommand(unit, UnitCommandType.Place_COP);
            c.AssignTarget(target);
            return c;
        }

        public Unit GetUnit()
        {
            return _unit;
        }

        public UnitCommandType GetUnitCommandType()
        {
            return _type;
        }

        public Unit GetTarget()
        {
            return _target;
        }

        public int GetSlot()
        {
            return _type == UnitCommandType.Cancel_Train_Slot ? _extra : -1;
        }

        public Position GetTargetPosition()
        {
            if (_type == UnitCommandType.Build || _type == UnitCommandType.Land || _type == UnitCommandType.Place_COP)
            {
                return new TilePosition(_x, _y).ToPosition();
            }

            return new Position(_x, _y);
        }

        public TilePosition GetTargetTilePosition()
        {
            if (_type == UnitCommandType.Build || _type == UnitCommandType.Land || _type == UnitCommandType.Place_COP)
            {
                return new TilePosition(_x, _y);
            }

            return new Position(_x, _y).ToTilePosition();
        }

        public UnitType GetUnitType()
        {
            if (_type == UnitCommandType.Build || _type == UnitCommandType.Build_Addon || _type == UnitCommandType.Train || _type == UnitCommandType.Morph)
            {
                return (UnitType)_extra;
            }

            return UnitType.None;
        }

        public TechType GetTechType()
        {
            if (_type == UnitCommandType.Research || _type == UnitCommandType.Use_Tech || _type == UnitCommandType.Use_Tech_Position || _type == UnitCommandType.Use_Tech_Unit)
            {
                return (TechType)_extra;
            }

            return TechType.None;
        }

        public UpgradeType GetUpgradeType()
        {
            return _type == UnitCommandType.Upgrade ? (UpgradeType)_extra : UpgradeType.None;
        }

        public bool IsQueued()
        {
            return (_type == UnitCommandType.Attack_Move || _type == UnitCommandType.Attack_Unit || _type == UnitCommandType.Move || _type == UnitCommandType.Patrol || _type == UnitCommandType.Hold_Position || _type == UnitCommandType.Stop || _type == UnitCommandType.Follow || _type == UnitCommandType.Gather || _type == UnitCommandType.Return_Cargo || _type == UnitCommandType.Load || _type == UnitCommandType.Unload_All || _type == UnitCommandType.Unload_All_Position || _type == UnitCommandType.Right_Click_Position || _type == UnitCommandType.Right_Click_Unit) && _extra != 0;
        }

        public bool Equals(UnitCommand other)
        {
            return _x == other._x && _y == other._y && _extra == other._extra && _type == other._type && object.Equals(_target, other._target) && object.Equals(_unit, other._unit);
        }

        public override bool Equals(object o)
        {
            return o is UnitCommand other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_type, _target, _x, _y, _extra, _unit);
        }
    }
}