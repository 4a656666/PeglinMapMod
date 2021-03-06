using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Worldmap;

namespace PeglinMapMod
{
    public class Configuration
    {
        // Constants
        public static Dictionary<string, RoomType> strToRoomTypeMap = new()
        {
            { "event", RoomType.SCENARIO },
            { "random", RoomType.UNKNOWN },
            { "battle", RoomType.BATTLE },
            { "elite", RoomType.MINI_BOSS },
            { "relic", RoomType.TREASURE }
        };
        public static Dictionary<RoomType, string> roomTypeToStrMap = new()
        {
            { RoomType.SCENARIO, "EVENT" },
            { RoomType.UNKNOWN, "RANDOM" },
            { RoomType.BATTLE, "BATTLE" },
            { RoomType.MINI_BOSS, "ELITE" },
            { RoomType.TREASURE, "RELIC" },
            { RoomType.NONE, "NONE" },
        };
        public static List<string> EasyBattles = new(new string[] { "EntryEncounter", "Bats1", "SlimeEncounter2", "SlimeEncounter3", "PlantEncounter1", "MuchoSlimeEncounter_Easy" });
        public static List<string> RandomBattles = new(new string[] { "Bats2", "Everything", "MuchoSlimeEncounter", "SlimeEncounter4", "PlantEncounter2", "ONLYBATS" });
        public static List<string> EliteBattles = new(new string[] { "MinotaurBossEncounter", "PlantMiniboss", "SlimeMiniBossEncounter_HARD" });
        public static List<string> Scenarios = new(new string[] { "BrambleTree", "SunnyClearing", "Thunderstorm", "MysteriousAltarOffer", "SlimyPath", "CrowClearing", "Inferno", "HaglinScouting" });



        // Config entries
        private static ConfigEntry<bool> enablePluginConfig;
        private static ConfigEntry<bool> enableDebugConfig;
        private static ConfigEntry<bool> logAvaliableRoomsConfig;

        private static ConfigEntry<string> guaranteedPathTypeConfig;
        private static ConfigEntry<bool> allowInefficientPathConfig;
        private static ConfigEntry<bool> preventElitesNearStartConfig;
        private static ConfigEntry<string> firstRoomTypeConfig;

        private static ConfigEntry<int> eventWeightConfig;
        private static ConfigEntry<int> randomWeightConfig;
        private static ConfigEntry<int> battleWeightConfig;
        private static ConfigEntry<int> eliteWeightConfig;
        private static ConfigEntry<int> relicWeightConfig;

        private static ConfigEntry<bool> fixInefficientEdgesConfig;
        private static ConfigEntry<bool> twoBossesMapEnabledConfig;
        private static ConfigEntry<int> extendMapAmountConfig;

        private static ConfigEntry<string> allowedEasyBattlesConfig;
        private static ConfigEntry<string> allowedRandomBattlesConfig;
        private static ConfigEntry<string> allowedEliteBattlesConfig;
        private static ConfigEntry<string> allowedScenariosConfig;



        // Config values
        public static bool EnablePlugin => enablePluginConfig.Value;
        public static bool EnableDebug => enableDebugConfig.Value;
        public static bool LogAvaliableRooms => logAvaliableRoomsConfig.Value;

        public static string GuaranteedPathType => guaranteedPathTypeConfig.Value;
        public static bool AllowInefficientPath => allowInefficientPathConfig.Value;
        public static bool PreventElitesNearStart => preventElitesNearStartConfig.Value;
        public static string FirstRoomType => firstRoomTypeConfig.Value;

        public static int EventWeight => eventWeightConfig.Value;
        public static int RandomWeight => randomWeightConfig.Value;
        public static int BattleWeight => battleWeightConfig.Value;
        public static int EliteWeight => eliteWeightConfig.Value;
        public static int RelicWeight => relicWeightConfig.Value;

        public static bool FixInefficientEdges => fixInefficientEdgesConfig.Value;
        public static bool TwoBossesMapEnabled => twoBossesMapEnabledConfig.Value;
        public static int ExtendMapAmount => extendMapAmountConfig.Value;

        public static string AllowedEasyBattles => allowedEasyBattlesConfig.Value;
        public static string AllowedRandomBattles => allowedRandomBattlesConfig.Value;
        public static string AllowedEliteBattles => allowedEliteBattlesConfig.Value;
        public static string AllowedScenarios => allowedScenariosConfig.Value;



