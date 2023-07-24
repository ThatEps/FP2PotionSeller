namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Bundles all of the patches to BSKalawCutscene.
    /// </summary>
    class BSKalawCutscenePatch
    {
        /// <summary>
        /// Applies all changes to BSKalawCutscene.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(BSKalawCutscenePatch_State_FadeIn));
            harmony.PatchAll(typeof(BSKalawCutscenePatch_State_Landing));
        }
    }

    /// <summary>
    /// Sets the PotionSellerUtils.InSpecialCutscene value and resets game speed when the cutscene starts.
    /// </summary>
    [HarmonyPatch(typeof(BSKalawCutscene), "State_FadeIn")]
    class BSKalawCutscenePatch_State_FadeIn
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
    [HarmonyPatch(typeof(BSKalawCutscene), "State_Landing")]
    class BSKalawCutscenePatch_State_Landing
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