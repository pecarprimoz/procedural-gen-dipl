﻿using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {

    // Terrain parameters, change this if you want a bigger/higher terrain
    public int TerrainWidth = 128;
    public int TerrainHeight = 128;
    public int TerrainDepth = 64;
    public bool UseCustomTerrainSizeDefinitions = false;

    // Manager for content
    public ContentManager ContentManager;

    // ContentGenerator
    public ContentGenerator ContentGenerator;

    // Manager for terrain 
    public TerrainInfo TerrainInfo;

    void Start() {
        ContentManager = GetComponent<ContentManager>();
        TerrainInfo = new TerrainInfo(GetComponent<Terrain>());
        if (UseCustomTerrainSizeDefinitions) {
            TerrainInfo.TerrainWidth = TerrainWidth;
            TerrainInfo.TerrainHeight = TerrainHeight;
            TerrainInfo.TerrainDepth = TerrainDepth;
        }
        // Figure out how to do this better, for now info has "manager" info (so we can use it elsewhere)
        TerrainInfo.ContentManager = ContentManager;
        ContentGenerator = GetComponent<ContentGenerator>();
        // Initialize manager, need to handle path for different platforms (OSX, Windows)
        SerializationManager.InitializeManager();
        // You can deserialize here and take the first NoiseParameter from the list if you dont want the default values
        InitializeTerrain();
    }

    private void Update() {
        if (TerrainInfo.GenerationType == GenerationType.kUpdating) {
            InitializeTerrain();
        }
    }

    public void InitializeTerrain() {
        // this kills the performance, is rly pretty tho :D
        TerrainInfo._Terrain.detailObjectDistance = 50000;
        TerrainInfo._Terrain.terrainData.heightmapResolution = TerrainInfo.TerrainWidth + 1;
        TerrainInfo._Terrain.terrainData.alphamapResolution = TerrainInfo.TerrainWidth;
        TerrainInfo._Terrain.terrainData.SetDetailResolution(TerrainInfo.TerrainWidth, 16);
        TerrainInfo._Terrain.terrainData.baseMapResolution = TerrainInfo.TerrainWidth * 2;
        TerrainInfo._Terrain.terrainData.size = new Vector3(TerrainInfo.TerrainWidth, TerrainInfo.TerrainDepth, TerrainInfo.TerrainHeight);
        GenerateTerrainFromPreset();
    }

    public void GenerateTerrainFromPreset() {
        TerrainInfo.HeightMap = NoiseGeneration.GenerateTerrain(TerrainInfo);
        // this can be parallelized 
        TerrainInfo._Terrain.terrainData.SetHeights(0, 0, TerrainInfo.HeightMap);
        TerrainInfo.TemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        TerrainInfo.MoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
    }
    // A lot of stuff can be paralelized, stuff is decoupled for clarity when developing, in the end will be re-facced so shit that can get run paralel will
    public void GenerateTerrainOnDemand() {
        TerrainInfo.HeightMap = NoiseGeneration.GenerateTerrain(TerrainInfo);
        TerrainInfo._Terrain.terrainData.SetHeights(0, 0, TerrainInfo.HeightMap);
        TerrainInfo.TemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        TerrainInfo.MoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        ApplyErosion();
        AssignSplatMap.DoSplat(TerrainInfo);
        TerrainInfo.BiomeMap = BiomeGeneration.GenerateBiomeMap(TerrainInfo);
        ContentManager.InitializeBiomePlacementObjects(TerrainInfo);
        ContentGenerator.GenerateBiomeContent(TerrainInfo);
    }

    public void AddObjectsToTerrain() {

    }

    public void ApplyErosion() {
        if (TerrainInfo.GenerationType == GenerationType.kUpdating) {
            Debug.LogWarning("You are applying erosion in runtime. Switching to single mode");
            TerrainInfo.GenerationType = GenerationType.kSingleRun;
        }
        switch (TerrainInfo.ErosionType) {
            case ErosionGeneration.ErosionType.kThermalErosion:
                TerrainInfo.HeightMap = ErosionGeneration.ThermalErosion(TerrainInfo);
                break;
            case ErosionGeneration.ErosionType.kHydraulicErosion:
                ErosionGeneration.ImprovedThermalErosion(TerrainInfo);
                break;
            case ErosionGeneration.ErosionType.kImprovedErosion:
                ErosionGeneration.HydraulicErosion(TerrainInfo);
                break;
            case ErosionGeneration.ErosionType.kNone:
                break;
            default:
                break;
        }
    }
}
