namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Expands the shop inventories of a number of NPC's.
    /// </summary>
    [HarmonyPatch(typeof(FPHubNPC), "Start")]
    class FPHubNPCPatch
    {
        static bool Prefix(FPHubNPC __instance)
        {
            PotionSellerUtils.ExpandNPCShopInventory(__instance);
            return true;
        }
    }
}
