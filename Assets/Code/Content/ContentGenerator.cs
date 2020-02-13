using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ContentGenerator : MonoBehaviour
{
    public static List<Vector3> CurrentHouses = new List<Vector3>();
    public GameObject House;
    public void GenerateBiomeContent(TerrainInfo info)
    {
        // testing for some biomes to get grass 
        if (info.ApplyDetail)
        {
            PlaceNature(info);
        }
        if (info.ApplyContent)
        {
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
    }

    public float[,] GetFlattendTerrain(float[,] hm, int x, int z, int width)
    {
        List<float> norm = new List<float>();
        float[,] flatten = new float[width, width];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float v = hm[x + i, z + j];
                norm.Add(v);
            }
        }
        float e_v = norm.Sum() / norm.Count();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                flatten[i, j] = e_v;
            }
        }
        return flatten;
    }

    public bool IsAnyHouseNear(float min_dist, Vector3 housePos, List<Vector3> houses)
    {
        foreach (var curHouse in houses)
        {
            if (Vector2.Distance(new Vector2(curHouse.x, curHouse.z), new Vector2(housePos.x, housePos.z)) < min_dist)
            {
                return true;
            }
        }
        return false;
    }

    public void PlaceHousesNearRoads(List<RoadGenerator.RoadWaypoint> roadWaypoints, TerrainInfo info, GameObject parent)
    {
        if (!info.ApplyRoads)
        {
            return;
        }
        int cnt = 0;
        int i = 0;
        while (cnt < 10)
        {
            if (i == roadWaypoints.Count - 1) { break; }
            var waypoint = roadWaypoints[i];
            Vector3 housePos = GetHousePosition(info._Terrain.terrainData, waypoint, 9);
            if (!IsAnyHouseNear(15, housePos, CurrentHouses))
            {
                info._Terrain.terrainData.SetHeights((int)housePos.x - 4, (int)housePos.z - 4, GetFlattendTerrain(info.HeightMap, (int)housePos.z, (int)housePos.x, 8));
                var h = info._Terrain.terrainData.GetHeight((int)housePos.x + 2, (int)housePos.z + 2);
                housePos.y = h;
                Instantiate(House, housePos, Quaternion.identity, parent.transform);
                cnt++;
                CurrentHouses.Add(housePos);
            }
            i++;
        }
    }

    public Vector3 GetHousePosition(TerrainData data, RoadGenerator.RoadWaypoint point, int offset)
    {
        Vector3 housePos = new Vector3(point.PointX, 0, point.PointZ);
        switch (point.RoadSpreadDirectionAtCreationTime)
        {
            case RoadGenerator.RoadSpreadDirection.kUp:
                housePos.z += offset;
                break;
            case RoadGenerator.RoadSpreadDirection.kDown:
                housePos.z -= offset;
                break;
            case RoadGenerator.RoadSpreadDirection.kLeft:
                housePos.x += offset;
                break;
            case RoadGenerator.RoadSpreadDirection.kRight:
                housePos.x -= offset;
                break;
        }
        housePos.y = data.GetHeight((int)housePos.x, (int)housePos.z);
        return housePos;
    }


    // placement of objects is invalid, since biomePoint.X and biomePoint.Z are ACTUAL INDICES IN THE ARRAY, NOT POINTS, TODO
    private void PlaceBiomeContent(TerrainInfo info, int biomeType, int objCount, GameObject placeableObject)
    {
        for (int i = 0; i < objCount; i++)
        {
            int randomPoint = UnityEngine.Random.Range(0, info.SeperatedBiomes[biomeType].Count - 1);
            var biomePoint = info.SeperatedBiomes[biomeType][randomPoint];
            if (!biomePoint.ContainsItem && !info.RoadGenerator.IsRoadOnCoordinates(biomePoint.X, biomePoint.Z, 5) && !IsAnyHouseNear(5, new Vector3(biomePoint.X, 0 ,biomePoint.Z), CurrentHouses))
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
        return UnityEngine.Random.Range(min, max);
    }
    public void ClearOutDetails(ref int[,] map, int x, int y)
    {
        try
        {
            map[x, y] = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int h = 0; h < 6; h++)
                    {
                        map[x + i, y + j] = 0;
                        map[x - i, y - j] = 0;
                        map[x + i, y - j] = 0;
                        map[x - i, y + j] = 0;
                    }
                }
            }
        }
        catch (Exception e) { }
    }
    public void PlaceNature(TerrainInfo info)
    {
        var tmpContentDelete = new List<(int, int)>();
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
                    if (info.RoadGenerator.IsRoadOnCoordinates(point.X, point.Z))
                    {
                        tmpContentDelete.Add((point.Z, point.X));
                        continue;
                    }
                    map[point.Z, point.X] = GetDensity(info.CurrentSeason, 13, 16);
                }
            }
            foreach (var item in tmpContentDelete)
            {
                ClearOutDetails(ref map, item.Item1, item.Item2);
            }
            tmpContentDelete.Clear();
            t.terrainData.SetDetailLayer(0, 0, j, map);
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
                    if (info.RoadGenerator.IsRoadOnCoordinates(point.X, point.Z))
                    {
                        tmpContentDelete.Add((point.Z, point.X));
                        continue;
                    }
                    map[point.Z, point.X] = GetDensity(info.CurrentSeason, 1, 5);
                }
            }
            foreach (var item in tmpContentDelete)
            {
                ClearOutDetails(ref map, item.Item1, item.Item2);
            }
            t.terrainData.SetDetailLayer(0, 0, j, map);
        }
    }
}
