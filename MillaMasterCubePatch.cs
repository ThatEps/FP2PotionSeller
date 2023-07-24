namespace PotionSeller
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Disables the sprite renderer if the player has the Invisibility Cloak equipped.
    /// </summary>
    [HarmonyPatch(typeof(MillaMasterCube), "ObjectCreated")]
    class MillaMasterCubePatch
    {
        static void Prefix(MillaMasterCube __instance)
        {
            FPPlayer player = (__instance.parentObject != null) ? __instance.parentObject : FPStage.currentStage?.GetPlayerInstance_FPPlayer();

            if ((player != null) && player.IsPowerupActive((FPPowerup)PotionSellerUtils.InvisibilityCloakIndex))
                __instance.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
