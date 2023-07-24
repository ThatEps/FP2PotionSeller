namespace PotionSeller
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Bundles all of the patches to FPHudMaster.
    /// </summary>
    class FPResultsMenuPatch
    {
        /// <summary>
        /// Applies all changes to FPHudMaster.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(FPResultsMenuPatch_Start));
            harmony.PatchAll(typeof(FPResultsMenuPatch_Update));
            harmony.PatchAll(typeof(FPResultsMenuPatch_SetBonuses));
        }
    }

    /// <summary>
    /// Adds changes to the Start method.
    /// </summary>
    [HarmonyPatch(typeof(FPResultsMenu), "Start")]
    class FPResultsMenuPatch_Start
    {
        /// <summary>
        /// Adds the text mesh for the number of defeats, and registers it for animations.
        /// </summary>
        static void Prefix(FPResultsMenu __instance)
        {
            GameObject defeatsObject = Object.Instantiate(__instance.bonusTotalText.gameObject);
            defeatsObject.transform.SetParent(__instance.transform, false);
            defeatsObject.transform.localPosition = new Vector3(127, -92, -4);
            TextMesh defeats = defeatsObject.GetComponent<TextMesh>();
            defeats.anchor = TextAnchor.UpperRight;

            GameObject[] newObjectList = new GameObject[__instance.objectList.Length + 1];
            for (int i = 0; i < __instance.objectList.Length; i++)
                newObjectList[i] = __instance.objectList[i];
            newObjectList[newObjectList.Length - 1] = defeatsObject;
            __instance.objectList = newObjectList;
        }

        /// <summary>
        /// Registers that the player is in the results screen.
        /// </summary>
        static void Postfix()
        {
            PotionSellerUtils.InResultsScreen = true;
            FPStage.SetGameSpeed(1f);
        }
    }

    /// <summary>
    /// Triggers the animation of the defeat text mesh.
    /// </summary>
    [HarmonyPatch(typeof(FPResultsMenu), "Update")]
    class FPResultsMenuPatch_Update
    {
        static void Postfix(FPResultsMenu __instance, ref int ___animationStep, ref float ___genericTimer, ref bool[] ___isActive)
        {
            if ((FPStage.state != FPStageState.STATE_PAUSED) && (___animationStep == 1) && (___genericTimer > 120f))
                ___isActive[__instance.objectList.Length - 1] = true;
        }
    }

    /// <summary>
    /// Displays the number of defeats and sets the total crystal bonus to 1% if the player was defeated. Then it resets the defeat count.
    /// </summary>
    [HarmonyPatch(typeof(FPResultsMenu), "SetBonuses")]
    class FPResultsMenuPatch_SetBonuses
    {
        static void Postfix(FPResultsMenu __instance, ref float ___bonusMultiplier, ref int ___itemBonus, ref byte ___rank)
        {
            if (PotionSellerUtils.StageDefeatsID != FPStage.currentStage.stageID)
                PotionSellerUtils.StageDefeats = 0;

            TextMesh defeats = __instance.objectList[__instance.objectList.Length - 1].GetComponent<TextMesh>();
            defeats.text = $"DEFEATS: {PotionSellerUtils.StageDefeats}";

            if (PotionSellerUtils.StageDefeats > 0)
            {
                defeats.color = new Color(1f, 0.3f, 0.3f);
                __instance.bonusTotalText.color = defeats.color;
                ___bonusMultiplier = 0.01f;
                ___itemBonus = (int)Mathf.Floor(FPCommon.RoundToQuantumWithinErrorThreshold(__instance.stageHud.targetPlayer.totalCrystals * 0.01f, 1f));
                ___rank = 1;
            }

            PotionSellerUtils.StageDefeatsID = -1;
            PotionSellerUtils.StageDefeats = 0;
        }
    }
}
