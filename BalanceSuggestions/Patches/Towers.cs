using System;
using System.Collections.Generic;
using thronefall.tower.upgrade.customizer.Utils;
using HarmonyLib;
using UnityEngine;
using static BuildSlot;

namespace thronefall.tower.upgrade.customizer.Towers
{
    [HarmonyPatch(typeof(BuildSlot))]
    class Tower
    {
        public class NewTowerUpgrade {
            public string upgradeName;
            public int newHpChange = 0;
            public int newAdditionalArrowsToShoot = -1;
            public float newDamageMulti = 0f;
            public float newRangeMulti = 0f;
            public float newAttackCooldownMulti = 0f;
            public float newProjectileSpeedMulti = 0f;
            public float newSlowDuration = 0f;

            public NewTowerUpgrade(string upgradeName, int newHpChange = 0, int newAdditionalArrowsToShoot = -1, float newDamageMulti = 0f, float newRangeMulti = 0f, float newAttackCooldownMulti = 0f, float newProjectileSpeedMulti = 0f, float newSlowDuration = 0f) {
                
                this.upgradeName = upgradeName;
                this.newHpChange = newHpChange;
                this.newAdditionalArrowsToShoot = newAdditionalArrowsToShoot;
                this.newDamageMulti = newDamageMulti;
                this.newRangeMulti = newRangeMulti;
                this.newAttackCooldownMulti = newAttackCooldownMulti;
                this.newProjectileSpeedMulti = newProjectileSpeedMulti;
                this.newSlowDuration = newSlowDuration;
            }
        }

        public enum TowerUpgradeNames
        {
            CastleTower,
            SniperTower,
            ShieldTower,
            BunkerTower,
            ArchersSpire,
            BallisticSpire,
            FireSpire,
            HealingSpire,
        }

        public static Dictionary<string, TowerUpgradeNames> ingameNamesToEnumNames = new Dictionary<string, TowerUpgradeNames>();

        public static Dictionary<TowerUpgradeNames, NewTowerUpgrade> TowerUpgradeRegistry = new Dictionary<TowerUpgradeNames, NewTowerUpgrade>();

