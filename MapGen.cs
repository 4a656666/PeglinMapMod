using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Worldmap;

namespace PeglinMapMod
{
    public class MapGen
    {
        public static MapData mapData = new();
        public static int mapNodeIndex = 38;

        public static void Reset()
        {
            mapData = new();
        }

        public static void GenerateMap()
        {
            List<int> guaranteedPath = new();
            if (Configuration.GuaranteedPathTypeValidated.Count > 0)
            {
                MapDataNode rootNode = mapData.GetMapDataNode(MapController.instance.rootNode);
                MapDataNode currentNode = rootNode;
                while (currentNode.children.Count > 0)
                {
                    int randChild = -1;
                    do randChild = currentNode.children[Random.Range(0, currentNode.children.Count)];
                    while (!Configuration.AllowInefficientPath && currentNode.children.Any(v => mapData.GetMapDataNode(v).children.Contains(randChild)));

                    guaranteedPath.Add(randChild);
                    currentNode = mapData.GetMapDataNode(randChild);
                }
            }

            foreach (var mapNode in mapData.map.Values)
            {
                RoomType selectedRoomType = RoomType.NONE;
                List<RoomType> possibleRoomTypes = guaranteedPath.Contains(mapNode.id) ?
                    new(Configuration.GuaranteedPathTypeValidated) :
                    new(Configuration.RoomWeights.Keys);

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

                if (Configuration.PreventElitesNearStart && selectedRoomType == RoomType.MINI_BOSS && !mapNode.associatedMapNode._canBeMiniboss) selectedRoomType = RoomType.BATTLE;

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

        public static MapNode CreateMapNode(MapNode orig)
        {
            // the commented out lines *appear* to be unecessary as unity *seems* to do it for you
            // nevertheless the line renderers on our custom nodes appear to be broken
            MapNode mapNode = GameObject.Instantiate(orig.gameObject, orig.transform.parent).GetComponent<MapNode>();
            //mapNode._icons =
            //    orig._icons.ToList()
            //    .ConvertAll(v => mapNode.transform.Find(v.name).gameObject)
            //    .ToArray();
            mapNode.gameObject.name = "Node " + mapNodeIndex++;
            mapNode._childNodes = new MapNode[0];
            //mapNode._leftRenderer = mapNode.transform.Find("leftLine").GetComponent<LineRenderer>();
            //mapNode._middleRenderer = mapNode.transform.Find("midLine").GetComponent<LineRenderer>();
            //mapNode._rightRenderer = mapNode.transform.Find("rightLine").GetComponent<LineRenderer>();
            mapNode.RoomType = RoomType.NONE;
            mapNode.transform.position = orig.transform.position;
            return mapNode;
        }

        public static MapNode CreateMapNode()
        {
            return CreateMapNode(MapController.instance.rootNode.gameObject.GetComponent<MapNode>());
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
            children = associatedMapNode.ChildNodes == null ?
                new List<int>() :
                new List<MapNode>(associatedMapNode.ChildNodes).ConvertAll(v => MapGen.GetIDFromMapNode(v));
            roomType = RoomType.NONE;
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
}
