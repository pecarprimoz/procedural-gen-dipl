using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    private struct RoadWaypoint
    {
        public int PointX;
        public int PointZ;
        // xyz coordinates for position
        public Vector3 WaypointPosition;
        public int TotalRoadLengthAtCreationTime;
        public RoadSpreadDirection RoadSpreadDirectionAtCreationTime;

        public RoadWaypoint(int pointX, int pointZ, Vector3 waypointPosition, int totalRoadLengthAtCreationTime, RoadSpreadDirection roadSpreadDirectionAtCreationTime)
        {
            PointX = pointX;
            PointZ = pointZ;
            WaypointPosition = waypointPosition;
            TotalRoadLengthAtCreationTime = totalRoadLengthAtCreationTime;
            RoadSpreadDirectionAtCreationTime = roadSpreadDirectionAtCreationTime;
        }
    }
    private enum RoadSpreadDirection : int
    {
        kUp = 0,
        kDown = 1,
        kLeft = 2,
        kRight = 3
    }
    public GameObject RoadTemp;
    public GameObject RoadHolder;

    public int TotalSpreadSize = 1000;
    public int RadiusSize = 3;
    private RoadSpreadDirection SpreadDirection = RoadSpreadDirection.kUp;
    private List<RoadWaypoint> RoadPointList;
    public void GenerateRoad(TerrainInfo terrainInfo)
    {
        var backupTotalSpreadSize = TotalSpreadSize;
        RoadPointList = new List<RoadWaypoint>();
        DoRoadGeneration(terrainInfo);
        TotalSpreadSize = backupTotalSpreadSize;
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
    // done
    // improvements
    // dont generate roads next to each other, add a radius check where roads should be generated, if roads in radius pick a new point
    private void DoRoadGeneration(TerrainInfo info)
    {
        // get the first two points from which we are gonna generate roads
        int pointX = (int)Random.Range(0, info.TerrainWidth);
        int pointZ = (int)Random.Range(0, info.TerrainHeight);
        SpreadDirection = (RoadSpreadDirection)Random.Range(0, 4);
        // how long the current road will be 
        for (int i = 0; i < TotalSpreadSize; i++)
        {
            // do coin flip
            bool coinFlip = Random.Range(0, 2) == 0;
            int CurrentSpreadSize = Random.Range(1, TotalSpreadSize / 4);
            Debug.Log(CurrentSpreadSize);
            for (int j = 0; j < CurrentSpreadSize; j++)
            {
                //Vector3 angle = new Vector3(0, 0, info._Terrain.terrainData.GetSteepness(pointX / info.TerrainWidth, pointZ / info.TerrainHeight));
                // create a new waypoint 
                var roadPointMapCoords = new Vector3(pointX, (int)info._Terrain.terrainData.GetHeight(pointX, pointZ), pointZ);
                RoadWaypoint waypoint = new RoadWaypoint(pointX, pointZ, roadPointMapCoords, TotalSpreadSize, SpreadDirection);
                RoadPointList.Add(waypoint);
                Instantiate(RoadTemp, waypoint.WaypointPosition, Quaternion.identity, RoadHolder.transform);
                TotalSpreadSize--;
                switch (SpreadDirection)
                {
                    // if we wanna go up, it means we gotta get a point at Vec3 (info.SeperatedBiomes[i][randomPoint].X + 1, terrainPositionY, info.SeperatedBiomes[i][randomPoint].Z);
                    case RoadSpreadDirection.kUp:
                        if (pointX < info.TerrainWidth - 1)
                        {
                            pointX += 1;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kDown:
                        if (pointX > 1)
                        {
                            pointX -= 1;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kLeft:
                        if (pointZ > 1)
                        {
                            pointZ -= 1;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kRight:
                        if (pointZ < info.TerrainHeight - 1)
                        {
                            pointZ += 1;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (coinFlip)
            {
                (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
            }
            else
            {
                (pointX, pointZ) = PickRandomNotInRangeWaypoint(info);
            }
        }
    }
    private (int, int) PickRoadWaypoint(int pointX, int pointZ)
    {
        // first get a point out of our list
        var newWaypoint = RoadPointList[Random.Range(0, RoadPointList.Count)];

        pointX = newWaypoint.PointX;
        pointZ = newWaypoint.PointZ;
        if (newWaypoint.RoadSpreadDirectionAtCreationTime == RoadSpreadDirection.kUp ||
            newWaypoint.RoadSpreadDirectionAtCreationTime == RoadSpreadDirection.kDown)
        {
            SpreadDirection = (RoadSpreadDirection)Random.Range(2, 4);
        }
        else
        {
            SpreadDirection = (RoadSpreadDirection)Random.Range(0, 2);
        }
        return (pointX, pointZ);
    }
    private (int, int) PickRandomNotInRangeWaypoint(TerrainInfo info)
    {
        // get the first two points from which we are gonna generate roads
        int pointX = 0;
        int pointZ = 0;
        int maxIters = 1000;
        int i = 0;
        do
        {
            pointX = (int)Random.Range(0, info.TerrainWidth);
            pointZ = (int)Random.Range(0, info.TerrainHeight);
            i++;
            if (i > maxIters)
            {
                break;
            }
        }
        while (AreAnyRoadsInRadius(info, pointX, pointZ));
        return (pointX, pointZ);
    }
    private bool AreAnyRoadsInRadius(TerrainInfo info, int pointX, int pointZ)
    {
        if (pointX + RadiusSize > info.TerrainWidth || pointZ + RadiusSize > info.TerrainHeight)
        {
            return true;
        }
        for (int i = 0; i < RoadPointList.Count; i++)
        {
            var roadPoint = RoadPointList[i];
            for (int j = 0; j < RadiusSize + 1; j++)
            {
                if (pointX + RadiusSize == roadPoint.PointX || pointZ + RadiusSize == roadPoint.PointZ)
                {
                    return true;
                }
            }
        }
        Debug.Log("Found start point!");
        return false;
    }

}
