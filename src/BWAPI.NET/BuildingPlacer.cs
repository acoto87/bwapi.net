using System;
using System.Linq;

namespace BWAPI.NET
{
    public static class BuildingPlacer
    {
        private const int MaxRange = 64;
        private static readonly TilePosition[] gDirections = new[]{new TilePosition(1, 1), new TilePosition(0, 1), new TilePosition(-1, 1), new TilePosition(1, 0), new TilePosition(-1, 0), new TilePosition(1, -1), new TilePosition(0, -1), new TilePosition(-1, -1)};
        private static readonly BuildTemplate[] buildTemplates = new[]{new BuildTemplate(32, 0, 0, 1), new BuildTemplate(0, 32, 1, 0), new BuildTemplate(31, 0, 0, 1), new BuildTemplate(0, 31, 1, 0), new BuildTemplate(33, 0, 0, 1), new BuildTemplate(0, 33, 1, 0), new BuildTemplate(30, 0, 0, 1), new BuildTemplate(29, 0, 0, 1), new BuildTemplate(0, 30, 1, 0), new BuildTemplate(28, 0, 0, 1), new BuildTemplate(0, 29, 1, 0), new BuildTemplate(27, 0, 0, 1), new BuildTemplate(0, 28, 1, 0), new BuildTemplate(-1, 0, 0, 0)};

        #pragma warning disable IDE0060
        public static TilePosition GetBuildLocation(UnitType type, TilePosition desiredPosition1, int maxRange, bool creep, Game game)
        #pragma warning restore IDE0060
        {
            // Make sure the type is compatible
            if (!type.IsBuilding())
            {
                return TilePosition.Invalid;
            }

            TilePosition desiredPosition = desiredPosition1;

            // Do type-specific checks
            bool trimPlacement = true;
            Region pTargRegion = null;
            switch (type)
            {
                case UnitType.Protoss_Pylon:
                {
                    Unit pSpecialUnitTarget = game.GetClosestUnit(desiredPosition.ToPosition(), (u) => u.GetPlayer().Equals(game.Self()) && !u.IsPowered());
                    if (pSpecialUnitTarget != null)
                    {
                        desiredPosition = pSpecialUnitTarget.GetPosition().ToTilePosition();
                        trimPlacement = false;
                    }

                    break;
                }
                case UnitType.Terran_Command_Center:
                case UnitType.Protoss_Nexus:
                case UnitType.Zerg_Hatchery:
                case UnitType.Special_Start_Location:
                {
                    trimPlacement = false;
                    break;
                }
                case UnitType.Zerg_Creep_Colony:
                case UnitType.Terran_Bunker:
                {
                    //if ( Get bunker placement region )
                    //  trimPlacement = false;
                    break;
                }
            }

            PlacementReserve reserve = new PlacementReserve(maxRange);
            ReservePlacement(reserve, type, desiredPosition, game);
            if (trimPlacement)
            {
                ReserveTemplateSpacing(reserve);
            }

            TilePosition centerPosition = desiredPosition.Subtract(new TilePosition(MaxRange, MaxRange).Divide(2));
            if (pTargRegion != null)
            {
                desiredPosition = pTargRegion.GetCenter().ToTilePosition();
            }

            // Find the best position
            int bestDistance = 999999;
            int fallbackDistance = 999999;
            TilePosition bestPosition = TilePosition.None;
            TilePosition fallbackPosition = TilePosition.None;
            for (int passCount = 0; passCount < (pTargRegion != null ? 2 : 1); ++passCount)
            {
                for (int y = 0; y < MaxRange; ++y)
                {
                    for (int x = 0; x < MaxRange; ++x)
                    {
                        // Ignore if space is reserved
                        if (reserve.GetValue(x, y) == 0)
                        {
                            continue;
                        }

                        TilePosition currentPosition = new TilePosition(x, y).Add(centerPosition);

                        //Broodwar->getGroundDistance( desiredPosition, currentPosition );
                        int currentDistance = desiredPosition.GetApproxDistance(currentPosition);
                        if (currentDistance < bestDistance)
                        {
                            if (currentDistance <= maxRange)
                            {
                                bestDistance = currentDistance;
                                bestPosition = currentPosition;
                            }
                            else if (currentDistance < fallbackDistance)
                            {
                                fallbackDistance = currentDistance;
                                fallbackPosition = currentPosition;
                            }
                        }
                    }
                }

                // Break pass if position is found
                if (!bestPosition.Equals(TilePosition.None))
                {
                    break;
                }

                // Break if an alternative position was found
                if (!fallbackPosition.Equals(TilePosition.None))
                {
                    bestPosition = fallbackPosition;
                    break;
                }

                // If we were really targetting a region, and couldn't find a position above
                if (pTargRegion != null)
                {
                    // Then fallback to the default build position
                    desiredPosition = centerPosition;
                }
            }

            return bestPosition;
        }

