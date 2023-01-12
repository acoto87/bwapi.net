using System;

namespace BWAPI.NET
{
    public delegate bool UnitFilter(Unit u);

    public static class UnitFilters
    {
        public static readonly UnitFilter IsTransPort = u => u.GetUnitType().SpaceProvided() > 0 && u.GetUnitType() != UnitType.Terran_Bunker;
        public static readonly UnitFilter CanProduce = u => u.GetUnitType().CanProduce();
        public static readonly UnitFilter CanAttack = u => u.GetUnitType().CanAttack();
        public static readonly UnitFilter CanMove = u => u.GetUnitType().CanMove();
        public static readonly UnitFilter IsFlyer = u => u.GetUnitType().IsFlyer();
        public static readonly UnitFilter IsFlying = u => u.IsFlying();
        public static readonly UnitFilter RegeneratesHP = u => u.GetUnitType().RegeneratesHP();
        public static readonly UnitFilter IsSpellcaster = u => u.GetUnitType().IsSpellcaster();
        public static readonly UnitFilter HasPermanentCloak = u => u.GetUnitType().HasPermanentCloak();
        public static readonly UnitFilter IsOrganic = u => u.GetUnitType().IsOrganic();
        public static readonly UnitFilter IsMechanical = u => u.GetUnitType().IsMechanical();
        public static readonly UnitFilter IsRobotic = u => u.GetUnitType().IsRobotic();
        public static readonly UnitFilter IsDetector = u => u.GetUnitType().IsDetector();
        public static readonly UnitFilter IsResourceContainer = u => u.GetUnitType().IsResourceContainer();
        public static readonly UnitFilter IsResourceDepot = u => u.GetUnitType().IsResourceDepot();
        public static readonly UnitFilter IsRefinery = u => u.GetUnitType().IsRefinery();
        public static readonly UnitFilter IsWorker = u => u.GetUnitType().IsWorker();
        public static readonly UnitFilter RequiresPsi = u => u.GetUnitType().RequiresPsi();
        public static readonly UnitFilter RequiresCreep = u => u.GetUnitType().RequiresCreep();
        public static readonly UnitFilter IsBurrowable = u => u.GetUnitType().IsBurrowable();
        public static readonly UnitFilter IsCloakable = u => u.GetUnitType().IsCloakable();
        public static readonly UnitFilter IsBuilding = u => u.GetUnitType().IsBuilding();
        public static readonly UnitFilter IsAddon = u => u.GetUnitType().IsAddon();
        public static readonly UnitFilter IsFlyingBuilding = u => u.GetUnitType().IsFlyingBuilding();
        public static readonly UnitFilter IsNeutral = u => u.GetUnitType().IsNeutral();
        public static readonly UnitFilter IsHero = u => u.GetUnitType().IsHero();
        public static readonly UnitFilter IsPowerup = u => u.GetUnitType().IsPowerup();
        public static readonly UnitFilter IsBeacon = u => u.GetUnitType().IsBeacon();
        public static readonly UnitFilter IsFlagBeacon = u => u.GetUnitType().IsFlagBeacon();
        public static readonly UnitFilter IsSpecialBuilding = u => u.GetUnitType().IsSpecialBuilding();
        public static readonly UnitFilter IsSpell = u => u.GetUnitType().IsSpell();
        public static readonly UnitFilter ProducesLarva = u => u.GetUnitType().ProducesLarva();
        public static readonly UnitFilter IsMineralField = u => u.GetUnitType().IsMineralField();
        public static readonly UnitFilter IsCritter = u => u.GetUnitType().IsCritter();
        public static readonly UnitFilter CanBuildAddon = u => u.GetUnitType().CanBuildAddon();

