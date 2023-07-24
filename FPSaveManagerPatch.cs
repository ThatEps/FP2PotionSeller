namespace PotionSeller
{
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Bundles all of the patches to FPSaveManager.
    /// </summary>
    static class FPSaveManagerPatch
    {
        /// <summary>
        /// Applies all changes to FPSaveManager.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(FPSaveManagerPatch_Awake));
            harmony.PatchAll(typeof(FPSaveManagerPatch_NewGame));
            harmony.PatchAll(typeof(FPSaveManagerPatch_LoadFromFile));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetItemName));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetItemDescription));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetItemBonus));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetPotionID));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetPotionInventoryID));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetPotionDescription1));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetPotionDescription2));
            harmony.PatchAll(typeof(FPSaveManagerPatch_GetPotionEffect));
            harmony.PatchAll(typeof(FPSaveManagerPatch_BadgeCheck));

            if (FPSaveManager.inventory.Length < PotionSellerUtils.GetItemArraySize())
                FPSaveManager.inventory = new byte[PotionSellerUtils.GetItemArraySize()];
        }
    }

    /// <summary>
    /// Makes sure that FPSaveManager's state is correct on awake.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), "Awake")]
    class FPSaveManagerPatch_Awake
    {
        static void Prefix(out bool __state)
        {
            __state = (FPSaveManager.itemSets == null);
        }
        
        static void Postfix(bool __state)
        {
            if (__state && (FPSaveManager.itemSets != null))
                for (int i = 0; i < FPSaveManager.itemSets.Length; i++)
                    if (FPSaveManager.itemSets[i].activePotions.Length < PotionSellerUtils.GetPotionArraySize())
                        FPSaveManager.itemSets[i].activePotions = new byte[PotionSellerUtils.GetPotionArraySize()];
        }
    }

    /// <summary>
    /// Makes sure that FPSaveManager's state is correct when a new game is started.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.NewGame))]
    class FPSaveManagerPatch_NewGame
    {
        static void Postfix()
        {
            if (FPSaveManager.inventory.Length < PotionSellerUtils.GetItemArraySize())
                FPSaveManager.inventory = new byte[PotionSellerUtils.GetItemArraySize()];

            if (FPSaveManager.itemSlotExpansionLevel < 2)
                FPSaveManager.itemSlotExpansionLevel = 2;

            for (int i = 0; i < FPSaveManager.itemSets.Length; i++)
                if (FPSaveManager.itemSets[i].activePotions.Length < PotionSellerUtils.GetPotionArraySize())
                    FPSaveManager.itemSets[i].activePotions = new byte[PotionSellerUtils.GetPotionArraySize()];
        }
    }

    /// <summary>
    /// Makes sure that FPSaveManager's state is correct when a game is loaded from a save.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.LoadFromFile))]
    class FPSaveManagerPatch_LoadFromFile
    {
        /// <summary>
        /// Alters the line 'inventory = ExpandByteArray(playerData.inventory, 50);' to expand to the desired array size instead.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <returns>The modified instructions.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            The IL we're looking to modify is as follows:
                ldfld uint8[] PlayerData::inventory
                ldc.i4.s  50
                call uint8[] FPSaveManager::ExpandByteArray(uint8[], int32)

            First we find the ldfld command with the operand "inventory".
            Then we replace the first ldc.i4.s command we encounter afterwards with ldc.i4, and the desired size as the operand.
             */

            bool foundStart = false;

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (foundStart && (codes[i].opcode == OpCodes.Ldc_I4_S))
                {
                    // Ldc_I4_S requires an int8 value, which can only go up to 127.
                    // We can't guarantee that the operand will not exceed 127.
                    // So we change the command to Ldc_I4 which accepts int32.

                    codes[i].opcode = OpCodes.Ldc_I4;   
                    codes[i].operand = PotionSellerUtils.GetItemArraySize();
                    break;
                }

                if ((codes[i].opcode == OpCodes.Ldfld) && (codes[i].operand as FieldInfo != null) && ((FieldInfo)codes[i].operand).Name == "inventory")
                    foundStart = true;
            }

            return codes.AsEnumerable();
        }

        /// <summary>
        /// Handles other parts of FPSaveManager's state.
        /// </summary>
        static void Postfix()
        {
            if (FPSaveManager.itemSlotExpansionLevel < 2)
                FPSaveManager.itemSlotExpansionLevel = 2;

            int oldLength;
            byte[] oldArray, newArray;

            if (FPSaveManager.itemSets != null)
                for (int i = 0; i < FPSaveManager.itemSets.Length; i++)
                {
                    oldArray = FPSaveManager.itemSets[i].activePotions;
                    oldLength = oldArray.Length;
                    if (oldLength < PotionSellerUtils.GetPotionArraySize())
                    {
                        newArray = new byte[PotionSellerUtils.GetPotionArraySize()];
                        for (int j = 0; j < oldLength; j++)
                            newArray[j] = oldArray[j];
                        FPSaveManager.itemSets[i].activePotions = newArray;
                    }
                }
        }
    }

    /// <summary>
    /// Adds the names for the new items and potions.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetItemName))]
    class FPSaveManagerPatch_GetItemName
    {
        /// <summary>
        /// Runs a custom check for extra items. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The name of the item.</param>
        /// <param name="item">The item parameter.</param>
        /// <returns><c>True</c> if it's a regular item, <c>false</c>.</returns>
        static bool Prefix(ref string __result, FPPowerup item)
        {
            if ((int)item == PotionSellerUtils.HoverPotionIndex) { __result = "Hover Potion"; return false; }
            if ((int)item == PotionSellerUtils.EnergyPotionIndex) { __result = "Energy Potion"; return false; }
            if ((int)item == PotionSellerUtils.ResonancePotionIndex) { __result = "Resonance Potion"; return false; }
            if ((int)item == PotionSellerUtils.AngelTearIndex) { __result = "Angel Tear"; return false; }
            if ((int)item == PotionSellerUtils.TurtleGodshellIndex) { __result = "Turtle Godshell"; return false; }
            if ((int)item == PotionSellerUtils.TinkerGloveIndex) { __result = "Tinker Glove"; return false; }
            if ((int)item == PotionSellerUtils.PhoenixTonicIndex) { __result = "Pheonix Tonic"; return false; }
            if ((int)item == PotionSellerUtils.WarpstoneIndex) { __result = "Warpstone"; return false; }
            if ((int)item == PotionSellerUtils.MadstoneIndex) { __result = "Madstone"; return false; }
            if ((int)item == PotionSellerUtils.GuardianCharmIndex) { __result = "Guardian Charm"; return false; }
            if ((int)item == PotionSellerUtils.ExplosiveFinaleIndex) { __result = "Explosive Finale"; return false; }
            if ((int)item == PotionSellerUtils.IdolOfGreedIndex) { __result = "Idol of Greed"; return false; }
            if ((int)item == PotionSellerUtils.BombMagnetIndex) { __result = "Bomb Magnet"; return false; }
            if ((int)item == PotionSellerUtils.NinjaGarbIndex) { __result = "Ninja Garb"; return false; }
            if ((int)item == PotionSellerUtils.IceCrownIndex) { __result = "Ice Crown"; return false; }
            if ((int)item == PotionSellerUtils.InvisibilityCloakIndex) { __result = "Invisibility Cloak"; return false; }
            if ((int)item == PotionSellerUtils.GravityBootsIndex) { __result = "Gravity Boots"; return false; }
            if ((int)item == PotionSellerUtils.MagicCompassIndex) { __result = "Magic Compass"; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the descriptions for the changed and new items/potions.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetItemDescription))]
    class FPSaveManagerPatch_GetItemDescription
    {
        /// <summary>
        /// Runs a custom check for changed and extra items. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The description of the item.</param>
        /// <param name="item">The item parameter.</param>
        /// <returns><c>True</c> if it's a regular item, <c>false</c>.</returns>
        static bool Prefix(ref string __result, FPPowerup item)
        {
            if (item == FPPowerup.PAYBACK_RING) { __result = "Gain +15% attack power when you have 1 stock left, and +25% power when you have none."; return false; }

            if ((int)item == PotionSellerUtils.HoverPotionIndex) { __result = "(Potion) Reduces your maximum falling speed."; return false; }
            if ((int)item == PotionSellerUtils.EnergyPotionIndex)
            {
                __result = (FPSaveManager.character == FPCharacterID.MILLA) ? "(Potion) Milla's Cube Blaster drains less Phantom Cube energy." : "(Potion) Your special meter regenerates faster.";
                return false;
            }
            if ((int)item == PotionSellerUtils.ResonancePotionIndex) { __result = "(Potion) Standing in place charges up a shockwave. Launch in front of you by pressing Attack."; return false; }
            if ((int)item == PotionSellerUtils.AngelTearIndex) { __result = "All damage to you is reduced by 50%, and you slowly renegerate life."; return false; }
            if ((int)item == PotionSellerUtils.TurtleGodshellIndex) { __result = "Blocking a hit with Guard grants 30% longer invincibility and a small amount of life."; return false; }
            if ((int)item == PotionSellerUtils.TinkerGloveIndex) { __result = "Gain 50% more Cores."; return false; }
            if ((int)item == PotionSellerUtils.PhoenixTonicIndex) { __result = "You start with 1 extra stock, and recover 2 extra life on your first revival."; return false; }
            if ((int)item == PotionSellerUtils.WarpstoneIndex) { __result = "While you are in a stage, everything is 20% slower."; return false; }
            if ((int)item == PotionSellerUtils.MadstoneIndex) { __result = "While you are in a stage, everything is 20% faster."; return false; }
            if ((int)item == PotionSellerUtils.GuardianCharmIndex) { __result = "After a shield takes elemental damage, it changes to that element."; return false; }
            if ((int)item == PotionSellerUtils.ExplosiveFinaleIndex) { __result = "Revivals only restore a sliver of life, but you cause an explosion when knocked down."; return false; }
            if ((int)item == PotionSellerUtils.IdolOfGreedIndex) { __result = "You have 1 less maximum life, but bosses drop crystals as you damage them."; return false; }
            if ((int)item == PotionSellerUtils.BombMagnetIndex) { __result = "Each non-boss enemy has a 35% chance to be rigged with a proximity bomb."; return false; }
            if ((int)item == PotionSellerUtils.NinjaGarbIndex) { __result = "Do a short roll in the direction you're holding when you Guard. (Except on bike)"; return false; }
            if ((int)item == PotionSellerUtils.IceCrownIndex) { __result = "Creates icy shockwaves when hit. Recharges after you lose 3 petals of life or shields."; return false; }
            if ((int)item == PotionSellerUtils.InvisibilityCloakIndex) { __result = "You can't see your own body."; return false; }
            if ((int)item == PotionSellerUtils.GravityBootsIndex) { __result = "The Down key is constantly held down while you are on the ground."; return false; }
            if ((int)item == PotionSellerUtils.MagicCompassIndex) { __result = "Points towards the closest checkpoint you haven't reached yet."; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the crystal bonuses for the changed and new items.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetItemBonus))]
    class FPSaveManagerPatch_GetItemBonus
    {
        /// <summary>
        /// Runs a custom check for changed and extra items. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The crystal bonus of the item.</param>
        /// <param name="item">The item parameter.</param>
        /// <returns><c>True</c> if it's a regular item, <c>false</c>.</returns>
        static bool Prefix(ref float __result, FPPowerup item)
        {
            if (item == FPPowerup.RAINBOW_CHARM) { __result = 0.1f; return false; }
            if ((item == FPPowerup.DOUBLE_DAMAGE) && FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.AngelTearIndex)) { __result = 0.1f; return false; }

            if ((int)item == PotionSellerUtils.HoverPotionIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.EnergyPotionIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.ResonancePotionIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.AngelTearIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.TurtleGodshellIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.TinkerGloveIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.PhoenixTonicIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.WarpstoneIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.MadstoneIndex) { __result = FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.WarpstoneIndex) ? 0.2f : 0.5f; return false; }
            if ((int)item == PotionSellerUtils.GuardianCharmIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.ExplosiveFinaleIndex) { __result = FPSaveManager.PowerupEquipped(FPPowerup.ONE_HIT_KO) ? 0.1f : 0.2f; return false; }
            if ((int)item == PotionSellerUtils.IdolOfGreedIndex) { __result = FPSaveManager.PowerupEquipped(FPPowerup.ONE_HIT_KO) ? 0.1f : 0.15f; return false; }
            if ((int)item == PotionSellerUtils.BombMagnetIndex) { __result = 0.25f; return false; }
            if ((int)item == PotionSellerUtils.NinjaGarbIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.IceCrownIndex) { __result = 0f; return false; }
            if ((int)item == PotionSellerUtils.InvisibilityCloakIndex) { __result = 0.4f; return false; }
            if ((int)item == PotionSellerUtils.GravityBootsIndex) { __result = 0.2f; return false; }
            if ((int)item == PotionSellerUtils.MagicCompassIndex) { __result = 0f; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the IDs for new potions.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetPotionID))]
    class FPSaveManagerPatch_GetPotionID
    {
        /// <summary>
        /// Runs a custom check for extra potions. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The ID of the potion.</param>
        /// <param name="item">The item parameter.</param>
        /// <returns><c>True</c> if it's a regular potion, <c>false</c>.</returns>
        static bool Prefix(ref int __result, FPPowerup item)
        {
            if ((int)item == PotionSellerUtils.HoverPotionIndex) { __result = PotionSellerUtils.PotionIdHover; return false; }
            if ((int)item == PotionSellerUtils.EnergyPotionIndex) { __result = PotionSellerUtils.PotionIdEnergy; return false; }
            if ((int)item == PotionSellerUtils.ResonancePotionIndex) { __result = PotionSellerUtils.PotionIdResonance; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the reverse-ID mappings for new potions.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetPotionInventoryID))]
    class FPSaveManagerPatch_GetPotionInventoryID
    {
        /// <summary>
        /// Runs a custom check for extra potions. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The index of the potion.</param>
        /// <param name="id">The potion ID parameter.</param>
        /// <returns><c>True</c> if it's a regular potion, <c>false</c>.</returns>
        static bool Prefix(ref FPPowerup __result, int id)
        {
            if (id == PotionSellerUtils.PotionIdHover) { __result = (FPPowerup)PotionSellerUtils.HoverPotionIndex; return false; }
            if (id == PotionSellerUtils.PotionIdEnergy) { __result = (FPPowerup)PotionSellerUtils.EnergyPotionIndex; return false; }
            if (id == PotionSellerUtils.PotionIdResonance) { __result = (FPPowerup)PotionSellerUtils.ResonancePotionIndex; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the descriptions of new potions. (Overload that takes a FPPowerup parameter)
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetPotionDescription), typeof(FPPowerup))]
    class FPSaveManagerPatch_GetPotionDescription1
    {
        /// <summary>
        /// Runs a custom check for extra potions. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The potion's description.</param>
        /// <param name="item">The item parameter.</param>
        /// <returns><c>True</c> if it's a regular potion, <c>false</c>.</returns>
        static bool Prefix(ref string __result, FPPowerup item)
        {
            if ((int)item == PotionSellerUtils.HoverPotionIndex) { __result = "Reduces your maximum falling speed by 10%."; return false; }
            if ((int)item == PotionSellerUtils.EnergyPotionIndex)
            {
                __result = (FPSaveManager.character == FPCharacterID.MILLA) ? "Cube Blaster drains 6% less Phantom Cube energy." : "Your special meter regenerates 4% faster.";
                return false;
            }
            if ((int)item == PotionSellerUtils.ResonancePotionIndex) { __result = "Standing in place charges a shockwave 20% faster. Launch in front of you by pressing Attack."; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the descriptions of new potions. (Overload that takes a potion ID parameter)
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetPotionDescription), typeof(int))]
    class FPSaveManagerPatch_GetPotionDescription2
    {
        /// <summary>
        /// Runs a custom check for extra potions. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The potion's description.</param>
        /// <param name="item">The potion ID parameter.</param>
        /// <returns><c>True</c> if it's a regular potion, <c>false</c>.</returns>
        static bool Prefix(ref string __result, int item)
        {
            if (item == PotionSellerUtils.PotionIdHover) { __result = "Reduces your maximum falling speed by 10%."; return false; }
            if (item == PotionSellerUtils.PotionIdEnergy)
            {
                __result = (FPSaveManager.character == FPCharacterID.MILLA) ? "Cube Blaster drains 6% less Phantom Cube energy." : "Your special meter regenerates 4% faster.";
                return false;
            }
            if (item == PotionSellerUtils.PotionIdResonance) { __result = "Standing in place charges a shockwave 20% faster. Launch in front of you by pressing Attack."; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the effect descriptions of new potions.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetPotionEffect))]
    class FPSaveManagerPatch_GetPotionEffect
    {
        /// <summary>
        /// Runs a custom check for extra potions. If one is detected, the rest of the method isn't run.
        /// </summary>
        /// <param name="__result">The effect's description.</param>
        /// <param name="item">The potion ID parameter.</param>
        /// <param name="amount">The amount of the potion used.</param>
        /// <returns><c>True</c> if it's a regular potion, <c>false</c>.</returns>
        static bool Prefix(ref string __result, int item, float amount)
        {
            if (item == PotionSellerUtils.PotionIdHover) { __result = "-" + amount * 10f + "% fall speed"; return false; }
            if (item == PotionSellerUtils.PotionIdEnergy)
            {
                if ((FPSaveManager.character == FPCharacterID.MILLA))
                    __result = "-" + amount * 6f + "% cube consumption";
                else
                    __result = "+" + amount * 4f + "% meter regen";
                return false;
            }
            if (item == PotionSellerUtils.PotionIdResonance) { __result = "+" + amount * 20f + "% shockwave charging"; return false; }
            return true;
        }
    }

    /// <summary>
    /// Adds the new Bravestones for the checks of the Challenger, Geologist, and Lategame Guts badges.
    /// </summary>
    [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.BadgeCheck))]
    class FPSaveManagerPatch_BadgeCheck
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            BadgeCheck consists of a big switch statement that checks a specific badge based on its paremeter.
            Each of the three switch cases we're interested in start in the same manner:
