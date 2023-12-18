namespace PotionSeller
{
    using HarmonyLib;
    using System;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Bundles all of the patches to MenuDifficulty.
    /// </summary>
    class MenuDifficultyPatch
    {
        public static MethodInfo UpdateMenuPosition;
        public static MethodInfo Stage_Go;

        public static FPPowerup ThirdItem = 0;
        public static FPPowerup FourthItem = 0;
        public static FPItemSet ChilloutItems;
        public static FPItemSet InsanityItems;

        /// <summary>
        /// Applies all changes to MenuDifficulty.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            UpdateMenuPosition = AccessTools.Method(typeof(MenuDifficulty), "UpdateMenuPosition");
            Stage_Go = AccessTools.Method(typeof(MenuDifficulty), "State_Go");

            harmony.PatchAll(typeof(MenuDifficultyPatch_Start));
            harmony.PatchAll(typeof(MenuDifficultyPatch_UpdateMenuPosition));
            harmony.PatchAll(typeof(MenuDifficultyPatch_State_Main));
        }
    }

    /// <summary>
    /// Alters existing properties and child objects, and adds new ones.
    /// </summary>
    [HarmonyPatch(typeof(MenuDifficulty), "Start")]
    class MenuDifficultyPatch_Start
    {
        static void Prefix(MenuDifficulty __instance)
        {
            MenuDifficultyPatch.ThirdItem = 0;
            MenuDifficultyPatch.FourthItem = 0;
            MenuDifficultyPatch.ChilloutItems = new FPItemSet()
            {
                powerups = new FPPowerup[4]
                {
                    (FPPowerup)PotionSellerUtils.AngelTearIndex,
                    (FPPowerup)PotionSellerUtils.TurtleGodshellIndex,
                    (FPPowerup)PotionSellerUtils.WarpstoneIndex,
                    FPPowerup.ELEMENT_BURST
                },
                activePotions = new byte[PotionSellerUtils.GetPotionArraySize()]
            };
            MenuDifficultyPatch.InsanityItems = new FPItemSet()
            {
                powerups = new FPPowerup[4]
                {
                    FPPowerup.ONE_HIT_KO,
                    FPPowerup.STOCK_DRAIN,
                    (FPPowerup)PotionSellerUtils.MadstoneIndex,
                    (FPPowerup)PotionSellerUtils.BombMagnetIndex
                },
                activePotions = new byte[PotionSellerUtils.GetPotionArraySize()]
            };

            __instance.easyItems.powerups = new FPPowerup[4] { FPPowerup.MORE_PETALS, (FPPowerup)PotionSellerUtils.TurtleGodshellIndex, FPPowerup.NONE, FPPowerup.NONE };
            __instance.easyItems.activePotions = new byte[PotionSellerUtils.GetPotionArraySize()];
            __instance.normalItems.powerups = new FPPowerup[4] { FPPowerup.NONE, FPPowerup.NONE, FPPowerup.NONE, FPPowerup.NONE };
            __instance.normalItems.activePotions = new byte[PotionSellerUtils.GetPotionArraySize()];
            __instance.hardItems.powerups = new FPPowerup[4] { FPPowerup.DOUBLE_DAMAGE, (FPPowerup)PotionSellerUtils.ExplosiveFinaleIndex, FPPowerup.NONE, FPPowerup.NONE };
            __instance.hardItems.activePotions = new byte[PotionSellerUtils.GetPotionArraySize()];

            // --------------------------------------------------------- //

            GameObject chilloutOption = DuplicateAndOffset("OptionGroupGeneral/OptionEasy", "OptionChillout", -32);
            chilloutOption.GetComponentInChildren<TextMesh>().text = "Chillout";
            MenuOption chilloutMenuOption = chilloutOption.GetComponent<MenuOption>();

            GameObject insanityOption = DuplicateAndOffset("OptionGroupGeneral/OptionHard", "OptionInsanity", 32);
            insanityOption.GetComponentInChildren<TextMesh>().text = "Insanity";
            MenuOption insanityMenuOption = insanityOption.GetComponent<MenuOption>();
            insanityMenuOption.normalSprite = PotionSellerUtils.InsanityDifficultySprite;
            insanityMenuOption.selectedSprite = PotionSellerUtils.InsanityDifficultyOnSprite;
            insanityMenuOption.GetComponent<SpriteRenderer>().sprite = PotionSellerUtils.InsanityDifficultySprite;

            __instance.menuOptions = new MenuOption[6];
            __instance.menuOptions[0] = chilloutMenuOption;
            __instance.menuOptions[1] = OffsetExistingOption("OptionGroupGeneral/OptionEasy", 0);
            __instance.menuOptions[2] = OffsetExistingOption("OptionGroupGeneral/OptionNormal", 0);
            __instance.menuOptions[3] = OffsetExistingOption("OptionGroupGeneral/OptionHard", 0);
            __instance.menuOptions[4] = insanityMenuOption;
            __instance.menuOptions[5] = OffsetExistingOption("OptionGroupGeneral/OptionGroupReturn/OptionReturn", 32);

            __instance.menuOptions[1].normalSprite = PotionSellerUtils.BeginnerDifficultySprite;
            __instance.menuOptions[1].selectedSprite = PotionSellerUtils.BeginnerDifficultyOnSprite;
            __instance.menuOptions[1].GetComponent<SpriteRenderer>().sprite = PotionSellerUtils.BeginnerDifficultySprite;
            __instance.menuOptions[2].normalSprite = PotionSellerUtils.NormalDifficultySprite;
            __instance.menuOptions[2].selectedSprite = PotionSellerUtils.NormalDifficultyOnSprite;
            __instance.menuOptions[2].GetComponent<SpriteRenderer>().sprite = PotionSellerUtils.NormalDifficultyOnSprite;
            __instance.transform.Find("OptionGroupGeneral/OptionCustom").gameObject.SetActive(false);

            // --------------------------------------------------------- //

            GameObject customSlot3 = DuplicateAndOffset("OptionGroupGeneral/OptionGroupRightside/OptionCustomSlot2", "OptionCustomSlot3", 8);
            GameObject customSlot4 = DuplicateAndOffset("OptionGroupGeneral/OptionGroupRightside/OptionCustomSlot2", "OptionCustomSlot4", 48);
            OffsetExistingOption("OptionGroupGeneral/OptionGroupRightside/OptionCustomSlot1", -32);
            OffsetExistingOption("OptionGroupGeneral/OptionGroupRightside/OptionCustomSlot2", -32);
            Offset(__instance.potionEffects.transform, 48);

            __instance.itemIcon = new FPHudDigit[4]
            {
                __instance.itemIcon[0],
                __instance.itemIcon[1],
                customSlot3.GetComponentInChildren<FPHudDigit>(),
                customSlot4.GetComponentInChildren<FPHudDigit>()
            };
            __instance.pfText = new MenuText[4]
            {
                __instance.pfText[0],
                __instance.pfText[1],
                customSlot3.GetComponentInChildren<MenuText>(),
                customSlot4.GetComponentInChildren<MenuText>()
            };

            // --------------------------------------------------------- //

            GameObject DuplicateAndOffset(string nameOfOriginal, string name, int offset)
            {
                Transform original = __instance.transform.Find(nameOfOriginal);
                GameObject gobject = UnityEngine.Object.Instantiate(original.gameObject);
                gobject.name = name;
                gobject.transform.parent = original.parent;
                Offset(gobject.transform, offset);
                return gobject;
            }

            void Offset(Transform transform, int offset)
            {
                Vector3 position = transform.localPosition;
                position.y = position.y - offset;
                transform.localPosition = position;
            }

            MenuOption OffsetExistingOption(string name, int offset)
            {
                GameObject gobject = __instance.transform.Find(name).gameObject;
                Offset(gobject.transform, offset);
                MenuOption option = gobject.GetComponent<MenuOption>();
                option.start.y = option.start.y - offset;
                return option;
            }
        }

        static void Postfix(ref int ___menuSelection)
        {
            ___menuSelection = 2;
        }
    }

    /// <summary>
    /// Handles the menu position update.
    /// </summary>
    [HarmonyPatch(typeof(MenuDifficulty), "UpdateMenuPosition")]
    class MenuDifficultyPatch_UpdateMenuPosition
    {
        static bool Prefix(MenuDifficulty __instance, ref int ___buttonCount, ref int ___menuSelection, ref FPPowerup ___firstItem, ref FPPowerup ___secondItem, ref byte[] ___activePotions)
        {
            Vector3 position;
            for (int i = 0; i < ___buttonCount; i++)
                if (i == ___menuSelection)
                {
                    position = __instance.menuOptions[i].transform.position;
                    position.x = position.x - 32f;
                    __instance.cursor.transform.position = position;
                    __instance.menuOptions[i].selected = true;
                }
                else
                {
                    __instance.menuOptions[i].selected = false;
                }

            __instance.potionEffects.textMesh.text = string.Empty;
            FPItemSet? itemSet = null;
            switch (___menuSelection)
            {
                case 0:
                    itemSet = MenuDifficultyPatch.ChilloutItems;
                    __instance.potionEffects.textMesh.text += "Auto Guard is ON";
                    break;
                case 1:
                    itemSet = __instance.easyItems;
                    __instance.potionEffects.textMesh.text += "Auto Guard is OFF";
                    break;
                case 2:
                    itemSet = __instance.normalItems;
                    __instance.potionEffects.textMesh.text += "Auto Guard is OFF";
                    break;
                case 3:
                    itemSet = __instance.hardItems;
                    __instance.potionEffects.textMesh.text += "Auto Guard is OFF";
                    break;
                case 4:
                    itemSet = MenuDifficultyPatch.InsanityItems;
                    __instance.potionEffects.textMesh.text += "Auto Guard is OFF";
                    break;
            }

            ___firstItem = itemSet.HasValue ? itemSet.Value.powerups[0] : FPPowerup.NONE;
            ___secondItem = itemSet.HasValue ? itemSet.Value.powerups[1] : FPPowerup.NONE;
            MenuDifficultyPatch.ThirdItem = itemSet.HasValue ? itemSet.Value.powerups[2] : FPPowerup.NONE;
            MenuDifficultyPatch.FourthItem = itemSet.HasValue ? itemSet.Value.powerups[3] : FPPowerup.NONE;
            if (itemSet.HasValue)
                ___activePotions = itemSet.Value.activePotions;


            __instance.itemIcon[0].SetDigitValue((int)___firstItem);
            __instance.itemIcon[1].SetDigitValue((int)___secondItem);
            __instance.itemIcon[2].SetDigitValue((int)MenuDifficultyPatch.ThirdItem);
            __instance.itemIcon[3].SetDigitValue((int)MenuDifficultyPatch.FourthItem);

            for (int i = 0; i < __instance.pfText.Length; i++)
                if ((___menuSelection < 4) && (__instance.itemIcon[i].digitValue == 0))
                    __instance.pfText[i].textMesh.text = "No Item";
                else
                    __instance.pfText[i].textMesh.text = FPSaveManager.GetItemName((FPPowerup)__instance.itemIcon[i].digitValue);

            return false;
        }
    }

    /// <summary>
    /// Default state of the menu.
    /// </summary>
    [HarmonyPatch(typeof(MenuDifficulty), "State_Main")]
    class MenuDifficultyPatch_State_Main
    {
        static bool Prefix(MenuDifficulty __instance, ref int ___menuSelection, ref FPPowerup ___firstItem, ref FPPowerup ___secondItem, ref byte[] ___activePotions, ref float ___genericTimer)
        {
            float scale = 5f * FPStage.frameScale;
            Vector3 position = __instance.transform.position;
            position.y = position.y * (scale - 1f) / scale;
            __instance.transform.position = position;

            int lowest = 0;
            int highest = 5;
            int wraparound = highest - lowest + 1;
            if (FPStage.menuInput.up)
            {
                ___menuSelection--;
                if (___menuSelection < lowest)
                    ___menuSelection += wraparound;
                FPAudio.PlayMenuSfx(1);
            }
            else if (FPStage.menuInput.down)
            {
                ___menuSelection++;
                if (___menuSelection > highest)
                    ___menuSelection -= wraparound;
                FPAudio.PlayMenuSfx(1);
            }
            else if (FPStage.menuInput.cancel && (___menuSelection < highest))
            {
                ___menuSelection = highest;
                ___genericTimer = 10f;
                FPAudio.PlayMenuSfx(1);
            }

            if (___genericTimer > 0f)
            {
                ___genericTimer -= FPStage.deltaTime;
            }
            else if (FPStage.menuInput.confirm && (___menuSelection < 5))
            {
                if (___menuSelection == 0)
                {
                    FPSaveManager.assistGuard = 1;
                    FPSaveManager.assistContinue = 1;
                }
                FPSaveManager.itemSets[FPSaveManager.activeItemSet].powerups[0] = ___firstItem;
                FPSaveManager.itemSets[FPSaveManager.activeItemSet].powerups[1] = ___secondItem;
                FPSaveManager.itemSets[FPSaveManager.activeItemSet].powerups[2] = MenuDifficultyPatch.ThirdItem;
                FPSaveManager.itemSets[FPSaveManager.activeItemSet].powerups[3] = MenuDifficultyPatch.FourthItem;
                FPSaveManager.itemSets[FPSaveManager.activeItemSet].activePotions = ___activePotions;
                FPSaveManager.inventory[(int)___firstItem] = 2;
                FPSaveManager.inventory[(int)___secondItem] = 2;
                FPSaveManager.inventory[(int)MenuDifficultyPatch.ThirdItem] = 2;
                FPSaveManager.inventory[(int)MenuDifficultyPatch.FourthItem] = 2;
                
                FPAudio.PlayMenuSfx(2);
                __instance.cursor.optionSelected = true;
                __instance.state = (FPObjectState)Delegate.CreateDelegate(typeof(FPObjectState), __instance, MenuDifficultyPatch.Stage_Go);
            }
            else if ((___menuSelection == 5) && (FPStage.menuInput.confirm || FPStage.menuInput.cancel))
            {
                FPAudio.PlayMenuSfx(2);
                UnityEngine.Object.Destroy(__instance.gameObject);
            }

            MenuDifficultyPatch.UpdateMenuPosition.Invoke(__instance, null);

            return false;
        }
    }
}
