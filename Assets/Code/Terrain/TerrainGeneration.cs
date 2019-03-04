using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {

    public TerrainInfo TerrainInfo;

    void Start() {
        TerrainInfo = new TerrainInfo(GetComponent<Terrain>());
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

    public void GenerateTerrainOnDemand() {
        TerrainInfo.HeightMap = NoiseGeneration.GenerateTerrain(TerrainInfo);
        TerrainInfo._Terrain.terrainData.SetHeights(0, 0, TerrainInfo.HeightMap);
        TerrainInfo.TemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        TerrainInfo.MoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight, TerrainInfo.HeightMap);
        ApplyErosion();
        AssignSplatMap.DoSplat(TerrainInfo);
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