switchLabel:    ldc.i4.0
                stloc.[X]

            This corresponds to setting a local variable to 0. All we need to do is add the new Bravestones to that 0.
            We do this by loading the labels from the 'switch' command and using them to find each of the 'ldc.i4.0' commands.
            Then we insert the following after each one:
                call    int32 FPSaveManagerPatch_BadgeCheck.[methodToCall]
                add
            */

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label[] switchCases = ReadSwitch();

            if (switchCases != null)
            {
                AddMethodCall(switchCases[7], "CountEquippedBravestones"); // Challenger
                AddMethodCall(switchCases[12], "CountUnlockedBravestones"); // Geologist
                AddMethodCall(switchCases[15], "CountEquippedBravestones"); // Lategame Guts
            }

            return codes.AsEnumerable();


            Label[] ReadSwitch()
            {
                for (int i = 0; i < codes.Count; i++)
                    if (codes[i].opcode == OpCodes.Switch)
                        return (Label[])codes[i].operand;

                return null;
            }

            void AddMethodCall(Label label, string methodToCall)
            {
                for (int i = 0; i < codes.Count; i++)
                    if ((codes[i].opcode == OpCodes.Ldc_I4_0) && (codes[i].labels != null) && codes[i].labels.Contains(label))
                    {
                        codes.InsertRange(i + 1, new List<CodeInstruction>
                        {
                            new CodeInstruction(OpCodes.Call, typeof(FPSaveManagerPatch_BadgeCheck).GetMethod(methodToCall)),
                            new CodeInstruction(OpCodes.Add),
                        });
                        break;
                    }
            }
        }

        public static int CountEquippedBravestones()
        {
            int bravestoneCount = 0;
            if (FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.MadstoneIndex)) bravestoneCount++;
            if (FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.ExplosiveFinaleIndex)) bravestoneCount++;
            if (FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.IdolOfGreedIndex)) bravestoneCount++;
            if (FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.BombMagnetIndex)) bravestoneCount++;
            if (FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.InvisibilityCloakIndex)) bravestoneCount++;
            if (FPSaveManager.PowerupEquipped((FPPowerup)PotionSellerUtils.GravityBootsIndex)) bravestoneCount++;
            return bravestoneCount;
        }

        public static int CountUnlockedBravestones()
        {
            int bravestoneCount = 0;
            if (FPSaveManager.inventory[PotionSellerUtils.MadstoneIndex] > 0) bravestoneCount++;
            if (FPSaveManager.inventory[PotionSellerUtils.ExplosiveFinaleIndex] > 0) bravestoneCount++;
            if (FPSaveManager.inventory[PotionSellerUtils.IdolOfGreedIndex] > 0) bravestoneCount++;
            if (FPSaveManager.inventory[PotionSellerUtils.BombMagnetIndex] > 0) bravestoneCount++;
            if (FPSaveManager.inventory[PotionSellerUtils.InvisibilityCloakIndex] > 0) bravestoneCount++;
            if (FPSaveManager.inventory[PotionSellerUtils.GravityBootsIndex] > 0) bravestoneCount++;
            return bravestoneCount;
        }
    }
}
