namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Expands the potion sprites in the item set panels.
    /// </summary>
    [HarmonyPatch(typeof(MenuItemSet), "Start")]
    class MenuItemSetPatch
    {
        static void Prefix(MenuItemSet __instance)
        {
            __instance.spriteBottom = PotionSellerUtils.ExpandBottleSprites(__instance.spriteBottom, PotionSellerBottleSection.Bottom);
            __instance.spriteMiddle = PotionSellerUtils.ExpandBottleSprites(__instance.spriteMiddle, PotionSellerBottleSection.Middle);
            __instance.spriteTop = PotionSellerUtils.ExpandBottleSprites(__instance.spriteTop, PotionSellerBottleSection.Top);
        }
    }
}