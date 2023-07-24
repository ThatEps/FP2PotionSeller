namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Alters bravestone icons on the world map.
    /// </summary>
    [HarmonyPatch(typeof(MenuWorldMap), "Start")]
    class MenuWorldMapPatch
    {
        static void Prefix(MenuWorldMap __instance)
        {
            FPMapLocation[] locations;
            FPMapPointer pointers;

            for (int i = 0; i < __instance.mapScreens.Length; i++)
            {
                locations = __instance.mapScreens[i].map.locations;
                for (int j = 0; j < locations.Length; j++)
                {
                    pointers = locations[j].pointers;

                    if ((pointers.hudChestItem == FPPowerup.NO_REVIVALS) && (pointers.stageID == 9))
                        pointers.hudChestItem = (FPPowerup)PotionSellerUtils.GravityBootsIndex;

                    if ((pointers.hudChestItem == FPPowerup.EXPENSIVE_STOCKS) && (pointers.stageID == 10))
                        pointers.hudChestItem = (FPPowerup)PotionSellerUtils.InvisibilityCloakIndex;
                }
            }
        }
    }
}