        public static void ManipulateTowerUpgrade(NewTowerUpgrade newTowerUpgrade, TowerUpgrade oldTowerUpgrade)
        {
            var towerUpgradeTraverse = Traverse.Create(oldTowerUpgrade);
            if (newTowerUpgrade.newAdditionalArrowsToShoot > -1)
            {
                PatchLogger.Log($"\tSetting {newTowerUpgrade.upgradeName} additionalArrowsToShoot to {newTowerUpgrade.newAdditionalArrowsToShoot}");
                towerUpgradeTraverse.Field("additionalArrowsToShoot").SetValue(newTowerUpgrade.newAdditionalArrowsToShoot);
            }
            if (newTowerUpgrade.newDamageMulti > 0)
            {
                PatchLogger.Log($"\tSetting {newTowerUpgrade.upgradeName} damageMulti to {newTowerUpgrade.newDamageMulti}");
                towerUpgradeTraverse.Field("damageMulti").SetValue(newTowerUpgrade.newDamageMulti);
            }
            if (newTowerUpgrade.newRangeMulti > 0)
            {
                PatchLogger.Log($"\tSetting {newTowerUpgrade.upgradeName} rangeMulti to {newTowerUpgrade.newRangeMulti}");
                towerUpgradeTraverse.Field("rangeMulti").SetValue(newTowerUpgrade.newRangeMulti);
            }
            if (newTowerUpgrade.newAttackCooldownMulti > 0)
            {
                PatchLogger.Log($"\tSetting {newTowerUpgrade.upgradeName} attackCooldownMulti to {newTowerUpgrade.newAttackCooldownMulti}");
                towerUpgradeTraverse.Field("attackCooldownMulti").SetValue(newTowerUpgrade.newAttackCooldownMulti);
            }
            if (newTowerUpgrade.newProjectileSpeedMulti > 0)
            {
                PatchLogger.Log($"\tSetting {newTowerUpgrade.upgradeName} projectileSpeedMulti to {newTowerUpgrade.newProjectileSpeedMulti}");
                towerUpgradeTraverse.Field("projectileSpeedMulti").SetValue(newTowerUpgrade.newProjectileSpeedMulti);
            }

            if (newTowerUpgrade.newSlowDuration != 0)
            {
                Func<SlowEffect> newSlowEffect = () =>
                {
                    SlowEffect slowEffect = ScriptableObject.CreateInstance<SlowEffect>();
                    slowEffect.name = $"Modded {newTowerUpgrade.upgradeName} Slowdown";
                    Traverse.Create(slowEffect).Field<float>("slowDuration").Value = newTowerUpgrade.newSlowDuration;
                    return slowEffect;
                };

                Traverse currentReplaceWeapon = towerUpgradeTraverse.Field("replaceWeapon");
                bool replaceWeaponExists = currentReplaceWeapon.GetValue<Weapon>() != null;
                if (replaceWeaponExists) {
                    List<AdditionalWeaponEffectScript> replaceWeaponEffects = currentReplaceWeapon.Field<List<AdditionalWeaponEffectScript>>("additionalWeaponEffects").Value;
                    int slowEffectExistsAlreadyAt = -1;
                    if (replaceWeaponEffects != null)
                    {
                        replaceWeaponEffects.ForEach((effect) => {
                            if (effect.GetType() == typeof(SlowEffect)) slowEffectExistsAlreadyAt = replaceWeaponEffects.IndexOf(effect);
                            PatchLogger.Log($"\tslowEffectExistsAlreadyAt: {slowEffectExistsAlreadyAt}");
                        });

                        if (slowEffectExistsAlreadyAt != -1)
                        {
                            if (newTowerUpgrade.newSlowDuration < 0)
                            {
                                PatchLogger.Log($"\tRemoving SlowEffect from {newTowerUpgrade.upgradeName} found at index {slowEffectExistsAlreadyAt}");
                                replaceWeaponEffects.RemoveAt(slowEffectExistsAlreadyAt);
                            }
                            else
                            {
                                PatchLogger.Log($"\tSetting new Value for SlowEffect of {newTowerUpgrade.upgradeName} found at index {slowEffectExistsAlreadyAt}");
                                Traverse.Create(replaceWeaponEffects[slowEffectExistsAlreadyAt]).Field<float>("slowDuration").Value = newTowerUpgrade.newSlowDuration;
                            }
                        }
                        else if (slowEffectExistsAlreadyAt == -1) {
                            if (newTowerUpgrade.newSlowDuration > 0)
                            {
                                PatchLogger.Log($"\tAdding new SlowEffect to {newTowerUpgrade.upgradeName} because index is at {slowEffectExistsAlreadyAt}");
                                replaceWeaponEffects.Add(newSlowEffect());
                            }
                        }
                    } 
                    else {
                        PatchLogger.Log("\treplaceWeaponEffects was null");
                    }
                }
                else if (newTowerUpgrade.newSlowDuration > 0 )
                {
                    Traverse currentWeapon = towerUpgradeTraverse.Field("autoAttackTower").Field("weapon");
                    PatchLogger.Log($"\treplacementWeapon was null, setting to current Weapon {currentWeapon.GetValue<Weapon>().name}");
                    List<AdditionalWeaponEffectScript> currentWeaponEffects = currentWeapon.Field<List<AdditionalWeaponEffectScript>>("additionalWeaponEffects").Value;
                    int hasSlowEffectAt = -1;
                    for (int i = 0; i < currentWeaponEffects.Count; i++) {
                        if (currentWeaponEffects[i].GetType() == typeof(SlowEffect))
                        {
                            hasSlowEffectAt = i;
                            break;
                        }
                    }
                    if (hasSlowEffectAt == -1){
                        PatchLogger.Log($"\tAdding new SlowEffect to existing Weapon since it has None");
                        currentWeaponEffects.Add(newSlowEffect());
                    }
                    else
                    {
                        PatchLogger.Log($"\tUpdating existing SlowEffect of current Weapon to {newTowerUpgrade.newSlowDuration}");
                        Traverse.Create(currentWeaponEffects[hasSlowEffectAt]).Field<float>("slowDuration").Value = newTowerUpgrade.newSlowDuration;
                    }
                    PatchLogger.Log($"\tSetting existing Weapon {currentWeapon.GetValue<Weapon>().name} as replaceWeapon for {newTowerUpgrade.upgradeName}");
                    towerUpgradeTraverse.Field<Weapon>("replaceWeapon").Value = currentWeapon.GetValue<Weapon>();
                }
            }
        }

