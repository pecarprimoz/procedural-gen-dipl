using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContentGenerator : MonoBehaviour {
    public void GenerateBiomeContent(TerrainInfo info) {
        // testing for some biomes to get grass 
        PlaceSomeShit(info);

        // Totaly noob approach, dont use blue noise or anything, just select a random point and place an item therev
        for (int i = 0; i < info.SeperatedBiomes.Keys.Count; i++) {
            // instead of taking the first item, take random ?
            for (int j = 0; j < info.TerrainParameterList[i].ObjectListCount; j++) {
                // iterate trough all the objects, then place them, first pass is for random ground bullshit
                PlaceContent(info, i, info.TerrainParameterList[i].TerrainParameterObjectCount[j], info.TerrainParameterList[i].TerrainParameterObjectList[0]);
            }
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

    private void PlaceSomeShit(TerrainInfo info) {
        var t = info._Terrain;
        var map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, 9);
        for (int i = 1; i < 4; i++) {
            foreach (var point in info.SeperatedBiomes[i]) {
                map[point.Z, point.X] = 1;
            }
        }
        t.terrainData.SetDetailLayer(0, 0, 0, map);
        map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, 1);

        foreach (var point in info.SeperatedBiomes[0]) {
            map[point.Z, point.X] = 1;
        }
        t.terrainData.SetDetailLayer(0, 0, 1, map);
    }
}
