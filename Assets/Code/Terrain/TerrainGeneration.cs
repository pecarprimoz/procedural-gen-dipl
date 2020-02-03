using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class TerrainGeneration : MonoBehaviour
{

    // Terrain parameters, change this if you want a bigger/higher terrain
    public int TerrainWidth = 128;
    public int TerrainHeight = 128;
    public int TerrainDepth = 64;
    public bool UseCustomTerrainSizeDefinitions = false;

    // Seasonal change component
    public SeasonalChange SeasonalChange;

    // Manager for content
    public ContentManager ContentManager;

    // ContentGenerator
    public ContentGenerator ContentGenerator;

    public RoadGenerator RoadGenerator;

    // Manager for terrain 
    public TerrainInfo TerrainInfo;

    public Spline SplineScript;

    void Start()
    {
        ContentManager = GetComponent<ContentManager>();
        SeasonalChange = GetComponent<SeasonalChange>();
        RoadGenerator = GetComponent<RoadGenerator>();
        TerrainInfo = new TerrainInfo(GetComponent<Terrain>());
        SplineScript = GetComponentInChildren<Spline>();
        if (UseCustomTerrainSizeDefinitions)
        {
            TerrainInfo.TerrainWidth = TerrainWidth;
            TerrainInfo.TerrainHeight = TerrainHeight;
            TerrainInfo.TerrainDepth = TerrainDepth;
        }
        // Figure out how to do this better, for now info has "manager" info (so we can use it elsewhere)
        TerrainInfo.ContentManager = ContentManager;
        ContentGenerator = GetComponent<ContentGenerator>();
        TerrainInfo.ContentGenerator = ContentGenerator;
        // Initialize manager, need to handle path for different platforms (OSX, Windows)
        SerializationManager.InitializeManager();
        // You can deserialize here and take the first NoiseParameter from the list if you dont want the default values
        InitializeTerrainWithPresetGeneration();
    }

    private void Update()
    {
        if (TerrainInfo.GenerationType == GenerationType.kUpdating)
        {
            InitializeTerrainWithPresetGeneration();
        }
        if (TerrainInfo.AreSeasonsChanging)
        {
            SeasonalChange.SeasonalChangeUpdate(TerrainInfo);
        }
    }

    public void InitializeTerrainWithPresetGeneration()
    {
        // this kills the performance, is rly pretty tho :D
        TerrainInfo._Terrain.detailObjectDistance = 1;
        TerrainInfo._Terrain.terrainData.heightmapResolution = TerrainInfo.TerrainWidth + 1;
        TerrainInfo._Terrain.terrainData.alphamapResolution = TerrainInfo.TerrainWidth;
        TerrainInfo._Terrain.terrainData.SetDetailResolution(TerrainInfo.TerrainWidth, 16);
        TerrainInfo._Terrain.terrainData.baseMapResolution = TerrainInfo.TerrainWidth * 2;
        TerrainInfo._Terrain.terrainData.size = new Vector3(TerrainInfo.TerrainWidth, TerrainInfo.TerrainDepth, TerrainInfo.TerrainHeight);
        GenerateTerrainFromPreset();
    }

    public void InitializeTerrain()
    {
        // this kills the performance, is rly pretty tho :D
        TerrainInfo._Terrain.detailObjectDistance = 50000;
        TerrainInfo._Terrain.terrainData.heightmapResolution = TerrainInfo.TerrainWidth + 1;
        TerrainInfo._Terrain.terrainData.alphamapResolution = TerrainInfo.TerrainWidth;
        TerrainInfo._Terrain.terrainData.SetDetailResolution(TerrainInfo.TerrainWidth, 16);
        TerrainInfo._Terrain.terrainData.baseMapResolution = TerrainInfo.TerrainWidth * 2;
        TerrainInfo._Terrain.terrainData.size = new Vector3(TerrainInfo.TerrainWidth, TerrainInfo.TerrainDepth, TerrainInfo.TerrainHeight);
    }


    public void GenerateTerrainAfterErosion()
    {
        // this can be parallelized 
        TerrainInfo._Terrain.terrainData.SetHeights(0, 0, TerrainInfo.HeightMap);
        TerrainInfo.TemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        TerrainInfo.MoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
    }

    public void GenerateTerrainFromPreset()
    {
        TerrainInfo.HeightMap = NoiseGeneration.GenerateTerrain(TerrainInfo);
        // this can be parallelized 
        TerrainInfo._Terrain.terrainData.SetHeights(0, 0, TerrainInfo.HeightMap);
        TerrainInfo.TemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        TerrainInfo.MoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
    }
    // A lot of stuff can be paralelized, stuff is decoupled for clarity when developing, in the end will be re-facced so shit that can get run paralel will
    public void GenerateTerrainOnDemand(bool onlyUseTerrainParameteters = false)
    {
        TerrainInfo.HeightMap = NoiseGeneration.GenerateTerrain(TerrainInfo);
        TerrainInfo._Terrain.terrainData.SetHeights(0, 0, TerrainInfo.HeightMap);
        TerrainInfo.TemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        TerrainInfo.MoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        if (!onlyUseTerrainParameteters)
        {
            ApplyErosion();
            AssignSplatMap.DoSplat(TerrainInfo);
            TerrainInfo.BiomeMap = BiomeGeneration.GenerateBiomeMap(TerrainInfo);
            ContentManager.InitializeBiomePlacementObjects(TerrainInfo);
            ContentGenerator.GenerateBiomeContent(TerrainInfo);
            var RoadList = RoadGenerator.GenerateRoad(TerrainInfo, SplineScript);
            ContentGenerator.PlaceHousesNearRoads(RoadList, TerrainInfo, ContentManager.GetParentContentObject());
        }
    }

    public void ApplyErosion()
    {
        if (TerrainInfo.GenerationType == GenerationType.kUpdating)
        {
            Debug.LogWarning("You are applying erosion in runtime. Switching to single mode");
            TerrainInfo.GenerationType = GenerationType.kSingleRun;
        }
        switch (TerrainInfo.ErosionType)
        {
            case ErosionGeneration.ErosionType.kThermalErosion:
                TerrainInfo.HeightMap = ErosionGeneration.ThermalErosion(TerrainInfo);
                break;
            case ErosionGeneration.ErosionType.kHydraulicErosion:
                TerrainInfo.HeightMap = ErosionGeneration.HydraulicErosion(TerrainInfo);
                break;
            case ErosionGeneration.ErosionType.kImprovedErosion:
                TerrainInfo.HeightMap = ErosionGeneration.ImprovedThermalErosion(TerrainInfo);
                break;
            case ErosionGeneration.ErosionType.kNone:
                break;
            default:
                break;
        }
        InitializeTerrain();
        GenerateTerrainAfterErosion();
    }
}
