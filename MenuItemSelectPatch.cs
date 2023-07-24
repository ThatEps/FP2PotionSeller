namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Expands the items and potions shown in the item select menu.
    /// </summary>
    [HarmonyPatch(typeof(MenuItemSelect), "Start")]
    class MenuItemSelectPatch
    {
        static void Prefix(MenuItemSelect __instance)
        {
            PotionSellerUtils.ExpandItemSelectAmulets(__instance);
            PotionSellerUtils.ExpandItemSelectPotions(__instance);

            __instance.spriteBottom = PotionSellerUtils.ExpandItemSelectBottleSprites(__instance.spriteBottom, PotionSellerBottleSection.Bottom);
            __instance.spriteMiddle = PotionSellerUtils.ExpandItemSelectBottleSprites(__instance.spriteMiddle, PotionSellerBottleSection.Middle);
            __instance.spriteTop = PotionSellerUtils.ExpandItemSelectBottleSprites(__instance.spriteTop, PotionSellerBottleSection.Top);
        }
    }
}