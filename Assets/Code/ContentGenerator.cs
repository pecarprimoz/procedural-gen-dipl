using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Need to have a managar that keeps track of all the prefabs, ContentManager maybe?

// 1. Pass terrain info to this guy, need to calc stuff for placement
public class ContentGenerator : MonoBehaviour {
    public void GenerateBiomeContent(TerrainInfo info) {
        // Seperate object placement by biomes or different game objects (trees, grass, buildings, road gen, rivers?, other stuff?)
        PlaceStuff(info);
    }

    // TESTING, NEED TO MAKE AN EXAMPLE FOR OBJECT PLACEMENT ON THE ALREADY GENERATED TERRAIN
    private void PlaceStuff(TerrainInfo info) {
        List<GameObject> placeobjs = new List<GameObject>();
        int tposx = (int)info._Terrain.transform.position.x;
        int tposz = (int)info._Terrain.transform.position.z;
        int twitdh = (int)info._Terrain.terrainData.size.x;
        int tlength = (int)info._Terrain.terrainData.size.z;

        // BALLS BALLS BALLS
        for (int i = 0; i < 100; i++) {
            int posx = Random.Range(tposx, tposx + twitdh);
            int posz = Random.Range(tposz, tposz + tlength);
            int posy = (int)info._Terrain.terrainData.GetHeight(posx, posz) + 10;
            Instantiate(info.ContentManager.PlaceableObject, new Vector3(posx, posy, posz), Quaternion.identity, info.ContentManager.PlaceableDict[ContentManager.PlaceableObjectType.kBall].transform);
        }
        // Everything here is now set up to work with biomes, when placing shit relevant to the biome, need to add it to the ContentManager.PlaceableDict so we have a parent for all created game object
        // Now the difference will be that every biome will get a seperate function call
        // Noob approach would be to get random x z indices, then check if its that biome then place it :D (probably can be done better, just need to figure out how)
    }

}
