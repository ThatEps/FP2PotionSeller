namespace PotionSeller
{
    using HarmonyLib;
    using System;
    using UnityEngine;

    /// <summary>
    /// Adds code that registers targetBoss as a boss in PotionSellerUtils.
    /// </summary>
    [HarmonyPatch(typeof(FPBossHud), "LateUpdate")]
    class FPBossHudPatch
    {
        static void Postfix(FPBossHud __instance)
        {
            FPBaseEnemy boss = __instance.targetBoss;
            if (boss != null)
                ApplyIdolOfGreedEffect(boss);
        }

        public static void ApplyIdolOfGreedEffect(FPBaseEnemy boss)
        {
            Vector2 state = PotionSellerUtils.BossHealthState.ContainsKey(boss) ? PotionSellerUtils.BossHealthState[boss] : new Vector2(boss.health, 0f);
            float healthBefore = state.x;
            float damageTotal = state.y;
            if (healthBefore <= 0f)
                return;

            if (boss.health < healthBefore)
            {
                damageTotal += healthBefore - boss.health;
                int amount;

                if (PotionSellerUtils.InShmup)
                {
                    amount = Mathf.FloorToInt(damageTotal / 3f);
                    damageTotal = damageTotal % 3f;
                }
                else
                {
                    amount = Mathf.FloorToInt(damageTotal / 15f) * 5;
                    damageTotal = damageTotal % 15f;
                }

                if (amount > 0)
                {
                    float conversion = (float)Math.PI / 180f;
                    FPPlayer player = FPStage.currentStage.GetPlayerInstance_FPPlayer();

                    if ((player != null) && player.IsPowerupActive((FPPowerup)PotionSellerUtils.IdolOfGreedIndex))
                        for (float angle = 22.5f; angle <= 157.5f; angle += 135f / (amount - 1))
                        {
                            ItemCrystal itemCrystal = (ItemCrystal)FPStage.CreateStageObject(
                                ItemCrystal.classID,
                                boss.position.x + Mathf.Cos((boss.transform.eulerAngles.z + angle) * conversion) * 6f,
                                boss.position.y + Mathf.Sin((boss.transform.eulerAngles.z + angle) * conversion) * 6f);
                            itemCrystal.state = PotionSellerUtils.InShmup ? itemCrystal.State_ReleasedInAir : itemCrystal.State_Released;
                            itemCrystal.velocity.x = Mathf.Cos((boss.transform.eulerAngles.z + angle) * conversion) * 10f;
                            itemCrystal.velocity.y = Mathf.Sin((boss.transform.eulerAngles.z + angle) * conversion) * 10f;
                        }
                }
            }

            PotionSellerUtils.BossHealthState[boss] = new Vector2(boss.health, damageTotal);
        }
    }
}
