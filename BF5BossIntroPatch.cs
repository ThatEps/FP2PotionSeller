namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Adds exra item code at the end of the State_1 method.
    /// </summary>
    [HarmonyPatch(typeof(BF5BossIntro), "State_1")]
    class BF5BossIntroPatch
    {
        static void Postfix(BF5BossIntro __instance, ref int ___frameCount)
        {
            if (FPStage.objectsRegistered &&
                FPStage.currentStage.finalBossStage &&
                FPStage.onFinalBoss &&
                (___frameCount <= 0) &&
                (__instance.targetPlayer != null) &&
                (__instance.targetPlayer.powerups.Length > 0))
            {
                for (int i = 0; i < __instance.targetPlayer.powerups.Length; i++)
                    if (__instance.targetPlayer.powerups[i] == (FPPowerup)PotionSellerUtils.PhoenixTonicIndex)
                    {
                        if (!__instance.targetPlayer.IsPowerupActive(FPPowerup.STOCK_DRAIN))
                            __instance.targetPlayer.lives++;
                        continue;
                    }
            }
        }
    }
}
