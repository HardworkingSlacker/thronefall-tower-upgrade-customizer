using thronefall.tower.upgrade.customizer.Utils;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using thronefall.tower.upgrade.customizer.Towers;
using System;

namespace thronefall.tower.upgrade.customizer;

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
            uint hpChange = 0;
            string towerNote = $"\nNote: Slowdown Effects added to this upgrade only carry over to the Archers Spire due to ingame logic currently!";
            switch (towerUpgradeName)
            {
                case Tower.TowerUpgradeNames.CastleTower:
                    upgradeIngameName = "Castle Tower";
                    hpChange = 225;
                    break;
                case Tower.TowerUpgradeNames.SniperTower:
                    hpChange = 0;
                    upgradeIngameName = "Sniper Tower";
                    break;
                case Tower.TowerUpgradeNames.ShieldTower:
                    hpChange = 525;
                    upgradeIngameName = "Armored Tower";
                    break;
                case Tower.TowerUpgradeNames.BunkerTower:
                    hpChange = 112;
                    upgradeIngameName = "Bunker Tower";
                    break;
                case Tower.TowerUpgradeNames.ArchersSpire:
                    hpChange = 525;
                    upgradeIngameName = "Archers Spire";
                    towerNote = "";
                    break;
                case Tower.TowerUpgradeNames.BallisticSpire:
                    hpChange = 0;
                    upgradeIngameName = "Ballistic Spire";
                    towerNote = "";
                    break;
                case Tower.TowerUpgradeNames.FireSpire:
                    hpChange = 375;
                    upgradeIngameName = "Fire Spire";
                    towerNote = "\nNote: Slowdown unfortunately doesn't work with splash damage yet.";
                    break;
                case Tower.TowerUpgradeNames.HealingSpire:
                    hpChange = 1125;
                    upgradeIngameName = "Healing Spire";
                    towerNote = "";
                    break;
            }
            if (upgradeIngameName != null){
                Tower.ingameNamesToEnumNames.Add(upgradeIngameName, towerUpgradeName);
                string section = $"Tower.Upgrade.{towerUpgradeName}";
                Tower.NewTowerUpgrade newTowerUpgrade = new Tower.NewTowerUpgrade(
                    Config.Bind<string>(section, "UpgradeName", upgradeIngameName, "Ingame Name of the Upgrade (Changing it will cause unexpected Behavior!)").Value,
                    Config.Bind<uint>(section, "HpChange", hpChange, $"How much additional HP {upgradeIngameName} grants. (negative values are not possible)\nNote: for some ingame reason the nominal hp change values from tooltip are multiplied by 1.5x!").Value,
                    Config.Bind<int>(section, "AdditionalArrows", -1, $"How many additional Arrows {upgradeIngameName} grants. (-1 to disable)").Value,
                    Config.Bind<float>(section, "DamageMultiplier", 0f, $"New Damage Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "RangeMultiplier", 0f, $"New Range Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "AttackCooldownMultiplier", 0f, $"New Attack Cooldown Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "ProjectileSpeedMultiplier", 0f, $"New Projectile Speed Multiplier for {upgradeIngameName}. (0 to disable)").Value,
                    Config.Bind<float>(section, "AttackSlowdownDuration", 0f, $"Slowdown Duration in seconds for {upgradeIngameName}. (0 to disable, -1 to remove existing){towerNote}").Value
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
