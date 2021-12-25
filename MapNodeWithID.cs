using UnityEngine;

namespace PeglinMapMod
{
    public class MapNodeWithID : MonoBehaviour
    {
        public int id;
        public static int nextID = 0;

        MapNodeWithID()
        {
            id = nextID++;
        }
    }
}
