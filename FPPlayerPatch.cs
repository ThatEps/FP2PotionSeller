namespace PotionSeller
{
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;

    /// <summary>
    /// Bundles all of the patches to FPPlayer.
    /// </summary>
    static class FPPlayerPatch
    {
        /// <summary>
        /// Applies all changes to FPPlayer.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(FPPlayerPatch_Start));
            harmony.PatchAll(typeof(FPPlayerPatch_Update));
            harmony.PatchAll(typeof(FPPlayerPatch_LateUpdate));
            harmony.PatchAll(typeof(FPPlayerPatch_ProcessInput));
            harmony.PatchAll(typeof(FPPlayerPatch_ApplyGravityForce));
            harmony.PatchAll(typeof(FPPlayerPatch_GetAttackModifier));
            harmony.PatchAll(typeof(FPPlayerPatch_Revive));
            harmony.PatchAll(typeof(FPPlayerPatch_Action_Carol_RemoveBike));
            harmony.PatchAll(typeof(FPPlayerPatch_Action_Hurt));
            harmony.PatchAll(typeof(FPPlayerPatch_Action_ShieldHurt));
            harmony.PatchAll(typeof(FPPlayerPatch_Action_ShadowGuard));
            harmony.PatchAll(typeof(FPPlayerPatch_State_Guard));
            harmony.PatchAll(typeof(FPPlayerPatch_State_KO));
            harmony.PatchAll(typeof(FPPlayerPatch_State_Carol_JumpDiscWarp));
        }
    }

    /// <summary>
    /// Adds exra item code at the end of the Start method.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), "Start")]
    class FPPlayerPatch_Start
    {
        static void Postfix(FPPlayer __instance)
        {
            PotionSellerUtils.MagicCompassCheckpoints = null;

            if (__instance.powerups.Length > 0)
                for (int k = 0; k < __instance.powerups.Length; k++)
                {
                    if (__instance.powerups[k] == (FPPowerup)PotionSellerUtils.PhoenixTonicIndex)
                    {
                        if (!__instance.IsPowerupActive(FPPowerup.STOCK_DRAIN) && !FPStage.checkpointEnabled)
                            __instance.lives++;
                        continue;
                    }

                    if (__instance.powerups[k] == (FPPowerup)PotionSellerUtils.ExplosiveFinaleIndex)
                    {
                        if (PotionSellerUtils.BigExplosionSfx != null)
                            __instance.sfxKO = PotionSellerUtils.BigExplosionSfx;
                        continue;
                    }

                    if (__instance.powerups[k] == (FPPowerup)PotionSellerUtils.InvisibilityCloakIndex)
                    {
                        __instance.GetComponent<SpriteRenderer>().enabled = false;
                        if (__instance.childSprite)
                            __instance.childSprite.GetComponent<SpriteRenderer>().enabled = false;
                        continue;
                    }

                    if (__instance.powerups[k] == (FPPowerup)PotionSellerUtils.MagicCompassIndex)
                    {
                        PotionSellerUtils.MagicCompassCheckpoints = UnityEngine.Object.FindObjectsOfType<FPCheckpoint>().ToList();

                        PotionSellerUtils.Arrow = new GameObject("Arrow");
                        PotionSellerUtils.Arrow.layer = 8;

                        SpriteRenderer renderer = PotionSellerUtils.Arrow.gameObject.AddComponent<SpriteRenderer>();
                        renderer.name = "ArrowRenderer";
                        renderer.sprite = PotionSellerUtils.ArrowSprite;
                        renderer.sortingOrder = 8;
                        continue;
                    }
                }
        }
    }

    /// <summary>
    /// Alters the Update method to apply the meter regeneration increase from the Energy Potion.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), "Update")]
    class FPPlayerPatch_Update
    {
        /// <summary>
        /// Further multiplies 'energy += energyRecoverRateCurrent * FPStage.deltaTime' by 'PotionSellerUtils.GetEnergyPotionRegenMultiplier(this);' within
        /// the 'if (energyRecoverRateCurrent > 0f &amp;&amp; energy < 100f)' block.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <returns>The modified instructions.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            The IL we're looking to modify is as follows:
                ldarg.0
                ldfld     float32 FPPlayer::energyRecoverRateCurrent
                ldc.r4    0.0
                ble.un    IL_026F

                ldarg.0
                ldfld     float32 FPPlayer::energy
                ldc.r4    100
                bge.un    IL_026F

                ldarg.0
                dup
                ldfld     float32 FPPlayer::energy
                ldarg.0
                ldfld     float32 FPPlayer::energyRecoverRateCurrent
                ldsfld    float32 FPStage::deltaTime
                mul
                add
                stfld     float32 FPPlayer::energy

            'energy < 100f' only appears once in the method, so we don't care about the rest of the 'if' condition.
            First we find the exact sequence of 'ldfld energy', 'ldc.r4 100', 'bge.un'.
            Then we find the first 'add' after that that is immediately followed by 'stfld energy'.
            Right before that 'add', we insert the following:

                ldarg.0
                call      void PotionSellerUtils::GetEnergyPotionRegenMultiplier(FPPlayer)
                mul
             */

            bool bgeUnFound = false;
            int insertIndex = -1;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (bgeUnFound &&
                    (codes[i].opcode == OpCodes.Add) &&
                    (codes[i + 1].opcode == OpCodes.Stfld) && (codes[i + 1].operand as FieldInfo != null) && (((FieldInfo)codes[i + 1].operand).Name == "energy"))
                {
                    insertIndex = i;
                    break;
                }
                
                if (!bgeUnFound &&
                    (codes[i].opcode == OpCodes.Ldfld) && (codes[i].operand as FieldInfo != null) && (((FieldInfo)codes[i].operand).Name == "energy") &&
                    (codes[i + 1].opcode == OpCodes.Ldc_R4) && ((float)codes[i + 1].operand == 100f) &&
                    (codes[i + 2].opcode == OpCodes.Bge_Un))
                    bgeUnFound = true;
            }

            if (insertIndex > -1)
                codes.InsertRange(insertIndex, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("GetEnergyPotionRegenMultiplier")),
                    new CodeInstruction(OpCodes.Mul),
                });

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Alters the LateUpdate method to apply health regeneration from the Angel Tear.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), "LateUpdate")]
    class FPPlayerPatch_LateUpdate
    {
        /// <summary>
        /// Prepends 'PotionSellerUtils.ApplyPlayerLateUpdateEffects(this);' right before the line 'if (IsPowerupActive(FPPowerup.BIPOLAR_LIFE) &amp;&amp; !IsKOd())';
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <returns>The modified instructions.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            The IL we're looking to modify is as follows:
                ldarg.0
                ldc.i4.s  40
                call      instance bool FPPlayer::IsPowerupActive(valuetype FPPowerup)

            All we need to do is find the "call" command with and operand of "IsPowerupActive", directly preceded by a "ldc.i4.s" command with an operand equal to the
            int8 value of FPPowerup.BIPOLAR_LIFE (which is 40). Right before the "ldc.i4.s" index, we insert the following:

                call      void PotionSellerUtils::ApplyPlayerLateUpdateEffects(FPPlayer)
                ldarg.0
             */

            int insertIndex = -1;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 1; i < codes.Count; i++)
                if ((codes[i].opcode == OpCodes.Call) && (codes[i].operand as MethodInfo != null) && (((MethodInfo)codes[i].operand).Name == "IsPowerupActive") &&
                    (codes[i - 1].opcode == OpCodes.Ldc_I4_S) && ((sbyte)codes[i - 1].operand == (sbyte)FPPowerup.BIPOLAR_LIFE))
                {
                    insertIndex = i - 1;
                    break;
                }
                
            if (insertIndex > -1)
                codes.InsertRange(insertIndex, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("ApplyPlayerLateUpdateEffects")),
                    new CodeInstruction(OpCodes.Ldarg_0),
                });

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Sets input.down to <c>true</c> if the player is on the ground and has the Gravity Boots equipped.
    /// </summary>
    [HarmonyPatch]
    class FPPlayerPatch_ProcessInput
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(FPPlayer).GetMethod("ProcessInputControl", BindingFlags.NonPublic | BindingFlags.Instance);
            yield return typeof(FPPlayer).GetMethod("ProcessRewired", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static void Postfix(FPPlayer __instance)
        {
            if (__instance.onGround && __instance.IsPowerupActive((FPPowerup)PotionSellerUtils.GravityBootsIndex))
                __instance.input.down = true;
        }
    }

    /// <summary>
    /// Adds the Hover potion effect.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), "ApplyGravityForce")]
    class FPPlayerPatch_ApplyGravityForce
    {
        static void Postfix(FPPlayer __instance)
        {
            byte hoverAmount = __instance.potions[PotionSellerUtils.PotionIdHover];
            if (hoverAmount > 0)
            {
                float fallLimit = -24f * (1 - 0.1f * hoverAmount);
                if (__instance.velocity.y < fallLimit)
                    __instance.velocity.y = fallLimit;
            }
        }
    }

    /// <summary>
    /// Adds the extra effect for the PAyback Ring at exactly 1 stock left.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.GetAttackModifier))]
    class FPPlayerPatch_GetAttackModifier
    {
        static void Postfix(FPPlayer __instance, ref float __result)
        {
            if ((__instance.lives == 1) && __instance.IsPowerupActive(FPPowerup.PAYBACK_RING))
                __result += 0.15f;
        }
    }

    /// <summary>
    /// Increases the base health received from reviving to 3 (or 5 with the assist option), and adds the Pheonix Tonic and Explosive Finale effects.
    /// typeof(bool) means this patch will only affect the overload that has the noVaRevive parameter.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), "Revive", typeof(bool))]
    class FPPlayerPatch_Revive
    {
        static void Postfix(FPPlayer __instance, FPPlayerKOStatus ___koStatus)
        {
            if (___koStatus != FPPlayerKOStatus.RECOVERING)
                return;

            if (PotionSellerUtils.ExplosiveFinaleFlags.ContainsKey(__instance))
                PotionSellerUtils.ExplosiveFinaleFlags.Remove(__instance);

            if (!__instance.IsPowerupActive(FPPowerup.ONE_HIT_KO))
            {
                float extraHealth = 0f;
                if (!__instance.IsPowerupActive((FPPowerup)PotionSellerUtils.ExplosiveFinaleIndex))
                    extraHealth += (__instance.IsPowerupActive(FPPowerup.STRONG_REVIVALS) || (FPSaveManager.assistRevive == 1)) ? 1 : 2.5f;

                if (__instance.IsPowerupActive((FPPowerup)PotionSellerUtils.PhoenixTonicIndex) && !PotionSellerUtils.PheonixTonicFlags.ContainsKey(__instance))
                {
                    extraHealth += 2;
                    PotionSellerUtils.PheonixTonicFlags[__instance] = true;
                }
                __instance.health = Mathf.Min(__instance.health + extraHealth, __instance.healthMax);
            }
        }
    }

    /// <summary>
    /// Disables Carol's child sprite's (tail's) renderer when she dismounts the bike with the Invisibility Cloak equipped.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_Carol_RemoveBike))]
    class FPPlayerPatch_Action_Carol_RemoveBike
    {
        static void Prefix(FPPlayer __instance, out FPCharacterID __state)
        {
            __state = __instance.characterID;
        }

        static void Postfix(FPPlayer __instance, FPCharacterID __state)
        {
            if (__instance.hasSwapCharacter && (__state == FPCharacterID.BIKECAROL) && (__instance.characterID != FPCharacterID.BIKECAROL) &&
                (__instance.childSprite != null) && __instance.IsPowerupActive((FPPowerup)PotionSellerUtils.InvisibilityCloakIndex))
                __instance.childSprite.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    /// <summary>
    /// Alters the Action_Hurt method to take the new items into account.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_Hurt))]
    class FPPlayerPatch_Action_Hurt
    {
        /// <summary>
        /// Stores the state for the postfix.
        /// </summary>
        static void Prefix(FPPlayer __instance, out float __state)
        {
            __state = __instance.health;
        }

        /// <summary>
        /// Counts damage towards the Ice Cronw if it's equipped.
        /// </summary>
        static void Postfix(FPPlayer __instance, float __state)
        {
            if ((__instance.health < __state) && __instance.IsPowerupActive((FPPowerup)PotionSellerUtils.IceCrownIndex))
                PotionSellerUtils.ApplyIceCrownEffect(__instance, __state - __instance.health);
        }

        /// <summary>
        /// Adds the Angel Tear and Turtle Godshell effects.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            AddAngelTearEffect(codes);
            AddTurtleGodshellEffect(codes);
            return codes.AsEnumerable();
        }

        /// <summary>
        /// Replaces the constant '35f' in 'guardTime = Mathf.Max(guardTime, 35f);' with a call to 'PotionSellerUtils.ApplyTurtleGodshellEffect(this);'
        /// </summary>
        /// <param name="codes">The instruction codes.</param>
        public static void AddTurtleGodshellEffect(List<CodeInstruction> codes)
        {
            /*
            The IL we're looking to modify is as follows:
                ldarg.0
	            ldc.i4.1
	            stfld     bool FPPlayer::guardEffectFlag
	            ldarg.0
	            ldarg.0
	            ldfld     float32 FPPlayer::guardTime
	            ldc.r4    35
	            call      float32 [UnityEngine]UnityEngine.Mathf::Max(float32, float32)

            The command "stfld guardEffectFlag" only appears once in the method.
            After we find it, we search for the first "ldc.r4 35" command and replace it with the following:
            
                ldarg.0
                call      void PotionSellerUtils::ApplyTurtleGodshellEffect(FPPlayer)
            */

            int replaceIndex = -1;
            bool foundStfld = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (foundStfld && (codes[i].opcode == OpCodes.Ldc_R4) && ((float)codes[i].operand == 35f))
                    { replaceIndex = i; break; }

                if (!foundStfld && (codes[i].opcode == OpCodes.Stfld) && (codes[i].operand as FieldInfo != null) && (((FieldInfo)codes[i].operand).Name == "guardEffectFlag"))
                    foundStfld = true;
            }

            if (replaceIndex > -1)
            {
                codes.RemoveAt(replaceIndex);
                codes.InsertRange(replaceIndex, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("ApplyTurtleGodshellEffect")),
                });
            }
        }

        /// <summary>
        /// Prepends 'PotionSellerUtils.ApplyEffectAngelTearDamageReduction(this);' right before the line 'health -= healthDamage;';
        /// </summary>
        /// <param name="codes">The instruction codes.</param>
        private static void AddAngelTearEffect(List<CodeInstruction> codes)
        {
            /*
            The IL we're looking to modify is as follows:
                ldarg.0
                dup
                ldfld     float32 FPPlayer::health
                ldarg.0
                ldfld     float32 FPPlayer::healthDamage
                sub
                stfld     float32 FPPlayer::health

            First we find the "stfld" command with the operand "health", which is directly preceded by a "sub" command.
            Then we reverse the search oand go backwards up to 5 commands. (For safety)
            We take note of when we find the first "ldarg.0" this way.
            Then we stop at the second "ldarg.0", or a "dup" that is directly preceded by "ldarg.0".
            At that index, we insert the following:

                call      void PotionSellerUtils::ApplyEffectAngelTearDamageReduction(FPPlayer)
                ldarg.0
            */

            int insertIndex = -1;
            int storeIndex = -1;
            bool firstLdarg = false;

            // Search for stfld FPPlayer::health which is preceded by a sub
            for (int i = 1; i < codes.Count; i++)
                if ((codes[i].opcode == OpCodes.Stfld) && (codes[i].operand as FieldInfo != null) && (((FieldInfo)codes[i].operand).Name == "health") &&
                    (codes[i - 1].opcode == OpCodes.Sub))
                { storeIndex = i; break; }

            // Search backwards for the insertion point
            if (storeIndex > -1)
                for (int i = storeIndex - 1; i >= (storeIndex - 5) && (i >= 1); i--)
                {
                    if (firstLdarg && (codes[i].opcode == OpCodes.Dup) && (codes[i - 1].opcode == OpCodes.Ldarg_0))
                    { insertIndex = i; break; }

                    if (codes[i].opcode == OpCodes.Ldarg_0)
                        if (firstLdarg)
                        { insertIndex = i; break; }
                        else
                            firstLdarg = true;
                }

            // Insert the call to PotionSellerUtils.ApplyEffectAngelTearDamageReduction
            if (insertIndex > -1)
                codes.InsertRange(insertIndex, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("ApplyEffectAngelTearDamageReduction")),
                    new CodeInstruction(OpCodes.Ldarg_0),
                });
        }
    }

    /// <summary>
    /// Alters the Action_ShieldHurt method to take the new items into account.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_ShieldHurt))]
    class FPPlayerPatch_Action_ShieldHurt
    {
        /// <summary>
        /// Stores the state for the postfix.
        /// </summary>
        static void Prefix(FPPlayer __instance, out Vector2 __state)
        {
            __state = new Vector2(__instance.shieldHealth, __instance.damageType);
        }
        
        /// <summary>
        /// Counts damage towards the Ice Crown if it's equipped.
        /// Changes the element of the shield to the element of the damage it took if the Guardian Charm is equipped.
        /// </summary>
        static void Postfix(FPPlayer __instance, ref Vector2 __state)
        {
            if (__instance.shieldHealth >= (int)__state.x)
                return;
            
            if (__instance.IsPowerupActive((FPPowerup)PotionSellerUtils.IceCrownIndex))
                PotionSellerUtils.ApplyIceCrownEffect(__instance, (int)__state.x - __instance.shieldHealth);

            if ((__instance.shieldHealth > 0) && ((int)__state.y > -1) && ((int)__state.y <= 4) && __instance.IsPowerupActive((FPPowerup)PotionSellerUtils.GuardianCharmIndex))
            {
                __instance.shieldID = (byte)__state.y;
                ShieldOrb shieldOrb = (ShieldOrb)FPStage.CreateStageObject(ShieldOrb.classID, __instance.position.x, __instance.position.y + 60f);
                shieldOrb.spawnLocation = __instance;
                shieldOrb.parentObject = __instance;
                switch (__instance.shieldID)
                {
                    case 0: shieldOrb.animator.Play("Wood", 0, 0f); break;
                    case 1: shieldOrb.animator.Play("Earth", 0, 0f); break;
                    case 2: shieldOrb.animator.Play("Water", 0, 0f); break;
                    case 3: shieldOrb.animator.Play("Fire", 0, 0f); break;
                    case 4: shieldOrb.animator.Play("Metal", 0, 0f); break;
                }
            }
        }

        /// <summary>
        /// Adds the Turtle Godshell effect for guarding with a shield equipped.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FPPlayerPatch_Action_Hurt.AddTurtleGodshellEffect(codes);
            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Applies the Ninja Garb effect at the end of the Shadow Guard check.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_ShadowGuard))]
    class FPPlayerPatch_Action_ShadowGuard
    {
        static void Postfix(FPPlayer __instance)
        {
            PotionSellerUtils.ApplyNinjaGarbEffect(__instance);
        }
    }

    /// <summary>
    /// Adds 'if (!PotionSellerUtils.NinjaRollInProgress(this))' in front of the line that start with 'if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f || (cancellableGuard...'.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_Guard))]
    class FPPlayerPatch_State_Guard
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            The IL we're looking to modify is as follows:
                ldarg.0
                ldfld     class [UnityEngine]UnityEngine.Animator FPPlayer::animator
                ldc.i4.0
                callvirt  instance valuetype [UnityEngine]UnityEngine.AnimatorStateInfo [UnityEngine]UnityEngine.Animator::GetCurrentAnimatorStateInfo(int32)
                stloc.0
                ldloca.s  V_0
                call      instance float32 [UnityEngine]UnityEngine.AnimatorStateInfo::get_normalizedTime()
                ldc.r4    1
                bge       IL_0163

                ldarg.0
                ldfld     bool FPPlayer::cancellableGuard
                brfalse   IL_028E

            First we find the 'callvirt' command to 'GetCurrentAnimatorStateInfo'
            Then we find the first 'ldfld cancellableGuard' command after it that's immediately followed by a 'brfalse' command.
            From this command we take the label passed to it as an argument.
            Finally, we go back to the 'callvirt' command and start moving backwards, until we find 'ldfld animator'.
            At that position, we add the following:

                call      bool PotionSellerUtils::NinjaRollInProgress(FPPlayer)
                brtrue    brLabel  
                ldarg.0
            */

            int insertIndex = -1;
            int callvirtIndex = -1;
            object brOperand = null;

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if ((callvirtIndex > -1) && (codes[i].opcode == OpCodes.Ldfld) && (codes[i].operand as FieldInfo != null) && (((FieldInfo)codes[i].operand).Name == "cancellableGuard") &&
                    (codes[i + 1].opcode == OpCodes.Brfalse))
                {
                    brOperand = codes[i + 1].operand;
                    break;
                }

                if ((callvirtIndex < 0) && (codes[i].opcode == OpCodes.Callvirt) && (codes[i].operand as MethodInfo != null) && (((MethodInfo)codes[i].operand).Name == "GetCurrentAnimatorStateInfo"))
                    callvirtIndex = i;
             }

            if ((callvirtIndex >= 0) && (brOperand != null))
            {
                for (int i = callvirtIndex; i > callvirtIndex - 4; i--)
                {
                    if ((codes[i].opcode == OpCodes.Ldfld) && (codes[i].operand as FieldInfo != null) && (((FieldInfo)codes[i].operand).Name == "animator"))
                    {
                        insertIndex = i;
                        break;
                    }
                }

                if (insertIndex > -1)
                {
                    codes.InsertRange(insertIndex, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("NinjaRollInProgress")),
                        new CodeInstruction(OpCodes.Brtrue, brOperand),
                        new CodeInstruction(OpCodes.Ldarg_0),
                    });
                }
            }

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Adds the Explosibve Finale effect on KO.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_KO))]
    class FPPlayerPatch_State_KO
    {
        static void Postfix(FPPlayer __instance, FPPlayerKOStatus ___koStatus)
        {
            if ((___koStatus != FPPlayerKOStatus.RECOVERING) &&
                (__instance.genericTimer <= 20f) &&
                __instance.IsPowerupActive((FPPowerup)PotionSellerUtils.ExplosiveFinaleIndex) &&
                !PotionSellerUtils.ExplosiveFinaleFlags.ContainsKey(__instance))
            {
                PotionSellerUtils.ExplosiveFinaleFlags[__instance] = true;
                FPStage.CreateStageObject(BigExplosion.classID, __instance.position.x, __instance.position.y);
            }
        }
    }

    /// <summary>
    /// Disables Carol's child sprite's (tail's) renderer when she warps to the Jump Disk with the bike and Invisibility Cloak equipped.
    /// </summary>
    [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_Carol_JumpDiscWarp))]
    class FPPlayerPatch_State_Carol_JumpDiscWarp
    {
        static void Prefix(bool ___useSpecialItem, out bool __state)
        {
            __state = ___useSpecialItem;
        }

        static void Postfix(FPPlayer __instance, bool ___useSpecialItem, bool __state)
        {
            if (__state && !___useSpecialItem && __instance.hasSwapCharacter && (__instance.genericTimer <= 0f) &&
                (__instance.childSprite != null) && __instance.IsPowerupActive((FPPowerup)PotionSellerUtils.InvisibilityCloakIndex))
                __instance.childSprite.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
