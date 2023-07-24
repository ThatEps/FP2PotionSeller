namespace PotionSeller
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Bundles all of the patches to FPPauseMenu.
    /// </summary>
    class FPPauseMenuPatch
    {
        /// <summary>
        /// Applies all changes to FPPauseMenu.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(FPPauseMenuPatch_Start));
            harmony.PatchAll(typeof(FPPauseMenuPatch_Update));
        }
    }

    /// <summary>
    /// Expands the item slots and potion sprites in the pause menu.
    /// </summary>
    [HarmonyPatch(typeof(FPPauseMenu), "Start")]
    class FPPauseMenuPatch_Start
    {
        static void Prefix(FPPauseMenu __instance)
        {
            __instance.spriteBottom = PotionSellerUtils.ExpandBottleSprites(__instance.spriteBottom, PotionSellerBottleSection.Bottom);
            __instance.spriteMiddle = PotionSellerUtils.ExpandBottleSprites(__instance.spriteMiddle, PotionSellerBottleSection.Middle);
            __instance.spriteTop = PotionSellerUtils.ExpandBottleSprites(__instance.spriteTop, PotionSellerBottleSection.Top);

            __instance.pfItemBox = new GameObject[4]
            {
                __instance.pfItemBox[0],
                __instance.pfItemBox[1],
                null,
                null
            };

            __instance.itemIcon = new FPHudDigit[4]
            {
                __instance.itemIcon[0],
                __instance.itemIcon[1],
                __instance.itemIcon[1],
                __instance.itemIcon[1]
            };

            GameObject gobject = Object.Instantiate(__instance.pfItemBox[1]);
            gobject.transform.parent = __instance.transform;
            gobject.transform.localPosition = new Vector3(112, 72, 0);
            __instance.pfItemBox[2] = gobject;

            gobject = Object.Instantiate(__instance.pfItemBox[1]);
            gobject.transform.parent = __instance.transform;
            gobject.transform.localPosition = new Vector3(160, 72, 0);
            __instance.pfItemBox[3] = gobject;
        }
    }

    /// <summary>
    /// Resets the defeat count when the player confirms either the "Restart stage" or "Return to map" option.
    /// </summary>
    [HarmonyPatch(typeof(FPPauseMenu), "Update")]
    class FPPauseMenuPatch_Update
    {
        static void Prefix(ref int ___state, ref int ___confirmSelection, ref int ___menuSelection)
        {
            if ((___state == 1) && FPStage.menuInput.confirm && (___confirmSelection == 1) && ((___menuSelection == 5) || (___menuSelection == 6)))
            {
                PotionSellerUtils.StageDefeatsID = -1;
                PotionSellerUtils.StageDefeats = 0;
            }
        }
    }
}
