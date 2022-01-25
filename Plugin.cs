using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using Worldmap;

namespace PeglinMapMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION), BepInProcess("Peglin.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        private void Awake()
        {
            Configuration.BindConfig(Config);

            logger = Logger;

            if (Configuration.EnablePlugin)
            {
                Logger.LogInfo($"Got the following guaranteed path room types: {Configuration.GuaranteedPathTypeValidated.ConvertAll(v => Configuration.roomTypeToStrMap[v] ).Join()}");

                Harmony.CreateAndPatchAll(typeof(Patches), PluginInfo.PLUGIN_GUID);

                if (Configuration.EnableDebug)
                {
                    Harmony.CreateAndPatchAll(typeof(DebugPatches), PluginInfo.PLUGIN_GUID);
                    Logger.LogInfo($"Debugging features enabled.");
                }

                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION} is loaded!");
            }
            else
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION} was disabled.");
            }
        }
    }
}
