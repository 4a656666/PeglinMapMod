using BepInEx.Configuration;
using System.Collections.Generic;

namespace PeglinMapMod
{
    public class Configuration
    {
        public static Dictionary<string, RoomType> strToRoomTypeMap = new Dictionary<string, RoomType>();

        private static ConfigEntry<bool> enablePluginConfig;
        private static ConfigEntry<bool> enableDebugConfig;

        private static ConfigEntry<string> guaranteedPathTypeConfig;
        private static ConfigEntry<bool> allowInefficientPathConfig;
        private static ConfigEntry<bool> preventElitesNearStartConfig;
        //private static ConfigEntry<string> firstRoomTypeConfig;

        private static ConfigEntry<int> eventWeightConfig;
        private static ConfigEntry<int> randomWeightConfig;
        private static ConfigEntry<int> battleWeightConfig;
        private static ConfigEntry<int> eliteWeightConfig;
        private static ConfigEntry<int> relicWeightConfig;


        public static bool EnablePlugin => enablePluginConfig.Value;
        public static bool EnableDebug => enableDebugConfig.Value;

        public static string GuaranteedPathType => guaranteedPathTypeConfig.Value;
        public static bool AllowInefficientPath => allowInefficientPathConfig.Value;
        public static bool PreventElitesNearStart => preventElitesNearStartConfig.Value;
        //public static string FirstRoomType => firstRoomTypeConfig.Value;
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

        //public static RoomType FirstRoomTypeValidated
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(FirstRoomType)) return RoomType.NONE;

        //        return strToRoomTypeMap.ContainsKey(FirstRoomType) ?
        //            strToRoomTypeMap[FirstRoomType] :
        //            RoomType.NONE;
        //    }
        //}

        public static int EventWeight => eventWeightConfig.Value;
        public static int RandomWeight => randomWeightConfig.Value;
        public static int BattleWeight => battleWeightConfig.Value;
        public static int EliteWeight => eliteWeightConfig.Value;
        public static int RelicWeight => relicWeightConfig.Value;
        public static Dictionary<RoomType, int> RoomWeights
        {
            get
            {
                Dictionary<RoomType, int> dict = new();
                dict.Add(RoomType.EVENT, EventWeight);
                dict.Add(RoomType.RANDOM, RandomWeight);
                dict.Add(RoomType.BATTLE, BattleWeight);
                dict.Add(RoomType.ELITE, EliteWeight);
                dict.Add(RoomType.RELIC, RelicWeight);
                return dict;
            }
        }

        public static void BindConfig(ConfigFile config)
        {
            strToRoomTypeMap.Add("event", RoomType.EVENT);
            strToRoomTypeMap.Add("random", RoomType.RANDOM);
            strToRoomTypeMap.Add("battle", RoomType.BATTLE);
            strToRoomTypeMap.Add("elite", RoomType.ELITE);
            strToRoomTypeMap.Add("relic", RoomType.RELIC);

            enablePluginConfig = config.Bind(
                "General", "EnablePlugin", true,
                "Whether or not to enable the plugin"
            );

            enableDebugConfig = config.Bind(
                "General", "EnableDebug", false,
                "Whether or not to enable some debugging tools (f12 to skip battle, among other things)\nWarning: May cause problems"
            );



            guaranteedPathTypeConfig = config.Bind(
                "Map", "GuaranteedPathType", "",
                "Comma-seperated list of types of rooms to use for generating a path from the start to the boss.\nEach item is equally likely.\nOptions:\n  'event' (displays as ?, only scenario)\n  'random' (displays as ?, scenario, battle, elite, or relic)\n  'battle'\n  'elite'\n  'relic'"
            );

            allowInefficientPathConfig = config.Bind(
                "Map", "AllowInefficientPath", true,
                "When generating a guaranteed path, whether or not to allow a path that goes through less rooms than otherwise possible (i.e. on the edge of the map)"
            );

            preventElitesNearStartConfig = config.Bind(
                "Map", "PreventElitesNearStart", true,
                "Prevents elites from spawning near the start of the map.\nDoes not affect the guaranteed path."
            );

            //firstRoomTypeConfig = config.Bind(
            //    "Map", "FirstRoomType", "battle",
            //    "Type of room for the first room.\nOptions:\n  'event' (displays as ?, only scenario)\n  'random' (displays as ?, scenario, battle, elite, or relic)\n  'battle'\n  'elite'\n  'relic'"
            //);



            eventWeightConfig = config.Bind(
                "Map", "EventWeight", 20,
                "Weight of event (only scenario) room types"
            );

            randomWeightConfig = config.Bind(
                "Map", "RandomWeight", 70,
                "Weight of random (scenario, battle, elite, or relic) room types"
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
        }
    }
}
