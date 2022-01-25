using HarmonyLib;
using Peglin.Demo;
using UnityEngine;
using Worldmap;

namespace PeglinMapMod
{
    public static class DebugPatches
	{
        [HarmonyPatch(typeof(DebugMenu), "OnEnable"), HarmonyPrefix]
        public static bool EnableDebugMenuPatch()
        {
            return false;
        }

        [HarmonyPatch(typeof(Debug), "isDebugBuild", MethodType.Getter), HarmonyPostfix]
        public static void SetIsDebugBuildTruePatch(ref bool __result)
        {
            __result = true;
        }

        [HarmonyPatch(typeof(BattleController), "Update"), HarmonyPostfix]
        public static void EnableDebugSkipBattlePatch(BattleController __instance)
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                __instance.TriggerVictory();
            }
        }

        [HarmonyPatch(typeof(MapController), "Start"), HarmonyPostfix]
        public static void DisplayMapNodeNamesPatch()
        {
            GameObject.FindObjectsOfType<MapNode>().Do(v =>
            {
                GameObject textMeshGo = new GameObject("debug_text");
                textMeshGo.transform.SetParent(v.transform);
                textMeshGo.transform.localPosition = Vector3.zero;
                TextMesh textMesh = textMeshGo.AddComponent<TextMesh>();
                textMesh.text = v.name;
                textMesh.characterSize = 0.2f;
                textMesh.color = Color.black;
            });
        }
    }
}
