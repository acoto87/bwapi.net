namespace BWAPI.NET
{
    public static class WeaponTypeHelper
    {
        private const int TARG_AIR = 0x01;
        private const int TARG_GROUND = 0x02;
        private const int TARG_MECH = 0x04;
        private const int TARG_ORGANIC = 0x08;
        private const int TARG_NOBUILD = 0x10;
        private const int TARG_NOROBOT = 0x20;
        private const int TARG_TERRAIN = 0x40;
        private const int TARG_ORGMECH = 0x80;
        private const int TARG_OWN = 0x100;

        private static readonly int[] _defaultWpnDamageAmt = { 6, 18, 10, 30, 20, 30, 125, 12, 10, 24, 20, 30, 70, 5, 0, 20, 8, 40, 16, 25, 25, 50, 50, 30, 30, 8, 16, 70, 150, 20, 260, 600, 0, 0, 250, 5, 10, 50, 10, 20, 20, 50, 4, 5, 0, 30, 20, 40, 9, 18, 5, 10, 15, 40, 500, 110, 0, 0, 0, 0, 300, 0, 5, 0, 8, 20, 20, 45, 5, 20, 30, 60, 4, 8, 14, 20, 28, 10, 20, 6, 20, 20, 100, 0, 14, 100, 45, 7, 7, 7, 7, 7, 7, 7, 4, 30, 10, 10, 8, 10, 5, 0, 20, 6, 25, 8, 8, 8, 0, 20, 6, 40, 25, 25, 20, 28, 30, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 0, 0 };
        private static readonly int[] _defaultWpnDamageBonus = { 1, 1, 1, 1, 2, 2, 0, 1, 2, 1, 1, 3, 3, 1, 0, 2, 1, 2, 1, 3, 3, 3, 3, 3, 3, 1, 1, 5, 5, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 3, 3, 1, 0, 0, 1, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 3, 3, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 25, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 2, 1, 1, 1, 1, 2, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 };
        private static readonly int[] _wpnDamageCooldowns = { 15, 15, 22, 22, 30, 22, 22, 22, 22, 22, 22, 37, 37, 15, 15, 22, 30, 22, 30, 30, 30, 30, 30, 22, 22, 22, 22, 75, 75, 15, 15, 1, 1, 1, 75, 8, 8, 15, 15, 15, 15, 15, 15, 22, 22, 22, 30, 30, 30, 30, 22, 22, 15, 32, 1, 1, 1, 1, 1, 1, 1, 1, 22, 22, 22, 22, 30, 22, 30, 22, 20, 20, 22, 30, 22, 30, 22, 45, 45, 1, 22, 22, 1, 1, 45, 22, 30, 22, 22, 22, 22, 22, 22, 22, 9, 22, 22, 22, 22, 22, 8, 22, 22, 64, 100, 22, 22, 22, 1, 37, 15, 30, 22, 22, 30, 22, 22, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 0, 0 };
        private static readonly int[] _wpnDamageFactor = { 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 };
        private static readonly int[] _wpnMinRange = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _wpnMaxRange = { 128, 160, 224, 192, 160, 160, 10, 192, 160, 160, 160, 224, 224, 10, 10, 160, 160, 160, 160, 192, 192, 192, 192, 192, 192, 32, 32, 384, 384, 224, 320, 3, 256, 256, 288, 15, 15, 15, 128, 160, 25, 25, 2, 32, 128, 64, 256, 256, 96, 96, 128, 128, 224, 224, 3, 3, 384, 288, 288, 288, 288, 16, 32, 10, 15, 15, 128, 128, 96, 96, 64, 64, 32, 128, 128, 128, 128, 160, 160, 128, 224, 224, 128, 288, 288, 15, 15, 128, 128, 32, 128, 224, 224, 128, 160, 192, 160, 160, 64, 160, 160, 288, 192, 192, 192, 256, 320, 288, 320, 192, 128, 15, 192, 192, 128, 128, 192, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 0, 0 };
        private static readonly int[] _wpnSplashRangeInner = { 0, 0, 0, 0, 0, 0, 50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 15, 10, 10, 0, 0, 128, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 0, 5, 0, 0, 5, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _wpnSplashRangeMid = { 0, 0, 0, 0, 0, 0, 75, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 20, 25, 25, 0, 0, 192, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 20, 0, 0, 40, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 40, 0, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 50, 0, 0, 50, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _wpnSplashRangeOuter = { 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 25, 25, 40, 40, 0, 0, 256, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30, 30, 0, 0, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30, 30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 60, 0, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 25, 0, 100, 0, 0, 100, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _wpnFlags = { TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND | TARG_NOBUILD, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND | TARG_MECH | TARG_NOBUILD, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_AIR | TARG_GROUND | TARG_NOBUILD, TARG_GROUND | TARG_NOBUILD | TARG_NOROBOT | TARG_ORGMECH, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_AIR | TARG_GROUND | TARG_ORGANIC | TARG_NOBUILD | TARG_OWN, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_AIR | TARG_GROUND | TARG_NOBUILD | TARG_TERRAIN, TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_AIR, TARG_AIR, TARG_GROUND, TARG_AIR, TARG_AIR, TARG_AIR, TARG_AIR, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_GROUND, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_AIR, TARG_AIR, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_GROUND, TARG_AIR | TARG_GROUND | TARG_TERRAIN, TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_GROUND, TARG_AIR, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, TARG_AIR | TARG_GROUND, 0, 0 };
        private static readonly UpgradeType[] _upgrade = { NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Zerg_Melee_Attacks, NET.UpgradeType.Zerg_Melee_Attacks, NET.UpgradeType.Zerg_Melee_Attacks, NET.UpgradeType.Zerg_Missile_Attacks, NET.UpgradeType.Zerg_Missile_Attacks, NET.UpgradeType.Zerg_Melee_Attacks, NET.UpgradeType.Zerg_Melee_Attacks, NET.UpgradeType.Zerg_Melee_Attacks, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Zerg_Flyer_Attacks, NET.UpgradeType.Zerg_Flyer_Attacks, NET.UpgradeType.Zerg_Flyer_Attacks, NET.UpgradeType.Zerg_Flyer_Attacks, NET.UpgradeType.Zerg_Flyer_Attacks, NET.UpgradeType.Zerg_Missile_Attacks, NET.UpgradeType.Zerg_Missile_Attacks, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Scarab_Damage, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Vehicle_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Terran_Ship_Weapons, NET.UpgradeType.Zerg_Flyer_Attacks, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Upgrade_60, NET.UpgradeType.Zerg_Missile_Attacks, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Protoss_Ground_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Protoss_Air_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.Terran_Infantry_Weapons, NET.UpgradeType.None, NET.UpgradeType.Unknown };
        private static readonly DamageType[] _damageType = { NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Concussive, NET.DamageType.Concussive, NET.DamageType.Concussive, NET.DamageType.Concussive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Concussive, NET.DamageType.Concussive, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Concussive, NET.DamageType.Concussive, NET.DamageType.Ignore_Armor, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Concussive, NET.DamageType.Concussive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Independent, NET.DamageType.Independent, NET.DamageType.Independent, NET.DamageType.Independent, NET.DamageType.Independent, NET.DamageType.Independent, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Independent, NET.DamageType.Ignore_Armor, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Concussive, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Ignore_Armor, NET.DamageType.Ignore_Armor, NET.DamageType.Explosive, NET.DamageType.Explosive, NET.DamageType.Normal, NET.DamageType.Ignore_Armor, NET.DamageType.Independent, NET.DamageType.Independent, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Concussive, NET.DamageType.Concussive, NET.DamageType.Normal, NET.DamageType.Explosive, NET.DamageType.Concussive, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.Normal, NET.DamageType.None, NET.DamageType.Unknown };
        private static readonly ExplosionType[] _explosionType = { ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Radial_Splash, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.None, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Enemy_Splash, ExplosionType.Enemy_Splash, ExplosionType.Radial_Splash, ExplosionType.Radial_Splash, ExplosionType.Normal, ExplosionType.Yamato_Gun, ExplosionType.Nuclear_Missile, ExplosionType.Lockdown, ExplosionType.EMP_Shockwave, ExplosionType.Irradiate, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Radial_Splash, ExplosionType.Radial_Splash, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Radial_Splash, ExplosionType.Normal, ExplosionType.Parasite, ExplosionType.Broodlings, ExplosionType.Ensnare, ExplosionType.Dark_Swarm, ExplosionType.Plague, ExplosionType.Consume, ExplosionType.Normal, ExplosionType.None, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Enemy_Splash, ExplosionType.Enemy_Splash, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Enemy_Splash, ExplosionType.Stasis_Field, ExplosionType.Radial_Splash, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Enemy_Splash, ExplosionType.Normal, ExplosionType.Air_Splash, ExplosionType.Disruption_Web, ExplosionType.Restoration, ExplosionType.Air_Splash, ExplosionType.Corrosive_Acid, ExplosionType.Mind_Control, ExplosionType.Feedback, ExplosionType.Optical_Flare, ExplosionType.Maelstrom, ExplosionType.Enemy_Splash, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.Normal, ExplosionType.None, ExplosionType.Unknown };
        private static readonly UnitType[] _whatUses = { UnitType.Terran_Marine, UnitType.Hero_Jim_Raynor_Marine, UnitType.Terran_Ghost, UnitType.Hero_Sarah_Kerrigan, UnitType.Terran_Vulture, UnitType.Hero_Jim_Raynor_Vulture, UnitType.Terran_Vulture_Spider_Mine, UnitType.Terran_Goliath, UnitType.Terran_Goliath, UnitType.Hero_Alan_Schezar, UnitType.Hero_Alan_Schezar, UnitType.Terran_Siege_Tank_Tank_Mode, UnitType.Hero_Edmund_Duke_Tank_Mode, UnitType.Terran_SCV, UnitType.Terran_SCV, UnitType.Terran_Wraith, UnitType.Terran_Wraith, UnitType.Hero_Tom_Kazansky, UnitType.Hero_Tom_Kazansky, UnitType.Terran_Battlecruiser, UnitType.Terran_Battlecruiser, UnitType.Hero_Norad_II, UnitType.Hero_Norad_II, UnitType.Hero_Hyperion, UnitType.Hero_Hyperion, UnitType.Terran_Firebat, UnitType.Hero_Gui_Montag, UnitType.Terran_Siege_Tank_Siege_Mode, UnitType.Hero_Edmund_Duke_Siege_Mode, UnitType.Terran_Missile_Turret, UnitType.Terran_Battlecruiser, UnitType.Terran_Ghost, UnitType.Terran_Ghost, UnitType.Terran_Science_Vessel, UnitType.Terran_Science_Vessel, UnitType.Zerg_Zergling, UnitType.Hero_Devouring_One, UnitType.Hero_Infested_Kerrigan, UnitType.Zerg_Hydralisk, UnitType.Hero_Hunter_Killer, UnitType.Zerg_Ultralisk, UnitType.Hero_Torrasque, UnitType.Zerg_Broodling, UnitType.Zerg_Drone, UnitType.Zerg_Drone, UnitType.None, UnitType.Zerg_Guardian, UnitType.Hero_Kukulza_Guardian, UnitType.Zerg_Mutalisk, UnitType.Hero_Kukulza_Mutalisk, UnitType.None, UnitType.None, UnitType.Zerg_Spore_Colony, UnitType.Zerg_Sunken_Colony, UnitType.Zerg_Infested_Terran, UnitType.Zerg_Scourge, UnitType.Zerg_Queen, UnitType.Zerg_Queen, UnitType.Zerg_Queen, UnitType.Zerg_Defiler, UnitType.Zerg_Defiler, UnitType.Zerg_Defiler, UnitType.Protoss_Probe, UnitType.Protoss_Probe, UnitType.Protoss_Zealot, UnitType.Hero_Fenix_Zealot, UnitType.Protoss_Dragoon, UnitType.Hero_Fenix_Dragoon, UnitType.None, UnitType.Hero_Tassadar, UnitType.Protoss_Archon, UnitType.Hero_Tassadar_Zeratul_Archon, UnitType.None, UnitType.Protoss_Scout, UnitType.Protoss_Scout, UnitType.Hero_Mojo, UnitType.Hero_Mojo, UnitType.Protoss_Arbiter, UnitType.Hero_Danimoth, UnitType.Protoss_Interceptor, UnitType.Protoss_Photon_Cannon, UnitType.Protoss_Photon_Cannon, UnitType.Protoss_Scarab, UnitType.Protoss_Arbiter, UnitType.Protoss_High_Templar, UnitType.Hero_Zeratul, UnitType.Hero_Dark_Templar, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.Special_Independant_Starport, UnitType.None, UnitType.None, UnitType.Special_Floor_Gun_Trap, UnitType.Special_Wall_Missile_Trap, UnitType.Special_Wall_Flame_Trap, UnitType.Special_Floor_Missile_Trap, UnitType.Protoss_Corsair, UnitType.Protoss_Corsair, UnitType.Terran_Medic, UnitType.Terran_Valkyrie, UnitType.Zerg_Devourer, UnitType.Protoss_Dark_Archon, UnitType.Protoss_Dark_Archon, UnitType.Terran_Medic, UnitType.Protoss_Dark_Archon, UnitType.Zerg_Lurker, UnitType.None, UnitType.Protoss_Dark_Templar, UnitType.Hero_Samir_Duran, UnitType.Hero_Infested_Duran, UnitType.Hero_Artanis, UnitType.Hero_Artanis, UnitType.Hero_Alexei_Stukov, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.Unknown };
        private static readonly TechType[] _attachedTech = { TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.Spider_Mines, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.Yamato_Gun, TechType.Nuclear_Strike, TechType.Lockdown, TechType.EMP_Shockwave, TechType.Irradiate, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.Parasite, TechType.Spawn_Broodlings, TechType.Ensnare, TechType.Dark_Swarm, TechType.Plague, TechType.Consume, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.Stasis_Field, TechType.Psionic_Storm, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.Disruption_Web, TechType.Restoration, TechType.None, TechType.None, TechType.Mind_Control, TechType.Feedback, TechType.Optical_Flare, TechType.Maelstrom, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None, TechType.None };

        /**
        * Retrieves the technology type that must be researched before this weapon can
        * be used.
        *
        * @return {@link TechType} required by this weapon.
        * Returns {@link TechType#None} if no tech type is required to use this weapon.
        * @see TechType#getWeapon
        */
        public static TechType GetTech(this WeaponType weaponType)
        {
            return _attachedTech[(int)weaponType];
        }

        /**
        * Retrieves the unit type that is intended to use this weapon type.
        * <p>
        * There is a rare case where some hero unit types use the same weapon.
        *
        * @return The {@link UnitType} that uses this weapon.
        * @see UnitType#groundWeapon
        * @see UnitType#airWeapon
        */
        public static UnitType WhatUses(this WeaponType weaponType)
        {
            return _whatUses[(int)weaponType];
        }

        /**
        * Retrieves the base amount of damage that this weapon can deal per attack.
        * <p>
        * That this damage amount must go through a {@link DamageType} and {@link UnitSizeType} filter
        * before it is applied to a unit.
        *
        * @return Amount of base damage that this weapon deals.
        */
        public static int DamageAmount(this WeaponType weaponType)
        {
            return _defaultWpnDamageAmt[(int)weaponType];
        }

        /**
        * Determines the bonus amount of damage that this weapon type increases by for every
        * upgrade to this type.
        *
        * @return Amount of damage added for every weapon upgrade.
        * @see #upgradeType
        */
        public static int DamageBonus(this WeaponType weaponType)
        {
            return _defaultWpnDamageBonus[(int)weaponType];
        }

        /**
        * Retrieves the base amount of cooldown time between each attack, in frames.
        *
        * @return The amount of base cooldown applied to the unit after an attack.
        * @see Unit#getGroundWeaponCooldown
        * @see Unit#getAirWeaponCooldown
        */
        public static int DamageCooldown(this WeaponType weaponType)
        {
            return _wpnDamageCooldowns[(int)weaponType];
        }

        /**
        * Obtains the intended number of missiles/attacks that are used.
        * This is used to multiply with the damage amount to obtain the full amount of damage
        * for an attack.
        *
        * @return The damage factor multiplied by the amount to obtain the total damage.
        * @see #damageAmount
        */
        public static int DamageFactor(this WeaponType weaponType)
        {
            return _wpnDamageFactor[(int)weaponType];
        }

        /**
        * Retrieves the upgrade type that increases this weapon's damage output.
        *
        * @return The {@link UpgradeType} used to upgrade this weapon's damage.
        * @see #damageBonus
        */
        public static UpgradeType UpgradeType(this WeaponType weaponType)
        {
            return _upgrade[(int)weaponType];
        }

        /**
        * Retrieves the damage type that this weapon applies to a unit type.
        *
        * @return {@link DamageType} used for damage calculation.
        * @see DamageType
        * @see UnitSizeType
        */
        public static DamageType DamageType(this WeaponType weaponType)
        {
            return _damageType[(int)weaponType];
        }

        /**
        * Retrieves the explosion type that indicates how the weapon deals damage.
        *
        * @return ExplosionType identifying how damage is applied to a target location.
        */
        public static ExplosionType ExplosionType_(this WeaponType weaponType)
        {
            return _explosionType[(int)weaponType];
        }

        /**
        * Retrieves the minimum attack range of the weapon, measured in pixels.
        * This value is 0 for almost all weapon types, except for {@link WeaponType#Arclite_Shock_Cannon}
        * and {@link WeaponType#Arclite_Shock_Cannon_Edmund_Duke}.
        *
        * @return Minimum attack range, in pixels.
        */
        public static int MinRange(this WeaponType weaponType)
        {
            return _wpnMinRange[(int)weaponType];
        }

        /**
        * Retrieves the maximum attack range of the weapon, measured in pixels.
        *
        * @return Maximum attack range, in pixels.
        */
        public static int MaxRange(this WeaponType weaponType)
        {
            return _wpnMaxRange[(int)weaponType];
        }

        /**
        * Retrieves the inner radius used for splash damage calculations, in pixels.
        *
        * @return Radius of the inner splash area, in pixels.
        */
        public static int InnerSplashRadius(this WeaponType weaponType)
        {
            return _wpnSplashRangeInner[(int)weaponType];
        }

        /**
        * Retrieves the middle radius used for splash damage calculations, in pixels.
        *
        * @return Radius of the middle splash area, in pixels.
        */
        public static int MedianSplashRadius(this WeaponType weaponType)
        {
            return _wpnSplashRangeMid[(int)weaponType];
        }

        /**
        * Retrieves the outer radius used for splash damage calculations, in pixels.
        *
        * @return Radius of the outer splash area, in pixels.
        */
        public static int OuterSplashRadius(this WeaponType weaponType)
        {
            return _wpnSplashRangeOuter[(int)weaponType];
        }

        /**
        * Checks if this weapon type can target air units.
        *
        * @return true if this weapon type can target air units, and false otherwise.
        * @see Unit#isFlying
        * @see UnitType#isFlyer
        */
        public static bool TargetsAir(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_AIR) != 0;
        }

        /**
        * Checks if this weapon type can target ground units.
        *
        * @return true if this weapon type can target ground units, and false otherwise.
        * @see Unit#isFlying
        * @see UnitType#isFlyer
        */
        public static bool TargetsGround(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_GROUND) != 0;
        }

        /**
        * Checks if this weapon type can only target mechanical units.
        *
        * @return true if this weapon type can only target mechanical units, and false otherwise.
        * @see #targetsOrgOrMech
        * @see UnitType#isMechanical
        */
        public static bool TargetsMechanical(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_MECH) != 0;
        }

        /**
        * Checks if this weapon type can only target organic units.
        *
        * @return true if this weapon type can only target organic units, and false otherwise.
        * @see #targetsOrgOrMech
        * @see UnitType#isOrganic
        */

        public static bool TargetsOrganic(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_ORGANIC) != 0;
        }

        /**
        * Checks if this weapon type cannot target structures.
        *
        * @return true if this weapon type cannot target buildings, and false if it can.
        * @see UnitType#isBuilding
        */
        public static bool TargetsNonBuilding(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_NOBUILD) != 0;
        }

        /**
        * Checks if this weapon type cannot target robotic units.
        *
        * @return true if this weapon type cannot target robotic units, and false if it can.
        * @see UnitType#isRobotic
        */
        public static bool TargetsNonRobotic(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_NOROBOT) != 0;
        }

        /**
        * Checks if this weapon type can target the ground.
        * <p>
        * This is more for attacks like @Psi_Storm which can target a location, not to be
        * confused with attack move.
        *
        * @return true if this weapon type can target a location, and false otherwise.
        */
        public static bool TargetsTerrain(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_TERRAIN) != 0;
        }

        /**
        * Checks if this weapon type can only target organic or mechanical units.
        *
        * @return true if this weapon type can only target organic or mechanical units, and false otherwise.
        * @see #targetsOrganic
        * @see #targetsMechanical
        * @see UnitType#isOrganic
        * @see UnitType#isMechanical
        */
        public static bool TargetsOrgOrMech(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_ORGMECH) != 0;
        }

        /**
        * Checks if this weapon type can only target units owned by the same player.
        * This is used for {@link WeaponType#Consume}.
        *
        * @return true if this weapon type can only target your own units, and false otherwise.
        * @see Unit#getPlayer
        */
        public static bool TargetsOwn(this WeaponType weaponType)
        {
            return (_wpnFlags[(int)weaponType] & TARG_OWN) != 0;
        }
    }
}