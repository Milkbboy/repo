using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine.Client
{

    public class MapHero : MonoBehaviour
    {
        void Start()
        {
            
        }

        void Update()
        {
            GameClient client = GameClient.Get();
            World world = client.GetWorld();
            if (world != null)
            {
                MapLocationDot location = MapLocationDot.Get(world.map_location_id);
                if (location != null)
                {
                    transform.position = location.transform.position;
                }
            }
        }
    }
}