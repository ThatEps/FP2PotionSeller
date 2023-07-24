namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Removes the checkpoint from the Magic Compass' list when it's activated.
    /// </summary>
    [HarmonyPatch(typeof(FPCheckpoint), nameof(FPCheckpoint.Set_Checkpoint))]
    class FPCheckpointPatch
    {
        static void Prefix(FPCheckpoint __instance)
        {
            if (PotionSellerUtils.MagicCompassCheckpoints != null)
                PotionSellerUtils.MagicCompassCheckpoints.Remove(__instance);
        }
    }
}
