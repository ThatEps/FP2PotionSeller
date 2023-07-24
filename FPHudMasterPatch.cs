namespace PotionSeller
{
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;

    /// <summary>
    /// Bundles all of the patches to FPHudMaster.
    /// </summary>
    class FPHudMasterPatch
    {
        public static GameObject DefeatLockObject = null;

        /// <summary>
        /// Applies all changes to FPHudMaster.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(FPHudMasterPatch_Start));
            harmony.PatchAll(typeof(FPHudMasterPatch_LateUpdate));
        }
    }

    /// <summary>
    /// Applies changes to the Start method.
    /// </summary>
    [HarmonyPatch(typeof(FPHudMaster), "Start")]
    class FPHudMasterPatch_Start
    {
        /// <summary>
        /// Increases base max health to 7 and applies the Idol of Greed health reduction.
        /// Also adds the defeat lock sprite.
        /// </summary>
        static void Prefix(FPHudMaster __instance)
        {
            FPPlayer player = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if ((player != null) && !player.IsPowerupActive((FPPowerup)PotionSellerUtils.IdolOfGreedIndex))
            {
                player.healthMax++;
                player.health++;
            }

            if (PotionSellerUtils.InStageThatCountsDefeats())
            {
                FPHudMasterPatch.DefeatLockObject = new GameObject("DefeatLock");
                FPHudMasterPatch.DefeatLockObject.layer = 5;
                FPHudMasterPatch.DefeatLockObject.transform.position = new Vector3(1000, -380, -4);
                FPHudMasterPatch.DefeatLockObject.transform.parent = __instance.transform;
                FPHudMasterPatch.DefeatLockObject.SetActive(false);
                SpriteRenderer renderer = FPHudMasterPatch.DefeatLockObject.AddComponent<SpriteRenderer>();
                renderer.sprite = PotionSellerUtils.DefeatLockSprite;
            }
            else
                FPHudMasterPatch.DefeatLockObject = null;
        }

        /// <summary>
        /// Resets PotionSellerUtils.StageBaseSpeedHasBeenSet and sets up the game-over sprite.
        /// </summary>
        static void Postfix(FPHudDigit ___gameOverText)
        {
            PotionSellerUtils.StageBaseSpeedHasBeenSet = false;

            if (PotionSellerUtils.InStageThatCountsDefeats())
            {
                Vector3 position = ___gameOverText.transform.position;
                position.y = position.y + 40;
                ___gameOverText.transform.position = position;
            }

            if (___gameOverText.digitFrames.Length > 1)
                ___gameOverText.digitFrames[1] = ___gameOverText.digitFrames[0];
        }
    }

    /// <summary>
    /// Applies changes to the LateUpdate method.
    /// </summary>
    [HarmonyPatch(typeof(FPHudMaster), "LateUpdate")]
    class FPHudMasterPatch_LateUpdate
    {
        /// <summary>
        /// Determines the value of PotionSellerUtils.InStage on each frame.
        /// Sets the stage's base speed on the first frame it's called.
        /// Animates the defeat lock text.
        /// </summary>
        static void Postfix(FPHudMaster __instance, ref bool ___isDead, ref float ___gameOverTimer)
        {
            PotionSellerUtils.InStage = !__instance.onlyShowHealth || (FPStage.currentStage.stageID == 30);

            if (!PotionSellerUtils.StageBaseSpeedHasBeenSet)
            {
                PotionSellerUtils.InResultsScreen = false;
                PotionSellerUtils.StageBaseSpeedHasBeenSet = true;
                FPStage.SetGameSpeed(1f);
            }

            Vector3 position;
            if ((__instance.targetPlayer != null) && (FPHudMasterPatch.DefeatLockObject != null))
            {
                if (___isDead)
                {
                    if (FPStage.currentStage.showRetryMenu && (__instance.targetPlayer.genericTimer < 250f) && (__instance.targetPlayer.recoveryTimer >= 150f))
                        FPHudMasterPatch.DefeatLockObject.transform.parent = null;

                    if (((__instance.targetPlayer.recoveryTimer < 150f) && (__instance.targetPlayer.genericTimer >= -1f) && (__instance.targetPlayer.lives <= 0)) ||
                        (__instance.targetPlayer.state == new FPObjectState(__instance.targetPlayer.State_Defeat)))
                    {
                        if (__instance.targetPlayer.recoveryTimer <= -9990f)
                            return;

                        FPHudMasterPatch.DefeatLockObject.SetActive(true);
                        position = FPHudMasterPatch.DefeatLockObject.transform.position;
                        position.x = (position.x * 9f + 320f) * 0.1f;
                        FPHudMasterPatch.DefeatLockObject.transform.position = position;
                    }
                }

                if ((!___isDead || (__instance.targetPlayer.lives > 0)) && FPHudMasterPatch.DefeatLockObject.activeSelf && !__instance.enableNoDeathFailures)
                {
                    position = FPHudMasterPatch.DefeatLockObject.transform.position;
                    position.x = Mathf.Min(position.x + (float)((___gameOverTimer < 60f) ? 0f : 16f) * FPStage.deltaTime, 1000f);
                    FPHudMasterPatch.DefeatLockObject.transform.position = position;
                    if (position.x >= 1000f)
                        FPHudMasterPatch.DefeatLockObject.SetActive(false);
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            AddGameOverCode(codes);
            AddReviveHudCode(codes);
            return codes.AsEnumerable();
        }

        /// <summary>
        /// Changes the line 'if (targetPlayer.lives <= 0 && (!FPStage.currentStage.finalBossStage || !FPStage.onFinalBoss))' to 'PotionSellerUtils.DoGameOver(this); if(false)'.
        /// </summary>
        private static void AddGameOverCode(List<CodeInstruction> codes)
        {
            /*
            The IL we're looking to modify is as follows:
                ldarg.0
                ldfld     class FPPlayer FPHudMaster::targetPlayer
                ldfld     uint8 FPPlayer::lives
                ldc.i4.0
                bgt       IL_1CF7
                
                ldsfld    class FPStage FPStage::currentStage
                ldfld     bool FPStage::finalBossStage
                brfalse   IL_1CCB
                
                ldsfld    bool FPStage::onFinalBoss
                brtrue    IL_1CF7

            First we look for the sequence: 'ldfld targetPlayer', 'ldfld lives', 'ldc.i4.0', 'bgt', 'ldsfld currentStage', 'ldfld finalBossStage', 'brfalse', 'ldsfld onFinalBoss', 'brtrue'.
            Then we remove everything from that sequence except the 'brtrue' command, which we will turn into a 'br' instead.
            In front of that 'br' we insert:
                call      void PotionSellerUtils::DoGameOver(FPHudMaster)
             */

            int replaceStartIndex = -1;

            for (int i = 0; i < codes.Count - 8; i++)
                if ((codes[i].opcode == OpCodes.Ldfld) && (codes[i].operand as FieldInfo != null) && (((FieldInfo)codes[i].operand).Name == "targetPlayer") &&
                    (codes[i + 1].opcode == OpCodes.Ldfld) && (codes[i + 1].operand as FieldInfo != null) && (((FieldInfo)codes[i + 1].operand).Name == "lives") &&
                    (codes[i + 2].opcode == OpCodes.Ldc_I4_0) &&
                    (codes[i + 3].opcode == OpCodes.Bgt) &&
                    (codes[i + 4].opcode == OpCodes.Ldsfld) && (codes[i + 4].operand as FieldInfo != null) && (((FieldInfo)codes[i + 4].operand).Name == "currentStage") &&
                    (codes[i + 5].opcode == OpCodes.Ldfld) && (codes[i + 5].operand as FieldInfo != null) && (((FieldInfo)codes[i + 5].operand).Name == "finalBossStage") &&
                    (codes[i + 6].opcode == OpCodes.Brfalse) &&
                    (codes[i + 7].opcode == OpCodes.Ldsfld) && (codes[i + 7].operand as FieldInfo != null) && (((FieldInfo)codes[i + 7].operand).Name == "onFinalBoss") &&
                    (codes[i + 8].opcode == OpCodes.Brtrue))
                {
                    replaceStartIndex = i;
                    codes[i + 8].opcode = OpCodes.Br;
                    break;
                }

            if (replaceStartIndex > -1)
            {
                codes.RemoveRange(replaceStartIndex, 8);
                codes.Insert(replaceStartIndex, new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("DoGameOver")));
            }
        }

        /// <summary>
        /// Adds 'a+= PotionSellerUtils.GetReviveHealthDelta(targetPlayer);' before the line 'a = Mathf.Min(a, targetPlayer.healthMax);'
        /// </summary>
        private static void AddReviveHudCode(List<CodeInstruction> codes)
        {
            /*
            The IL we're looking to modify is as follows:
                ldloc.s   V_56
                ldarg.0
                ldfld     class FPPlayer FPHudMaster::targetPlayer
                ldfld     float32 FPPlayer::healthMax
                call      float32 [UnityEngine]UnityEngine.Mathf::Min(float32, float32)

            First we look for the sequence: 'ldloc.s', 'ldarg.0', 'ldfld targetPlayer', 'ldfld healthMax', 'call Min'.
            Then after the 'ldarg.0' we insert:
                ldarg.0
                ldfld     class FPPlayer FPHudMaster::targetPlayer
                call      float32 PotionSellerUtils.GetReviveHealthDelta(FPPlayer)
                add
             */

            int insertIndex = -1;

            for (int i = 0; i < codes.Count - 4; i++)
                if ((codes[i].opcode == OpCodes.Ldloc_S) &&
                    (codes[i + 1].opcode == OpCodes.Ldarg_0) &&
                    (codes[i + 2].opcode == OpCodes.Ldfld) && (codes[i + 2].operand as FieldInfo != null) && (((FieldInfo)codes[i + 2].operand).Name == "targetPlayer") &&
                    (codes[i + 3].opcode == OpCodes.Ldfld) && (codes[i + 3].operand as FieldInfo != null) && (((FieldInfo)codes[i + 3].operand).Name == "healthMax") &&
                    (codes[i + 4].opcode == OpCodes.Call) && (codes[i + 4].operand as MethodInfo != null) && (((MethodInfo)codes[i + 4].operand).Name == "Min"))
                {
                    insertIndex = i + 1;
                    break;
                }

            if (insertIndex > -1)
            {
                codes.InsertRange(insertIndex, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, typeof(FPHudMaster).GetField("targetPlayer")),
                    new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("GetReviveHealthDelta")),
                    new CodeInstruction(OpCodes.Add),
                });
            }
        }
    }
}