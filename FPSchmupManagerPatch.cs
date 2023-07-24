namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Sets InShmup to <c>true</c> on each frame.
    /// </summary>
    [HarmonyPatch(typeof(FPSchmupManager), "Update")]
    class FPSchmupManagerPatch
    {
        static void Postfix()
        {
            PotionSellerUtils.InShmup = true;
        }
    }
}
