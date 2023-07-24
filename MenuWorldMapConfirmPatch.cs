namespace PotionSeller
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Expands the item slots and potion sprites in the stage confirm menu.
    /// </summary>
    [HarmonyPatch(typeof(MenuWorldMapConfirm), "Start")]
    class MenuWorldMapConfirmPatch
    {
        static void Prefix(MenuWorldMapConfirm __instance)
        {
            __instance.spriteBottom = PotionSellerUtils.ExpandBottleSprites(__instance.spriteBottom, PotionSellerBottleSection.Bottom);
            __instance.spriteMiddle = PotionSellerUtils.ExpandBottleSprites(__instance.spriteMiddle, PotionSellerBottleSection.Middle);
            __instance.spriteTop = PotionSellerUtils.ExpandBottleSprites(__instance.spriteTop, PotionSellerBottleSection.Top);

            if (__instance.pfItemBox.Length > 0)
                for (int i = 0; i < __instance.pfItemBox.Length; i++)
                    __instance.pfItemBox[i].SetActive(true);

            Offset("StageIcon", 22);
            Offset("StageName", 22);
            Offset("HideFromDialog/StageInfo", 22);
            Offset("HideFromDialog/HubInfo", 12);

            void Offset(string name, int offset)
            {
                Transform transform = __instance.transform.Find(name).transform;
                Vector3 position = transform.localPosition;
                position.x = position.x + offset;
                transform.localPosition = position;
            }
        }
    }
}
