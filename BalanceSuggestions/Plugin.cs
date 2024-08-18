using BalanceSuggestions.Utils;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using BalanceSuggestions.Towers;
using System;

namespace BalanceSuggestions;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ConfigEntry<bool> debug;
    private ConfigEntry<PatchLogger.LoggerMethods> loggerMethod;

    private void loadTowerUpgradeConfig()
    {

        foreach (Tower.TowerUpgradeNames towerUpgradeName in (Tower.TowerUpgradeNames[]) Enum.GetValues(typeof(Tower.TowerUpgradeNames)))
        {
            string upgradeIngameName = null;
            string towerNote = $"Note: Slowdown Effects added to this upgrade only carry over to the Archers Spire due to ingame logic currently!";
            switch (towerUpgradeName)
            {
                case Tower.TowerUpgradeNames.CastleTower:
                    upgradeIngameName = "Castle Tower";
                    break;
                case Tower.TowerUpgradeNames.SniperTower:
                    upgradeIngameName = "Sniper Tower";
                    break;
                case Tower.TowerUpgradeNames.ShieldTower:
                    upgradeIngameName = "Armored Tower";
                    break;
                case Tower.TowerUpgradeNames.BunkerTower:
                    upgradeIngameName = "Bunker Tower";
                    break;
                case Tower.TowerUpgradeNames.ArchersSpire:
                    upgradeIngameName = "Archers Spire";
                    towerNote = "";
                    break;
                case Tower.TowerUpgradeNames.BallisticSpire:
                    upgradeIngameName = "Ballistic Spire";
                    towerNote = "";
                    break;
                case Tower.TowerUpgradeNames.FireSpire:
                    upgradeIngameName = "Fire Spire";
                    towerNote = "";
                    break;
                case Tower.TowerUpgradeNames.HealingSpire:
                    upgradeIngameName = "Healing Spire";
                    towerNote = "";
                    break;
            }
            if (upgradeIngameName != null){
                Tower.ingameNamesToEnumNames.Add(upgradeIngameName, towerUpgradeName);
                string section = $"Tower.Upgrade.{towerUpgradeName}";
                Tower.NewTowerUpgrade newTowerUpgrade = new Tower.NewTowerUpgrade(
                    Config.Bind<string>(section, "UpgradeName", upgradeIngameName, "Ingame Name of the Upgrade (Changing it will cause unexpected Behavior!)").Value,
                    Config.Bind<int>(section, "AdditionalArrows", 0, $"How many additional Arrows {upgradeIngameName} grants. (0 to disable)").Value,
                    Config.Bind<float>(section, "DamageMultiplier", 0f, $"New Damage Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "RangeMultiplier", 0f, $"New Range Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "AttackCooldownMultiplier", 0f, $"New Attack Cooldown Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "ProjectileSpeedMultiplier", 0f, $"New Projectile Speed Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "AttackSlowdownDuration", 0f, $"Slowdown Duration in seconds for {upgradeIngameName}. (0 to disable, -1 to remove existing)\n{towerNote}").Value
                );
                Tower.TowerUpgradeRegistry.Add(towerUpgradeName, newTowerUpgrade);
            }
        }
    }

    private void loadGeneralConfig()
    {
        debug = Config.Bind<bool>("General", "Debug", false, "Enables Logging of Debug Messages.");
        loggerMethod = Config.Bind("General", "LoggerMethod", PatchLogger.LoggerMethods.Bepinex, "What Channel to use for Logging.");
    }

    private void Awake()
    {
        loadGeneralConfig();

        loadTowerUpgradeConfig();

        // Plugin startup logic
        Logger = base.Logger;
        PatchLogger.IsDebugEnabled = debug.Value;
        PatchLogger.BepinexLogger = Logger;
        PatchLogger.loggerMethod = loggerMethod.Value;
        Harmony harmony = new Harmony("balsug.patch");
        harmony.PatchAll();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
