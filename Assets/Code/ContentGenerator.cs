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
        // Totaly noob approach, dont use blue noise or anything, just select a random point and place an item therev
        for (int i = 0; i < info.SeperatedBiomes.Keys.Count; i++) {
            // instead of taking the first item, take random ?
            PlaceContent(info, i, info.TerrainParameterList[i].ObjectListCount, info.TerrainParameterList[i].TerrainParameterObjectList[0]);
        }
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
}
