using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContentGenerator : MonoBehaviour
{
    public GameObject House;
    public void GenerateBiomeContent(TerrainInfo info)
    {
        // testing for some biomes to get grass 
        PlaceNature(info);

        // Totaly noob approach, dont use blue noise or anything, just select a random point and place an item therev
        for (int i = 0; i < info.SeperatedBiomes.Keys.Count; i++)
        {
            // instead of taking the first item, take random ?
            for (int j = 0; j < info.TerrainParameterList[i].ObjectListCount; j++)
            {
                // iterate trough all the objects, then place them, first pass is for random ground bullshit
                PlaceBiomeContent(info, i, info.TerrainParameterList[i].TerrainParameterObjectCount[j], info.TerrainParameterList[i].TerrainParameterObjectList[j]);
            }
        }
    }

    public void PlaceHousesNearRoads(List<RoadGenerator.RoadWaypoint> roadWaypoints, TerrainInfo info, GameObject parent)
    {
        for (int i = 0; i < roadWaypoints.Count; i++)
        {
            var waypoint = roadWaypoints[i];
            Vector3 housePos = GetHousePosition(info._Terrain.terrainData, waypoint);
            Instantiate(House, housePos, Quaternion.identity, parent.transform);
        }
    }

    public Vector3 GetHousePosition(TerrainData data, RoadGenerator.RoadWaypoint point)
    {
        Vector3 housePos = new Vector3(point.PointX, 0, point.PointZ);
        switch (point.RoadSpreadDirectionAtCreationTime)
        {
            case RoadGenerator.RoadSpreadDirection.kUp:
                housePos.z += 3;
                break;
            case RoadGenerator.RoadSpreadDirection.kDown:
                housePos.z -= 3;
                break;
            case RoadGenerator.RoadSpreadDirection.kLeft:
                housePos.x += 3;
                break;
            case RoadGenerator.RoadSpreadDirection.kRight:
                housePos.x -= 3;
                break;
        }
        housePos.y = data.GetHeight((int)housePos.x, (int)housePos.z) + 4;
        return housePos;
    }


    // placement of objects is invalid, since biomePoint.X and biomePoint.Z are ACTUAL INDICES IN THE ARRAY, NOT POINTS, TODO
    private void PlaceBiomeContent(TerrainInfo info, int biomeType, int objCount, GameObject placeableObject)
    {
        // good for debugging
        //for (int i = 0; i < info.SeperatedBiomes[biomeType].Count; i++) {
        //    var biomePoint = info.SeperatedBiomes[biomeType][i];
        //    int terrainPositionY = (int)info._Terrain.terrainData.GetHeight(biomePoint.X, biomePoint.Z);
        //    Instantiate(placeableObject, new Vector3(biomePoint.X, terrainPositionY, biomePoint.Z), Quaternion.identity);
        //}
        for (int i = 0; i < objCount; i++)
        {
            int randomPoint = UnityEngine.Random.Range(0, info.SeperatedBiomes[biomeType].Count - 1);
            var biomePoint = info.SeperatedBiomes[biomeType][randomPoint];
            if (!biomePoint.ContainsItem)
            {
                int terrainPositionY = (int)info._Terrain.terrainData.GetHeight(biomePoint.X, biomePoint.Z);
                Instantiate(placeableObject, new Vector3(biomePoint.X, terrainPositionY, biomePoint.Z), Quaternion.identity, info.ContentManager.BiomeParentGameObjects[biomeType].transform);
                // to avoid placing multiple objects on one point, since we are doing it randomly
                info.SeperatedBiomes[biomeType][randomPoint].ContainsItem = true;
            }
        }
    }
    // note to self
    // if you want to add additional vegetation & floura, has to be done by unity's Terrain Details, 4 tab on the Terrain component

    private int GetDensity(SeasonType season, int min, int max)
    {
        if (season == SeasonType.kWinter)
        {
            return 0;
        }
        return UnityEngine.Random.Range(13, 16);
    }

    public void PlaceNature(TerrainInfo info)
    {
        // 0 - 5 indices are for grass atm, 3-5 are flowers (less dense patches)
        var t = info._Terrain;
        // go trough the biomes that need grass
        for (int j = 0; j < 3; j++)
        {
            var map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, j);
            for (int i = 1; i < 5; i++)
            {
                // go trough the biome points
                foreach (var point in info.SeperatedBiomes[i])
                {
                    // go trough the detail layers
                    // https://answers.unity.com/questions/182147/terraindatagetdetaillayer.html

                    map[point.Z, point.X] = GetDensity(info.CurrentSeason, 13, 16);
                }
            }
        }

        // add the flowers
        for (int j = 3; j < 6; j++)
        {
            var map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, j);
            for (int i = 1; i < 5; i++)
            {
                // go trough the biome points
                foreach (var point in info.SeperatedBiomes[i])
                {
                    map[point.Z, point.X] = GetDensity(info.CurrentSeason, 1, 5);
                }
            }
            t.terrainData.SetDetailLayer(0, 0, j, map);
        }

    }
}