        private static void ReservePlacement(PlacementReserve reserve, UnitType type, TilePosition desiredPosition, Game game)
        {
            // Reset the array
            reserve.Reset();
            AssignBuildableLocations(reserve, type, desiredPosition, game);
            RemoveDisconnected(reserve, desiredPosition, game);

            // @TODO: Assign 0 to all locations that have a ground distance > maxRange
            // exclude positions off the map
            TilePosition start = desiredPosition.Subtract(new TilePosition(MaxRange, MaxRange).Divide(2));
            reserve.Iterate((pr, x, y) =>
            {
                if (!start.Add(new TilePosition(x, y)).IsValid(game))
                {
                    pr.SetValue(x, y, (byte)0);
                }
            });

            // Return if can't find a valid space
            if (!reserve.HasValidSpace())
            {
                return;
            }

            ReserveGroundHeight(reserve, desiredPosition, game);

            //ReserveUnbuildable(reserve, type, desiredPosition); // NOTE: canBuildHere already includes this!
            if (!type.IsResourceDepot())
            {
                ReserveAllStructures(reserve, type, desiredPosition, game);
                ReserveExistingAddonPlacement(reserve, desiredPosition, game);
            }

            // Unit-specific reservations
            switch (type)
            {
                case UnitType.Protoss_Pylon:
                {
                    //reservePylonPlacement();
                    break;
                }
                case UnitType.Terran_Bunker:
                {
                    //if ( !GetBunkerPlacement() ){
                    //reserveTurretPlacement();
                    //}
                    break;
                }
                case UnitType.Terran_Missile_Turret:
                case UnitType.Protoss_Photon_Cannon:
                {
                    //reserveTurretPlacement();
                    break;
                }
                case UnitType.Zerg_Creep_Colony:
                {
                    //if ( creep || !GetBunkerPlacement() ){
                    //reserveTurretPlacement();
                    // }
                    break;
                }
                default:
                {
                    if (!type.IsResourceDepot())
                    {
                        ReserveDefault(reserve, type, desiredPosition, game);
                    }

                    break;
                }
            }
        }

        private static void AssignBuildableLocations(PlacementReserve reserve, UnitType type, TilePosition desiredPosition, Game game)
        {
            TilePosition start = desiredPosition.Subtract(new TilePosition(MaxRange, MaxRange).Divide(2));

            // Reserve space for the addon as well
            bool hasAddon = type.CanBuildAddon();

            // Assign 1 to all buildable locations
            reserve.Iterate((pr, x, y) =>
            {
                if ((!hasAddon || game.CanBuildHere(start.Add(new TilePosition(x + 4, y + 1)), UnitType.Terran_Missile_Turret)) && game.CanBuildHere(start.Add(new TilePosition(x, y)), type))
                {
                    pr.SetValue(x, y, (byte)1);
                }
            });
        }

        private static void RemoveDisconnected(PlacementReserve reserve, TilePosition desiredPosition, Game game)
        {
            TilePosition start = desiredPosition.Subtract(new TilePosition(MaxRange, MaxRange).Divide(2));

            // Assign 0 to all locations that aren't connected
            reserve.Iterate((pr, x, y) =>
            {
                if (!game.HasPath(desiredPosition.ToPosition(), start.Add(new TilePosition(x, y)).ToPosition()))
                {
                    pr.SetValue(x, y, (byte)0);
                }
            });
        }

        private static void ReserveGroundHeight(PlacementReserve reserve, TilePosition desiredPosition, Game game)
        {
            TilePosition start = desiredPosition.Subtract(new TilePosition(MaxRange, MaxRange).Divide(2));

            // Exclude locations with a different ground height, but restore a backup in case there are no more build locations
            reserve.Backup();
            int targetHeight = game.GetGroundHeight(desiredPosition);
            reserve.Iterate((pr, x, y) =>
            {
                if (game.GetGroundHeight(start.Add(new TilePosition(x, y))) != targetHeight)
                {
                    pr.SetValue(x, y, (byte)0);
                }
            });

            // Restore original if there is nothing left
            reserve.RestoreIfInvalid();
        }

