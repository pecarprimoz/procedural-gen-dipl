using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
public class RoadGenerator : MonoBehaviour
{
    public struct RoadWaypoint
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
    public enum RoadSpreadDirection : int
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
    public List<RoadWaypoint> RoadPointList;
    public List<RoadWaypoint> GenerateRoad(TerrainInfo terrainInfo, Spline splineScript)
    {
        var backupTotalSpreadSize = TotalSpreadSize;
        RoadPointList = new List<RoadWaypoint>();
        DoRoadGeneration(terrainInfo, splineScript);
        TotalSpreadSize = backupTotalSpreadSize;
        return RoadPointList;
    }

    public Vector3 GetDirection(RoadSpreadDirection direction)
    {
        switch (direction)
        {
            case (RoadSpreadDirection.kDown):
                return new Vector3(-1, 0, 0);
            case RoadSpreadDirection.kUp:
                return new Vector3(1, 0, 0);
            case RoadSpreadDirection.kLeft:
                return new Vector3(0, 0, -1);
            case RoadSpreadDirection.kRight:
                return new Vector3(0, 0, 1);
        }
        return Vector3.forward;
    }

    private void SetRoadSemgent(int startSegmentIdx, Spline roadSpline, Vector3 currentNodePosition, RoadSpreadDirection dir)
    {
        var moveAmnt = GetDirection(dir) * 2;
        Vector3 firstNodePosition = currentNodePosition;
        Vector3 secondNodePosition = firstNodePosition + moveAmnt;

        roadSpline.nodes[startSegmentIdx].Position = firstNodePosition;
        roadSpline.nodes[startSegmentIdx].Direction = Vector3.Normalize(secondNodePosition - firstNodePosition);
        roadSpline.nodes[startSegmentIdx + 1].Position = secondNodePosition;
        roadSpline.nodes[startSegmentIdx + 1].Direction = Vector3.Normalize(secondNodePosition + moveAmnt - secondNodePosition);
    }

    private void DoRoadGeneration(TerrainInfo info, Spline splineScript)
    {
        // get the first two points from which we are gonna generate roads
        // init start point

        int pointX = (int)Random.Range(0, info.TerrainWidth);
        int pointZ = (int)Random.Range(0, info.TerrainHeight);

        var newRoad = Instantiate(splineScript.gameObject, Vector3.zero, Quaternion.identity);
        var newRoadSpline = newRoad.GetComponent<Spline>();

        SpreadDirection = (RoadSpreadDirection)Random.Range(0, 4);
        // create instance of this gobject and place it
        SetRoadSemgent(0, newRoadSpline, new Vector3(pointX, info._Terrain.terrainData.GetHeight(pointX, pointZ), pointZ), SpreadDirection);
        RoadPointList.Add(new RoadWaypoint((int)newRoadSpline.nodes[0].Position.x, (int)newRoadSpline.nodes[0].Position.y, newRoadSpline.nodes[0].Position, TotalSpreadSize - 1, SpreadDirection));
        int nextRoadSegment = 1;
        // how long the current road will be 
        for (int i = 0; i < TotalSpreadSize; i++)
        {
            // do coin flip
            bool coinFlip = Random.Range(0, 2) == 0;
            int CurrentSpreadSize = Random.Range(1, TotalSpreadSize);
            Debug.Log(CurrentSpreadSize);
            for (int j = 0; j < CurrentSpreadSize; j++)
            {
                var roadPointMapCoords = new Vector3(pointX, (int)info._Terrain.terrainData.GetHeight(pointX, pointZ), pointZ);

                newRoadSpline.AddNode(new SplineNode(Vector3.zero, Vector3.zero));
                SetRoadSemgent(nextRoadSegment, newRoadSpline, roadPointMapCoords, SpreadDirection);
                //Instantiate(RoadTemp, waypoint.WaypointPosition, Quaternion.identity, RoadHolder.transform);
                nextRoadSegment++;
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
