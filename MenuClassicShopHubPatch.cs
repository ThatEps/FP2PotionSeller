namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Expands the shop inventories of a number of NPC's in quick-shops on the world map.
    /// </summary>
    [HarmonyPatch(typeof(MenuClassicShopHub), "Start")]
    class MenuClassicShopHubPatch
    {
        static bool Prefix(MenuClassicShopHub __instance)
        {
            if ((__instance.shopkeepers != null) && (__instance.shopkeepers.Length > 1))
                for (int x = 0; x < __instance.shopkeepers.Length - 1; x++)
                    PotionSellerUtils.ExpandNPCShopInventory(__instance.shopkeepers[x]);
            
            return true;
        }
    }
}
