using HarmonyLib;
using Worldmap;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

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

            Plugin.logger.LogDebug("Transpiling MapNode.SetActiveState()...");

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