        public static readonly UnitFilter Exists = u => u.Exists();
        public static readonly UnitFilter IsAttacking = u => u.IsAttacking();
        public static readonly UnitFilter IsBeingConstructed = u => u.IsBeingConstructed();
        public static readonly UnitFilter IsBeingGathered = u => u.IsBeingGathered();
        public static readonly UnitFilter IsBeingHealed = u => u.IsBeingHealed();
        public static readonly UnitFilter IsBlind = u => u.IsBlind();
        public static readonly UnitFilter IsBraking = u => u.IsBraking();
        public static readonly UnitFilter IsBurrowed = u => u.IsBurrowed();
        public static readonly UnitFilter IsCarryingGas = u => u.IsCarryingGas();
        public static readonly UnitFilter IsCarryingMinerals = u => u.IsCarryingMinerals();
        public static readonly UnitFilter IsCarryingSomething = u => u.IsCarryingMinerals() || u.IsCarryingGas();
        public static readonly UnitFilter IsCloaked = u => u.IsCloaked();
        public static readonly UnitFilter IsCompleted = u => u.IsCompleted();
        public static readonly UnitFilter IsConstructing = u => u.IsConstructing();
        public static readonly UnitFilter IsDefenseMatrixed = u => u.IsDefenseMatrixed();
        public static readonly UnitFilter IsDetected = u => u.IsDetected();
        public static readonly UnitFilter IsEnsnared = u => u.IsEnsnared();
        public static readonly UnitFilter IsFollowing = u => u.IsFollowing();
        public static readonly UnitFilter IsGatheringGas = u => u.IsGatheringGas();
        public static readonly UnitFilter IsGatheringMinerals = u => u.IsGatheringMinerals();
        public static readonly UnitFilter IsHallucination = u => u.IsHallucination();
        public static readonly UnitFilter IsHoldingPosition = u => u.IsHoldingPosition();
        public static readonly UnitFilter IsIdle = u => u.IsIdle();
        public static readonly UnitFilter IsInterruptible = u => u.IsInterruptible();
        public static readonly UnitFilter IsInvincible = u => u.IsInvincible();
        public static readonly UnitFilter IsIrradiated = u => u.IsIrradiated();
        public static readonly UnitFilter IsLifted = u => u.IsLifted();
        public static readonly UnitFilter IsLoaded = u => u.IsLoaded();
        public static readonly UnitFilter IsLockedDown = u => u.IsLockedDown();
        public static readonly UnitFilter IsMaelstrommed = u => u.IsMaelstrommed();
        public static readonly UnitFilter IsMorphing = u => u.IsMorphing();
        public static readonly UnitFilter IsMoving = u => u.IsMoving();
        public static readonly UnitFilter IsParasited = u => u.IsParasited();
        public static readonly UnitFilter IsPatrolling = u => u.IsPatrolling();
        public static readonly UnitFilter IsPlagued = u => u.IsPlagued();
        public static readonly UnitFilter IsRepairing = u => u.IsRepairing();
        public static readonly UnitFilter IsResearching = u => u.IsResearching();
        public static readonly UnitFilter IsSieged = u => u.IsSieged();
        public static readonly UnitFilter IsStartingAttack = u => u.IsStartingAttack();
        public static readonly UnitFilter IsStasised = u => u.IsStasised();
        public static readonly UnitFilter IsStimmed = u => u.IsStimmed();
        public static readonly UnitFilter IsStuck = u => u.IsStuck();
        public static readonly UnitFilter IsTraining = u => u.IsTraining();
        public static readonly UnitFilter IsUnderAttack = u => u.IsUnderAttack();
        public static readonly UnitFilter IsUnderDarkSwarm = u => u.IsUnderDarkSwarm();
        public static readonly UnitFilter IsUnderDisruptionWeb = u => u.IsUnderDisruptionWeb();
        public static readonly UnitFilter IsUnderStorm = u => u.IsUnderStorm();
        public static readonly UnitFilter IsPowered = u => u.IsPowered();
        public static readonly UnitFilter IsVisible = u => u.IsVisible();

        public static UnitFilter HP(Func<int, bool> c)
        {
            return u => c(u.GetHitPoints());
        }

