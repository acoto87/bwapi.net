namespace BWAPI.NET
{
    /// <summary>
    /// Latency Compensation:
    /// Only need to implement LatCom for current frame, the server updates the next frame already if latcom is enabled.
    /// Use Caches for all internal state that might be affected by latcom, and add the (current) frame, to let Player & Unit
    /// check if they need to use the cached/latcom version of the value or the from server (or a combination of both)
    /// <p>
    /// Inspiration:
    /// https://github.com/bwapi/bwapi/blob/e4a29d73e6021037901da57ceb06e37248760240/bwapi/include/BWAPI/Client/CommandTemp.h
    /// </summary>
    public class CommandTemp
    {
        public enum EventType
        {
            Order,
            Resource,
            Finish
        }

        private readonly UnitCommand _command;
        private readonly Game _game;

        private EventType _eventType;
        private Player _player;

        public CommandTemp(UnitCommand command, Game game)
        {
            _command = command;
            _game = game;

            _eventType = EventType.Resource;
        }

        public int GetUnitID(Unit unit)
        {
            if (unit == null)
            {
                return -1;
            }

            return unit.GetID();
        }

        public void Execute()
        {
            switch (_command._type)
            {
                case UnitCommandType.Halt_Construction:
                {
                    _eventType = EventType.Order;
                    Execute(_game.GetRemainingLatencyFrames() == 0);
                    break;
                }
                default:
                {
                    Execute(_game.GetRemainingLatencyFrames() == 0);
                    break;
                }
            }
        }

        public void Execute(bool isCurrentFrame)
        {
            // Immediately return if latency compensation is disabled or if the command was queued
            if (!_game.IsLatComEnabled() || _command.IsQueued())
            {
                return;
            }

            var unit = _command._unit;
            var target = _command._target;
            var frame = _game.GetFrameCount();
            if (isCurrentFrame)
            {
                switch (_command._type)
                {
                    case UnitCommandType.Morph:
                    case UnitCommandType.Build_Addon:
                    case UnitCommandType.Train:
                    {
                        if (_eventType == EventType.Resource)
                        {
                            break;
                        }

                        return;
                    }
                    default:
                        return;
                }
            }

            // Get the player (usually the unit's owner)
            _player ??= unit != null ? unit.GetPlayer() : _game.Self();

            // Existence test
            if (unit == null || !unit.Exists())
            {
                return;
            }

            // Move test
            switch (_command._type)
            {
                case UnitCommandType.Follow:
                case UnitCommandType.Hold_Position:
                case UnitCommandType.Move:
                case UnitCommandType.Patrol:
                case UnitCommandType.Right_Click_Position:
                case UnitCommandType.Attack_Move:
                {
                    if (!unit.GetUnitType().CanMove())
                    {
                        return;
                    }

                    break;
                }
                default:
                    break;
            }

            switch (_command._type)
            {
                case UnitCommandType.Attack_Move:
                {
                    unit.Self().Order.Set(Order.AttackMove, frame);
                    unit.Self().TargetPositionX.Set(_command._x, frame);
                    unit.Self().TargetPositionY.Set(_command._y, frame);
                    unit.Self().OrderTargetPositionX.Set(_command._x, frame);
                    unit.Self().OrderTargetPositionY.Set(_command._y, frame);
                    break;
                }
                case UnitCommandType.Attack_Unit:
                {
                    if (target == null || !target.Exists() || !unit.GetUnitType().CanAttack())
                    {
                        return;
                    }

                    unit.Self().Order.Set(Order.AttackUnit, frame);
                    unit.Self().Target.Set(GetUnitID(target), frame);
                    break;
                }
                case UnitCommandType.Build:
                {
                    unit.Self().Order.Set(Order.PlaceBuilding, frame);
                    unit.Self().IsConstructing.Set(true, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    unit.Self().BuildType.Set((UnitType)_command._extra, frame);
                    break;
                }
                case UnitCommandType.Build_Addon:
                {
                    var addonType = (UnitType)_command._extra;
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            _player.Self().Minerals.SetOrAdd(-addonType.MineralPrice(), frame);
                            _player.Self().Gas.SetOrAdd(-addonType.GasPrice(), frame);
                            if (!isCurrentFrame)
                            {

                                // We will pretend the building is busy building, this doesn't
                                unit.Self().IsIdle.Set(false, frame);
                                unit.Self().Order.Set(Order.PlaceAddon, frame);
                            }

                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().IsConstructing.Set(true, frame);
                            unit.Self().Order.Set(Order.Nothing, frame);
                            unit.Self().SecondaryOrder.Set(Order.BuildAddon, frame);
                            unit.Self().BuildType.Set((UnitType)_command._extra, frame);
                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Burrow:
                {
                    unit.Self().Order.Set(Order.Burrowing, frame);
                    break;
                }
                case UnitCommandType.Cancel_Addon:
                {
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            var addonType = unit.GetBuildType();
                            _player.Self().Minerals.SetOrAdd((int)(addonType.MineralPrice() * 0.75), frame);
                            _player.Self().Gas.SetOrAdd((int)(addonType.GasPrice() * 0.75), frame);
                            unit.Self().BuildType.Set(UnitType.None, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().RemainingBuildTime.Set(0, frame);
                            unit.Self().IsConstructing.Set(false, frame);
                            unit.Self().Order.Set(Order.Nothing, frame);
                            unit.Self().IsIdle.Set(true, frame);
                            unit.Self().BuildUnit.Set(-1, frame);
                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Cancel_Construction:
                {
                    if (unit.GetUnitType().GetRace() == Race.Terran)
                    {
                        var builder = unit.GetBuildUnit();
                        if (builder != null && builder.Exists())
                        {
                            switch (_eventType)
                            {
                                case EventType.Resource:
                                {
                                    builder.Self().BuildType.Set(UnitType.None, frame);
                                    break;
                                }
                                case EventType.Order:
                                {
                                    builder.Self().IsConstructing.Set(false, frame);
                                    builder.Self().Order.Set(Order.ResetCollision, frame);
                                    break;
                                }
                                case EventType.Finish:
                                {
                                    builder.Self().Order.Set(Order.PlayerGuard, frame);
                                    break;
                                }
                            }
                        }
                    }

                    if (_eventType == EventType.Resource)
                    {
                        unit.Self().BuildUnit.Set(-1, frame);
                        _player.Self().Minerals.SetOrAdd((int)(unit.GetUnitType().MineralPrice() * 0.75), frame);
                        _player.Self().Gas.SetOrAdd((int)(unit.GetUnitType().GasPrice() * 0.75), frame);
                        unit.Self().RemainingBuildTime.Set(0, frame);
                    }

                    if (unit.GetUnitType().GetRace() == Race.Zerg)
                    {
                        switch (_eventType)
                        {
                            case EventType.Resource:
                            {
                                unit.Self().Type.Set(unit.GetUnitType().WhatBuilds().GetFirst(), frame);
                                unit.Self().BuildType.Set(UnitType.None, frame);
                                unit.Self().IsMorphing.Set(false, frame);
                                unit.Self().Order.Set(Order.ResetCollision, frame);
                                unit.Self().IsConstructing.Set(false, frame);
                                _player.Self().SupplyUsed[(int)unit.GetUnitType().GetRace()].SetOrAdd(unit.GetUnitType().SupplyRequired(), frame);
                                break;
                            }
                            case EventType.Order:
                            {
                                unit.Self().Order.Set(Order.PlayerGuard, frame);
                                unit.Self().IsIdle.Set(true, frame);
                                break;
                            }
                        }
                    }

                    break;
                }
                case UnitCommandType.Cancel_Morph:
                {
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            var builtType = unit.GetBuildType();
                            var newType = builtType.WhatBuilds().GetFirst();
                            if (newType.IsBuilding())
                            {
                                _player.Self().Minerals.SetOrAdd((int)(builtType.MineralPrice() * 0.75), frame);
                                _player.Self().Gas.SetOrAdd((int)(builtType.GasPrice() * 0.75), frame);
                            }
                            else
                            {
                                _player.Self().Minerals.SetOrAdd(builtType.MineralPrice(), frame);
                                _player.Self().Gas.SetOrAdd(builtType.GasPrice(), frame);
                            }

                            if (newType.IsBuilding() && newType.ProducesCreep())
                            {
                                unit.Self().Order.Set(Order.InitCreepGrowth, frame);
                            }

                            if (unit.GetUnitType() != UnitType.Zerg_Egg)
                            {
                                // Issue #781
                                // https://github.com/bwapi/bwapi/issues/781
                                unit.Self().Type.Set(newType, frame);
                            }

                            unit.Self().BuildType.Set(UnitType.None, frame);
                            unit.Self().IsConstructing.Set(false, frame);
                            unit.Self().IsMorphing.Set(false, frame);
                            unit.Self().IsCompleted.Set(true, frame);
                            unit.Self().RemainingBuildTime.Set(0, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            if (unit.GetUnitType().IsBuilding())
                            {
                                // This event would hopefully not have been created
                                // if this wasn't true (see event note above)
                                unit.Self().IsIdle.Set(true, frame);
                                unit.Self().Order.Set(Order.Nothing, frame);
                                if (unit.GetUnitType() == UnitType.Zerg_Hatchery || unit.GetUnitType() == UnitType.Zerg_Lair)
                                {
                                    // Type should have updated during last event to the cancelled type
                                    unit.Self().SecondaryOrder.Set(Order.SpreadCreep, frame);
                                }
                            }
                            else
                            {
                                _player.Self().SupplyUsed[(int)unit.GetUnitType().GetRace()].SetOrAdd(-(unit.GetUnitType().SupplyRequired() * (1 + (unit.GetUnitType().IsTwoUnitsInOneEgg() ? 1 : 0))), frame);
                                _player.Self().SupplyUsed[(int)unit.GetUnitType().GetRace()].SetOrAdd(unit.GetUnitType().WhatBuilds().GetFirst().SupplyRequired() * unit.GetUnitType().WhatBuilds().GetSecond(), frame); // Note: unit.getType().whatBuilds().second is always 1 but we
                                // might as well handle the general case, in case Blizzard
                                // all of a sudden allows you to cancel archon morphs
                            }

                            break;
                        }
                        case EventType.Finish:
                        {
                            if (unit.GetUnitType() == UnitType.Zerg_Hatchery || unit.GetUnitType() == UnitType.Zerg_Lair)
                            {
                                unit.Self().SecondaryOrder.Set(Order.SpawningLarva, frame);
                            }
                            else if (!unit.GetUnitType().IsBuilding())
                            {
                                unit.Self().Order.Set(Order.PlayerGuard, frame);
                                unit.Self().IsCompleted.Set(true, frame);
                                unit.Self().IsConstructing.Set(false, frame);
                                unit.Self().IsIdle.Set(true, frame);
                                unit.Self().IsMorphing.Set(false, frame);
                            }

                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Cancel_Research:
                {
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            var techType = unit.GetTech();
                            _player.Self().Minerals.SetOrAdd(techType.MineralPrice(), frame);
                            _player.Self().Gas.SetOrAdd(techType.GasPrice(), frame);
                            unit.Self().RemainingResearchTime.Set(0, frame);
                            unit.Self().Tech.Set(TechType.None, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().Order.Set(Order.Nothing, frame);
                            unit.Self().IsIdle.Set(true, frame);
                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Cancel_Train_Slot:
                {
                    if (_command._extra != 0)
                    {
                        if (_eventType == EventType.Resource)
                        {
                            var unitType = unit.GetTrainingQueue()[_command._extra];
                            _player.Self().Minerals.SetOrAdd(unitType.MineralPrice(), frame);
                            _player.Self().Gas.SetOrAdd(unitType.GasPrice(), frame);

                            // Shift training queue back one slot after the cancelled unit
                            for (var i = _command._extra; i < 4; ++i)
                            {
                                unit.Self().TrainingQueue[i].Set(unit.GetTrainingQueue()[i + 1], frame);
                            }

                            unit.Self().TrainingQueueCount.SetOrAdd(-1, frame);
                        }
                    }
                    else
                    {
                        switch (_eventType)
                        {
                            case EventType.Resource:
                            {
                                var unitType = unit.GetTrainingQueue()[unit.GetTrainingQueueCount() - 1];
                                _player.Self().Minerals.SetOrAdd(unitType.MineralPrice(), frame);
                                _player.Self().Gas.SetOrAdd(unitType.GasPrice(), frame);
                                unit.Self().BuildUnit.Set(-1, frame);
                                if (unit.GetTrainingQueueCount() == 1)
                                {
                                    unit.Self().IsIdle.Set(false, frame);
                                    unit.Self().IsTraining.Set(false, frame);
                                }

                                break;
                            }
                            case EventType.Order:
                            {
                                unit.Self().TrainingQueueCount.SetOrAdd(-1, frame);
                                var unitType = unit.GetTrainingQueue()[unit.GetTrainingQueueCount()];
                                _player.Self().SupplyUsed[(int)unitType.GetRace()].SetOrAdd(-unitType.SupplyRequired(), frame);
                                if (unit.GetTrainingQueueCount() == 0)
                                {
                                    unit.Self().BuildType.Set(UnitType.None, frame);
                                }
                                else
                                {
                                    var ut = unit.GetTrainingQueue()[unit.GetTrainingQueueCount() - 1];

                                    // Actual time decreases, but we'll let it be the buildTime until latency catches up.
                                    unit.Self().RemainingTrainTime.Set(ut.BuildTime(), frame);
                                    unit.Self().BuildType.Set(ut, frame);
                                }

                                break;
                            }
                            case EventType.Finish:
                            {
                                if (unit.GetBuildType() == UnitType.None)
                                {
                                    unit.Self().Order.Set(Order.Nothing, frame);
                                }

                                break;
                            }
                        }
                    }

                    break;
                }
                case UnitCommandType.Cancel_Train:
                {
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            var unitType = unit.GetTrainingQueue()[unit.GetTrainingQueueCount() - 1];
                            _player.Self().Minerals.SetOrAdd(unitType.MineralPrice(), frame);
                            _player.Self().Gas.SetOrAdd(unitType.GasPrice(), frame);
                            unit.Self().BuildUnit.Set(-1, frame);
                            if (unit.GetTrainingQueueCount() == 1)
                            {
                                unit.Self().IsIdle.Set(false, frame);
                                unit.Self().IsTraining.Set(false, frame);
                            }

                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().TrainingQueueCount.SetOrAdd(-1, frame);
                            var unitType = unit.GetTrainingQueue()[unit.GetTrainingQueueCount()];
                            _player.Self().SupplyUsed[(int)unitType.GetRace()].SetOrAdd(-unitType.SupplyRequired(), frame);
                            if (unit.GetTrainingQueueCount() == 0)
                            {
                                unit.Self().BuildType.Set(UnitType.None, frame);
                            }
                            else
                            {
                                var ut = unit.GetTrainingQueue()[unit.GetTrainingQueueCount() - 1];

                                // Actual time decreases, but we'll let it be the buildTime until latency catches up.
                                unit.Self().RemainingTrainTime.Set(ut.BuildTime(), frame);
                                unit.Self().BuildType.Set(ut, frame);
                            }

                            break;
                        }
                        case EventType.Finish:
                        {
                            if (unit.GetBuildType() == UnitType.None)
                            {
                                unit.Self().Order.Set(Order.Nothing, frame);
                            }

                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Cancel_Upgrade:
                {
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            var upgradeType = unit.GetUpgrade();
                            var nextLevel = unit.GetPlayer().GetUpgradeLevel(upgradeType) + 1;
                            _player.Self().Minerals.SetOrAdd(upgradeType.MineralPrice(nextLevel), frame);
                            _player.Self().Gas.SetOrAdd(upgradeType.GasPrice(nextLevel), frame);
                            unit.Self().Upgrade.Set(UpgradeType.None, frame);
                            unit.Self().RemainingUpgradeTime.Set(0, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().Order.Set(Order.Nothing, frame);
                            unit.Self().IsIdle.Set(true, frame);
                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Cloak:
                {
                    unit.Self().Order.Set(Order.Cloak, frame);
                    unit.Self().Energy.SetOrAdd(-unit.GetUnitType().CloakingTech().EnergyCost(), frame);
                    break;
                }
                case UnitCommandType.Decloak:
                {
                    unit.Self().Order.Set(Order.Decloak, frame);
                    break;
                }
                case UnitCommandType.Follow:
                {
                    unit.Self().Order.Set(Order.Follow, frame);
                    unit.Self().Target.Set(GetUnitID(target), frame);
                    unit.Self().IsIdle.Set(false, frame);
                    unit.Self().IsMoving.Set(true, frame);
                    break;
                }
                case UnitCommandType.Gather:
                {
                    unit.Self().Target.Set(GetUnitID(target), frame);
                    unit.Self().IsIdle.Set(false, frame);
                    unit.Self().IsMoving.Set(true, frame);
                    unit.Self().IsGathering.Set(true, frame);

                    // @TODO: Fully time and test this order
                    if (target != null && target.Exists() && target.GetUnitType().IsMineralField())
                    {
                        unit.Self().Order.Set(Order.MoveToMinerals, frame);
                    }
                    else if (target != null && target.Exists() && target.GetUnitType().IsRefinery())
                    {
                        unit.Self().Order.Set(Order.MoveToGas, frame);
                    }

                    break;
                }
                case UnitCommandType.Halt_Construction:
                {
                    switch (_eventType)
                    {
                        case EventType.Order:
                            var building = unit.GetBuildUnit();

#pragma warning disable IDE0031
                            if (building != null)
#pragma warning restore IDE0031
                            {
                                building.Self().BuildUnit.Set(-1, frame);
                            }

                            unit.Self().BuildUnit.Set(-1, frame);
                            unit.Self().Order.Set(Order.ResetCollision, frame);
                            unit.Self().IsConstructing.Set(false, frame);
                            unit.Self().BuildType.Set(UnitType.None, frame);
                            break;
                        case EventType.Finish:
                            unit.Self().Order.Set(Order.PlayerGuard, frame);
                            unit.Self().IsIdle.Set(true, frame);
                            break;
                    }

                    break;
                }
                case UnitCommandType.Hold_Position:
                {
                    unit.Self().IsMoving.Set(false, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    unit.Self().Order.Set(Order.HoldPosition, frame);
                    break;
                }
                case UnitCommandType.Land:
                {
                    unit.Self().Order.Set(Order.BuildingLand, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Lift:
                {
                    unit.Self().Order.Set(Order.BuildingLiftOff, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Load:
                {
                    if (unit.GetUnitType() == UnitType.Terran_Bunker)
                    {
                        unit.Self().Order.Set(Order.PickupBunker, frame);
                        unit.Self().Target.Set(GetUnitID(target), frame);
                    }
                    else if (unit.GetUnitType().SpaceProvided() != 0)
                    {
                        unit.Self().Order.Set(Order.PickupTransport, frame);
                        unit.Self().Target.Set(GetUnitID(target), frame);
                    }
                    else if (target != null && target.Exists() && target.GetUnitType().SpaceProvided() != 0)
                    {
                        unit.Self().Order.Set(Order.EnterTransport, frame);
                        unit.Self().Target.Set(GetUnitID(target), frame);
                    }

                    unit.Self().IsIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Morph:
                {
                    var morphType = (UnitType)_command._extra;
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            if (!isCurrentFrame)
                            {
                                unit.Self().IsCompleted.Set(false, frame);
                                unit.Self().IsIdle.Set(false, frame);
                                unit.Self().IsConstructing.Set(true, frame);
                                unit.Self().IsMorphing.Set(true, frame);
                                unit.Self().BuildType.Set(morphType, frame);
                            }

                            if (unit.GetUnitType().IsBuilding())
                            {
                                if (!isCurrentFrame)
                                {

                                    // Actions that don't happen when we're reserving resources
                                    unit.Self().Order.Set(Order.ZergBuildingMorph, frame);
                                    unit.Self().Type.Set(morphType, frame);
                                }

                                _player.Self().Minerals.SetOrAdd(-morphType.MineralPrice(), frame);
                                _player.Self().Gas.SetOrAdd(-morphType.GasPrice(), frame);
                            }
                            else
                            {
                                _player.Self().SupplyUsed[(int)morphType.GetRace()].SetOrAdd(morphType.SupplyRequired() * (1 + (morphType.IsTwoUnitsInOneEgg() ? 1 : 0)) - unit.GetUnitType().SupplyRequired(), frame);
                                if (!isCurrentFrame)
                                {
                                    unit.Self().Order.Set(Order.ZergUnitMorph, frame);
                                    _player.Self().Minerals.SetOrAdd(-morphType.MineralPrice(), frame);
                                    _player.Self().Gas.SetOrAdd(-morphType.GasPrice(), frame);
                                    switch (morphType)
                                    {
                                        case UnitType.Zerg_Lurker_Egg:
                                            unit.Self().Type.Set(UnitType.Zerg_Lurker_Egg, frame);
                                            break;
                                        case UnitType.Zerg_Devourer:
                                        case UnitType.Zerg_Guardian:
                                            unit.Self().Type.Set(UnitType.Zerg_Cocoon, frame);
                                            break;
                                        default:
                                            unit.Self().Type.Set(UnitType.Zerg_Egg, frame);
                                            break;
                                    }

                                    unit.Self().TrainingQueue[unit.GetTrainingQueueCount()].Set(morphType, frame);
                                    unit.Self().TrainingQueueCount.SetOrAdd(+1, frame);
                                }
                            }

                            break;
                        }
                        case EventType.Order:
                        {
                            if (unit.GetUnitType().IsBuilding())
                            {
                                unit.Self().Order.Set(Order.IncompleteBuilding, frame);
                            }

                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Move:
                {
                    unit.Self().Order.Set(Order.Move, frame);
                    unit.Self().TargetPositionX.Set(_command._x, frame);
                    unit.Self().TargetPositionY.Set(_command._y, frame);
                    unit.Self().OrderTargetPositionX.Set(_command._x, frame);
                    unit.Self().OrderTargetPositionY.Set(_command._y, frame);
                    unit.Self().IsMoving.Set(true, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Patrol:
                {
                    unit.Self().Order.Set(Order.Patrol, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    unit.Self().IsMoving.Set(true, frame);
                    unit.Self().TargetPositionX.Set(_command._x, frame);
                    unit.Self().TargetPositionY.Set(_command._y, frame);
                    unit.Self().OrderTargetPositionX.Set(_command._x, frame);
                    unit.Self().OrderTargetPositionY.Set(_command._y, frame);
                    break;
                }
                case UnitCommandType.Repair:
                {
                    if (unit.GetUnitType() != UnitType.Terran_SCV)
                    {
                        return;
                    }

                    unit.Self().Order.Set(Order.Repair, frame);
                    unit.Self().Target.Set(GetUnitID(target), frame);
                    unit.Self().IsIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Research:
                {
                    var techType = (TechType)_command._extra;
                    unit.Self().Order.Set(Order.ResearchTech, frame);
                    unit.Self().Tech.Set(techType, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    unit.Self().RemainingResearchTime.Set(techType.ResearchTime(), frame);
                    _player.Self().Minerals.SetOrAdd(-techType.MineralPrice(), frame);
                    _player.Self().Gas.SetOrAdd(-techType.GasPrice(), frame);
                    _player.Self().IsResearching[(int)techType].Set(true, frame);
                    break;
                }
                case UnitCommandType.Return_Cargo:
                {
                    if (!unit.IsCarrying())
                    {
                        return;
                    }

                    unit.Self().Order.Set(unit.IsCarryingGas() ? Order.ReturnGas : Order.ReturnMinerals, frame);
                    unit.Self().IsGathering.Set(true, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Right_Click_Position:
                {
                    unit.Self().Order.Set(Order.Move, frame);
                    unit.Self().TargetPositionX.Set(_command._x, frame);
                    unit.Self().TargetPositionY.Set(_command._y, frame);
                    unit.Self().OrderTargetPositionX.Set(_command._x, frame);
                    unit.Self().OrderTargetPositionY.Set(_command._y, frame);
                    unit.Self().IsMoving.Set(true, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Right_Click_Unit:
                {
                    if (target != null && target.Exists())
                    {
                        unit.Self().Target.Set(GetUnitID(target), frame);
                        unit.Self().IsIdle.Set(false, frame);
                        unit.Self().IsMoving.Set(true, frame);
                        if (unit.GetUnitType().IsWorker() && target.GetUnitType().IsMineralField())
                        {
                            unit.Self().IsGathering.Set(true, frame);
                            unit.Self().Order.Set(Order.MoveToMinerals, frame);
                        }
                        else if (unit.GetUnitType().IsWorker() && target.GetUnitType().IsRefinery())
                        {
                            unit.Self().IsGathering.Set(true, frame);
                            unit.Self().Order.Set(Order.MoveToGas, frame);
                        }
                        else if (unit.GetUnitType().IsWorker() && target.GetUnitType().GetRace() == Race.Terran && target.GetUnitType().WhatBuilds().GetFirst() == unit.GetUnitType() && !target.IsCompleted())
                        {
                            unit.Self().Order.Set(Order.ConstructingBuilding, frame);
                            unit.Self().BuildUnit.Set(GetUnitID(target), frame);
                            target.Self().BuildUnit.Set(GetUnitID(unit), frame);
                            unit.Self().IsConstructing.Set(true, frame);
                            target.Self().IsConstructing.Set(true, frame);
                        }
                        else if (unit.GetUnitType().CanAttack() && target.GetPlayer() != unit.GetPlayer() && !target.GetUnitType().IsNeutral())
                        {
                            unit.Self().Order.Set(Order.AttackUnit, frame);
                        }
                        else if (unit.GetUnitType().CanMove())
                        {
                            unit.Self().Order.Set(Order.Follow, frame);
                        }
                    }

                    break;
                }
                case UnitCommandType.Set_Rally_Position:
                {
                    if (!unit.GetUnitType().CanProduce())
                    {
                        return;
                    }

                    unit.Self().Order.Set(Order.RallyPointTile, frame);
                    unit.Self().RallyPositionX.Set(_command._x, frame);
                    unit.Self().RallyPositionY.Set(_command._y, frame);
                    unit.Self().RallyUnit.Set(-1, frame);
                    break;
                }
                case UnitCommandType.Set_Rally_Unit:
                {
                    if (!unit.GetUnitType().CanProduce())
                    {
                        return;
                    }

                    if (target == null || !target.Exists())
                    {
                        return;
                    }

                    unit.Self().Order.Set(Order.RallyPointUnit, frame);
                    unit.Self().RallyUnit.Set(GetUnitID(target), frame);
                    break;
                }
                case UnitCommandType.Siege:
                {
                    unit.Self().Order.Set(Order.Sieging, frame);
                    break;
                }
                case UnitCommandType.Stop:
                {
                    unit.Self().Order.Set(Order.Stop, frame);
                    unit.Self().IsIdle.Set(true, frame);
                    break;
                }
                case UnitCommandType.Train:
                {
                    var unitType = (UnitType)_command._extra;
                    if (!isCurrentFrame)
                    {

                        // Happens on RLF, we don't want to duplicate this.
                        _player.Self().Minerals.SetOrAdd(-unitType.MineralPrice(), frame);
                        _player.Self().Gas.SetOrAdd(-unitType.GasPrice(), frame);
                    }


                    // Happens on RLF + 1, we want to pretend this happens on RLF.
                    unit.Self().TrainingQueue[unit.GetTrainingQueueCount()].Set(unitType, frame);
                    unit.Self().TrainingQueueCount.SetOrAdd(+1, frame);
                    _player.Self().SupplyUsed[(int)unitType.GetRace()].SetOrAdd(unitType.SupplyRequired(), frame);

                    // Happens on RLF or RLF + 1, doesn't matter if we do twice
                    unit.Self().IsTraining.Set(true, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    unit.Self().RemainingTrainTime.Set(unitType.BuildTime(), frame);
                    if (unitType == UnitType.Terran_Nuclear_Missile)
                    {
                        unit.Self().SecondaryOrder.Set(Order.Train, frame);
                    }

                    break;
                }
                case UnitCommandType.Unburrow:
                {
                    unit.Self().Order.Set(Order.Unburrowing, frame);
                    break;
                }
                case UnitCommandType.Unload:
                {
                    unit.Self().Order.Set(Order.Unload, frame);
                    unit.Self().Target.Set(GetUnitID(target), frame);
                    break;
                }
                case UnitCommandType.Unload_All:
                {
                    if (unit.GetUnitType() == UnitType.Terran_Bunker)
                    {
                        unit.Self().Order.Set(Order.Unload, frame);
                    }
                    else
                    {
                        unit.Self().Order.Set(Order.MoveUnload, frame);
                        unit.Self().TargetPositionX.Set(_command._x, frame);
                        unit.Self().TargetPositionY.Set(_command._y, frame);
                        unit.Self().OrderTargetPositionX.Set(_command._x, frame);
                        unit.Self().OrderTargetPositionY.Set(_command._y, frame);
                    }

                    break;
                }
                case UnitCommandType.Unload_All_Position:
                {
                    unit.Self().Order.Set(Order.MoveUnload, frame);
                    unit.Self().TargetPositionX.Set(_command._x, frame);
                    unit.Self().TargetPositionY.Set(_command._y, frame);
                    unit.Self().OrderTargetPositionX.Set(_command._x, frame);
                    unit.Self().OrderTargetPositionY.Set(_command._y, frame);
                    break;
                }
                case UnitCommandType.Unsiege:
                {
                    unit.Self().Order.Set(Order.Unsieging, frame);
                    break;
                }
                case UnitCommandType.Upgrade:
                {
                    var upgradeType = (UpgradeType)_command._extra;
                    unit.Self().Order.Set(Order.Upgrade, frame);
                    unit.Self().Upgrade.Set(upgradeType, frame);
                    unit.Self().IsIdle.Set(false, frame);
                    var level = unit.GetPlayer().GetUpgradeLevel(upgradeType);
                    unit.Self().RemainingUpgradeTime.Set(upgradeType.UpgradeTime(level + 1), frame);
                    _player.Self().Minerals.SetOrAdd(-upgradeType.MineralPrice(level + 1), frame);
                    _player.Self().Gas.SetOrAdd(upgradeType.GasPrice(level + 1), frame);
                    _player.Self().IsUpgrading[(int)upgradeType].Set(true, frame);
                    break;
                }
                case UnitCommandType.Use_Tech:
                {
                    if ((TechType)_command._extra == TechType.Stim_Packs && unit.GetHitPoints() > 10)
                    {
                        unit.Self().HitPoints.SetOrAdd(-10, frame);
                        unit.Self().StimTimer.Set(17, frame);
                    }

                    break;
                }
                case UnitCommandType.Use_Tech_Position:
                {
                    var techType = (TechType)_command._extra;
                    if (!techType.TargetsPosition())
                    {
                        return;
                    }

                    unit.Self().Order.Set(techType.GetOrder(), frame);
                    unit.Self().TargetPositionX.Set(_command._x, frame);
                    unit.Self().TargetPositionY.Set(_command._y, frame);
                    unit.Self().OrderTargetPositionX.Set(_command._x, frame);
                    unit.Self().OrderTargetPositionY.Set(_command._y, frame);
                    break;
                }
                case UnitCommandType.Use_Tech_Unit:
                {
                    if (target != null && target.Exists())
                    {
                        var techType = (TechType)_command._extra;
                        if (!techType.TargetsUnit())
                        {
                            return;
                        }

                        unit.Self().Order.Set(techType.GetOrder(), frame);
                        unit.Self().OrderTarget.Set(GetUnitID(target), frame);
                        var targetPosition = target.GetPosition();
                        unit.Self().TargetPositionX.Set(targetPosition.x, frame);
                        unit.Self().TargetPositionY.Set(targetPosition.y, frame);
                        unit.Self().OrderTargetPositionX.Set(targetPosition.x, frame);
                        unit.Self().OrderTargetPositionY.Set(targetPosition.y, frame);
                    }

                    break;
                }
            }
        }
    }
}