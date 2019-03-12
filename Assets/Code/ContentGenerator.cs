using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Need to have a managar that keeps track of all the prefabs, ContentManager maybe?

// 1. Pass terrain info to this guy, need to calc stuff for placement
public class ContentGenerator : MonoBehaviour {
    int TerrainPositionX;
    int TerrainPositionZ;
    int TerrainWidth;
    int TerrainLength;
    public void GenerateBiomeContent(TerrainInfo info) {
        TerrainPositionX = (int)info._Terrain.transform.position.x;
        TerrainPositionZ = (int)info._Terrain.transform.position.z;
        TerrainWidth = (int)info._Terrain.terrainData.size.x;
        TerrainLength = (int)info._Terrain.terrainData.size.z;
        // Seperate object placement by biomes or different game objects (trees, grass, buildings, road gen, rivers?, other stuff?)
        //PlaceStuff(info);
        // Totaly noob approach, dont use blue noise or anything, just select a random point and place an item therev
        var obj = new GameObject();
        for (int i = 0; i < info.SeperatedBiomes.Keys.Count; i++) {
            PlaceContent(info, i, 100, obj);
        }
        Destroy(obj);
    }

    // placement of objects is invalid, since biomePoint.X and biomePoint.Z are ACTUAL INDICES IN THE ARRAY, NOT POINTS, TODO
    private void PlaceContent(TerrainInfo info, int biomeType, int objCount, GameObject placeableObject) {
        // good for debugging
        //for (int i = 0; i < info.SeperatedBiomes[biomeType].Count; i++) {
        //    var biomePoint = info.SeperatedBiomes[biomeType][i];
        //    int terrainPositionY = (int)info._Terrain.terrainData.GetHeight(biomePoint.X, biomePoint.Z);
        //    Instantiate(placeableObject, new Vector3(biomePoint.X, terrainPositionY, biomePoint.Z), Quaternion.identity);
        //}
        for (int i = 0; i < objCount; i++) {
            int randomPoint = UnityEngine.Random.Range(0, info.SeperatedBiomes[biomeType].Count - 1);
            var biomePoint = info.SeperatedBiomes[biomeType][randomPoint];
            if (!biomePoint.ContainsItem) {
                int terrainPositionY = (int)info._Terrain.terrainData.GetHeight(biomePoint.X, biomePoint.Z);
                Instantiate(placeableObject, new Vector3(biomePoint.X, terrainPositionY, biomePoint.Z), Quaternion.identity, info.ContentManager.BiomeParentGameObjects[biomeType].transform);
                // to avoid placing multiple objects on one point, since we are doing it randomly
                info.SeperatedBiomes[biomeType][randomPoint].ContainsItem = true;
            }
        }
    }
    // TESTING, NEED TO MAKE AN EXAMPLE FOR OBJECT PLACEMENT ON THE ALREADY GENERATED TERRAIN
    //private void PlaceStuff(TerrainInfo info) {


    //    // BALLS BALLS BALLS
    //    for (int i = 0; i < 100; i++) {
    //        int posx = Random.Range(tposx, tposx + twitdh);
    //        int posz = Random.Range(tposz, tposz + tlength);
    //        int posy = (int)info._Terrain.terrainData.GetHeight(posx, posz) + 10;
    //        Instantiate(info.ContentManager.PlaceableObject, new Vector3(posx, posy, posz), Quaternion.identity, info.ContentManager.PlaceableDict[PlaceableObjectType.kBall].transform);
    //    }

    //    // Everything here is now set up to work with biomes, when placing shit relevant to the biome, need to add it to the ContentManager.PlaceableDict so we have a parent for all created game object
    //    // Now the difference will be that every biome will get a seperate function call
    //    // Noob approach would be to get random x z indices, then check if its that biome then place it :D (probably can be done better, just need to figure out how)
    //}

}
