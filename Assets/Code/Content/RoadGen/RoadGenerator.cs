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

    public struct SplineNodeHelper
    {
        public Spline spline;
        public List<RoadWaypoint> waypoints;

        public SplineNodeHelper(Spline spline, List<RoadWaypoint> waypoints)
        {
            this.spline = spline;
            this.waypoints = waypoints;
        }
    }

    public enum RoadSpreadDirection : int
    {
        kUp = 0,
        kDown = 1,
        kLeft = 2,
        kRight = 3
    }
    public const int MAX_X_OFFSET = 32;
    public const int MAX_Z_OFFSET = 32;

    public GameObject RoadTemp;
    public GameObject RoadHolder;

    public int TotalSpreadSize = 1000;
    public int RadiusSize = 3;
    private RoadSpreadDirection SpreadDirection = RoadSpreadDirection.kUp;
    public List<RoadWaypoint> RoadPointList;
    public List<SplineNodeHelper> Splines;
    public List<RoadWaypoint> GenerateRoad(TerrainInfo terrainInfo, Spline splineScript)
    {
        var backupTotalSpreadSize = TotalSpreadSize;
        RoadPointList = new List<RoadWaypoint>();
        Splines = new List<SplineNodeHelper>();
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

    private void SetRoadSegment(int index, Spline roadSpline, List<RoadWaypoint> waypoints, Vector3 currentNodePosition, RoadSpreadDirection dir)
    {
        var nodePosition = currentNodePosition;
        if (index < waypoints.Count - 1)
        {
            if (currentNodePosition == waypoints[index + 1].WaypointPosition)
            {
                var a = 1;
            }
            var nextNodePosition = waypoints[index + 1].WaypointPosition;
            var splineNode = new SplineNode(
                nodePosition,
                nodePosition + (nextNodePosition - nodePosition).normalized / 1000
            );
            roadSpline.AddNode(splineNode);
        }
        else if (index >= 1)
        {
            if (currentNodePosition == waypoints[index - 1].WaypointPosition)
            {
                var a = 1;
            }
            var previousNodePosition = waypoints[index - 1].WaypointPosition;
            var splineNode = new SplineNode(
                nodePosition,
                nodePosition + (nodePosition - previousNodePosition).normalized / 1000
            );
            roadSpline.AddNode(splineNode);
        }
    }

    private void DoRoadGeneration(TerrainInfo info, Spline splineScript)
    {
        int pointX = (int)Random.Range(MAX_X_OFFSET, info.TerrainWidth - MAX_X_OFFSET);
        int pointZ = (int)Random.Range(MAX_Z_OFFSET, info.TerrainHeight - MAX_Z_OFFSET);
        SpreadDirection = (RoadSpreadDirection)Random.Range(0, 4);

        for (int i = 0; i < TotalSpreadSize; i++)
        {
            // do coin flip
            bool coinFlip = Random.Range(0, 2) == 0;
            int CurrentSpreadSize = Random.Range(2, TotalSpreadSize + 2);
            var newRoad = Instantiate(splineScript.gameObject, Vector3.zero, Quaternion.identity);
            var newRoadSpline = newRoad.GetComponent<Spline>();
            newRoadSpline.nodes.Clear();
            newRoadSpline.curves.Clear();
            List<RoadWaypoint> currentSplineWaypoints = new List<RoadWaypoint>();
            Splines.Add(new SplineNodeHelper(newRoadSpline, currentSplineWaypoints));
            for (int j = 0; j < CurrentSpreadSize; j++)
            {
                var roadPointMapCoords = new Vector3(pointX, (int)info._Terrain.terrainData.GetHeight(pointX, pointZ), pointZ);
                RoadWaypoint waypoint = new RoadWaypoint(pointX, pointZ, roadPointMapCoords, TotalSpreadSize, SpreadDirection);
                foreach (var item in RoadPointList)
                {
                    if (item.WaypointPosition == roadPointMapCoords)
                    {
                        var a = 1;
                    }
                }
                RoadPointList.Add(waypoint);
                Splines[i].waypoints.Add(waypoint);
                TotalSpreadSize--;
                switch (SpreadDirection)
                {
                    // if we wanna go up, it means we gotta get a point at Vec3 (info.SeperatedBiomes[i][randomPoint].X + 1, terrainPositionY, info.SeperatedBiomes[i][randomPoint].Z);
                    case RoadSpreadDirection.kUp:
                        if (pointX < info.TerrainWidth - MAX_X_OFFSET)
                        {
                            pointX += 4;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kDown:
                        if (pointX > MAX_X_OFFSET)
                        {
                            pointX -= 4;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kLeft:
                        if (pointZ > MAX_Z_OFFSET)
                        {
                            pointZ -= 4;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kRight:
                        if (pointZ < info.TerrainHeight - MAX_Z_OFFSET)
                        {
                            pointZ += 4;
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
        foreach (var spline in Splines)
        {
            for (int i = 0; i < spline.waypoints.Count; i++)
            {
                var pos = spline.waypoints[i].WaypointPosition;
                SetRoadSegment(i, spline.spline, spline.waypoints, pos, spline.waypoints[i].RoadSpreadDirectionAtCreationTime);
                info.HeightMap[(int)pos.z, (int)pos.x] = info.HeightMap[(int)pos.z, (int)pos.x] - 0.05f;
                info.HeightMap[(int)pos.z - 1, (int)pos.x] = info.HeightMap[(int)pos.z - 1, (int)pos.x] - 0.05f;
                info.HeightMap[(int)pos.z + 1, (int)pos.x] = info.HeightMap[(int)pos.z + 1, (int)pos.x] - 0.05f;
            }
        }
        info._Terrain.terrainData.SetHeights(0, 0, info.HeightMap);
    }
    private void OnDrawGizmos()
    {
        //if (RoadPointList != null)
        //{
        //    foreach (var nodePosition in RoadPointList)
        //    {
        //        Gizmos.color = Color.red;
        //        Gizmos.DrawSphere(nodePosition.WaypointPosition, 1.25f);
        //    }
        //}
    }
    private (int, int) PickRoadWaypoint(int pointX, int pointZ)
    {
        // first get a point out of our list
        var newWaypoint = RoadPointList[RoadPointList.Count - 1];

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
            pointX = (int)Random.Range(MAX_X_OFFSET, info.TerrainWidth - MAX_X_OFFSET);
            pointZ = (int)Random.Range(MAX_Z_OFFSET, info.TerrainHeight - MAX_Z_OFFSET);
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
