namespace PotionSeller
{
    using BepInEx;
    using HarmonyLib;
    using System;
    using System.IO;
    using System.Reflection;

    [BepInPlugin("com.eps.plugin.fp2.potion-seller", "PotionSeller", "1.0.2")]
    [BepInProcess("FP2.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Version = "1.0.2";

        private void Awake()
        {
            string path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            PotionSellerUtils.Init(Path.Combine(path, "PotionSeller.assets"), Logger);

            Harmony harmony = new Harmony("com.eps.plugin.fp2.potion-seller");
            FPSaveManagerPatch.Apply(harmony);
            FPPlayerPatch.Apply(harmony);
            FPStagePatch.Apply(harmony);
            FPHudMasterPatch.Apply(harmony);
            FPPauseMenuPatch.Apply(harmony);
            FPResultsMenuPatch.Apply(harmony);
            GOAuditoriumCutscenePatch.Apply(harmony);
            BSKalawCutscenePatch.Apply(harmony);
            EnemyPatch.Apply(harmony);
            MenuDifficultyPatch.Apply(harmony);

            harmony.PatchAll(typeof(MillaMasterCubePatch));
            harmony.PatchAll(typeof(MillaShieldPatch));
            harmony.PatchAll(typeof(RunePatch));
            harmony.PatchAll(typeof(GuardFlashPatch));
            harmony.PatchAll(typeof(PlayerShadowPatch));
            harmony.PatchAll(typeof(ItemChestPatch));
            harmony.PatchAll(typeof(FPBossHudPatch));
            harmony.PatchAll(typeof(FPHudDigitPatch));
            harmony.PatchAll(typeof(FPHubNPCPatch));
            harmony.PatchAll(typeof(FPCheckpointPatch));
            harmony.PatchAll(typeof(MenuClassicPatch));
            harmony.PatchAll(typeof(MenuClassicShopHubPatch));
            harmony.PatchAll(typeof(MenuItemSelectPatch));
            harmony.PatchAll(typeof(MenuItemSetPatch));
            harmony.PatchAll(typeof(MenuFilePatch));
            harmony.PatchAll(typeof(MenuWorldMapPatch));
            harmony.PatchAll(typeof(MenuWorldMapConfirmPatch));
            harmony.PatchAll(typeof(BF5BossIntroPatch));
            harmony.PatchAll(typeof(FPSchmupManagerPatch));
            harmony.PatchAll(typeof(MenuMainPatch));
        }
    }
}