        private static void ReserveAllStructures(PlacementReserve reserve, UnitType type, TilePosition desiredPosition, Game game)
        {
            if (type.IsAddon())
            {
                return;
            }

            reserve.Backup();

            // Reserve space around owned resource depots and resource containers
            game.Self().GetUnits().Where((u) =>
            {
                UnitType ut = u.GetUnitType();
                return u.Exists() && (u.IsCompleted() || ut.ProducesLarva() && u.IsMorphing()) && ut.IsBuilding() && (ut.IsResourceDepot() || ut.IsRefinery());
            }).ForEach((u) => ReserveStructure(reserve, u, 2, type, desiredPosition));

            // Reserve space around neutral resources
            if (type != UnitType.Terran_Bunker)
            {
                game.GetNeutralUnits().Where((u) => u.Exists() && u.GetUnitType().IsResourceContainer()).ForEach((u) => ReserveStructure(reserve, u, 2, type, desiredPosition));
            }

            reserve.RestoreIfInvalid();
        }

        private static void ReserveExistingAddonPlacement(PlacementReserve reserve, TilePosition desiredPosition, Game game)
        {
            TilePosition start = desiredPosition.Subtract(new TilePosition(MaxRange, MaxRange)).Divide(2);

            //Exclude addon placement locations
            reserve.Backup();
            game.Self().GetUnits().Where((u) => u.Exists() && u.GetUnitType().CanBuildAddon()).ForEach((u) =>
            {
                TilePosition addonPos = u.GetTilePosition().Add(new TilePosition(4, 1)).Subtract(start);
                reserve.SetRange(addonPos, addonPos.Add(new TilePosition(2, 2)), (byte)0);
            });

            // Restore if this gave us no build locations
            reserve.RestoreIfInvalid();
        }

        private static void ReserveDefault(PlacementReserve reserve, UnitType type, TilePosition desiredPosition, Game game)
        {
            reserve.Backup();
            PlacementReserve original = reserve;

            // Reserve some space around some specific units
            foreach (Unit it in game.Self().GetUnits())
            {
                if (!it.Exists())
                {
                    continue;
                }

                switch (it.GetUnitType())
                {
                    case UnitType.Terran_Factory:
                    case UnitType.Terran_Missile_Turret:
                    case UnitType.Protoss_Robotics_Facility:
                    case UnitType.Protoss_Gateway:
                    case UnitType.Protoss_Photon_Cannon:
                    case UnitType.Terran_Barracks:
                    case UnitType.Terran_Bunker:
                    case UnitType.Zerg_Creep_Colony:
                    {
                        ReserveStructure(reserve, it, 1, type, desiredPosition);
                        break;
                    }
                    default:
                    {
                        ReserveStructure(reserve, it, 2, type, desiredPosition);
                        break;
                    }
                }
            }

            switch (type)
            {
                case UnitType.Terran_Barracks:
                case UnitType.Terran_Factory:
                case UnitType.Terran_Missile_Turret:
                case UnitType.Terran_Bunker:
                case UnitType.Protoss_Robotics_Facility:
                case UnitType.Protoss_Gateway:
                case UnitType.Protoss_Photon_Cannon:
                {
                    for (int y = 0; y < 64; ++y)
                    {
                        for (int x = 0; x < 64; ++x)
                        {
                            for (int dir = 0; dir < 8; ++dir)
                            {
                                TilePosition p = new TilePosition(x, y).Add(gDirections[dir]);
                                if (!PlacementReserve.IsValidPos(p) || original.GetValue(p) == 0)
                                {
                                    reserve.SetValue(p, (byte)0);
                                }
                            }
                        }
                    }

                    break;
                }
            }

            reserve.RestoreIfInvalid();
        }

        private static void ReserveTemplateSpacing(PlacementReserve reserve)
        {
            reserve.Backup();
            for (int j = 0; buildTemplates[j].startX != -1; ++j)
            {
                BuildTemplate t = buildTemplates[j];
                int x = t.startX;
                int y = t.startY;
                for (int i = 0; i < 64; ++i)
                {
                    reserve.SetValue(x, y, (byte)0);
                    x += t.stepX;
                    y += t.stepY;
                }
            }

            reserve.RestoreIfInvalid();
        }

