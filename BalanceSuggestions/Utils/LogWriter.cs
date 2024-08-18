
using HarmonyLib;
using BepInEx.Logging;

namespace thronefall.tower.upgrade.customizer.Utils
{
    internal class PatchLogger
    {
        public static bool IsDebugEnabled { get; set; }

        public enum LoggerMethods
        {
            File,
            Bepinex,
        }

        public static LoggerMethods loggerMethod = LoggerMethods.File;

        public static ManualLogSource BepinexLogger;

        public static void Log(string message)
        {
            if (IsDebugEnabled)
            {
                switch (loggerMethod){
                    case LoggerMethods.File:
                        FileLog.Log(message);
                        break;
                    case LoggerMethods.Bepinex:
                        BepinexLogger.LogDebug(message);
                        break;
                }
            }
        }
    }
}
