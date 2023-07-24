namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Bundles all of the patches to GOAuditoriumCutscene.
    /// </summary>
    class GOAuditoriumCutscenePatch
    {
        /// <summary>
        /// Applies all changes to GOAuditoriumCutscene.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(GOAuditoriumCutscenePatch_State_FadeIn));
            harmony.PatchAll(typeof(GOAuditoriumCutscenePatch_State_LightStop));
            harmony.PatchAll(typeof(GOAuditoriumCutscenePatch_SkipScene));
        }
    }

    /// <summary>
    /// Sets the PotionSellerUtils.InSpecialCutscene value and resets game speed when the cutscene starts.
    /// </summary>
    [HarmonyPatch(typeof(GOAuditoriumCutscene), "State_FadeIn")]
    class GOAuditoriumCutscenePatch_State_FadeIn
    {
        static void Postfix()
        {
            PotionSellerUtils.InSpecialCutscene = true;
            FPStage.SetGameSpeed(1f);
        }
    }

    /// <summary>
    /// Resets the PotionSellerUtils.InSpecialCutscene value and game speed when the cutscene ends.
    /// </summary>
    [HarmonyPatch(typeof(GOAuditoriumCutscene), "State_LightStop")]
    class GOAuditoriumCutscenePatch_State_LightStop
    {
        static void Postfix()
        {
            if (PotionSellerUtils.InSpecialCutscene)
            {
                PotionSellerUtils.InSpecialCutscene = false;
                FPStage.SetGameSpeed(1f);
            }
        }
    }

    /// <summary>
    /// Resets the PotionSellerUtils.InSpecialCutscene value and game speed when the cutscene is skipped.
    /// </summary>
    [HarmonyPatch(typeof(GOAuditoriumCutscene), nameof(GOAuditoriumCutscene.SkipScene))]
    class GOAuditoriumCutscenePatch_SkipScene
    {
        static void Postfix()
        {
            if (PotionSellerUtils.InSpecialCutscene)
            {
                PotionSellerUtils.InSpecialCutscene = false;
                FPStage.SetGameSpeed(1f);
            }
        }
    }
}