        // Aditional parsing/validation
        public static Dictionary<RoomType, int> RoomWeights
        {
            get
            {
                Dictionary<RoomType, int> dict = new();
                dict.Add(RoomType.SCENARIO, EventWeight);
                dict.Add(RoomType.UNKNOWN, RandomWeight);
                dict.Add(RoomType.BATTLE, BattleWeight);
                dict.Add(RoomType.MINI_BOSS, EliteWeight);
                dict.Add(RoomType.TREASURE, RelicWeight);
                return dict;
            }
        }
        public static List<RoomType> GuaranteedPathTypeValidated
        {
            get
            {
                if (string.IsNullOrEmpty(GuaranteedPathType)) return new();

                List<string> _ = new List<string>(GuaranteedPathType.Split(',')).ConvertAll(v => v.Trim());
                _.RemoveAll(v => !strToRoomTypeMap.ContainsKey(v));
                return _.ConvertAll(v => strToRoomTypeMap[v.ToLower()]);
            }
        }
        public static RoomType FirstRoomTypeValidated
        {
            get
            {
                RoomType res;
                if (strToRoomTypeMap.TryGetValue(FirstRoomType, out res)) return res;
                else return RoomType.NONE;
            }
        }
        public static List<string> AllowedEasyBattlesValidated
        {
            get
            {
                if (string.IsNullOrEmpty(AllowedEasyBattles)) return new();

                List<string> _ = new List<string>(AllowedEasyBattles.Split(',')).ConvertAll(v => v.Trim());
                //_.RemoveAll(v => !EasyBattles.Contains(v));
                return _;
            }
        }
        public static List<string> AllowedRandomBattlesValidated
        {
            get
            {
                if (string.IsNullOrEmpty(AllowedRandomBattles)) return new();

                List<string> _ = new List<string>(AllowedRandomBattles.Split(',')).ConvertAll(v => v.Trim());
                //_.RemoveAll(v => !RandomBattles.Contains(v));
                return _;
            }
        }
        public static List<string> AllowedEliteBattlesValidated
        {
            get
            {
                if (string.IsNullOrEmpty(AllowedEliteBattles)) return new();

                List<string> _ = new List<string>(AllowedEliteBattles.Split(',')).ConvertAll(v => v.Trim());
                //_.RemoveAll(v => !EliteBattles.Contains(v));
                return _;
            }
        }
        public static List<string> AllowedScenariosValidated
        {
            get
            {
                if (string.IsNullOrEmpty(AllowedScenarios)) return new();

                List<string> _ = new List<string>(AllowedScenarios.Split(',')).ConvertAll(v => v.Trim());
                //_.RemoveAll(v => !Scenarios.Contains(v));
                return _;
            }
        }



        public static void BindConfig(ConfigFile config)
        {
            enablePluginConfig = config.Bind(
                "General", "EnablePlugin", true,
                "Whether or not to enable the plugin"
            );

            enableDebugConfig = config.Bind(
                "General", "EnableDebug", false,
                "Whether or not to enable some debugging tools (f12 to skip battle, among other things)\nWarning: May cause problems"
            );

            logAvaliableRoomsConfig = config.Bind(
                "General", "LogAvaliableRooms", false,
                "Whether or not to log the avaliable rooms when you start a new run"
            );



            guaranteedPathTypeConfig = config.Bind(
                "Map", "GuaranteedPathType", "",
                "Comma-seperated list of types of rooms to use for generating a path from the start to the boss.\nEach item is equally likely.\nOptions:\n  'event' (displays as ?, only scenario)\n  'random' (displays as ?, scenario, battle, or relic)\n  'battle'\n  'elite'\n  'relic'"
            );

            allowInefficientPathConfig = config.Bind(
                "Map", "AllowInefficientPath", true,
                "When generating a guaranteed path, whether or not to allow a path that goes through less rooms than otherwise possible (i.e. on the edge of the map)"
            );

            preventElitesNearStartConfig = config.Bind(
                "Map", "PreventElitesNearStart", true,
                "Prevents elites from spawning near the start of the map.\nDoes not affect the guaranteed path."
            );

            firstRoomTypeConfig = config.Bind(
                "Map", "FirstRoomType", "battle",
                "Type of room for the first room.\nOptions:\n  'event' (displays as ?, only scenario)\n  'random' (displays as ?, scenario, battle, or relic)\n  'battle'\n  'elite'\n  'relic'\nor put nothing to generate randomly."
            );



            fixInefficientEdgesConfig = config.Bind(
                "Map Shape", "FixInefficientEdges", false,
                "Warning: do not change the settings in this section and then load a saved game.\nAdds an extra room at the edges where you could otherwise skip a room and get a shorter path."
            );

            twoBossesMapEnabledConfig = config.Bind(
                "Map Shape", "TwoBossesMap", false,
                "Modifes the map to have two bosses, which will always be different."
            );

            extendMapAmountConfig = config.Bind(
                "Map Shape", "ExtendMapAmount", 0,
                "How much to extend the map by. Incrementing this by one adds seven new rooms, in two layers."
            );



            eventWeightConfig = config.Bind(
                "Map", "EventWeight", 20,
                "Weight of event (only scenario) room types"
            );

            randomWeightConfig = config.Bind(
                "Map", "RandomWeight", 70,
                "Weight of random (scenario, battle, or relic) room types"
            );

            battleWeightConfig = config.Bind(
                "Map", "BattleWeight", 71,
                "Weight of battle room types"
            );

            eliteWeightConfig = config.Bind(
                "Map", "EliteWeight", 26,
                "Weight of elite room types"
            );

            relicWeightConfig = config.Bind(
                "Map", "RelicWeight", 13,
                "Weight of relic room types"
            );



            allowedEasyBattlesConfig = config.Bind(
                "Rooms", "AllowedEasyBattles", string.Join(", ", EasyBattles),
                "Controls what types of easy battles can be found. See https://github.com/4a656666/PeglinMapMod/blob/master/Rooms.md for more info."
            );

            allowedRandomBattlesConfig = config.Bind(
                "Rooms", "AllowedRandomBattles", string.Join(", ", RandomBattles),
                "Controls what types of random battles can be found. See https://github.com/4a656666/PeglinMapMod/blob/master/Rooms.md for more info."
            );

            allowedEliteBattlesConfig = config.Bind(
                "Rooms", "AllowedEliteBattles", string.Join(", ", EliteBattles),
                "Controls what types of elite battles can be found. See https://github.com/4a656666/PeglinMapMod/blob/master/Rooms.md for more info."
            );

            allowedScenariosConfig = config.Bind(
                "Rooms", "AllowedScenarios", string.Join(", ", Scenarios),
                "Controls what types of scenarios can be found. See https://github.com/4a656666/PeglinMapMod/blob/master/Rooms.md for more info."
            );
        }
    }
}
