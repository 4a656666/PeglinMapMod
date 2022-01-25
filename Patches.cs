using HarmonyLib;
using Worldmap;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace PeglinMapMod
{
    public static class Patches
    {
        public static List<MapDataBattle> _potentialEliteBattles = new();

        [HarmonyPatch(typeof(MapController), "CreateMapDataLists"), HarmonyPostfix]
        public static void RestrictBattlesAndScenariosPatch()
        {
            MapController mc = MapController.instance;

            // needs to be done because mc._potentialEliteBattles is modified and there is no mc._remainingEliteBattles
            if (_potentialEliteBattles.Count == 0) _potentialEliteBattles.AddRange(mc._potentialEliteBattles);

            mc._remainingEasyBattles.RemoveAll(v => !Configuration.AllowedEasyBattlesValidated.Contains(v.name));
            mc._remainingRandomBattles.RemoveAll(v => !Configuration.AllowedRandomBattlesValidated.Contains(v.name));
            mc._remainingRandomScenarios.RemoveAll(v => !Configuration.AllowedScenariosValidated.Contains(v.name));
            mc._potentialEliteBattles.RemoveAll(v => !Configuration.AllowedEliteBattlesValidated.Contains(v.name)); // i dont like this but i dont see a different option

            if (Configuration.LogAvaliableRooms)
            {
                Plugin.logger.LogInfo("Avaliable easy battles: " + string.Join(", ", mc._potentialEasyBattles.ConvertAll(v => "'" + v.name + "'")));
                Plugin.logger.LogInfo("Avaliable random battles: " + string.Join(", ", mc._potentialRandomBattles.ConvertAll(v => "'" + v.name + "'")));
                Plugin.logger.LogInfo("Avaliable scenarios: " + string.Join(", ", mc._potentialRandomScenarios.ConvertAll(v => "'" + v.name + "'")));
                Plugin.logger.LogInfo("Avaliable elite battles: " + string.Join(", ", _potentialEliteBattles.ConvertAll(v => "'" + v.name + "'")));
            }
        }

        [HarmonyPatch(typeof(MapController), "Start"), HarmonyPrefix]
        public static void FirstRoomTypePatch()
        {
            MapController.instance.rootNode.RoomType = Configuration.FirstRoomTypeValidated;
        }

        [HarmonyPatch(typeof(MapController), "Start"), HarmonyPrefix]
        public static void ReshapeMapPatch()
        {
            ExtendMap(Configuration.ExtendMapAmount);
            if (Configuration.TwoBossesMapEnabled) TwoBossesMap();
            if (Configuration.FixInefficientEdges) FixInefficientEdges();
            MapController.instance._nodes = GameObject.FindObjectsOfType<MapNode>();
        }

        public static void FixInefficientEdges()
        {
            GameObject.FindObjectsOfType<MapNode>().Do(v =>
            {
                foreach (MapNode child in v.ChildNodes)
                {
                    if (v.ChildNodes.Any(otherChild => otherChild.ChildNodes.Contains(child)))
                    {
                        MapNode mapNode = MapGen.CreateMapNode();
                        mapNode._childNodes = new MapNode[] { child };
                        v._childNodes[System.Array.IndexOf(v._childNodes, child)] = mapNode;
                        mapNode.transform.position = (child.transform.position + v.transform.position) / 2f;
                        break;
                    }
                }
            });
        }

        public static void TwoBossesMap()
        {
            // Original
            // 08    25    28    13
            //   \  /  \  /  \  /
            //    05    33    34
            //      \  /  \  /
            //       26    32
            //         \  /
            //         BOSS

            // New
            // 08    25    28    13
            // | \  /        \  / |
            // |  05          34  |
            // | /              \ |
            // 26                32
            // |                  |
            // BOSS            BOSS

            GameObject.Destroy(GameObject.Find("Node 33"));

            MapNode node5 = GameObject.Find("Node 5").GetComponent<MapNode>();
            MapNode node34 = GameObject.Find("Node 34").GetComponent<MapNode>();

            GameObject.Find("Node 25").GetComponent<MapNode>()._childNodes = new MapNode[] { node5 };
            GameObject.Find("Node 28").GetComponent<MapNode>()._childNodes = new MapNode[] { node34 };

            MapNode node26 = GameObject.Find("Node 26").GetComponent<MapNode>();
            MapNode node32 = GameObject.Find("Node 32").GetComponent<MapNode>();

            node26.transform.position += new Vector3(-4, 0, 0);
            node32.transform.position += new Vector3(4, 0, 0);

            GameObject.Find("Node 8").GetComponent<MapNode>()._childNodes = new MapNode[] { node26, node5 };
            GameObject.Find("Node 13").GetComponent<MapNode>()._childNodes = new MapNode[] { node34, node32 };

            MapNode boss1 = GameObject.Find("BossNode").GetComponent<MapNode>();
            MapNode boss2 = MapGen.CreateMapNode(boss1);

            boss1.transform.position += new Vector3(-7, 0, 0);
            boss2.transform.position += new Vector3(7, 0, 0);

            // dont need this line cause its already that way
            // node26._childNodes = new MapNode[] { boss1 };
            node32._childNodes = new MapNode[] { boss2 };

            int rand = Random.Range(0, 2);

            boss1._potentialMapData = new global::MapData[] { boss1._potentialMapData[rand] };
            boss2._potentialMapData = new global::MapData[] { boss2._potentialMapData[1 - rand] };
            boss2.RoomType = RoomType.BOSS; // this is for some reason necessary

            boss1.gameObject.name = "BossNode1";
            boss2.gameObject.name = "BossNode2";
        }

        public static void ExtendMap(int n)
        {
            // Original
            // 17    35    19    09
            // | \  /  \  /  \  / |
            // |  30    37    16  |
            // | /  \  /  \  /  \ |
            // 08    25    28    13

            // New
            // 17    35    19    09
            // | \  /  \  /  \  / |
            // |  n1    n2    n3  | --+
            // | /  \  /  \  /  \ |   | repeated n times
            // n4    n5    n6    n7   |
            // | \  /  \  /  \  / | --+
            // |  30    37    16  |
            // | /  \  /  \  /  \ |
            // 08    25    28    13

            MapNode[] previous = new MapNode[] {
                GameObject.Find("Node 17").GetComponent<MapNode>(),
                GameObject.Find("Node 35").GetComponent<MapNode>(),
                GameObject.Find("Node 19").GetComponent<MapNode>(),
                GameObject.Find("Node 9").GetComponent<MapNode>()
            };

            MapNode[] toClone = new MapNode[]
            {
                previous[0].RightChild, // n1
                previous[1].RightChild, // n2
                previous[2].RightChild, // n3

                previous[0]._childNodes[0], // n4
                previous[0].RightChild.RightChild, // n5
                previous[1].RightChild.RightChild, // n6
                previous[2].RightChild.RightChild // n7
            };

            MapNode[] toMoveDown = new MapNode[]
            {
                toClone[0],
                toClone[1],
                toClone[2],
                toClone[3],
                toClone[4],
                toClone[5],
                toClone[6],
                GameObject.Find("Node 5").GetComponent<MapNode>(),
                GameObject.Find("Node 33").GetComponent<MapNode>(),
                GameObject.Find("Node 34").GetComponent<MapNode>(),
                GameObject.Find("Node 26").GetComponent<MapNode>(),
                GameObject.Find("Node 32").GetComponent<MapNode>(),
                GameObject.Find("BossNode").GetComponent<MapNode>()
            };

            for (int _ = 0; _ < n; _++)
            {
                MapNode[] cloned = toClone.ToList().ConvertAll(v => MapGen.CreateMapNode(v)).ToArray();

                previous[0]._childNodes = new MapNode[] { cloned[3], cloned[0] };
                previous[1]._childNodes = new MapNode[] { cloned[0], cloned[1] };
                previous[2]._childNodes = new MapNode[] { cloned[1], cloned[2] };
                previous[3]._childNodes = new MapNode[] { cloned[2], cloned[6] };

                cloned[0]._childNodes = new MapNode[] { cloned[3], cloned[4] };
                cloned[1]._childNodes = new MapNode[] { cloned[4], cloned[5] };
                cloned[2]._childNodes = new MapNode[] { cloned[5], cloned[6] };

                cloned[3]._childNodes = new MapNode[] { toClone[3], toClone[0] };
                cloned[4]._childNodes = new MapNode[] { toClone[0], toClone[1] };
                cloned[5]._childNodes = new MapNode[] { toClone[1], toClone[2] };
                cloned[6]._childNodes = new MapNode[] { toClone[2], toClone[6] };

                float heightDiff = previous[0].transform.position.y - toClone[3].transform.position.y;

                toMoveDown.Do(v => { v.transform.position += Vector3.down * heightDiff; });

                previous = new MapNode[] { cloned[3], cloned[4], cloned[5], cloned[6] };
            }
        }

        [HarmonyPatch(typeof(MapController), "CreateMapDataLists"), HarmonyPrefix]
        public static void GenerateRandomMapPatch()
        {
            MapGen.Reset();
            GameObject.FindObjectsOfType<MapNode>().Do(v => MapGen.InstantiateMapDataNode(v));
            MapGen.GenerateMap();
        }

        // The goal of this transpilation is to insert our method call after randomization but inside the if statement surrounding randomization
        [HarmonyPatch(typeof(MapNode), "SetActiveState"), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InsertMethodAtRandomizationPatch(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo roomTypeField = AccessTools.Field(typeof(MapNode), "RoomType");
            List<Label> labelList = new List<Label>();
            Label jumpOverRandomizationLabel = new();

            int state = 0;

            // We are looking for the following:
            // ldarg.0
            // ldfld valuetype Worldmap.RoomType Worldmap.MapNode::RoomType
            // brtrue.s <label>
            // ...
            // some branches the may or may not branch to <label>
            // ...
            // here is where we want our new instruction
            // an instruction with the label <label>

            foreach (var instruction in instructions)
            {
                switch (state)
                {
                    case 0: // First we expect a ldarg.0 (LoaD/push ARGument 0 (this) to stack)
                        if (instruction.IsLdarg(0)) state = 1;
                        else state = 0;
                        break;

                    case 1: // Then we expect a ldfld MapNode.RoomType (LoaD/push FieLD RoomType from the thing on top of the stack (this) to the stack)
                        if (instruction.LoadsField(roomTypeField)) state = 2;
                        else state = 0;
                        break;

                    case 2: // The we expect a brtrue.s (BRanch/jump over the randomization if the value on top of the stack is TRUE/not 0 (RoomType.None)) and we need to store the label
                        if (instruction.opcode.Equals(OpCodes.Brtrue_S) || instruction.opcode.Equals(OpCodes.Brtrue))
                        {
                            state = 3;
                            jumpOverRandomizationLabel = (Label)instruction.operand;
                        }
                        else state = 0;
                        break;

                    case 3: // Every branching instruction needs it label stored
                        Label? outLabel;
                        if (instruction.labels.Contains(jumpOverRandomizationLabel)) // This is the end of the if statement surrounding the randomization
                        {
                            labelList.RemoveAll(v => !instruction.labels.Contains(v)); // Any stored labels that dont jump here are unimportant
                            instruction.labels.RemoveAll(v => labelList.Contains(v)); // Any stored labels that do jump here dont anymore

                            // ldarg.0 pushes "this" to the stack and then our method call consumes it as the first param
                            CodeInstruction loadThisInstruction = new CodeInstruction(OpCodes.Ldarg_0);
                            CodeInstruction callMethodInsruction = new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo((MapNode _this) => SetActiveStateInsertion(_this)));

                            // Those stored labels that used to jump to the current instruction now jump to our method call
                            loadThisInstruction.labels.AddRange(labelList);

                            // Insert our instructions before the end of the if statement
                            yield return loadThisInstruction;
                            yield return callMethodInsruction;

                            state = 4; // We are done
                        }
                        else if (instruction.Branches(out outLabel) && outLabel.HasValue) labelList.Add(outLabel.Value);
                        break;

                    default:
                        break;
                }
                yield return instruction;
            }
        }

        public static void SetActiveStateInsertion(MapNode instance)
        {
            instance.RoomType = MapGen.mapData.GetMapDataNode(instance).roomType;
        }
    }
}
