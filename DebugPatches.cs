using HarmonyLib;
using Peglin.Demo;
using UnityEngine;

namespace PeglinMapMod
{
    public static class DebugPatches
	{
        [HarmonyPatch(typeof(DebugMenu), "OnEnable"), HarmonyPrefix, HarmonyDebug]
        public static bool EnableDebugMenuPatch()
        {
            return false;
        }

        [HarmonyPatch(typeof(Debug), "isDebugBuild", MethodType.Getter), HarmonyPostfix, HarmonyDebug]
        public static void EnableDebugMenu2Patch(ref bool __result)
        {
            __result = true;
        }

        [HarmonyPatch(typeof(BattleController), "Update"), HarmonyPostfix, HarmonyDebug]
        public static void EnableDebugSkipBattlePatch(BattleController __instance)
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                __instance.TriggerVictory();
            }
        }
    }
}
