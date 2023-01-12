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
            if (_player == null)
            {
                _player = unit != null ? unit.GetPlayer() : _game.Self();
            }

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
                    unit.Self().order.Set(Order.AttackMove, frame);
                    unit.Self().targetPositionX.Set(_command._x, frame);
                    unit.Self().targetPositionY.Set(_command._y, frame);
                    unit.Self().orderTargetPositionX.Set(_command._x, frame);
                    unit.Self().orderTargetPositionY.Set(_command._y, frame);
                    break;
                }
                case UnitCommandType.Attack_Unit:
                {
                    if (target == null || !target.Exists() || !unit.GetUnitType().CanAttack())
                    {
                        return;
                    }

                    unit.Self().order.Set(Order.AttackUnit, frame);
                    unit.Self().target.Set(GetUnitID(target), frame);
                    break;
                }
                case UnitCommandType.Build:
                {
                    unit.Self().order.Set(Order.PlaceBuilding, frame);
                    unit.Self().isConstructing.Set(true, frame);
                    unit.Self().isIdle.Set(false, frame);
                    unit.Self().buildType.Set((UnitType)_command._extra, frame);
                    break;
                }
                case UnitCommandType.Build_Addon:
                {
                    var addonType = (UnitType)_command._extra;
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            _player.Self().minerals.SetOrAdd(-addonType.MineralPrice(), frame);
                            _player.Self().gas.SetOrAdd(-addonType.GasPrice(), frame);
                            if (!isCurrentFrame)
                            {

                                // We will pretend the building is busy building, this doesn't
                                unit.Self().isIdle.Set(false, frame);
                                unit.Self().order.Set(Order.PlaceAddon, frame);
                            }

                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().isConstructing.Set(true, frame);
                            unit.Self().order.Set(Order.Nothing, frame);
                            unit.Self().secondaryOrder.Set(Order.BuildAddon, frame);
                            unit.Self().buildType.Set((UnitType)_command._extra, frame);
                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Burrow:
                {
                    unit.Self().order.Set(Order.Burrowing, frame);
                    break;
                }
                case UnitCommandType.Cancel_Addon:
                {
                    switch (_eventType)
                    {
                        case EventType.Resource:
                        {
                            var addonType = unit.GetBuildType();
                            _player.Self().minerals.SetOrAdd((int)(addonType.MineralPrice() * 0.75), frame);
                            _player.Self().gas.SetOrAdd((int)(addonType.GasPrice() * 0.75), frame);
                            unit.Self().buildType.Set(UnitType.None, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().remainingBuildTime.Set(0, frame);
                            unit.Self().isConstructing.Set(false, frame);
                            unit.Self().order.Set(Order.Nothing, frame);
                            unit.Self().isIdle.Set(true, frame);
                            unit.Self().buildUnit.Set(-1, frame);
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
                                    builder.Self().buildType.Set(UnitType.None, frame);
                                    break;
                                }
                                case EventType.Order:
                                {
                                    builder.Self().isConstructing.Set(false, frame);
                                    builder.Self().order.Set(Order.ResetCollision, frame);
                                    break;
                                }
                                case EventType.Finish:
                                {
                                    builder.Self().order.Set(Order.PlayerGuard, frame);
                                    break;
                                }
                            }
                        }
                    }

                    if (_eventType == EventType.Resource)
                    {
                        unit.Self().buildUnit.Set(-1, frame);
                        _player.Self().minerals.SetOrAdd((int)(unit.GetUnitType().MineralPrice() * 0.75), frame);
                        _player.Self().gas.SetOrAdd((int)(unit.GetUnitType().GasPrice() * 0.75), frame);
                        unit.Self().remainingBuildTime.Set(0, frame);
                    }

                    if (unit.GetUnitType().GetRace() == Race.Zerg)
                    {
                        switch (_eventType)
                        {
                            case EventType.Resource:
                            {
                                unit.Self().type.Set(unit.GetUnitType().WhatBuilds().GetFirst(), frame);
                                unit.Self().buildType.Set(UnitType.None, frame);
                                unit.Self().isMorphing.Set(false, frame);
                                unit.Self().order.Set(Order.ResetCollision, frame);
                                unit.Self().isConstructing.Set(false, frame);
                                _player.Self().supplyUsed[(int)unit.GetUnitType().GetRace()].SetOrAdd(unit.GetUnitType().SupplyRequired(), frame);
                                break;
                            }
                            case EventType.Order:
                            {
                                unit.Self().order.Set(Order.PlayerGuard, frame);
                                unit.Self().isIdle.Set(true, frame);
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
                                _player.Self().minerals.SetOrAdd((int)(builtType.MineralPrice() * 0.75), frame);
                                _player.Self().gas.SetOrAdd((int)(builtType.GasPrice() * 0.75), frame);
                            }
                            else
                            {
                                _player.Self().minerals.SetOrAdd(builtType.MineralPrice(), frame);
                                _player.Self().gas.SetOrAdd(builtType.GasPrice(), frame);
                            }

                            if (newType.IsBuilding() && newType.ProducesCreep())
                            {
                                unit.Self().order.Set(Order.InitCreepGrowth, frame);
                            }

                            if (unit.GetUnitType() != UnitType.Zerg_Egg)
                            {
                                // Issue #781
                                // https://github.com/bwapi/bwapi/issues/781
                                unit.Self().type.Set(newType, frame);
                            }

                            unit.Self().buildType.Set(UnitType.None, frame);
                            unit.Self().isConstructing.Set(false, frame);
                            unit.Self().isMorphing.Set(false, frame);
                            unit.Self().isCompleted.Set(true, frame);
                            unit.Self().remainingBuildTime.Set(0, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            if (unit.GetUnitType().IsBuilding())
                            {
                                // This event would hopefully not have been created
                                // if this wasn't true (see event note above)
                                unit.Self().isIdle.Set(true, frame);
                                unit.Self().order.Set(Order.Nothing, frame);
                                if (unit.GetUnitType() == UnitType.Zerg_Hatchery || unit.GetUnitType() == UnitType.Zerg_Lair)
                                {
                                    // Type should have updated during last event to the cancelled type
                                    unit.Self().secondaryOrder.Set(Order.SpreadCreep, frame);
                                }
                            }
                            else
                            {
                                _player.Self().supplyUsed[(int)unit.GetUnitType().GetRace()].SetOrAdd(-(unit.GetUnitType().SupplyRequired() * (1 + (unit.GetUnitType().IsTwoUnitsInOneEgg() ? 1 : 0))), frame);
                                _player.Self().supplyUsed[(int)unit.GetUnitType().GetRace()].SetOrAdd(unit.GetUnitType().WhatBuilds().GetFirst().SupplyRequired() * unit.GetUnitType().WhatBuilds().GetSecond(), frame); // Note: unit.getType().whatBuilds().second is always 1 but we
                                // might as well handle the general case, in case Blizzard
                                // all of a sudden allows you to cancel archon morphs
                            }

                            break;
                        }
                        case EventType.Finish:
                        {
                            if (unit.GetUnitType() == UnitType.Zerg_Hatchery || unit.GetUnitType() == UnitType.Zerg_Lair)
                            {
                                unit.Self().secondaryOrder.Set(Order.SpawningLarva, frame);
                            }
                            else if (!unit.GetUnitType().IsBuilding())
                            {
                                unit.Self().order.Set(Order.PlayerGuard, frame);
                                unit.Self().isCompleted.Set(true, frame);
                                unit.Self().isConstructing.Set(false, frame);
                                unit.Self().isIdle.Set(true, frame);
                                unit.Self().isMorphing.Set(false, frame);
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
                            _player.Self().minerals.SetOrAdd(techType.MineralPrice(), frame);
                            _player.Self().gas.SetOrAdd(techType.GasPrice(), frame);
                            unit.Self().remainingResearchTime.Set(0, frame);
                            unit.Self().tech.Set(TechType.None, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().order.Set(Order.Nothing, frame);
                            unit.Self().isIdle.Set(true, frame);
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
                            _player.Self().minerals.SetOrAdd(unitType.MineralPrice(), frame);
                            _player.Self().gas.SetOrAdd(unitType.GasPrice(), frame);

                            // Shift training queue back one slot after the cancelled unit
                            for (var i = _command._extra; i < 4; ++i)
                            {
                                unit.Self().trainingQueue[i].Set(unit.GetTrainingQueue()[i + 1], frame);
                            }

                            unit.Self().trainingQueueCount.SetOrAdd(-1, frame);
                        }
                    }
                    else
                    {
                        switch (_eventType)
                        {
                            case EventType.Resource:
                            {
                                var unitType = unit.GetTrainingQueue()[unit.GetTrainingQueueCount() - 1];
                                _player.Self().minerals.SetOrAdd(unitType.MineralPrice(), frame);
                                _player.Self().gas.SetOrAdd(unitType.GasPrice(), frame);
                                unit.Self().buildUnit.Set(-1, frame);
                                if (unit.GetTrainingQueueCount() == 1)
                                {
                                    unit.Self().isIdle.Set(false, frame);
                                    unit.Self().isTraining.Set(false, frame);
                                }

                                break;
                            }
                            case EventType.Order:
                            {
                                unit.Self().trainingQueueCount.SetOrAdd(-1, frame);
                                var unitType = unit.GetTrainingQueue()[unit.GetTrainingQueueCount()];
                                _player.Self().supplyUsed[(int)unitType.GetRace()].SetOrAdd(-unitType.SupplyRequired(), frame);
                                if (unit.GetTrainingQueueCount() == 0)
                                {
                                    unit.Self().buildType.Set(UnitType.None, frame);
                                }
                                else
                                {
                                    var ut = unit.GetTrainingQueue()[unit.GetTrainingQueueCount() - 1];

                                    // Actual time decreases, but we'll let it be the buildTime until latency catches up.
                                    unit.Self().remainingTrainTime.Set(ut.BuildTime(), frame);
                                    unit.Self().buildType.Set(ut, frame);
                                }

                                break;
                            }
                            case EventType.Finish:
                            {
                                if (unit.GetBuildType() == UnitType.None)
                                {
                                    unit.Self().order.Set(Order.Nothing, frame);
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
                            _player.Self().minerals.SetOrAdd(unitType.MineralPrice(), frame);
                            _player.Self().gas.SetOrAdd(unitType.GasPrice(), frame);
                            unit.Self().buildUnit.Set(-1, frame);
                            if (unit.GetTrainingQueueCount() == 1)
                            {
                                unit.Self().isIdle.Set(false, frame);
                                unit.Self().isTraining.Set(false, frame);
                            }

                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().trainingQueueCount.SetOrAdd(-1, frame);
                            var unitType = unit.GetTrainingQueue()[unit.GetTrainingQueueCount()];
                            _player.Self().supplyUsed[(int)unitType.GetRace()].SetOrAdd(-unitType.SupplyRequired(), frame);
                            if (unit.GetTrainingQueueCount() == 0)
                            {
                                unit.Self().buildType.Set(UnitType.None, frame);
                            }
                            else
                            {
                                var ut = unit.GetTrainingQueue()[unit.GetTrainingQueueCount() - 1];

                                // Actual time decreases, but we'll let it be the buildTime until latency catches up.
                                unit.Self().remainingTrainTime.Set(ut.BuildTime(), frame);
                                unit.Self().buildType.Set(ut, frame);
                            }

                            break;
                        }
                        case EventType.Finish:
                        {
                            if (unit.GetBuildType() == UnitType.None)
                            {
                                unit.Self().order.Set(Order.Nothing, frame);
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
                            _player.Self().minerals.SetOrAdd(upgradeType.MineralPrice(nextLevel), frame);
                            _player.Self().gas.SetOrAdd(upgradeType.GasPrice(nextLevel), frame);
                            unit.Self().upgrade.Set(UpgradeType.None, frame);
                            unit.Self().remainingUpgradeTime.Set(0, frame);
                            break;
                        }
                        case EventType.Order:
                        {
                            unit.Self().order.Set(Order.Nothing, frame);
                            unit.Self().isIdle.Set(true, frame);
                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Cloak:
                {
                    unit.Self().order.Set(Order.Cloak, frame);
                    unit.Self().energy.SetOrAdd(-unit.GetUnitType().CloakingTech().EnergyCost(), frame);
                    break;
                }
                case UnitCommandType.Decloak:
                {
                    unit.Self().order.Set(Order.Decloak, frame);
                    break;
                }
                case UnitCommandType.Follow:
                {
                    unit.Self().order.Set(Order.Follow, frame);
                    unit.Self().target.Set(GetUnitID(target), frame);
                    unit.Self().isIdle.Set(false, frame);
                    unit.Self().isMoving.Set(true, frame);
                    break;
                }
                case UnitCommandType.Gather:
                {
                    unit.Self().target.Set(GetUnitID(target), frame);
                    unit.Self().isIdle.Set(false, frame);
                    unit.Self().isMoving.Set(true, frame);
                    unit.Self().isGathering.Set(true, frame);

                    // @TODO: Fully time and test this order
                    if (target != null && target.Exists() && target.GetUnitType().IsMineralField())
                    {
                        unit.Self().order.Set(Order.MoveToMinerals, frame);
                    }
                    else if (target != null && target.Exists() && target.GetUnitType().IsRefinery())
                    {
                        unit.Self().order.Set(Order.MoveToGas, frame);
                    }

                    break;
                }
                case UnitCommandType.Halt_Construction:
                {
                    switch (_eventType)
                    {
                        case EventType.Order:
                            var building = unit.GetBuildUnit();
                            if (building != null)
                            {
                                building.Self().buildUnit.Set(-1, frame);
                            }

                            unit.Self().buildUnit.Set(-1, frame);
                            unit.Self().order.Set(Order.ResetCollision, frame);
                            unit.Self().isConstructing.Set(false, frame);
                            unit.Self().buildType.Set(UnitType.None, frame);
                            break;
                        case EventType.Finish:
                            unit.Self().order.Set(Order.PlayerGuard, frame);
                            unit.Self().isIdle.Set(true, frame);
                            break;
                    }

                    break;
                }
                case UnitCommandType.Hold_Position:
                {
                    unit.Self().isMoving.Set(false, frame);
                    unit.Self().isIdle.Set(false, frame);
                    unit.Self().order.Set(Order.HoldPosition, frame);
                    break;
                }
                case UnitCommandType.Land:
                {
                    unit.Self().order.Set(Order.BuildingLand, frame);
                    unit.Self().isIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Lift:
                {
                    unit.Self().order.Set(Order.BuildingLiftOff, frame);
                    unit.Self().isIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Load:
                {
                    if (unit.GetUnitType() == UnitType.Terran_Bunker)
                    {
                        unit.Self().order.Set(Order.PickupBunker, frame);
                        unit.Self().target.Set(GetUnitID(target), frame);
                    }
                    else if (unit.GetUnitType().SpaceProvided() != 0)
                    {
                        unit.Self().order.Set(Order.PickupTransport, frame);
                        unit.Self().target.Set(GetUnitID(target), frame);
                    }
                    else if (target != null && target.Exists() && target.GetUnitType().SpaceProvided() != 0)
                    {
                        unit.Self().order.Set(Order.EnterTransport, frame);
                        unit.Self().target.Set(GetUnitID(target), frame);
                    }

                    unit.Self().isIdle.Set(false, frame);
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
                                unit.Self().isCompleted.Set(false, frame);
                                unit.Self().isIdle.Set(false, frame);
                                unit.Self().isConstructing.Set(true, frame);
                                unit.Self().isMorphing.Set(true, frame);
                                unit.Self().buildType.Set(morphType, frame);
                            }

                            if (unit.GetUnitType().IsBuilding())
                            {
                                if (!isCurrentFrame)
                                {

                                    // Actions that don't happen when we're reserving resources
                                    unit.Self().order.Set(Order.ZergBuildingMorph, frame);
                                    unit.Self().type.Set(morphType, frame);
                                }

                                _player.Self().minerals.SetOrAdd(-morphType.MineralPrice(), frame);
                                _player.Self().gas.SetOrAdd(-morphType.GasPrice(), frame);
                            }
                            else
                            {
                                _player.Self().supplyUsed[(int)morphType.GetRace()].SetOrAdd(morphType.SupplyRequired() * (1 + (morphType.IsTwoUnitsInOneEgg() ? 1 : 0)) - unit.GetUnitType().SupplyRequired(), frame);
                                if (!isCurrentFrame)
                                {
                                    unit.Self().order.Set(Order.ZergUnitMorph, frame);
                                    _player.Self().minerals.SetOrAdd(-morphType.MineralPrice(), frame);
                                    _player.Self().gas.SetOrAdd(-morphType.GasPrice(), frame);
                                    switch (morphType)
                                    {
                                        case UnitType.Zerg_Lurker_Egg:
                                            unit.Self().type.Set(UnitType.Zerg_Lurker_Egg, frame);
                                            break;
                                        case UnitType.Zerg_Devourer:
                                        case UnitType.Zerg_Guardian:
                                            unit.Self().type.Set(UnitType.Zerg_Cocoon, frame);
                                            break;
                                        default:
                                            unit.Self().type.Set(UnitType.Zerg_Egg, frame);
                                            break;
                                    }

                                    unit.Self().trainingQueue[unit.GetTrainingQueueCount()].Set(morphType, frame);
                                    unit.Self().trainingQueueCount.SetOrAdd(+1, frame);
                                }
                            }

                            break;
                        }
                        case EventType.Order:
                        {
                            if (unit.GetUnitType().IsBuilding())
                            {
                                unit.Self().order.Set(Order.IncompleteBuilding, frame);
                            }

                            break;
                        }
                    }

                    break;
                }
                case UnitCommandType.Move:
                {
                    unit.Self().order.Set(Order.Move, frame);
                    unit.Self().targetPositionX.Set(_command._x, frame);
                    unit.Self().targetPositionY.Set(_command._y, frame);
                    unit.Self().orderTargetPositionX.Set(_command._x, frame);
                    unit.Self().orderTargetPositionY.Set(_command._y, frame);
                    unit.Self().isMoving.Set(true, frame);
                    unit.Self().isIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Patrol:
                {
                    unit.Self().order.Set(Order.Patrol, frame);
                    unit.Self().isIdle.Set(false, frame);
                    unit.Self().isMoving.Set(true, frame);
                    unit.Self().targetPositionX.Set(_command._x, frame);
                    unit.Self().targetPositionY.Set(_command._y, frame);
                    unit.Self().orderTargetPositionX.Set(_command._x, frame);
                    unit.Self().orderTargetPositionY.Set(_command._y, frame);
                    break;
                }
                case UnitCommandType.Repair:
                {
                    if (unit.GetUnitType() != UnitType.Terran_SCV)
                    {
                        return;
                    }

                    unit.Self().order.Set(Order.Repair, frame);
                    unit.Self().target.Set(GetUnitID(target), frame);
                    unit.Self().isIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Research:
                {
                    var techType = (TechType)_command._extra;
                    unit.Self().order.Set(Order.ResearchTech, frame);
                    unit.Self().tech.Set(techType, frame);
                    unit.Self().isIdle.Set(false, frame);
                    unit.Self().remainingResearchTime.Set(techType.ResearchTime(), frame);
                    _player.Self().minerals.SetOrAdd(-techType.MineralPrice(), frame);
                    _player.Self().gas.SetOrAdd(-techType.GasPrice(), frame);
                    _player.Self().isResearching[(int)techType].Set(true, frame);
                    break;
                }
                case UnitCommandType.Return_Cargo:
                {
                    if (!unit.IsCarrying())
                    {
                        return;
                    }

                    unit.Self().order.Set(unit.IsCarryingGas() ? Order.ReturnGas : Order.ReturnMinerals, frame);
                    unit.Self().isGathering.Set(true, frame);
                    unit.Self().isIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Right_Click_Position:
                {
                    unit.Self().order.Set(Order.Move, frame);
                    unit.Self().targetPositionX.Set(_command._x, frame);
                    unit.Self().targetPositionY.Set(_command._y, frame);
                    unit.Self().orderTargetPositionX.Set(_command._x, frame);
                    unit.Self().orderTargetPositionY.Set(_command._y, frame);
                    unit.Self().isMoving.Set(true, frame);
                    unit.Self().isIdle.Set(false, frame);
                    break;
                }
                case UnitCommandType.Right_Click_Unit:
                {
                    if (target != null && target.Exists())
                    {
                        unit.Self().target.Set(GetUnitID(target), frame);
                        unit.Self().isIdle.Set(false, frame);
                        unit.Self().isMoving.Set(true, frame);
                        if (unit.GetUnitType().IsWorker() && target.GetUnitType().IsMineralField())
                        {
                            unit.Self().isGathering.Set(true, frame);
                            unit.Self().order.Set(Order.MoveToMinerals, frame);
                        }
                        else if (unit.GetUnitType().IsWorker() && target.GetUnitType().IsRefinery())
                        {
                            unit.Self().isGathering.Set(true, frame);
                            unit.Self().order.Set(Order.MoveToGas, frame);
                        }
                        else if (unit.GetUnitType().IsWorker() && target.GetUnitType().GetRace() == Race.Terran && target.GetUnitType().WhatBuilds().GetFirst() == unit.GetUnitType() && !target.IsCompleted())
                        {
                            unit.Self().order.Set(Order.ConstructingBuilding, frame);
                            unit.Self().buildUnit.Set(GetUnitID(target), frame);
                            target.Self().buildUnit.Set(GetUnitID(unit), frame);
                            unit.Self().isConstructing.Set(true, frame);
                            target.Self().isConstructing.Set(true, frame);
                        }
                        else if (unit.GetUnitType().CanAttack() && target.GetPlayer() != unit.GetPlayer() && !target.GetUnitType().IsNeutral())
                        {
                            unit.Self().order.Set(Order.AttackUnit, frame);
                        }
                        else if (unit.GetUnitType().CanMove())
                        {
                            unit.Self().order.Set(Order.Follow, frame);
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

                    unit.Self().order.Set(Order.RallyPointTile, frame);
                    unit.Self().rallyPositionX.Set(_command._x, frame);
                    unit.Self().rallyPositionY.Set(_command._y, frame);
                    unit.Self().rallyUnit.Set(-1, frame);
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

                    unit.Self().order.Set(Order.RallyPointUnit, frame);
                    unit.Self().rallyUnit.Set(GetUnitID(target), frame);
                    break;
                }
                case UnitCommandType.Siege:
                {
                    unit.Self().order.Set(Order.Sieging, frame);
                    break;
                }
                case UnitCommandType.Stop:
                {
                    unit.Self().order.Set(Order.Stop, frame);
                    unit.Self().isIdle.Set(true, frame);
                    break;
                }
                case UnitCommandType.Train:
                {
                    var unitType = (UnitType)_command._extra;
                    if (!isCurrentFrame)
                    {

                        // Happens on RLF, we don't want to duplicate this.
                        _player.Self().minerals.SetOrAdd(-unitType.MineralPrice(), frame);
                        _player.Self().gas.SetOrAdd(-unitType.GasPrice(), frame);
                    }


                    // Happens on RLF + 1, we want to pretend this happens on RLF.
                    unit.Self().trainingQueue[unit.GetTrainingQueueCount()].Set(unitType, frame);
                    unit.Self().trainingQueueCount.SetOrAdd(+1, frame);
                    _player.Self().supplyUsed[(int)unitType.GetRace()].SetOrAdd(unitType.SupplyRequired(), frame);

                    // Happens on RLF or RLF + 1, doesn't matter if we do twice
                    unit.Self().isTraining.Set(true, frame);
                    unit.Self().isIdle.Set(false, frame);
                    unit.Self().remainingTrainTime.Set(unitType.BuildTime(), frame);
                    if (unitType == UnitType.Terran_Nuclear_Missile)
                    {
                        unit.Self().secondaryOrder.Set(Order.Train, frame);
                    }

                    break;
                }
                case UnitCommandType.Unburrow:
                {
                    unit.Self().order.Set(Order.Unburrowing, frame);
                    break;
                }
                case UnitCommandType.Unload:
                {
                    unit.Self().order.Set(Order.Unload, frame);
                    unit.Self().target.Set(GetUnitID(target), frame);
                    break;
                }
                case UnitCommandType.Unload_All:
                {
                    if (unit.GetUnitType() == UnitType.Terran_Bunker)
                    {
                        unit.Self().order.Set(Order.Unload, frame);
                    }
                    else
                    {
                        unit.Self().order.Set(Order.MoveUnload, frame);
                        unit.Self().targetPositionX.Set(_command._x, frame);
                        unit.Self().targetPositionY.Set(_command._y, frame);
                        unit.Self().orderTargetPositionX.Set(_command._x, frame);
                        unit.Self().orderTargetPositionY.Set(_command._y, frame);
                    }

                    break;
                }
                case UnitCommandType.Unload_All_Position:
                {
                    unit.Self().order.Set(Order.MoveUnload, frame);
                    unit.Self().targetPositionX.Set(_command._x, frame);
                    unit.Self().targetPositionY.Set(_command._y, frame);
                    unit.Self().orderTargetPositionX.Set(_command._x, frame);
                    unit.Self().orderTargetPositionY.Set(_command._y, frame);
                    break;
                }
                case UnitCommandType.Unsiege:
                {
                    unit.Self().order.Set(Order.Unsieging, frame);
                    break;
                }
                case UnitCommandType.Upgrade:
                {
                    var upgradeType = (UpgradeType)_command._extra;
                    unit.Self().order.Set(Order.Upgrade, frame);
                    unit.Self().upgrade.Set(upgradeType, frame);
                    unit.Self().isIdle.Set(false, frame);
                    var level = unit.GetPlayer().GetUpgradeLevel(upgradeType);
                    unit.Self().remainingUpgradeTime.Set(upgradeType.UpgradeTime(level + 1), frame);
                    _player.Self().minerals.SetOrAdd(-upgradeType.MineralPrice(level + 1), frame);
                    _player.Self().gas.SetOrAdd(upgradeType.GasPrice(level + 1), frame);
                    _player.Self().isUpgrading[(int)upgradeType].Set(true, frame);
                    break;
                }
                case UnitCommandType.Use_Tech:
                {
                    if ((TechType)_command._extra == TechType.Stim_Packs && unit.GetHitPoints() > 10)
                    {
                        unit.Self().hitPoints.SetOrAdd(-10, frame);
                        unit.Self().stimTimer.Set(17, frame);
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

                    unit.Self().order.Set(techType.GetOrder(), frame);
                    unit.Self().targetPositionX.Set(_command._x, frame);
                    unit.Self().targetPositionY.Set(_command._y, frame);
                    unit.Self().orderTargetPositionX.Set(_command._x, frame);
                    unit.Self().orderTargetPositionY.Set(_command._y, frame);
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

                        unit.Self().order.Set(techType.GetOrder(), frame);
                        unit.Self().orderTarget.Set(GetUnitID(target), frame);
                        var targetPosition = target.GetPosition();
                        unit.Self().targetPositionX.Set(targetPosition.x, frame);
                        unit.Self().targetPositionY.Set(targetPosition.y, frame);
                        unit.Self().orderTargetPositionX.Set(targetPosition.x, frame);
                        unit.Self().orderTargetPositionY.Set(targetPosition.y, frame);
                    }

                    break;
                }
            }
        }
    }
}