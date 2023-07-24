namespace PotionSeller
{
    using BepInEx.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class PotionSellerUtils
    {
        #region Setup

        public static int PotionIdHover = 8;
        public static int PotionIdEnergy = 9;
        public static int PotionIdResonance = 10;
        public static int PotionEndIndex = 11;

        public static int HoverPotionIndex = 80;
        public static int EnergyPotionIndex = 81;
        public static int ResonancePotionIndex = 82;
        public static int AngelTearIndex = 83;
        public static int TurtleGodshellIndex = 84;
        public static int TinkerGloveIndex = 85;
        public static int PhoenixTonicIndex = 86;
        public static int WarpstoneIndex = 87;
        public static int MadstoneIndex = 88;
        public static int GuardianCharmIndex = 89;
        public static int ExplosiveFinaleIndex = 90;
        public static int IdolOfGreedIndex = 91;
        public static int BombMagnetIndex = 92;
        public static int NinjaGarbIndex = 93;
        public static int IceCrownIndex = 94;
        public static int InvisibilityCloakIndex = 95;
        public static int GravityBootsIndex = 96;
        public static int MagicCompassIndex = 97;
        public static int ItemEndIndex = 99;

        public static Sprite ArrowSprite = null;
        public static Sprite DefeatLockSprite = null;
        public static Sprite BeginnerDifficultySprite = null;
        public static Sprite BeginnerDifficultyOnSprite = null;
        public static Sprite NormalDifficultySprite = null;
        public static Sprite NormalDifficultyOnSprite = null;
        public static Sprite InsanityDifficultySprite = null;
        public static Sprite InsanityDifficultyOnSprite = null;
        public static Dictionary<int, PotionSellerBottleSprites> PotionSprites = null;
        public static Dictionary<int, Sprite> ItemSprites = null;
        public static AudioClip BigExplosionSfx = null;
        public static AudioClip[] BombExplosionSfx = null;
        public static AudioClip BeepSfx = null;
        public static AudioClip BeepLowSfx = null;
        public static AudioClip IceSfx = null;

        /// <summary>
        /// Loads all assets from the asset bundle and initializes the mod's state.
        /// </summary>
        /// <param name="assetBundlePath">The asset bundle path.</param>
        /// <param name="logger">The logger.</param>
        public static void Init(string assetBundlePath, ManualLogSource logger)
        {
            PotionSprites = new Dictionary<int, PotionSellerBottleSprites>(3);
            ItemSprites = new Dictionary<int, Sprite>(18);
            AssetBundle bundle;
            try
            {
                bundle = AssetBundle.LoadFromFile(assetBundlePath);

                PotionSprites[PotionIdHover] = LoadBottleSprites("Assets/Sprites/hoverPotion{0}.png");
                PotionSprites[PotionIdEnergy] = LoadBottleSprites("Assets/Sprites/energyPotion{0}.png");
                PotionSprites[PotionIdResonance] = LoadBottleSprites("Assets/Sprites/resonancePotion{0}.png");

                ItemSprites[HoverPotionIndex] = LoadSprite("Assets/Sprites/hoverPotion.png");
                ItemSprites[EnergyPotionIndex] = LoadSprite("Assets/Sprites/energyPotion.png");
                ItemSprites[ResonancePotionIndex] = LoadSprite("Assets/Sprites/resonancePotion.png");
                ItemSprites[AngelTearIndex] = LoadSprite("Assets/Sprites/angelTear.png");
                ItemSprites[TurtleGodshellIndex] = LoadSprite("Assets/Sprites/turtleGodshell.png");
                ItemSprites[TinkerGloveIndex] = LoadSprite("Assets/Sprites/tinkerGlove.png");
                ItemSprites[PhoenixTonicIndex] = LoadSprite("Assets/Sprites/phoenixTonic.png");
                ItemSprites[WarpstoneIndex] = LoadSprite("Assets/Sprites/warpstone.png");
                ItemSprites[MadstoneIndex] = LoadSprite("Assets/Sprites/madstone.png");
                ItemSprites[GuardianCharmIndex] = LoadSprite("Assets/Sprites/guardianCharm.png");
                ItemSprites[ExplosiveFinaleIndex] = LoadSprite("Assets/Sprites/explosiveFinale.png");
                ItemSprites[IdolOfGreedIndex] = LoadSprite("Assets/Sprites/idolOfGreed.png");
                ItemSprites[BombMagnetIndex] = LoadSprite("Assets/Sprites/bombMagnet.png");
                ItemSprites[NinjaGarbIndex] = LoadSprite("Assets/Sprites/ninjaGarb.png");
                ItemSprites[IceCrownIndex] = LoadSprite("Assets/Sprites/iceCrown.png");
                ItemSprites[InvisibilityCloakIndex] = LoadSprite("Assets/Sprites/invisibilityCloak.png");
                ItemSprites[GravityBootsIndex] = LoadSprite("Assets/Sprites/gravityBoots.png");
                ItemSprites[MagicCompassIndex] = LoadSprite("Assets/Sprites/magicCompass.png");
                
                BeginnerDifficultySprite = LoadSprite("Assets/Sprites/beginnerOff.png");
                BeginnerDifficultyOnSprite = LoadSprite("Assets/Sprites/beginnerOn.png");
                NormalDifficultySprite = LoadSprite("Assets/Sprites/normalOff.png");
                NormalDifficultyOnSprite = LoadSprite("Assets/Sprites/normalOn.png");
                InsanityDifficultySprite = LoadSprite("Assets/Sprites/insanityOff.png");
                InsanityDifficultyOnSprite = LoadSprite("Assets/Sprites/insanityOn.png");
                DefeatLockSprite = LoadSprite("Assets/Sprites/gameOver.png");
                ArrowSprite = LoadSprite("Assets/Sprites/arrow.png");

                BombExplosionSfx = new AudioClip[3];
                BombExplosionSfx[0] = LoadAudioClip("Assets/AudioClips/explosionLarge08.ogg");
                BombExplosionSfx[1] = LoadAudioClip("Assets/AudioClips/explosionLarge07.ogg");
                BombExplosionSfx[2] = LoadAudioClip("Assets/AudioClips/explosionMed01.ogg");
                BigExplosionSfx = LoadAudioClip("Assets/AudioClips/explosionLarge05.ogg");
                BeepSfx = LoadAudioClip("Assets/AudioClips/beep.ogg");
                BeepLowSfx = LoadAudioClip("Assets/AudioClips/beepLow.ogg");
                IceSfx = LoadAudioClip("Assets/AudioClips/magicIce03.ogg");
            }
            catch(Exception ex)
            {
                logger.LogError($"PotionSeller: Failed to load asset bundle from '{assetBundlePath}' due to exception: {ex.Message}");
            }

            BuildItemMappings();
            VerticalBoosts = new Dictionary<FPPlayer, bool>(4);
            AngelTearTimers = new Dictionary<FPPlayer, float>(4);
            PheonixTonicFlags = new Dictionary<FPPlayer, bool>(4);
            ExplosiveFinaleFlags = new Dictionary<FPPlayer, bool>(4);
            NinjaGarbTimers = new Dictionary<FPPlayer, float>(4);
            IceCrownDamage = new Dictionary<FPPlayer, float>(4);
            ResonanceStates = new Dictionary<FPPlayer, PotionSellerResonanceState>(4);
            BossHealthState = new Dictionary<FPBaseEnemy, Vector2>(2);
            EnemyBombState = new List<PotionSellerBombState>(100);

            Sprite LoadSprite(string path)
            {
                try
                {
                    Texture2D texture = (Texture2D)bundle.LoadAsset(path);
                    return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f, 1, SpriteMeshType.Tight);
                }
                catch (Exception ex)
                {
                    logger.LogError($"PotionSeller: Failed to load sprite {path} due to exception: {ex.Message}");
                    return null;
                }
            }

            PotionSellerBottleSprites LoadBottleSprites(string path)
            {
                PotionSellerBottleSprites sprites = new PotionSellerBottleSprites();
                sprites.Top = LoadSprite(string.Format(path, "Top"));
                sprites.Middle = LoadSprite(string.Format(path, "Middle"));
                sprites.Bottom = LoadSprite(string.Format(path, "Bottom"));
                sprites.Side = LoadSprite(string.Format(path, "Side"));
                return sprites;
            }

            AudioClip LoadAudioClip(string path)
            {
                try
                {
                    return (AudioClip)bundle.LoadAsset(path);
                }
                catch (Exception ex)
                {
                    logger.LogError($"PotionSeller: Failed to load audio clip {path} due to exception: {ex.Message}");
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the size that potion arrays need to be for this mod.
        /// </summary>
        public static int GetPotionArraySize()
        {
            return PotionEndIndex + 1;
        }

        /// <summary>
        /// Gets the size that item arrays need to be for this mod.
        /// </summary>
        public static int GetItemArraySize()
        {
            return ItemEndIndex + 1;
        }

        #endregion Setup

        #region Item Definition

        #endregion Item Definition

        #region Inventory

        public static int[] NewAmulets = null;
        public static int[] NewPotions = null;
        public static Dictionary<string, PotionSellerShopInventoryItem[]> NPCShops = null;
        public static PotionSellerShopInventoryItem[] ClassicShop = null;
        public static string[] ShopKeys = null;
        public static List<FPPowerup> RemovedFromSale = null;

        /// <summary>
        /// Builds item inventory mappings.
        /// </summary>
        public static void BuildItemMappings()
        {
            NewPotions = new int[]
            {
                HoverPotionIndex,
                EnergyPotionIndex,
                ResonancePotionIndex
            };

            NewAmulets = new int[]
            {
                TurtleGodshellIndex,
                WarpstoneIndex,
                AngelTearIndex,
                MagicCompassIndex,
                PhoenixTonicIndex,
                GuardianCharmIndex,
                NinjaGarbIndex,
                IceCrownIndex,
                TinkerGloveIndex,
                IdolOfGreedIndex,
                GravityBootsIndex,
                ExplosiveFinaleIndex,
                BombMagnetIndex,
                MadstoneIndex,
                InvisibilityCloakIndex
            };

            NPCShops = new Dictionary<string, PotionSellerShopInventoryItem[]>
            {
                ["NPC_Yuni"] = new PotionSellerShopInventoryItem[]
                {
                    new PotionSellerShopInventoryItem(TurtleGodshellIndex, 1),
                    new PotionSellerShopInventoryItem(WarpstoneIndex, 0),
                    new PotionSellerShopInventoryItem(AngelTearIndex, 0),
                    new PotionSellerShopInventoryItem(MagicCompassIndex, 0),
                    new PotionSellerShopInventoryItem((int)FPPowerup.RAINBOW_CHARM, 1)
                },
                ["NPC_Milla"] = new PotionSellerShopInventoryItem[]
                {
                    new PotionSellerShopInventoryItem(PhoenixTonicIndex, 2),
                    new PotionSellerShopInventoryItem(HoverPotionIndex, 2),
                    new PotionSellerShopInventoryItem(EnergyPotionIndex, 2),
                    new PotionSellerShopInventoryItem(ResonancePotionIndex, 2)
                },
                ["NPC_Florin"] = new PotionSellerShopInventoryItem[]
                {
                    new PotionSellerShopInventoryItem(GuardianCharmIndex, 2)
                },
                ["NPC_Chloe"] = new PotionSellerShopInventoryItem[]
                {
                    new PotionSellerShopInventoryItem(NinjaGarbIndex, 2){ Requirement = 3 },
                    new PotionSellerShopInventoryItem(IceCrownIndex, 2){ Requirement = 5 },
                    new PotionSellerShopInventoryItem(TinkerGloveIndex, 2) { Requirement = 11 }
                },
                ["NPC_BlakeAndBalthazar"] = new PotionSellerShopInventoryItem[]
                {
                    new PotionSellerShopInventoryItem(IdolOfGreedIndex, 1),
                    new PotionSellerShopInventoryItem(ExplosiveFinaleIndex, 1),
                    new PotionSellerShopInventoryItem(BombMagnetIndex, 2),
                    new PotionSellerShopInventoryItem(MadstoneIndex, 1)
                }
            };

            RemovedFromSale = new List<FPPowerup>
            {
                FPPowerup.EXTRA_STOCK,
                FPPowerup.STRONG_REVIVALS,
                FPPowerup.MAX_LIFE_UP
            };

            ShopKeys = NPCShops.Keys.ToArray();
            ClassicShop = NPCShops.Values.SelectMany(item => item).Where(item => item.ID != (int)FPPowerup.RAINBOW_CHARM).ToArray();
        }

        /// <summary>
        /// Expands an NPC's shop inventory with new items.
        /// </summary>
        /// <param name="npc">The NPC.</param>
        public static void ExpandNPCShopInventory(FPHubNPC npc)
        {
            if ((npc == null) || (npc.itemsForSale == null) || (npc.musicID.Length <= 0))
                return;

            PotionSellerShopInventoryItem[] newEntries = null;

            for (int i = 0; i < ShopKeys.Length; i++)
                if (npc.name.StartsWith(ShopKeys[i]))
                {
                    newEntries = NPCShops[ShopKeys[i]];
                    break;
                }

            if (newEntries == null)
                return;

            int baseLength = npc.itemsForSale.Length;
            int newLength = newEntries.Length;
            for (int i = 0; i < baseLength; i++)
            {
                newLength += !RemovedFromSale.Contains(npc.itemsForSale[i]) ? 1 : 0;
                if (npc.itemsForSale[i] == (FPPowerup)newEntries[0].ID)
                    return;
            }

            FPPowerup[] itemsForSale = new FPPowerup[newLength];
            FPMusicTrack[] musicID = new FPMusicTrack[newLength];
            int[] starCardRequirements = new int[newLength];
            int[] itemCosts = new int[newLength];
            int index = 0;

            for (int i = 0; i < baseLength; i++)
                if (!RemovedFromSale.Contains(npc.itemsForSale[i]))
                {
                    itemsForSale[index] = npc.itemsForSale[i];
                    starCardRequirements[index] = npc.starCardRequirements[i];
                    itemCosts[index] = (itemsForSale[index] == FPPowerup.RAINBOW_CHARM) ? 1 : npc.itemCosts[i];
                    musicID[index] = npc.musicID[i];
                    index++;
                }

            PotionSellerShopInventoryItem item;
            FPMusicTrack emptyTrack = npc.musicID[0];

            for (int i = 0; i < newEntries.Length; i++)
            {
                item = newEntries[i];
                itemsForSale[index] = (FPPowerup)item.ID;
                starCardRequirements[index] = item.Requirement;
                itemCosts[index] = item.Cost;
                musicID[index] = emptyTrack;
                index++;
            }

            npc.itemsForSale = itemsForSale;
            npc.starCardRequirements = starCardRequirements;
            npc.itemCosts = itemCosts;
            npc.musicID = musicID;
        }

        /// <summary>
        /// Expands the classic shop's inventory with new items.
        /// </summary>
        /// <param name="menu">The classic shop menu.</param>
        public static void ExpandClassicShopInventory(MenuClassic menu)
        {
            PotionSellerShopInventoryItem[] newEntries = ClassicShop;

            int baseLength = menu.itemsForSale.Length;
            int newLength = newEntries.Length;
            for (int i = 0; i < baseLength; i++)
                newLength += !RemovedFromSale.Contains(menu.itemsForSale[i]) ? 1 : 0;
            
            FPPowerup[] itemsForSale = new FPPowerup[newLength];
            int[] starCardRequirements = new int[newLength];
            int[] itemCosts = new int[newLength];
            int index = 0;

            for (int i = 0; i < baseLength; i++)
                if (!RemovedFromSale.Contains(menu.itemsForSale[i]))
                {
                    itemsForSale[index] = menu.itemsForSale[i];
                    starCardRequirements[index] = menu.starCardRequirements[i];
                    itemCosts[index] = (itemsForSale[index] == FPPowerup.RAINBOW_CHARM) ? 1 : menu.itemCosts[i];
                    index++;
                }

            PotionSellerShopInventoryItem item;

            for (int i = 0; i < newEntries.Length; i++)
            {
                item = newEntries[i];
                itemsForSale[index] = (FPPowerup)item.ID;
                starCardRequirements[index] = item.Requirement;
                itemCosts[index] = item.Cost;
                index++;
            }

            menu.itemsForSale = itemsForSale;
            menu.starCardRequirements = starCardRequirements;
            menu.itemCosts = itemCosts;
        }

        /// <summary>
        /// Expands the list of equippable items in the item select menu.
        /// </summary>
        /// <param name="menu">The menu.</param>
        public static void ExpandItemSelectAmulets(MenuItemSelect menu)
        {
            int baseLength = menu.amuletList.Length;
            int newLength = baseLength + NewAmulets.Length;

            FPPowerup[] amuletList = new FPPowerup[newLength];
            for (int i = 0; i < newLength; i++)
                amuletList[i] = (i < baseLength) ? menu.amuletList[i] : (FPPowerup)NewAmulets[i - baseLength];

            bool[] amulets = new bool[newLength];
            for (int i = 0; i < newLength; i++)
                amulets[i] = (i < baseLength) ? menu.amulets[i] : false;

            menu.amuletList = amuletList;
            menu.amulets = amulets;
        }

        /// <summary>
        /// Expands the list of equippable potions in the item select menu.
        /// </summary>
        /// <param name="menu">The menu.</param>
        public static void ExpandItemSelectPotions(MenuItemSelect menu)
        {
            int baseLength = menu.potionList.Length;
            int newLength = baseLength + NewPotions.Length;

            FPPowerup[] potionList = new FPPowerup[newLength];
            for (int i = 0; i < newLength; i++)
                potionList[i] = (i < baseLength) ? menu.potionList[i] : (FPPowerup)NewPotions[i - baseLength];

            bool[] potions = new bool[newLength];
            for (int i = 0; i < newLength; i++)
                potions[i] = (i < baseLength) ? menu.potions[i] : false;

            menu.potionList = potionList;
            menu.potions = potions;
        }

        /// <summary>
        /// Expands the bottle sprite arrays in the item select menu. (It has an extra index in its array that other bottle sprite arrays don't)
        /// </summary>
        /// <param name="bottleSprites">The existing bottle sprite array.</param>
        /// <param name="section">The bottle section being expanded.</param>
        /// <returns>The expanded sprite array.</returns>
        public static Sprite[] ExpandItemSelectBottleSprites(Sprite[] bottleSprites, PotionSellerBottleSection section)
        {
            int baseLength = bottleSprites.Length;
            int newLength = baseLength + NewPotions.Length;

            Sprite blankSprite = bottleSprites[0];
            Sprite[] newSprites = new Sprite[newLength];

            for (int i = 0; i < newLength - 1; i++)
                if (i < baseLength - 1)
                    newSprites[i] = bottleSprites[i];
                else
                    newSprites[i] = PotionSprites.ContainsKey(i - 1) ? PotionSprites[i - 1].GetSection(section) : blankSprite;

            newSprites[newLength - 1] = bottleSprites[baseLength - 1];

            return newSprites;
        }

        /// <summary>
        /// Expands the bottle sprite arrays in a menu
        /// </summary>
        /// <param name="bottleSprites">The existing bottle sprite array.</param>
        /// <param name="section">The bottle section being expanded.</param>
        /// <returns>The expanded sprite array.</returns>
        public static Sprite[] ExpandBottleSprites(Sprite[] bottleSprites, PotionSellerBottleSection section)
        {
            int baseLength = bottleSprites.Length;
            int newLength = baseLength + NewPotions.Length;

            Sprite blankSprite = bottleSprites[0];
            Sprite[] newSprites = new Sprite[newLength];

            for (int i = 0; i < newLength; i++)
                if (i < baseLength)
                    newSprites[i] = bottleSprites[i];
                else
                    newSprites[i] = PotionSprites.ContainsKey(i - 1) ? PotionSprites[i - 1].GetSection(section) : blankSprite;

            return newSprites;
        }

        #endregion Inventory

        #region ItemEffects

        public static bool StageBaseSpeedHasBeenSet = false;
        public static bool InStage = false;
        public static bool InSpecialCutscene = false;
        public static bool InResultsScreen = false;
        public static bool InShmup = false;
        public static GameObject Arrow = null;
        public static Dictionary<FPPlayer, bool> VerticalBoosts = null;
        public static Dictionary<FPPlayer, float> AngelTearTimers = null;
        public static Dictionary<FPPlayer, bool> PheonixTonicFlags = null;
        public static Dictionary<FPPlayer, bool> ExplosiveFinaleFlags = null;
        public static Dictionary<FPPlayer, float> NinjaGarbTimers = null;
        public static Dictionary<FPPlayer, float> IceCrownDamage = null;
        public static Dictionary<FPPlayer, PotionSellerResonanceState> ResonanceStates = null;
        public static Dictionary<FPBaseEnemy, Vector2> BossHealthState = null;
        public static List<PotionSellerBombState> EnemyBombState = null;
        public static List<FPCheckpoint> MagicCompassCheckpoints = null;
        public static int TinkerGloveTicks = 1;

        /// <summary>
        /// Clears all data attached to players and enemies.
        /// </summary>
        public static void ClearCharacterData()
        {
            VerticalBoosts.Clear();
            AngelTearTimers.Clear();
            PheonixTonicFlags.Clear();
            ExplosiveFinaleFlags.Clear();
            NinjaGarbTimers.Clear();
            IceCrownDamage.Clear();
            ResonanceStates.Clear();
            BossHealthState.Clear();
            EnemyBombState.Clear();
            InResultsScreen = false;
        }

        /// <summary>
        /// Gets the game speed multiplayer in stages based on the player's current loadout.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>The speed multiplier.</returns>
        public static float GetStageSpeedMultiplier(FPPlayer player)
        {
            bool slower = player.IsPowerupActive((FPPowerup)WarpstoneIndex);
            bool faster = player.IsPowerupActive((FPPowerup)MadstoneIndex);
            return (slower != faster) ? (slower ? 0.8f : 1.2f) : 1f;
        }

        /// <summary>
        /// Applies all LateUpdate effects on the player
        /// </summary>
        /// <param name="player">The player object.</param>
        public static void ApplyPlayerLateUpdateEffects(FPPlayer player)
        {
            if (player.onGround)
                VerticalBoosts[player] = true;

            if (!player.IsKOdOrRecovering() &&
                (player.health < player.healthMax) &&
                player.IsPowerupActive((FPPowerup)AngelTearIndex) &&
                !player.IsPowerupActive(FPPowerup.ONE_HIT_KO))
            {
                float time = (AngelTearTimers.ContainsKey(player) ? AngelTearTimers[player] : 0f) + FPStage.deltaTime;
                if (time > 300f)
                {
                    time = 0f;
                    player.health = (player.healthMax - player.health > 0.25f) ? (player.health + 0.25f) : player.healthMax;
                    Heart heart = (Heart)FPStage.CreateStageObject(Heart.classID, 50f + UnityEngine.Random.Range(-24f, 24f), -32f + UnityEngine.Random.Range(-24f, 24f));
                    heart.animator.Play("Plus", -1, UnityEngine.Random.Range(0f, 0.6f));
                }

                AngelTearTimers[player] = time;
            }

            ApplyResonancePotionEffect(player);
            UpdateCompass();
        }

        /// <summary>
        /// Reduces a player's healthDamage by half if they have the Angel Tear equipped.
        /// </summary>
        /// <param name="player">The player object.</param>
        public static void ApplyEffectAngelTearDamageReduction(FPPlayer player)
        {
            if (player.IsPowerupActive((FPPowerup)AngelTearIndex))
                player.healthDamage *= 0.5f;
        }

        /// <summary>
        /// If the player has the Turtle Godshell equipped, they recover 0.25 health and get 30% longer guard extension.
        /// </summary>
        /// <param name="player">The player object.</param>
        /// <returns>45.5f if the Turtle Godshell is equipped. Otherwise 35f (the normal guard extension).</returns>
        public static float ApplyTurtleGodshellEffect(FPPlayer player)
        {
            if (player.IsPowerupActive((FPPowerup)TurtleGodshellIndex))
            {
                if ((player.health < player.healthMax) && !player.IsPowerupActive(FPPowerup.ONE_HIT_KO))
                {
                    player.health = (player.healthMax - player.health > 0.25f) ? (player.health + 0.25f) : player.healthMax;
                    Heart heart = (Heart)FPStage.CreateStageObject(Heart.classID, 50f + UnityEngine.Random.Range(-24f, 24f), -32f + UnityEngine.Random.Range(-24f, 24f));
                    heart.animator.Play("Plus", -1, UnityEngine.Random.Range(0f, 0.6f));
                }

                if ((player.characterID != FPCharacterID.LILAC) || !player.input.guardHold)
                    return 45.5f;
            }
            return 35f;
        }

        /// <summary>
        /// Runs through every enemy rigged with a bomb and runs the bomb's update code.
        /// </summary>
        /// <param name="player"></param>
        public static void ApplyBombMagnetEffect(FPPlayer player)
        {
            if (EnemyBombState.Count <= 0)
                return;

            PotionSellerBombState state;
            for (int i = 0; i < EnemyBombState.Count; i++)
            {
                state = EnemyBombState[i];
                if (state.Ignore)
                    continue;

                if ((state.Enemy == null) || (state.Enemy.gameObject == null) || BossHealthState.ContainsKey(state.Enemy))
                {
                    state.Ignore = true;
                    continue;
                }

                if (!state.Enemy.gameObject.activeSelf)
                    continue;

                if (!state.FuseLit)
                {
                    if (FPCollision.CheckOOBB(state.Enemy, state.DetectionHitbox, player, player.hbTouch))
                    {
                        state.FuseLit = true;
                        SetMaterial(3);
                        FPAudio.PlaySfx(BeepSfx);
                    }
                    continue;
                }

                if (state.Countdown <= 0f)
                    continue;
                
                state.Countdown -= FPStage.deltaTime;
                if (state.Countdown <= 0f)
                {
                    SetMaterial(0);
                    FPCamera.stageCamera.screenShake = Mathf.Max(FPCamera.stageCamera.screenShake, 15f);
                    FPAudio.PlaySfx(BombExplosionSfx[UnityEngine.Random.Range(0, 2)]);
                    FPStage.CreateStageObject(BigExplosion.classID, state.Enemy.position.x, state.Enemy.position.y);
                    state.Ignore = true;
                    continue;
                }

                if ((int)state.Countdown / 10 % 2 == state.FlashChange)
                    if (state.FlashChange == 1)
                    {
                        state.FlashChange = 0;
                        SetMaterial(3);
                        FPAudio.PlaySfx(BeepLowSfx);
                    }
                    else
                    {
                        state.FlashChange = 1;
                        SetMaterial(0);
                    }
            }

            void SetMaterial(int material)
            {
                Renderer renderer = state.Enemy.GetComponentInChildren<Renderer>();
                if (renderer != null)
                    renderer.material = FPResources.material[material];
            }
        }

        /// <summary>
        /// Rigs an enemy with a proximity bomb.
        /// </summary>
        /// <param name="enemy">The enemy.</param>
        public static void RigBombOnEnemy(FPBaseEnemy enemy)
        {
            PotionSellerBombState state = new PotionSellerBombState
            {
                Enemy = enemy,
                DetectionHitbox = new FPHitBox { top = 120, bottom = -120, left = -120, right = 120, enabled = true, visible = true }
            };
            if (enemy is KoiCannon)
                state.Countdown = 100f;

            EnemyBombState.Add(state);
        }

        /// <summary>
        /// Performs a short roll if the player has the Ninja Garb equipped.
        /// </summary>
        /// <param name="player">The player.</param>
        public static void ApplyNinjaGarbEffect(FPPlayer player)
        {
            if (!player.IsPowerupActive((FPPowerup)NinjaGarbIndex) || (player.characterID == FPCharacterID.BIKECAROL))
                return;

            float vertical = player.input.up ? 1f : (player.input.down ? -1f : 0f);
            if (vertical > 0f)
                if (VerticalBoosts.ContainsKey(player) && VerticalBoosts[player])
                    VerticalBoosts[player] = false;
                else
                    vertical = 0f;
            
            float horizontal = player.input.right ? 1f : (player.input.left ? -1f : 0f);
            if ((vertical == 0f) && (horizontal == 0f))
                return;

            Vector2 direction = new Vector2(horizontal, vertical).normalized;
            float speed = (vertical < 0) ? 4f : ((vertical == 0f) ? 6f : 8f);
            float dot = Vector2.Dot(player.velocity, direction);
            Vector2 velocity = dot * direction;
            if ((dot < 0f) || (velocity.magnitude < speed))
                velocity = direction * speed;


            NinjaGarbTimers[player] = 24f;
            player.onGround = false;
            player.velocity = velocity;
            player.SetPlayerAnimation("Rolling", 0f, 0f);

            player.attackStats = AttackStats_NinjaGarbRoll;
        }

        /// <summary>
        /// Gets the Ninja Garb roll's attack stats.
        /// </summary>
        public static void AttackStats_NinjaGarbRoll()
        {
            FPPlayer player = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            player.attackPower = 0.5f;
            player.attackHitstun = 0f;
            player.attackEnemyInvTime = 10f;
            player.attackKnockback.x = 0f;
            player.attackKnockback.y = 0f;
            player.attackSfx = 5;
        }

        /// <summary>
        /// Determines whether the player is in the middle of a Ninja Garb roll or not.
        /// </summary>
        /// <param name="player">THe player.</param>
        /// <returns><c>True</c> if the player is still in the middle of the roll. <c>False</c> otherwise.</returns>
        public static bool NinjaRollInProgress(FPPlayer player)
        {
            float timeRemaining = NinjaGarbTimers.ContainsKey(player) ? NinjaGarbTimers[player] : 0f;
            if (timeRemaining <= 0f)
                return false;

            timeRemaining -= FPStage.deltaTime;
            NinjaGarbTimers[player] = timeRemaining;

            return timeRemaining > 0f;
        }

        /// <summary>
        /// Keeps track of damage to the player, and launches shockwaves when it's 3 or higher.
        /// </summary>
        /// <param name="player">The player.</param>
        public static void ApplyIceCrownEffect(FPPlayer player, float damage)
        {
            float totalDamage = (IceCrownDamage.ContainsKey(player) ? IceCrownDamage[player] : 3f) + damage;
            if (totalDamage >= 3f)
            {
                totalDamage -= 3f;

                float radConversion = (float)Math.PI / 180f;
                float playerAngle = radConversion * player.angle;
                float horizontalSpeed, verticalSpeed;
                ProjectileBasic projectileBasic;

                for (int i = 0; i < 8; i++)
                {
                    horizontalSpeed = Mathf.Cos(radConversion * (i * 45)) * 9f;
                    verticalSpeed = Mathf.Sin(radConversion * (i * 45)) * 9f;

                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x, player.position.y);
                    projectileBasic.velocity.x = Mathf.Cos(playerAngle) * horizontalSpeed - Mathf.Sin(playerAngle) * verticalSpeed;
                    projectileBasic.velocity.y = Mathf.Sin(playerAngle) * horizontalSpeed + Mathf.Cos(playerAngle) * verticalSpeed;

                    if (FPCollision.CheckTerrainCircleThroughPlatforms(projectileBasic, 8f, false))
                        projectileBasic.velocity.x *= -2f;

                    projectileBasic.animatorController = FPResources.animator[2];
                    projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                    projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                    projectileBasic.direction = FPDirection.FACING_RIGHT;
                    projectileBasic.angle = player.angle;
                    projectileBasic.explodeType = FPExplodeType.ICEWALL;
                    projectileBasic.parentObject = player;
                    projectileBasic.faction = player.faction;
                    projectileBasic.timeBeforeCollisions = 8f;
                }

                WhiteBurst whiteBurst = (WhiteBurst)FPStage.CreateStageObject(WhiteBurst.classID, player.position.x, player.position.y);
                whiteBurst.SetType(FPExplodeType.WHITEBURST);
                FPAudio.PlaySfx(IceSfx);
            }

            IceCrownDamage[player] = totalDamage;
        }

        /// <summary>
        /// Charges up a shockwave if the player has the Resonance Potion equipped.
        /// </summary>
        /// <param name="player">The player.</param>
        public static void ApplyResonancePotionEffect(FPPlayer player)
        {
            byte amount = player.potions[PotionIdResonance];
            if (amount <= 0)
                return;

            if (!InStage || FPStage.eventIsActive || InSpecialCutscene)
                return;

            if (!ResonanceStates.ContainsKey(player))
                ResonanceStates[player] = new PotionSellerResonanceState();
            PotionSellerResonanceState state = ResonanceStates[player];

            if ((state.GroundTimer < (120f - 12f * amount)) && player.onGround && (Math.Abs(player.groundVel) < 0.5f) && !player.IsKOd())
            {
                state.GroundTimer += FPStage.deltaTime;
                if (Mathf.Repeat(state.GroundTimer, 3f) < 1f)
                {
                    Sparkle sparkle = (Sparkle)FPStage.CreateStageObject(Sparkle.classID, player.transform.position.x + UnityEngine.Random.Range(-24f, 24f), player.transform.position.y + UnityEngine.Random.Range(-32f, 32f));
                    sparkle.spriteRenderer.sprite = null;
                }
            }

            if ((state.GroundTimer >= (120f - 12f * player.potions[PotionIdResonance])) && (player.input.attackPress || player.input.attackHold) && !player.IsKOd())
            {
                state.GroundTimer = 0f;
                state.ShockwaveTimer = 15f;

                float verticalOffset = player.currentAnimation == "CrouchAttack" ? -8f : 8f;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    state.Shockwave = (ProjectileBasic)FPStage.CreateStageObject(
                        ProjectileBasic.classID,
                        player.position.x - Mathf.Cos((float)Math.PI / 180f * player.angle) * 32f + Mathf.Sin((float)Math.PI / 180f * player.angle) * verticalOffset,
                        player.position.y + Mathf.Cos((float)Math.PI / 180f * player.angle) * verticalOffset - Mathf.Sin((float)Math.PI / 180f * player.angle) * 32f);
                    state.Shockwave.velocity.x = Mathf.Cos((float)Math.PI / 180f * player.angle) * -16f;
                    state.Shockwave.velocity.y = Mathf.Sin((float)Math.PI / 180f * player.angle) * -16f;
                }
                else
                {
                    state.Shockwave = (ProjectileBasic)FPStage.CreateStageObject(
                        ProjectileBasic.classID,
                        player.position.x + Mathf.Cos((float)Math.PI / 180f * player.angle) * 32f + Mathf.Sin((float)Math.PI / 180f * player.angle) * verticalOffset,
                        player.position.y + Mathf.Cos((float)Math.PI / 180f * player.angle) * verticalOffset + Mathf.Sin((float)Math.PI / 180f * player.angle) * 32f);
                    state.Shockwave.velocity.x = Mathf.Cos((float)Math.PI / 180f * player.angle) * 16f;
                    state.Shockwave.velocity.y = Mathf.Sin((float)Math.PI / 180f * player.angle) * 16f;
                }
                state.Shockwave.animatorController = FPResources.animator[2];
                state.Shockwave.animator = state.Shockwave.GetComponent<Animator>();
                state.Shockwave.animator.runtimeAnimatorController = state.Shockwave.animatorController;
                state.Shockwave.direction = player.direction;
                state.Shockwave.angle = player.angle;
                state.Shockwave.explodeType = FPExplodeType.NONE;
                state.Shockwave.sfxExplode = null;
                state.Shockwave.parentObject = player;
                state.Shockwave.faction = player.faction;
                state.Shockwave.attackPower = 5f;
                state.Shockwave.destroyOnHit = true;
            }

            if (state.Shockwave != null)
            {
                state.Shockwave.attackPower = 5f;
                state.Shockwave.destroyOnHit = true;
                state.ShockwaveTimer -= FPStage.deltaTime;
                if (state.ShockwaveTimer <= 0f)
                {
                    FPStage.DestroyStageObject(state.Shockwave);
                    state.Shockwave = null;
                }
            }
        }

        /// <summary>
        /// Gets the multiplier applied by the Energy Potion to meter regeneration.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>The multiplier, or 1 if the potion isn't equipped.</returns>
        public static float GetEnergyPotionRegenMultiplier(FPPlayer player)
        {
            byte amount = (player.characterID == FPCharacterID.MILLA) ? (byte)0 : player.potions[PotionIdEnergy];
            return (amount > 0) ? (1f + 0.04f * amount) : 1f;
        }

        /// <summary>
        /// Gets the amount of Cube consumed with the Energy Potion in play.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>The amount of cube consumed.</returns>
        public static float GetEnergyPotionCubeDrain(FPPlayer player)
        {
            byte amount = player.potions[PotionIdEnergy];
            return (amount > 0) ? (10f - 0.6f * amount) : 10f;
        }

        /// <summary>
        /// Updates the state and position of the compass, based MagicCompassCheckpoints and whether the player is in a stage and not in a cutscene. 
        /// </summary>
        public static void UpdateCompass()
        {
            if (Arrow == null)
                return;

            Arrow.SetActive(InStage && !FPStage.eventIsActive && !InSpecialCutscene && (MagicCompassCheckpoints != null) && (MagicCompassCheckpoints.Count > 0));
            if (!Arrow.activeSelf)
                return;

            FPCamera camera = FPCamera.stageCamera;
            Vector2 start = new Vector2((camera.right + camera.left) * 0.5f, (camera.top + camera.bottom) * 0.5f);

            Vector2 position, line;
            FPCheckpoint checkpoint;
            Vector2 target = Vector2.zero;
            Vector2 shortestLine = Vector2.zero;
            float shortestDistance = float.MaxValue;
            FPCheckpointType progression = FPStage.currentStage.checkpointProgression;

            for (int x = 0; x < MagicCompassCheckpoints.Count; x++)
            {
                checkpoint = MagicCompassCheckpoints[x];
                position = checkpoint.transform.position;
                switch (progression)
                {
                    case FPCheckpointType.RIGHT:
                        if (position.x <= FPStage.checkpointPos.x)
                            continue;
                        break;
                    case FPCheckpointType.LEFT:
                        if (position.x >= FPStage.checkpointPos.x)
                            continue;
                        break;
                }

                line = position - start;
                if (line.magnitude < shortestDistance)
                {
                    target = position;
                    shortestDistance = line.magnitude;
                    shortestLine = line;
                }
            }

            Vector2 direction = shortestLine.normalized;
            Vector2 end = BoundArrowToCamera(camera, start, direction);

            if ((end - start).magnitude > shortestLine.magnitude)
                if (shortestLine.magnitude > 120f)
                    end = target;
                else
                {
                    Arrow.SetActive(false);
                    return;
                }

            if ((end - start).magnitude < 60f)
            {
                Arrow.SetActive(false);
                return;
            }

            Arrow.transform.position = new Vector3(end.x, end.y, Arrow.transform.position.z);
            Arrow.transform.rotation = Quaternion.FromToRotation(Vector2.right, direction);
        }

        /// <summary>
        /// Makes sure the arrow doesn't leave a limited rectangular area of the screen.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="start">The arrow's anchor point.</param>
        /// <param name="dir">The direction the arrow will be pointing in.</param>
        /// <returns>The adjusted position of the arrow.</returns>
        public static Vector2 BoundArrowToCamera(FPCamera camera, Vector2 start, Vector2 dir)
        {
            bool found = false;
            Vector2 intersect = Vector2.zero;

            Vector2 end = start + dir * 200f;
            float left = camera.left * 0.95f + camera.right * 0.05f;
            float right = camera.left * 0.05f + camera.right * 0.95f;
            float lower = camera.bottom * 0.92f + camera.top * 0.08f;
            float upper = camera.bottom * 0.23f + camera.top * 0.77f;

            if ((dir.y < -0.4f) || (dir.y > 0.3f))
                intersect = GetIntersectionPoint(start, end, new Vector2(left, (dir.y > 0f) ? upper : lower), new Vector2(right, (dir.y > 0f) ? upper : lower), out found);

            if (!found || (intersect.x < left) || (intersect.x > right))
                intersect = GetIntersectionPoint(start, end, new Vector2((dir.x >= 0f) ? right : left, lower), new Vector2((dir.x >= 0f) ? right : left, upper), out found);

            return found ? intersect : end;

            // Gets the intersection point of two infinite lines, defined by 2 points each.
            Vector2 GetIntersectionPoint(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
            {
                float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
                if (tmp == 0)
                {
                    found = false;
                    return Vector2.zero;
                }

                float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;
                found = true;

                return new Vector2(
                    B1.x + (B2.x - B1.x) * mu,
                    B1.y + (B2.y - B1.y) * mu
                );
            }
        }

        #endregion ItemEffects

        #region Other

        public static int StageDefeats = 1;
        public static int StageDefeatsID = -1;
        
        /// <summary>
        /// Determines whether the player is in a stage that counts defeats or not.
        /// </summary>
        public static bool InStageThatCountsDefeats()
        {
            switch (FPStage.currentStage.stageID)
            {
                default:
                    return true;
                case 8:  // Snowfields
                case 13: // Auditorium
                case 22: // Diamond Point
                case 25: // Refinery Room
                case 29: // Merga
                case 30: // Weapon's Core
                    return false;
            }
        }

        /// <summary>
        /// Runs game-over code.
        /// </summary>
        /// <param name="masterHud">The master HUD object.</param>
        public static void DoGameOver(FPHudMaster masterHud)
        {
            if ((masterHud.targetPlayer.lives <= 0) && (!FPStage.currentStage.finalBossStage || !FPStage.onFinalBoss))
            {
                FPSaveManager.KOs = 0;

                if (InStageThatCountsDefeats())
                {
                    if (StageDefeatsID != FPStage.currentStage.stageID)
                        StageDefeats = 0;
                    StageDefeatsID = FPStage.currentStage.stageID;
                    StageDefeats++;
                }
                else
                    StageDefeats = 0;
                
                if (FPStage.checkpointEnabled)
                {
                    masterHud.targetPlayer.lives = 3;
                    if (masterHud.targetPlayer.powerups.Length > 0)
                    {
                        if (masterHud.targetPlayer.IsPowerupActive(FPPowerup.ONE_HIT_KO))
                            masterHud.targetPlayer.health = 0f;

                        if (masterHud.targetPlayer.IsPowerupActive(FPPowerup.STOCK_DRAIN))
                            masterHud.targetPlayer.lives = 0;
                        else
                        {
                            for (int i = 0; i < masterHud.targetPlayer.powerups.Length; i++)
                            {
                                if (masterHud.targetPlayer.powerups[i] == (FPPowerup)PhoenixTonicIndex) masterHud.targetPlayer.lives++;
                                if (masterHud.targetPlayer.powerups[i] == FPPowerup.EXTRA_STOCK) masterHud.targetPlayer.lives++;
                                if (masterHud.targetPlayer.powerups[i] == FPPowerup.MINUS_STOCK) masterHud.targetPlayer.lives--;
                            }
                        }
                    }

                    if (masterHud.targetPlayer.potions.Length > 0)
                        masterHud.targetPlayer.lives += (byte)(masterHud.targetPlayer.potions[0] / 4);
                }
            }
        }

        /// <summary>
        /// Gets how many life petals need to be added to the revive hud.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>The number of extra petals.</returns>
        public static float GetReviveHealthDelta(FPPlayer player)
        {
            float extraHealth = 0f;

            if (!player.IsPowerupActive((FPPowerup)ExplosiveFinaleIndex))
                extraHealth += (player.IsPowerupActive(FPPowerup.STRONG_REVIVALS) || (FPSaveManager.assistRevive == 1)) ? 1 : 2.5f;

            if (player.IsPowerupActive((FPPowerup)PhoenixTonicIndex) && !PheonixTonicFlags.ContainsKey(player))
                extraHealth += 2;

            return extraHealth;
        }

        #endregion Other
    }

    /// <summary>
    /// Enumerates the sections of the potion bottle.
    /// </summary>
    public enum PotionSellerBottleSection
    {
        Bottom,
        Middle,
        Top
    }

    /// <summary>
    /// Holds the bottle sprites for a single potion.
    /// </summary>
    public class PotionSellerBottleSprites
    {
        /// <summary>
        /// The top bottle sprite.
        /// </summary>
        public Sprite Top = null;

        /// <summary>
        /// The middle bottle sprite.
        /// </summary>
        public Sprite Middle = null;

        /// <summary>
        /// The bottom bottle sprite.
        /// </summary>
        public Sprite Bottom = null;

        /// <summary>
        /// The side bottle sprite.
        /// </summary>
        public Sprite Side = null;

        /// <summary>
        /// Gets the sprite for a specific section of the bottle.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>The sprite.</returns>
        public Sprite GetSection(PotionSellerBottleSection section)
        {
            switch(section)
            {
                case PotionSellerBottleSection.Top: return Top;
                case PotionSellerBottleSection.Middle: return Middle;
                case PotionSellerBottleSection.Bottom: return Bottom;
                default: return Side;
            }
        }
    }

    /// <summary>
    /// Denotes a single item to add to a shop inventory.
    /// </summary>
    public class PotionSellerShopInventoryItem
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PotionSellerShopInventoryItem()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemId">The item's ID.</param>
        /// <param name="cost">The item's cost (in Gems or Crystals, depending on the shop).</param>
        public PotionSellerShopInventoryItem(int itemId, int cost)
        {
            ID = itemId;
            Cost = cost;
        }

        /// <summary>
        /// The item's ID.
        /// </summary>
        public int ID = 0;

        /// <summary>
        /// The item's cost (in Gems or Crystals, depending on the shop).
        /// </summary>
        public int Cost = 0;

        /// <summary>
        /// The star card requirement before this item can be purchased.
        /// </summary>
        public int Requirement = 0;
    }

    /// <summary>
    /// Holds the state for enemies rigged with bombs via the Bomb Magnet.
    /// </summary>
    public class PotionSellerBombState
    {
        /// <summary>
        /// Whether to ignore this entry.
        /// </summary>
        public bool Ignore = false;

        /// <summary>
        /// The enemy.
        /// </summary>
        public FPBaseEnemy Enemy = null;

        /// <summary>
        /// The hitbox used to detect proximity with the player.
        /// </summary>
        public FPHitBox DetectionHitbox;

        /// <summary>
        /// Whether the bomb has started ticking yet.
        /// </summary>
        public bool FuseLit = false;

        /// <summary>
        /// Countdown til detonation.
        /// </summary>
        public float Countdown = 120f;

        /// <summary>
        /// The next flashing change value.
        /// </summary>
        public int FlashChange = 0;
    }

    public class PotionSellerResonanceState
    {
        /// <summary>
        /// How long the player has been standing still on the ground for.
        /// </summary>
        public float GroundTimer = 0f;

        /// <summary>
        /// The current shockwave.
        /// </summary>
        public ProjectileBasic Shockwave = null;

        /// <summary>
        /// Timer until the shockwave's dissapearance.
        /// </summary>
        public float ShockwaveTimer = -1f;
    }
}