namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Increases the distance and duration of Shadow Guard.
    /// </summary>
    [HarmonyPatch(typeof(PlayerShadow), "Start")]
    class PlayerShadowPatch
    {
        static void Postfix(PlayerShadow __instance)
        {
            __instance.distance = 128f;
            __instance.spawnTime = 150f;
        }
    }
}