        public static UnitFilter MaxHP(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().MaxHitPoints());
        }

        public static UnitFilter HP_Percent(Func<int, bool> c)
        {
            return u => c((u.GetUnitType().MaxHitPoints() != 0) ? ((u.GetHitPoints() * 100) / u.GetUnitType().MaxHitPoints()) : 0);
        }

        public static UnitFilter Shields(Func<int, bool> c)
        {
            return u => c(u.GetShields());
        }

        public static UnitFilter MaxShields(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().MaxShields());
        }

        public static UnitFilter Shields_Percent(Func<int, bool> c)
        {
            return u => c((u.GetUnitType().MaxShields() != 0) ? ((u.GetShields() * 100) / u.GetUnitType().MaxShields()) : 0);
        }

        public static UnitFilter Energy(Func<int, bool> c)
        {
            return u => c(u.GetEnergy());
        }

        public static UnitFilter MaxEnergy(Func<int, bool> c)
        {
            return u => c(u.GetPlayer().MaxEnergy(u.GetUnitType()));
        }

        public static UnitFilter Energy_Percent(Func<int, bool> c)
        {
            return u => c((u.GetPlayer().MaxEnergy(u.GetUnitType()) != 0) ? ((u.GetEnergy() * 100) / u.GetPlayer().MaxEnergy(u.GetUnitType())) : 0);
        }

        public static UnitFilter Armor(Func<int, bool> c)
        {
            return u => c(u.GetPlayer().Armor(u.GetUnitType()));
        }

        public static UnitFilter ArmorUpgrade(Func<UpgradeType, bool> c)
        {
            return u => c(u.GetUnitType().ArmorUpgrade());
        }

        public static UnitFilter MineralPrice(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().MineralPrice());
        }

        public static UnitFilter GasPrice(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().GasPrice());
        }

        public static UnitFilter BuildTime(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().BuildTime());
        }

        public static UnitFilter SupplyRequired(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().SupplyRequired());
        }

        public static UnitFilter SupplyProvided(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().SupplyProvided());
        }

        public static UnitFilter SpaceRequired(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().SpaceRequired());
        }

        public static UnitFilter SpaceRemaining(Func<int, bool> c)
        {
            return u => c(u.GetSpaceRemaining());
        }

        public static UnitFilter SpaceProvided(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().SpaceProvided());
        }

        public static UnitFilter BuildScore(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().BuildScore());
        }

        public static UnitFilter DestroyScore(Func<int, bool> c)
        {
            return u => c(u.GetUnitType().DestroyScore());
        }

        public static UnitFilter TopSpeed(Func<double, bool> c)
        {
            return u => c(u.GetPlayer().TopSpeed(u.GetUnitType()));
        }

        public static UnitFilter SightRange(Func<int, bool> c)
        {
            return u => c(u.GetPlayer().SightRange(u.GetUnitType()));
        }

        public static UnitFilter MaxWeaponCooldown(Func<int, bool> c)
        {
            return u => c(u.GetPlayer().WeaponDamageCooldown(u.GetUnitType()));
        }

        public static UnitFilter SizeType(Func<UnitSizeType, bool> c)
        {
            return u => c(u.GetUnitType().Size());
        }

        public static UnitFilter GroundWeapon(Func<WeaponType, bool> c)
        {
            return u => c(u.GetUnitType().GroundWeapon());
        }

        public static UnitFilter AirWeapon(Func<WeaponType, bool> c)
        {
            return u => c(u.GetUnitType().AirWeapon());
        }

        public static UnitFilter GetType(Func<UnitType, bool> c)
        {
            return u => c(u.GetUnitType());
        }

        public static UnitFilter GetRace(Func<Race, bool> c)
        {
            return u => c(u.GetUnitType().GetRace());
        }

        public static UnitFilter GetPlayer(Func<Player, bool> c)
        {
            return u => c(u.GetPlayer());
        }

        public static UnitFilter Resources(Func<int, bool> c)
        {
            return u => c(u.GetResources());
        }

        public static UnitFilter ResourceGroup(Func<int, bool> c)
        {
            return u => c(u.GetResourceGroup());
        }

        public static UnitFilter AcidSporeCount(Func<int, bool> c)
        {
            return u => c(u.GetAcidSporeCount());
        }

        public static UnitFilter InterceptorCount(Func<int, bool> c)
        {
            return u => c(u.GetInterceptorCount());
        }

        public static UnitFilter ScarabCount(Func<int, bool> c)
        {
            return u => c(u.GetScarabCount());
        }

        public static UnitFilter SpiderMineCount(Func<int, bool> c)
        {
            return u => c(u.GetSpiderMineCount());
        }

        public static UnitFilter WeaponCooldown(Func<int, bool> c)
        {
            return u => c(u.GetGroundWeaponCooldown());
        }

        public static UnitFilter SpellCooldown(Func<int, bool> c)
        {
            return u => c(u.GetSpellCooldown());
        }

        public static UnitFilter DefenseMatrixPoints(Func<int, bool> c)
        {
            return u => c(u.GetDefenseMatrixPoints());
        }

        public static UnitFilter DefenseMatrixTime(Func<int, bool> c)
        {
            return u => c(u.GetDefenseMatrixTimer());
        }

        public static UnitFilter EnsnareTime(Func<int, bool> c)
        {
            return u => c(u.GetEnsnareTimer());
        }

        public static UnitFilter IrradiateTime(Func<int, bool> c)
        {
            return u => c(u.GetIrradiateTimer());
        }

        public static UnitFilter LockdownTime(Func<int, bool> c)
        {
            return u => c(u.GetLockdownTimer());
        }

        public static UnitFilter MaelstromTime(Func<int, bool> c)
        {
            return u => c(u.GetMaelstromTimer());
        }

        public static UnitFilter OrderTime(Func<int, bool> c)
        {
            return u => c(u.GetOrderTimer());
        }

        public static UnitFilter PlagueTimer(Func<int, bool> c)
        {
            return u => c(u.GetPlagueTimer());
        }

        public static UnitFilter RemoveTime(Func<int, bool> c)
        {
            return u => c(u.GetRemoveTimer());
        }

        public static UnitFilter StasisTime(Func<int, bool> c)
        {
            return u => c(u.GetStasisTimer());
        }

        public static UnitFilter StimTime(Func<int, bool> c)
        {
            return u => c(u.GetStimTimer());
        }

        public static UnitFilter BuildType(Func<UnitType, bool> c)
        {
            return u => c(u.GetBuildType());
        }

        public static UnitFilter RemainingBuildTime(Func<int, bool> c)
        {
            return u => c(u.GetRemainingBuildTime());
        }

        public static UnitFilter RemainingTrainTime(Func<int, bool> c)
        {
            return u => c(u.GetRemainingTrainTime());
        }

        public static UnitFilter Target(Func<Unit, bool> c)
        {
            return u => c(u.GetTarget());
        }

        public static UnitFilter CurrentOrder(Func<Order, bool> c)
        {
            return u => c(u.GetOrder());
        }

        public static UnitFilter SecondaryOrder(Func<Order, bool> c)
        {
            return u => c(u.GetSecondaryOrder());
        }

        public static UnitFilter OrderTarget(Func<Unit, bool> c)
        {
            return u => c(u.GetOrderTarget());
        }

        public static UnitFilter GetLeft(Func<int, bool> c)
        {
            return u => c(u.GetLeft());
        }

        public static UnitFilter GetTop(Func<int, bool> c)
        {
            return u => c(u.GetTop());
        }

        public static UnitFilter GetRight(Func<int, bool> c)
        {
            return u => c(u.GetRight());
        }

        public static UnitFilter GetBottom(Func<int, bool> c)
        {
            return u => c(u.GetBottom());
        }
    }
}