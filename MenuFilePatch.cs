namespace PotionSeller
{
    using HarmonyLib;

    /// <summary>
    /// Expands the potion sprites in the file menu.
    /// </summary>
    [HarmonyPatch(typeof(MenuFile), "Start")]
    class MenuFilePatch
    {
        static void Prefix(MenuFile __instance)
        {
            MenuFilePanel panel;
            for (int x = 0; x < __instance.files.Length; x++)
            {
                panel = __instance.files[x];
                panel.spriteBottom = PotionSellerUtils.ExpandBottleSprites(panel.spriteBottom, PotionSellerBottleSection.Bottom);
                panel.spriteMiddle = PotionSellerUtils.ExpandBottleSprites(panel.spriteMiddle, PotionSellerBottleSection.Middle);
                panel.spriteTop = PotionSellerUtils.ExpandBottleSprites(panel.spriteTop, PotionSellerBottleSection.Top);
            }
        }
    }
}
