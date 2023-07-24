namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Bundles all of the patches to FPStage.
    /// </summary>
    class FPStagePatch
    {
        /// <summary>
        /// Applies all changes to FPStage.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(FPStagePatch_Update));
            harmony.PatchAll(typeof(FPStagePatch_SetGameSpeed));
            harmony.PatchAll(typeof(FPStagePatch_ClearPlayerInstanceReferences));
            harmony.PatchAll(typeof(FPStagePatch_CreateStageObject));
        }
    }

    /// <summary>
    /// Resets InStage and InShmup on each frame to issue a challenge to FPHudMaster and FPSchmupManager to set them back.
    /// This is done so that it reverts automatically once a scene without FPHudMaster is loaded.
    /// Also applies the Bomb Magnet effect.
    /// </summary>
    [HarmonyPatch(typeof(FPStage), "Update")]
    class FPStagePatch_Update
    {
        static void Postfix(FPStage __instance)
        {
            PotionSellerUtils.ApplyBombMagnetEffect(__instance.GetPlayerInstance_FPPlayer());
            PotionSellerUtils.InStage = false;
            PotionSellerUtils.InShmup = false;
        }
    }

    /// <summary>
    /// Alters the parameter passed to SetGameSpeed based on the player's loadout if they are in a stage and a cutscene isn't playing.
    /// </summary>
    [HarmonyPatch(typeof(FPStage), nameof(FPStage.SetGameSpeed))]
    class FPStagePatch_SetGameSpeed
    {
        static void Prefix(ref float speed)
        {
            if (PotionSellerUtils.InStage && !PotionSellerUtils.InResultsScreen && !FPStage.eventIsActive && !PotionSellerUtils.InSpecialCutscene && (FPStage.currentStage != null))
            {
                FPPlayer player = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                if (player != null)
                    speed *= PotionSellerUtils.GetStageSpeedMultiplier(player);
            }
        }
    }

    /// <summary>
    /// Performs PotionSellerUtils.ClearPlayerData whenever FPStage.ClearPlayerInstanceReferences is called.
    /// </summary>
    [HarmonyPatch(typeof(FPStage), "ClearPlayerInstanceReferences")]
    class FPStagePatch_ClearPlayerInstanceReferences
    {
        static void Postfix()
        {
            PotionSellerUtils.ClearCharacterData();
        }
    }

    /// <summary>
    /// Runs CreateStageObject a second time if the object being spawned is a Core and Tinker Glove conditions are met.
    /// </summary>
    [HarmonyPatch(typeof(FPStage), nameof(FPStage.CreateStageObject))]
    class FPStagePatch_CreateStageObject
    {
        /// <summary>
        /// Flag that prevents the extra call to CreateStageObject from itself triggering the TinkerGlove postfix.
        /// </summary>
        public static bool RunningDuplication = false;

        static void Postfix(FPBaseObject __result, int classID, float xpos, float ypos)
        {
            if (!RunningDuplication && (classID == ItemCore.classID) && (__result != null))
            {
                FPPlayer player = FPStage.currentStage.GetPlayerInstance_FPPlayer();

                if ((player != null) && player.IsPowerupActive((FPPowerup)PotionSellerUtils.TinkerGloveIndex))
                    if (++PotionSellerUtils.TinkerGloveTicks >= 2)
                    {
                        PotionSellerUtils.TinkerGloveTicks = 0;

                        RunningDuplication = true;
                        ItemCore core = (ItemCore)FPStage.CreateStageObject(ItemCore.classID, xpos + 0.05f, ypos);
                        core.state = core.State_Released;
                        core.velocity.x = __result.velocity.x + UnityEngine.Random.Range(-0.2f, 0.2f);
                        core.velocity.y = __result.velocity.y + UnityEngine.Random.Range(-0.2f, 0.2f);
                        RunningDuplication = false;
                    };
            }
        }
    }
}