        private static void ReserveStructure(PlacementReserve reserve, Unit pUnit, int padding, UnitType type, TilePosition desiredPosition)
        {
            ReserveStructureWithPadding(reserve, pUnit.GetPosition().ToTilePosition(), pUnit.GetUnitType().TileSize(), padding, type, desiredPosition);
        }

        private static void ReserveStructureWithPadding(PlacementReserve reserve, TilePosition currentPosition, TilePosition sizeExtra, int padding, UnitType type, TilePosition desiredPosition)
        {
            TilePosition paddingSize = sizeExtra.Add(new TilePosition(padding, padding).Multiply(2));
            TilePosition topLeft = currentPosition.Subtract(type.TileSize()).Subtract(paddingSize.Divide(2)).Subtract(new TilePosition(1, 1));
            TilePosition topLeftRelative = topLeft.Subtract(desiredPosition).Add(new TilePosition(MaxRange, MaxRange).Divide(2));
            TilePosition maxSize = topLeftRelative.Add(type.TileSize()).Add(paddingSize).Add(new TilePosition(1, 1));
            reserve.SetRange(topLeftRelative, maxSize, (byte)0);
        }

        private delegate void PlacementReserveExec(PlacementReserve placementReserve, int x, int y);

        class BuildTemplate
        {
            public readonly int startX;
            public readonly int startY;
            public readonly int stepX;
            public readonly int stepY;

            public BuildTemplate(int startX, int startY, int stepX, int stepY)
            {
                this.startX = startX;
                this.startY = startY;
                this.stepX = stepX;
                this.stepY = stepY;
            }
        }

        class PlacementReserve
        {
            public readonly int maxSearch;
            public byte[][] data;
            public byte[][] save;

            public PlacementReserve(int maxRange)
            {
                maxSearch = Math.Min(Math.Max(0, maxRange), MaxRange);
                Reset();
                Backup();
            }

            // Checks if the given x/y value is valid for the Placement position
            public static bool IsValidPos(int x, int y)
            {
                return x >= 0 && x < MaxRange && y >= 0 && y < MaxRange;
            }

            public static bool IsValidPos(TilePosition p)
            {
                return IsValidPos(p.x, p.y);
            }

            public void Reset()
            {
                data = new byte[MaxRange][];
                save = new byte[MaxRange][];
                for (int i = 0; i < MaxRange; i++)
                {
                    data[i] = new byte[MaxRange];
                    save[i] = new byte[MaxRange];
                }
            }

            // Sets the value in the placement reserve array
            public void SetValue(int x, int y, byte value)
            {
                if (IsValidPos(x, y))
                {
                    data[y][x] = value;
                }
            }

            public void SetValue(TilePosition p, byte value)
            {
                SetValue(p.x, p.y, value);
            }

            public void SetRange(int left, int top, int right, int bottom, byte value)
            {
                for (int y = top; y < bottom; ++y)
                {
                    for (int x = left; x < right; ++x)
                    {
                        SetValue(x, y, value);
                    }
                }
            }

            public void SetRange(TilePosition lt, TilePosition rb, byte value)
            {
                SetRange(lt.x, lt.y, rb.x, rb.y, value);
            }

            // Gets the value from the placement reserve array, 0 if position is invalid
            public byte GetValue(int x, int y)
            {
                if (IsValidPos(x, y))
                {
                    return data[y][x];
                }

                return 0;
            }

            public byte GetValue(TilePosition p)
            {
                return GetValue(p.x, p.y);
            }

            public void Iterate(PlacementReserveExec proc)
            {

                // Get min/max distances
                int min = MaxRange / 2 - maxSearch / 2;
                int max = min + maxSearch;
                for (int y = min; y < max; ++y)
                {
                    for (int x = min; x < max; ++x)
                    {
                        proc(this, x, y);
                    }
                }
            }

            public bool HasValidSpace()
            {

                // Get min/max distances
                int min = MaxRange / 2 - maxSearch / 2;
                int max = min + maxSearch;
                for (int y = min; y < max; ++y)
                {
                    for (int x = min; x < max; ++x)
                    {
                        if (GetValue(x, y) == 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public void Backup()
            {
                for (int i = 0; i < MaxRange; i++)
                {
                    Array.Copy(data[i], 0, save[i], 0, MaxRange);
                }
            }

            public void Restore()
            {
                for (int i = 0; i < MaxRange; i++)
                {
                    Array.Copy(save[i], 0, data[i], 0, MaxRange);
                }
            }

            public void RestoreIfInvalid()
            {
                if (!HasValidSpace())
                {
                    Restore();
                }
            }
        }
    }
}