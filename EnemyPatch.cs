namespace PotionSeller
{
    using HarmonyLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Bundles all of the patches to FPBaseEnemy.
    /// </summary>
    class EnemyPatch
    {
        /// <summary>
        /// Applies all changes to FPBaseEnemy.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(EnemyPatch_Start));
        }

        /// <summary>
        /// Enumerates the classes of all regular enemies.
        /// </summary>
        public static List<Type> EnemyClasses
        {
            get
            {
                return new List<Type>
                {
                    typeof(AquaTrooper),
                    typeof(Beartle),
                    typeof(BlastCone),
                    typeof(Bonecrawler),
                    typeof(Bonespitter),
                    typeof(BoomBeth),
                    typeof(Bubblorbiter),
                    typeof(Burro),
                    typeof(Cocoon),
                    typeof(Crowitzer),
                    typeof(Crustaceon),
                    typeof(DartHog),
                    typeof(DinoWalker),
                    typeof(Discord),
                    typeof(DropletShip),
                    typeof(Durugin),
                    typeof(DrakeCoccoon),
                    typeof(DrakeFly),
                    typeof(FireHopper),
                    typeof(Flamingo),
                    typeof(FlashMouth),
                    typeof(FlyingSaucer),
                    typeof(FoldingSnake),
                    typeof(FPSchmupEnemy),
                    typeof(Hellpo),
                    typeof(HotPlate),
                    typeof(GatHog),
                    typeof(Girder),
                    typeof(KoiCannon),
                    typeof(Herald),
                    typeof(HijackedPoliceCar),
                    typeof(Iris),
                    typeof(Jawdrop),
                    typeof(Kakugan),
                    typeof(Keon),
                    typeof(LineCutter),
                    typeof(Macer),
                    typeof(Manpowa),
                    typeof(Mantis),
                    typeof(MeteorRoller),
                    typeof(MonsterCube),
                    typeof(Peller),
                    typeof(Pendurum),
                    typeof(PogoSnail),
                    typeof(Prawn),
                    typeof(ProtoPincer),
                    typeof(RailTurretus),
                    typeof(Raytracker),
                    typeof(RifleTrooper),
                    typeof(Rosebud),
                    typeof(SawShrimp),
                    typeof(Sentinel),
                    typeof(ShellGrowth),
                    typeof(Shockula),
                    typeof(Softballer),
                    typeof(SpyTurretus),
                    typeof(Stahp),
                    typeof(SwordTrooper),
                    typeof(TombstoneTurretus),
                    typeof(Torcher),
                    typeof(TowerCannon),
                    typeof(ToyDecoy),
                    typeof(Traumagotcha),
                    typeof(TriggerJoy),
                    typeof(Troopish),
                    typeof(Turretus),
                    typeof(WaterHopper),
                    typeof(WeatherFace),
                    typeof(WoodHopper),
                    typeof(ZombieTrooper)
                };
            }
        }
    }

    /// <summary>
    /// If the player has the Bomb Magnet equipped, a random number is rolled with a 35% chance to rig this enemy with a bomb.
    /// </summary>
    [HarmonyPatch]
    class EnemyPatch_Start
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return EnemyPatch.EnemyClasses.Select(enemyClass => (MethodBase)enemyClass.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance));
        }

        static void Postfix(FPBaseEnemy __instance)
        {
            FPPlayer player = FPStage.currentStage?.GetPlayerInstance_FPPlayer();

            if ((player != null) && player.IsPowerupActive((FPPowerup)PotionSellerUtils.BombMagnetIndex) && (UnityEngine.Random.Range(0, 99) < 35))
                PotionSellerUtils.RigBombOnEnemy(__instance);
        }
    }
}