        public static TowerUpgrade getTowerUpgrade(GameObject upgrade){

            Component[] archersSpireComponents = upgrade.GetComponents<Component>();
            PatchLogger.Log($"\nTrying to manipulate: {upgrade.name}");
            foreach (Component comp in archersSpireComponents)
            {
                PatchLogger.Log($"\tHas component \"{comp.name}\" satisfies typeof(TowerUpgrade) == {comp.GetType()}: {typeof(TowerUpgrade) == comp.GetType()}");
                if (typeof(TowerUpgrade) == comp.GetType())
                {
                    return (TowerUpgrade)comp;
                }
            }
            throw new UnityException($"Couldn't find TowerUpgrade in {upgrade.GetType()} Upgrade \"{upgrade.name}\"");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BuildSlot.ApplyLocalUpgradeChanges))]
        public static void applyNewTowerUpgrade(UpgradeBranch __0/*, string upgradeName, NewTowerUpgrade newTowerUpgrade*/)
        {
            foreach (var go in __0.objectsToActivate)
            {
                PatchLogger.Log($"Object To Activate \"{go.name}\" of type \"{go.GetType()}\"");
                TowerUpgradeNames upgradeName;
                try
                {
                    upgradeName = ingameNamesToEnumNames[go.name];
                    PatchLogger.Log($"Found associated Enum value {upgradeName}");
                    TowerUpgrade oldTowerUpgrade = getTowerUpgrade(go);

                    NewTowerUpgrade newTowerUpgrade = new NewTowerUpgrade(go.name);
                    if (TowerUpgradeRegistry.TryGetValue(upgradeName, out newTowerUpgrade))
                    {
                        ManipulateTowerUpgrade(newTowerUpgrade, oldTowerUpgrade);

                        PatchLogger.Log($"Changing hpChange of UpgradePath from {__0.hpChange} to {newTowerUpgrade.newHpChange}");
                        __0.hpChange = newTowerUpgrade.newHpChange;
                    }
                }
                catch (KeyNotFoundException) {
                    PatchLogger.Log($"\"{go.name}\" not known in Dictionary \"ingameNamesToEnumNames\"");
                    continue;
                }
            }


        }
            
        
    }

    [HarmonyPatch(typeof(TowerUpgrade))]
    class TowerUpgrade_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnEnable")]
        static void _OnEnable(int ___additionalArrowsToShoot, float ___damageMulti, float ___rangeMulti, float ___attackCooldownMulti, float ___projectileSpeedMulti, Weapon ___replaceWeapon)
        {
            PatchLogger.Log($"\nTower Upgrade was Enabled: ");
            if ((bool)___replaceWeapon)
            {
                PatchLogger.Log($"\tWeapon Replacement: {___replaceWeapon.name}");
                List<AdditionalWeaponEffectScript> additionalEffects = Traverse.Create(___replaceWeapon).Field("additionalWeaponEffects").GetValue<List<AdditionalWeaponEffectScript>>();
                PatchLogger.Log($"\t\tLength Addtional Weapon Effects {additionalEffects.Count}");
                foreach (AdditionalWeaponEffectScript additionalEffect in additionalEffects)
                {
                    PatchLogger.Log($"\t\tScript Name \"{additionalEffect.name}\" and type \"{additionalEffect.GetType()}\"");
                    if (typeof(SlowEffect) == additionalEffect.GetType())
                    {
                        PatchLogger.Log($"\t\t\tSlow Duration: {Traverse.Create(additionalEffect).Field<float>("slowDuration").Value}s");
                    }
                }
            }
            PatchLogger.Log($"\tAdditional Arrows to Shoot: {___additionalArrowsToShoot}");
            PatchLogger.Log($"\tDamage Multiplier: {___damageMulti}");
            PatchLogger.Log($"\tRange Multiplier: {___rangeMulti}");
            PatchLogger.Log($"\tAttack Cooldown Multiplier: {___attackCooldownMulti}");
            PatchLogger.Log($"\tProjectile Speed Multiplier: {___projectileSpeedMulti}");
        }
    }
}