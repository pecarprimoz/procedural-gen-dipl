using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {
    public enum GenerationType {
        kUpdating,
        kSingleRun
    }

    public ErosionGeneration.ErosionType _ErosionType = ErosionGeneration.ErosionType.kThermalErosion;
    public int ErosionIterations = 50;
    public bool RuntimeErosion = false;

    // Terrain parameters, change this if you want a bigger/higher terrain
    public readonly int TerrainWidth = 128;
    public readonly int TerrainHeight = 128;
    public readonly int TerrainDepth = 64;

    // Parameters for perlin generations, overwritten with editor parameters (see Editor/ParameterEditorWidget)
    public float NoiseScale = 100.0f;
    public float BaseFrequency = 1.0f;
    public float Persistance = 0.5f;
    public float Lacunarity = 2.0f;
    public int NumberOfOctaves = 3;
    public float GlobalNoiseAddition = 0.0f;

    public string Seed = "";
    public Vector2 UserOffset = Vector2.zero;
    public NoiseGeneration.CustomFunctionType CustomFunction = NoiseGeneration.CustomFunctionType.kNone;
    public float CustomExponent = 2.0f;
    public NoiseParameters.TextureType TerrainTextureType = NoiseParameters.TextureType.kGrayscale;

    // Variables for terrain
    public float[,] TerrainHeightMap;
    public float[,] TerrainTemperatureMap;
    public float[,] TerrainMoistureMap;
    public Terrain _Terrain;


    [SerializeField]
    public List<TerrainParameters> TerrainParameterList = new List<TerrainParameters>();

    // Used for creating the terrain in runtime, then switch to single run-s to paint the terrain
    public GenerationType _GenerationType = GenerationType.kSingleRun;

    void Start() {
        // You can deserialize here and take the first NoiseParameter from the list if you dont want the default values
        InitializeTerrain();
    }

    private void Update() {
        if (_GenerationType == GenerationType.kUpdating) {
            InitializeTerrain();
        }
    }

    public void InitializeTerrain() {
        _Terrain.terrainData.heightmapResolution = TerrainWidth + 1;
        _Terrain.terrainData.alphamapResolution = TerrainWidth;
        _Terrain.terrainData.SetDetailResolution(TerrainWidth, 16);
        _Terrain.terrainData.baseMapResolution = TerrainWidth * 2;
        _Terrain.terrainData.size = new Vector3(TerrainWidth, TerrainDepth, TerrainHeight);
        GenerateTerrainFromPreset();
    }

    public void GenerateTerrainFromPreset() {
        TerrainHeightMap = NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, NoiseScale,
                    BaseFrequency, NumberOfOctaves, Persistance, Lacunarity, UserOffset, ErosionIterations, Seed, CustomFunction, CustomExponent, GlobalNoiseAddition, _ErosionType, RuntimeErosion);
        // this can be parallelized 
        _Terrain.terrainData.SetHeights(0, 0, TerrainHeightMap);
        TerrainTemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainWidth, TerrainHeight, TerrainHeightMap);
        TerrainMoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainWidth, TerrainHeight, TerrainHeightMap);
    }

    public void GenerateTerrainOnDemand() {
        TerrainHeightMap = NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, NoiseScale,
                    BaseFrequency, NumberOfOctaves, Persistance, Lacunarity, UserOffset, ErosionIterations, Seed, CustomFunction, CustomExponent, GlobalNoiseAddition, _ErosionType, RuntimeErosion);
        _Terrain.terrainData.SetHeights(0, 0, TerrainHeightMap);
        TerrainTemperatureMap = NoiseGeneration.GenerateTemperatureMap(TerrainWidth, TerrainHeight, TerrainHeightMap);
        TerrainMoistureMap = NoiseGeneration.GenerateMoistureMap(TerrainWidth, TerrainHeight, TerrainHeightMap);
        ApplyErosion();
        AssignSplatMap.DoSplat(TerrainHeightMap, TerrainTemperatureMap, TerrainMoistureMap, _Terrain, _Terrain.terrainData, TerrainParameterList, TerrainWidth, TerrainHeight);
    }

    public void ApplyErosion() {
        if (_GenerationType == GenerationType.kUpdating) {
            Debug.LogWarning("You are applying erosion in runtime. Switching to single mode");
            _GenerationType = GenerationType.kSingleRun;
        }
        switch (_ErosionType) {
            case ErosionGeneration.ErosionType.kThermalErosion:
                ErosionGeneration.ThermalErosion(ref TerrainHeightMap, TerrainWidth, TerrainHeight, ErosionIterations);
                break;
            case ErosionGeneration.ErosionType.kHydraulicErosion:
                ErosionGeneration.ImprovedThermalErosion(ref TerrainHeightMap, TerrainWidth, TerrainHeight, ErosionIterations);
                break;
            case ErosionGeneration.ErosionType.kImprovedErosion:
                ErosionGeneration.HydraulicErosion(ref TerrainHeightMap, TerrainWidth, TerrainHeight, ErosionIterations);
                break;
            case ErosionGeneration.ErosionType.kNone:
                break;
            default:
                break;
        }
    }
}
