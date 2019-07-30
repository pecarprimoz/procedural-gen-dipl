using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    private enum RoadSpreadDirection : int
    {
        kUp = 0,
        kDown = 1,
        kLeft = 2,
        kRight = 3
    }
    public GameObject RoadTemp;
    public GameObject RoadHolder;

    private int TotalSpreadSize = 0;
    private RoadSpreadDirection SpreadDirection = RoadSpreadDirection.kUp;

    public void GenerateRoad(TerrainInfo terrainInfo)
    {
        // placeholder, just for testing
        TotalSpreadSize = Random.Range(100, 120);
        DoRoadGeneration(terrainInfo);
    }
    // issues
    // road steepnes isnt calculated correctly and the angle at witch we place the road is wrong
    // we can go deeper with this, instead of just placing roads, track them, when we get cut off (either we are gonna be OOB next iteration 
    // or our currentSpreadSize ran out, take a random road point and iterate in the OPPOSITE direction that we previously went
    // psedocode
    // track all points and in which direction we were spreading
    // when we reach our end at currentSpreadSize
    // pick random road that we placed
    // check in what direction we went (if we went up, we ignore directions up and down, but randomly pick left or right)
    // continue the iteration
    private void DoRoadGeneration(TerrainInfo info)
    {
        int pointX = (int)Random.Range(0, info.TerrainWidth);
        int pointZ = (int)Random.Range(0, info.TerrainHeight);
        // how long the current road will be 
        for (int i = 0; i < TotalSpreadSize; i++)
        {
            int CurrentSpreadSize = Random.Range(1, TotalSpreadSize / 2);
            SpreadDirection = (RoadSpreadDirection)Random.Range(0, 4);
            for (int j = 0; j < CurrentSpreadSize; j++)
            {
                Vector3 angle = new Vector3(0, 0, -info._Terrain.terrainData.GetSteepness(pointX / info.TerrainWidth, pointZ / info.TerrainHeight));
                var roadPointMapCoords = new Vector3(pointX, (int)info._Terrain.terrainData.GetHeight(pointX, pointZ), pointZ);
                Instantiate(RoadTemp, roadPointMapCoords, Quaternion.Euler(angle), RoadHolder.transform);
                switch (SpreadDirection)
                {
                    // if we wanna go up, it means we gotta get a point at Vec3 (info.SeperatedBiomes[i][randomPoint].X + 1, terrainPositionY, info.SeperatedBiomes[i][randomPoint].Z);
                    case RoadSpreadDirection.kUp:
                        if (pointX < info.TerrainWidth)
                        {
                            pointX += 1;
                        }
                        else { }
                        break;
                    case RoadSpreadDirection.kDown:
                        if (pointX > 0)
                        {
                            pointX -= 1;
                        }
                        else { }
                        break;
                    case RoadSpreadDirection.kLeft:
                        if (pointZ > 0)
                        {
                            pointZ -= 1;
                        }
                        else { }
                        break;
                    case RoadSpreadDirection.kRight:
                        if (pointZ < info.TerrainHeight)
                        {
                            pointZ += 1;
                        }
                        else { }
                        break;
                    default:
                        break;
                }
                TotalSpreadSize--;
            }
        }

    }


}
