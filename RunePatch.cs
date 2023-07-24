namespace PotionSeller
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Disables the sprite renderer if the player has the Invisibility Cloak equipped.
    /// </summary>
    [HarmonyPatch(typeof(Rune), "Start")]
    class RunePatch
    {
        static void Postfix(Rune __instance)
        {
            if ((__instance.parentObject as FPPlayer != null) && ((FPPlayer)__instance.parentObject).IsPowerupActive((FPPowerup)PotionSellerUtils.InvisibilityCloakIndex))
                __instance.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
