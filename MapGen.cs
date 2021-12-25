using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Worldmap;

namespace PeglinMapMod
{
    public class MapGen
    {
        public static MapData mapData = new();

        public static void Reset()
        {
            mapData = new();
        }

        public static void GenerateMap()
        {
            MapDataNode rootNode = mapData.GetMapDataNode(MapController.instance.rootNode);
            List<int> guaranteedPath = new();
            MapDataNode currentNode = rootNode;
            while (currentNode.children.Count > 0)
            {
                int randChild = -1;
                do randChild = currentNode.children[Random.Range(0, currentNode.children.Count)];
                while (!Configuration.AllowInefficientPath && currentNode.children.Any(v => mapData.GetMapDataNode(v).children.Contains(randChild)));


                guaranteedPath.Add(randChild);
                currentNode = mapData.GetMapDataNode(randChild);
            }

            foreach (var mapNode in mapData.map.Values)
            {
                RoomType selectedRoomType = RoomType.NONE;
                if (guaranteedPath.Contains(mapNode.id) && Configuration.GuaranteedPathTypeValidated.Count > 0)
                {
                    selectedRoomType = Configuration.GuaranteedPathTypeValidated[Random.Range(0, Configuration.GuaranteedPathTypeValidated.Count)];
                }
                else
                {
                    List<RoomType> possibleRoomTypes = new(Configuration.RoomWeights.Keys);

                    int totalWeight = possibleRoomTypes.ConvertAll(v => Configuration.RoomWeights[v]).Sum();
                    int rand = Random.Range(0, totalWeight);

                    int currentTotalWeight = 0;
                    foreach (var possibleRoomType in possibleRoomTypes)
                    {
                        currentTotalWeight += Configuration.RoomWeights[possibleRoomType];
                        if (rand < currentTotalWeight)
                        {
                            selectedRoomType = possibleRoomType;
                            break;
                        }
                    }

                    if (Configuration.PreventElitesNearStart && selectedRoomType == RoomType.ELITE && !mapNode.associatedMapNode._canBeMiniboss) selectedRoomType = RoomType.BATTLE;
                }

                mapNode.roomType = selectedRoomType;
            }
        }

        public static void InstantiateMapDataNode(MapNode mapNode)
        {
            mapData.AddMapDataNode(new MapDataNode(mapNode));
        }

        public static int GetIDFromMapNode(MapNode mapNode)
        {
            MapNodeWithID mapNodeWithIDComp;
            if (!mapNode.gameObject.TryGetComponent<MapNodeWithID>(out mapNodeWithIDComp))
                mapNodeWithIDComp = mapNode.gameObject.AddComponent<MapNodeWithID>();
            return mapNodeWithIDComp.id;
        }
    }

    public class MapDataNode
    {
        public int id;
        public List<int> children;
        public RoomType roomType;

        public MapNode associatedMapNode;

        public MapDataNode(MapNode _associatedMapNode)
        {
            associatedMapNode = _associatedMapNode;
            id = MapGen.GetIDFromMapNode(associatedMapNode);
            children = new List<MapNode>(associatedMapNode.ChildNodes).ConvertAll(v => MapGen.GetIDFromMapNode(v));
            roomType = RoomType.NONE;
        }

        public Worldmap.RoomType GetWorldmapDotRoomType()
        {
            return roomType switch
            {
                RoomType.NONE => Worldmap.RoomType.NONE,
                RoomType.BATTLE => Worldmap.RoomType.BATTLE,
                RoomType.ELITE => Worldmap.RoomType.MINI_BOSS,
                RoomType.RELIC => Worldmap.RoomType.TREASURE,
                RoomType.EVENT => Worldmap.RoomType.SCENARIO,
                RoomType.RANDOM => Worldmap.RoomType.UNKNOWN,
                _ => Worldmap.RoomType.NONE,
            };
        }
    }

    public class MapData
    {
        public Dictionary<int, MapDataNode> map;

        public MapData(List<MapDataNode> _map)
        {
            map = new();
            _map.ForEach(AddMapDataNode);
        }

        public MapData()
        {
            map = new();
        }

        public MapDataNode GetMapDataNode(int id)
        {
            return map[id];
        }

        public MapDataNode GetMapDataNode(MapNode mapNode)
        {
            return GetMapDataNode(MapGen.GetIDFromMapNode(mapNode));
        }

        public void AddMapDataNode(MapDataNode mapNode)
        {
            map.Add(mapNode.id, mapNode);
        }
    }

    public enum RoomType
    {
        NONE,
        BATTLE,
        ELITE,
        RELIC,
        EVENT,
        RANDOM
    }
}
