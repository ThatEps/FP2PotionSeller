namespace PotionSeller
{
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    [HarmonyPatch(typeof(MillaShield), "Update")]
    class MillaShieldPatch
    {
        /// <summary>
        /// Replaces the '10f' in 'parentObject.millaCubeEnergy -= 10f;' with 'PotionSellerUtils.GetEnergyPotionCubeDrain(this);' inside of
        /// the 'if (parentObject.IsPowerupActive(FPPowerup.LONG_SPECIALS))' block.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <returns>The modified instructions.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            The IL we're looking to modify is as follows:
                ldarg.0
                ldfld     class FPPlayer MillaShield::parentObject
                ldc.i4.s  37
                callvirt  instance bool FPPlayer::IsPowerupActive(valuetype FPPowerup)
                brfalse   IL_02D6

                ...

                ldarg.0
                ldfld     class FPPlayer MillaShield::parentObject
                dup
                ldfld     float32 FPPlayer::millaCubeEnergy
                ldc.r4    10
                sub
                stfld     float32 FPPlayer::millaCubeEnergy

            First we find the 'callvirt IsPowerupActive' command that's directly after a 'ldc.i4.s 37' command.
            Then we find the first 'ldc.r4 10' command after it, and replace it with the following:

                ldarg.0
                ldfld     class FPPlayer MillaShield::parentObject
                call      void PotionSellerUtils::GetEnergyPotionCubeDrain(FPPlayer)
             */

            bool callVirtFound = false;
            int replaceIndex = -1;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 1; i < codes.Count; i++)
            {
                if (callVirtFound && (codes[i].opcode == OpCodes.Ldc_R4) && ((float)codes[i].operand == 10f))
                {
                    replaceIndex = i;
                    break;
                }

                if (!callVirtFound &&
                    (codes[i].opcode == OpCodes.Callvirt) && (codes[i].operand as MethodInfo != null) && (((MethodInfo)codes[i].operand).Name == "IsPowerupActive") &&
                    (codes[i - 1].opcode == OpCodes.Ldc_I4_S) && ((sbyte)codes[i - 1].operand == (sbyte)37))
                    callVirtFound = true;
            }

            if (replaceIndex > -1)
            {
                codes.RemoveAt(replaceIndex);
                codes.InsertRange(replaceIndex, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, typeof(MillaShield).GetField("parentObject")),
                    new CodeInstruction(OpCodes.Call, typeof(PotionSellerUtils).GetMethod("GetEnergyPotionCubeDrain")),
                });
            }

            return codes.AsEnumerable();
        }
    }
}
