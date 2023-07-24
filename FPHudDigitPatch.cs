namespace PotionSeller
{
    using HarmonyLib;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// FPHudDigit is a single visual box in an item menu. It holds a list of sprites indexed by FPPowerup values. So in order to have extra items,
    /// we need to expand that list. This is done via a check any time SetDigitValue or GetRenderer is called.
    /// </summary>
    [HarmonyPatch]
    class FPHudDigitPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            Type type = typeof(FPHudDigit);
            return new List<MethodBase>
            {
                type.GetMethod("SetDigitValue"),
                type.GetMethod("GetRenderer")
            };
        }

        /// <summary>
        /// If the name of the object starts with "Hud Powerup Icon" or "ItemIcon", it's an item digit. If it starts with "Hud Potions Slot", it's a potion digit.
        /// In both cases, we only expand the digitFrames array if it's currently too small. The new indexes are pre-filled with a pre-set value.
        /// </summary>
        /// <param name="__instance">The object instance of the digit.</param>
        static void Prefix(FPHudDigit __instance)
        {
            if ((__instance.name.StartsWith("Hud Powerup Icon") || __instance.name.StartsWith("ItemIcon")) && (__instance.digitFrames.Length < PotionSellerUtils.GetItemArraySize()))
            {
                Sprite blankSprite = __instance.digitFrames[(__instance.digitFrames.Length > 1) ? 1 : 0];
                Sprite[] replacement = new Sprite[PotionSellerUtils.GetItemArraySize()];
                for (int i = 0; i < replacement.Length; i++)
                    if (i < __instance.digitFrames.Length)
                        replacement[i] = __instance.digitFrames[i];
                    else
                        replacement[i] = PotionSellerUtils.ItemSprites.ContainsKey(i) ? PotionSellerUtils.ItemSprites[i] : blankSprite;
                    
                __instance.digitFrames = replacement;
            }

            if (__instance.name.StartsWith("Hud Potion Slot") && (__instance.digitFrames.Length < PotionSellerUtils.GetPotionArraySize() + 1))
            {
                Sprite blankSprite = __instance.digitFrames[(__instance.digitFrames.Length > 1) ? 1 : 0];
                Sprite[] replacement = new Sprite[PotionSellerUtils.GetPotionArraySize() + 1];
                int potionId;
                for (int i = 0; i < replacement.Length; i++)
                    if (i < __instance.digitFrames.Length)
                        replacement[i] = __instance.digitFrames[i];
                    else
                    {
                        potionId = i - 2;
                        replacement[i] = PotionSellerUtils.PotionSprites.ContainsKey(potionId) ? PotionSellerUtils.PotionSprites[potionId].Side : blankSprite;
                    }

                __instance.digitFrames = replacement;
            }
        }
    }
}
