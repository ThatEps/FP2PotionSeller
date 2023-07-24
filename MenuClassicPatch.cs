namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Expands the shop inventory and alters bravestone icons in classic mode.
    /// </summary>
    [HarmonyPatch(typeof(MenuClassic), "Start")]
    class MenuClassicPatch
    {
        static void Prefix(MenuClassic __instance)
        {
            PotionSellerUtils.ExpandClassicShopInventory(__instance);

            MenuClassicTile stage;
            for (int i = 0; i < __instance.stages.Length; i++)
            {
                stage = __instance.stages[i];

                if ((stage.hudChestItem == FPPowerup.NO_REVIVALS) && (stage.stageID == 9))
                    stage.hudChestItem = (FPPowerup)PotionSellerUtils.GravityBootsIndex;

                if ((stage.hudChestItem == FPPowerup.EXPENSIVE_STOCKS) && (stage.stageID == 10))
                    stage.hudChestItem = (FPPowerup)PotionSellerUtils.InvisibilityCloakIndex;
            }
        }
    }
}