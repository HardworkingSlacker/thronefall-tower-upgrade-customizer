using BalanceSuggestions.Utils;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using BalanceSuggestions.Patches;
using BepInEx.Configuration;

namespace BalanceSuggestions;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ConfigEntry<bool> debug;
    private ConfigEntry<PatchLogger.LoggerMethods> loggerMethod;

    private ConfigEntry<int> newAdditionalArrowsToShoot;
    private ConfigEntry<float> newRangeMulti;
    private ConfigEntry<float> newAttackCooldownMulti;
    private ConfigEntry<float> newProjectileSpeedMulti;
    private ConfigEntry<float> newSlowDuration;

    private void configureArcherSpirePatches()
    {
        ArcherSpire_Patch_TowerUpgrades.newAdditionalArrowsToShoot = newAdditionalArrowsToShoot.Value;
        ArcherSpire_Patch_TowerUpgrades.newRangeMulti = newRangeMulti.Value;
        ArcherSpire_Patch_TowerUpgrades.newAttackCooldownMulti = newAttackCooldownMulti.Value;
        ArcherSpire_Patch_TowerUpgrades.newProjectileSpeedMulti = newProjectileSpeedMulti.Value;
        ArcherSpire_Patch_TowerUpgrades.newSlowDuration = newSlowDuration.Value;
    }

    private void loadConfig()
    {
        debug = Config.Bind<bool>("General", "Debug", false, "Enables Logging of Debug Messages.");
        loggerMethod = Config.Bind("General", "LoggerMethod", PatchLogger.LoggerMethods.Bepinex, "What Channel to use for Logging.");

        newAdditionalArrowsToShoot = Config.Bind<int>("Spires.ArchersSpire", "AdditionalArrows", 0, "How many additional Arrows an Archer's Spire grants. (0 to disable)");
        newRangeMulti = Config.Bind<float>("Spires.ArchersSpire", "RangeMultiplier", 0f, "New Range Multiplier for Archer's Spire. (0 to disable)");
        newAttackCooldownMulti = Config.Bind<float>("Spires.ArchersSpire", "AttackCooldownMultiplier", 0f, "New Attack Cooldown Multiplier for Archer's Spire. (0 to disable)");
        newProjectileSpeedMulti = Config.Bind<float>("Spires.ArchersSpire", "ProjectileSpeedMultiplier", 0f, "New Projectile Speed Multiplier for Archer's Spire. (0 to disable)");
        newSlowDuration = Config.Bind<float>("Spires.ArchersSpire", "AttackSlowdownDuration", 0f, "Slowdown Duration in seconds for Archer's Spire. (0 to disable)");
    }

    private void Awake()
    {
        loadConfig();

        configureArcherSpirePatches();

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
