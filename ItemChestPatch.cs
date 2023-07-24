namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Changes which bravestones you get in which stage.
    /// </summary>
    [HarmonyPatch(typeof(ItemChest), "Start")]
    class ItemChestPatch
    {
        static void Prefix(ItemChest __instance)
        {
            if ((__instance.contents == FPItemChestContent.POWERUP) && (FPStage.currentStage != null))
            {
                if ((__instance.powerupType == FPPowerup.NO_REVIVALS) && (FPStage.currentStage.stageID == 9))
                {
                    __instance.powerupType = (FPPowerup)PotionSellerUtils.GravityBootsIndex;
                    __instance.itemSprite = PotionSellerUtils.ItemSprites[PotionSellerUtils.GravityBootsIndex];
                }

                if ((__instance.powerupType == FPPowerup.EXPENSIVE_STOCKS) && (FPStage.currentStage.stageID == 10))
                {
                    __instance.powerupType = (FPPowerup)PotionSellerUtils.InvisibilityCloakIndex;
                    __instance.itemSprite = PotionSellerUtils.ItemSprites[PotionSellerUtils.InvisibilityCloakIndex];
                }
            }
        }
    }
}