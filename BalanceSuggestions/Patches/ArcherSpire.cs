using System.Collections.Generic;
using BalanceSuggestions.Utils;
using HarmonyLib;
using UnityEngine;

namespace BalanceSuggestions.Patches
{
    /*[HarmonyPatch(typeof(AutoAttackTower))]
    class ArcherSpire_Patch_AutoAttackTower_FindAutoAttackTargets_SetSimultaneousProjectilesForArchersSpire
    {
        [HarmonyPrefix]
        [HarmonyPatch("FindAutoAttackTargets")]
        static void _FindAutoAttackTargets (AutoAttackTower __instance)
        {
            PatchLogger.Log($"Simultaneous Projectiles: {__instance.simultaneousProjectiles}");
        }
    }*/

    [HarmonyPatch(typeof(TowerUpgrade))]
    class ArcherSpire_Patch_TowerUpgrade
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnEnable")]
        static void _OnEnable(int ___additionalArrowsToShoot, float ___rangeMulti, float ___attackCooldownMulti, float ___projectileSpeedMulti, Weapon ___replaceWeapon)
        {
            PatchLogger.Log($"\nTower Upgrade was Enabled: ");
            if ((bool)___replaceWeapon) { 
                PatchLogger.Log($"\tWeapon Replacement: {___replaceWeapon.name}");
                List<AdditionalWeaponEffectScript> additionalEffects = Traverse.Create(___replaceWeapon).Field("additionalWeaponEffects").GetValue<List<AdditionalWeaponEffectScript>>();
                PatchLogger.Log($"\t\tLength Addtional Weapon Effects {additionalEffects.Count}");
                foreach (AdditionalWeaponEffectScript additionalEffect in additionalEffects) {
                    PatchLogger.Log($"\t\tScript Name \"{additionalEffect.name}\" and type \"{additionalEffect.GetType()}\"");
                    if (typeof(SlowEffect) == additionalEffect.GetType())
                    {
                        PatchLogger.Log($"\t\t\tSlow Duration: {Traverse.Create(additionalEffect).Field<float>("slowDuration").Value}s");
                    }
                }
            }
            PatchLogger.Log($"\tAdditional Arrows to Shoot: {___additionalArrowsToShoot}");
            PatchLogger.Log($"\tRange Multiplier: {___rangeMulti}");
            PatchLogger.Log($"\tAttack Cooldown Multiplier: {___attackCooldownMulti}");
            PatchLogger.Log($"\tProjectile Speed Multiplier: {___projectileSpeedMulti}");
        }
    }

    [HarmonyPatch(typeof(BuildSlot))]
    class ArcherSpire_Patch_TowerUpgrades
    {
        public static int newAdditionalArrowsToShoot = 0;
        public static float newRangeMulti = 0f;
        public static float newAttackCooldownMulti = 0f;
        public static float newProjectileSpeedMulti = 0f;
        public static float newSlowDuration = 0f;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BuildSlot.OnUpgradeChoiceComplete))]
        static void _OnUpgradeChoiceComplete(Choice __0, BuildSlot __instance)
        {
            PatchLogger.Log($"Choice name: \"{__0.name}\"");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BuildSlot.ApplyLocalUpgradeChanges))]
        static void _ApplyLocalUpgradeChanges(BuildSlot.UpgradeBranch __0)
        {
            __0.objectsToActivate.ForEach((go) =>
            {
                PatchLogger.Log($"Object To Activate \"{go.name}\" of type \"{go.GetType()}\"");
                if (go.name == "Archers Spire")
                {
                    Component[] archersSpireComponents = go.GetComponents<Component>();
                    PatchLogger.Log($"\nTrying to manipulate: {go.name}");
                    foreach (Component comp in archersSpireComponents)
                    {
                        PatchLogger.Log($"\tHas component \"{comp.name}\" satisfies typeof(TowerUpgrade) == ${comp.GetType()}: {typeof(TowerUpgrade) == comp.GetType()}");
                        if (typeof(TowerUpgrade) == comp.GetType())
                        {
                            
                            var towerUpgradeTraverse = Traverse.Create(comp);
                            if (newAdditionalArrowsToShoot > 0) { 
                                PatchLogger.Log($"\tSetting Archer's Spire additionalArrowsToShoot to {newAdditionalArrowsToShoot}"); 
                                towerUpgradeTraverse.Field("additionalArrowsToShoot").SetValue(newAdditionalArrowsToShoot);
                            }
                            if (newRangeMulti > 0) { 
                                PatchLogger.Log($"\tSetting Archer's Spire rangeMulti to {newRangeMulti}");
                                towerUpgradeTraverse.Field("rangeMulti").SetValue(newRangeMulti);
                            }
                            if (newAttackCooldownMulti > 0) {
                                PatchLogger.Log($"\tSetting Archer's Spire attackCooldownMulti to {newAttackCooldownMulti}");
                                towerUpgradeTraverse.Field("attackCooldownMulti").SetValue(newAttackCooldownMulti);
                            }
                            if (newProjectileSpeedMulti > 0) {
                                PatchLogger.Log($"\tSetting Archer's Spire projectileSpeedMulti to {newProjectileSpeedMulti}");
                                towerUpgradeTraverse.Field("projectileSpeedMulti").SetValue(newProjectileSpeedMulti);
                            }

                            if (newSlowDuration > 0)
                            {
                                PatchLogger.Log($"\tAdding new SlowEffect to Archer's Spire: {newSlowDuration}");
                                Traverse currentWeapon = towerUpgradeTraverse.Field("autoAttackTower").Field("weapon");
                                List<AdditionalWeaponEffectScript> currentWeaponEffects = currentWeapon.Field<List<AdditionalWeaponEffectScript>>("additionalWeaponEffects").Value;
                                SlowEffect archerSpireSlowEffect = ScriptableObject.CreateInstance<SlowEffect>();
                                archerSpireSlowEffect.name = "Modded Archer's Spire Slowdown";
                                Traverse.Create(archerSpireSlowEffect).Field<float>("slowDuration").Value = newSlowDuration;
                                currentWeaponEffects.Add(archerSpireSlowEffect);
                                towerUpgradeTraverse.Field<Weapon>("replaceWeapon").Value = currentWeapon.GetValue<Weapon>();
                            }
                        }
                    }
                }
            });
        }
    }
}