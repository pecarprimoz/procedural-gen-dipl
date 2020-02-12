using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System.Linq;

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
    public const int MAX_X_OFFSET = 16;
    public const int MAX_Z_OFFSET = 16;

    public GameObject RoadTemp;
    public GameObject RoadHolder;

    public int TotalSpreadSize = 1000;
    public int RadiusSize = 3;
    private RoadSpreadDirection SpreadDirection = RoadSpreadDirection.kUp;
    public List<RoadWaypoint> RoadPointList;
    public static List<RoadWaypoint> AllRoadPoints = new List<RoadWaypoint>();
    public List<SplineNodeHelper> Splines;

    private bool DirectionChanged = false;
    private int SplineSize = 5;
    private int MinRoadSize = 5;

    public Texture2D RoadTexture;

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
        return Vector3.zero;
    }

    private void SetRoadSegment(int index, Spline roadSpline, List<RoadWaypoint> waypoints, Vector3 currentNodePosition)
    {
        var nodePosition = currentNodePosition;
        if (index < waypoints.Count - 1)
        {
            var nextNodePosition = waypoints[index + 1].WaypointPosition;
            var splineNode = new SplineNode(
                nodePosition,
                nodePosition + (nextNodePosition - nodePosition).normalized / 1000
            );
            roadSpline.AddNode(splineNode);
        }
        else if (index >= 1)
        {
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

        // total spread size is the total road size
        for (int i = 0; i < TotalSpreadSize; i++)
        {
            if (TotalSpreadSize <= 10) TotalSpreadSize = 11;
            // do coin flip
            bool coinFlip = Random.Range(0, 2) == 0;
            // current road segment size, min 2 due to splines
            int CurrentSpreadSize = Random.Range(TotalSpreadSize / 2, TotalSpreadSize);

            if (CurrentSpreadSize < MinRoadSize)
            {
                break;
            }
            var newRoad = Instantiate(splineScript.gameObject, Vector3.zero, Quaternion.identity);
            newRoad.SetActive(false);
            var newRoadSpline = newRoad.GetComponent<Spline>();
            newRoadSpline.nodes.Clear();
            newRoadSpline.curves.Clear();
            List<RoadWaypoint> currentSplineWaypoints = new List<RoadWaypoint>();
            Splines.Add(new SplineNodeHelper(newRoadSpline, currentSplineWaypoints));
            for (int j = 0; j < CurrentSpreadSize; j++)
            {
                // prev iter we changed direction, break off this spline, create a new one and continue
                if (DirectionChanged && j != 0)
                {
                    DirectionChanged = false;
                    break;
                }
                //Debug.LogFormat("Movement dir: {0}", SpreadDirection.ToString());
                var roadPointMapCoords = new Vector3(pointX, (int)info._Terrain.terrainData.GetHeight(pointX, pointZ), pointZ);
                RoadWaypoint waypoint = new RoadWaypoint(pointX, pointZ, roadPointMapCoords, CurrentSpreadSize, SpreadDirection);
                RoadPointList.Add(waypoint);
                AllRoadPoints.Add(waypoint);
                Splines[i].waypoints.Add(waypoint);
                TotalSpreadSize--;
                switch (SpreadDirection)
                {
                    // if we wanna go up, it means we gotta get a point at Vec3 (info.SeperatedBiomes[i][randomPoint].X + 1, terrainPositionY, info.SeperatedBiomes[i][randomPoint].Z);
                    case RoadSpreadDirection.kUp:
                        if (pointX + SplineSize < info.TerrainWidth - MAX_X_OFFSET)
                        {
                            pointX += SplineSize;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kDown:
                        if (pointX - SplineSize > MAX_X_OFFSET)
                        {
                            pointX -= SplineSize;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kLeft:
                        if (pointZ - SplineSize > MAX_Z_OFFSET)
                        {
                            pointZ -= SplineSize;
                        }
                        else
                        {
                            (pointX, pointZ) = PickRoadWaypoint(pointX, pointZ);
                            break;
                        }
                        break;
                    case RoadSpreadDirection.kRight:
                        if (pointZ + SplineSize < info.TerrainHeight - MAX_Z_OFFSET)
                        {
                            pointZ += SplineSize;
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
                SetRoadSegment(i, spline.spline, spline.waypoints, pos);
            }
        }
        info._Terrain.terrainData.SetHeights(0, 0, info.HeightMap);
    }

    public bool IsRoadOnCoordinates(int x, int z)
    {
        foreach (var roadPoint in AllRoadPoints)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (roadPoint.PointX == x + i && roadPoint.PointZ == z + j ||
                        roadPoint.PointX == x - i && roadPoint.PointZ == z - j ||
                        roadPoint.PointX == x + i && roadPoint.PointZ == z - j ||
                        roadPoint.PointX == x - i && roadPoint.PointZ == z + j ||
                        roadPoint.PointX == x && roadPoint.PointZ == z)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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
            if (SpreadDirection == RoadSpreadDirection.kLeft)
            {
                pointZ -= SplineSize;
            }
            else
            {
                pointZ += SplineSize;
            }
            DirectionChanged = true;
        }
        else
        {
            SpreadDirection = (RoadSpreadDirection)Random.Range(0, 2);
            if (SpreadDirection == RoadSpreadDirection.kUp)
            {
                pointX += SplineSize;
            }
            else
            {
                pointX -= SplineSize;
            }
            DirectionChanged = true;
        }
        return (pointX, pointZ);
    }

    public bool WillSpreadToWater(TerrainInfo info, int cX, int cZ)
    {
        var water_points = info.SeperatedBiomes[info.SeperatedBiomes.Keys.Count - 1];
        if (water_points.Where(x => x.X == cX).Count() > 0)
        {
            return true;
        }
        if (water_points.Where(x => x.Z == cZ).Count() > 0)
        {
            return true;
        }
        return false;
    }

    private (int, int) PickRandomNotInRangeWaypoint(TerrainInfo info)
    {
        // get the first two points from which we are gonna generate roads
        var water_points = info.SeperatedBiomes[info.SeperatedBiomes.Keys.Count - 1];
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
                Debug.Log("Couldnt find a point without any roads in radius.");
                // in this case, we need to find a point where no roads exist
                break;
            }
            if (water_points.Where(x => x.X == pointX).Count() > 0)
            {
                continue;
            }
            if (water_points.Where(x => x.Z == pointZ).Count() > 0)
            {
                continue;
            }
        }
        while (AreAnyRoadsInRadius(info, pointX, pointZ));
        return (pointX, pointZ);
    }
    private bool AreAnyRoadsInRadius(TerrainInfo info, int pointX, int pointZ)
    {
        if (pointX + RadiusSize > info.TerrainWidth || pointZ + RadiusSize > info.TerrainHeight &&
            pointX - RadiusSize < 0 || pointZ - RadiusSize < 0)
        {
            return true;
        }
        for (int i = 0; i < AllRoadPoints.Count; i++)
        {
            var roadPoint = AllRoadPoints[i];
            for (int j = -RadiusSize; j < RadiusSize; j++)
            {
                // go trough square from pointX, pointZ
                (int pX1, int pZ1) = (pointX + j, pointZ + j);
                (int pX2, int pZ2) = (pointX, pointZ + j);
                (int pX3, int pZ3) = (pointX + j, pointZ);
                (int pX4, int pZ4) = (pointX + j, pointZ);

                // if any point in roadPointList matches with the coordinates of pX pZ, means we found a road in our vicinity
                if (pX1 == roadPoint.PointX || pZ1 == roadPoint.PointZ ||
                    pX2 == roadPoint.PointX || pZ2 == roadPoint.PointZ ||
                    pX3 == roadPoint.PointX || pZ3 == roadPoint.PointZ ||
                    pX4 == roadPoint.PointX || pZ4 == roadPoint.PointZ)
                {
                    Debug.LogFormat("Found a road segment");
                    return true;
                }
            }
        }
        Debug.Log("Found start point!");
        return false;
    }

}
