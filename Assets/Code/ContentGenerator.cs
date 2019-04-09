using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContentGenerator : MonoBehaviour {
    public void GenerateBiomeContent(TerrainInfo info) {
        for (int i = 0; i < info.SeperatedBiomes.Keys.Count; i++) {
            PlaceGrass(info);
        }

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
    // wip
    private void PlaceGrass(TerrainInfo info) {
        int detailWidth = info._Terrain.terrainData.detailWidth;
        int detailHeight = info._Terrain.terrainData.detailHeight;

        int[,] details0 = new int[detailWidth, detailHeight];
        int[,] details1 = new int[detailWidth, detailHeight];

        int x, y;

        for (x = 0; x < detailWidth; x++) // divided by 4 just to show a test patch
        {
            for (y = 0; y < detailHeight; y++) // test patch
            {
                details0[y, x] = 0;
            }
        }

        info._Terrain.terrainData.SetDetailLayer(0, 0, 0, details0);
        //info._Terrain.terrainData.SetDetailLayer(0, 0, 1, details1);
    }
}
