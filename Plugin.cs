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
        public static Harmony harmony;

        private void Awake()
        {
            Configuration.BindConfig(Config);

            logger = Logger;

            if (Configuration.EnablePlugin)
            {
                Logger.LogInfo($"Got the following guaranteed path room types: {Configuration.GuaranteedPathTypeValidated.ConvertAll(v => Configuration.roomTypeToStrMap[v] ).Join()}");

                harmony = new Harmony(PluginInfo.PLUGIN_GUID);

                harmony.PatchAll(typeof(Patches));

                if (Configuration.EnableDebug)
                {
                    harmony.PatchAll(typeof(DebugPatches));
                    Logger.LogInfo($"Debugging features enabled.");
                }

                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} (GUID {PluginInfo.PLUGIN_GUID}) {PluginInfo.PLUGIN_VERSION} is loaded!");
            }
            else
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} (GUID {PluginInfo.PLUGIN_GUID}) {PluginInfo.PLUGIN_VERSION} was disabled.");
            }
        }
    }
}
