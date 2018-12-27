using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {
    public class Node {
        public float value;
        public int x;
        public int y;
        public Node(float value, int x, int y) {
            this.value = value;
            this.x = x;
            this.y = y;
        }
    }
    public enum GenerationType {
        kDev,
        kMultiPerlin
    }

    public ErosionGeneration.ErosionType _ErosionType = ErosionGeneration.ErosionType.kThermalErosion;
    public int ErosionIterations = 50;

    // Terrain parameters, change this if you want a bigger/higher terrain
    public readonly int TerrainWidth = 128;
    public readonly int TerrainHeight = 128;
    public readonly int TerrainDepth = 60;

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
    public Terrain _Terrain;

    [SerializeField]
    public List<TerrainParameters> TerrainParameterList = new List<TerrainParameters>();

    // Temporary parameters, will be removed in the future
    public GenerationType _GenerationType = GenerationType.kDev;

    void Start() {
        // You can deserialize here and take the first NoiseParameter from the list if you dont want the default values
        // InitializeTerrain();
    }

    private void Update() {
        InitializeTerrain();
    }

    void InitializeTerrain() {
        _Terrain.terrainData.heightmapResolution = TerrainWidth < TerrainHeight ? TerrainWidth : TerrainHeight;
        _Terrain.terrainData.size = new Vector3(TerrainWidth, TerrainDepth, TerrainHeight);
        switch (_GenerationType) {
            case GenerationType.kDev:
                GenerateTerrainFromPreset();
                break;
            case GenerationType.kMultiPerlin:
                //MultiPerlinTerrainGeneration();
                break;
            default:
                Debug.LogError("Unknown generation type");
                break;
        }
    }

    private void GenerateTerrainFromPreset() {
        TerrainHeightMap = NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, Seed, NoiseScale,
                    BaseFrequency, NumberOfOctaves, Persistance, Lacunarity, UserOffset, CustomFunction, CustomExponent, GlobalNoiseAddition, _ErosionType, ErosionIterations);
        _Terrain.terrainData.SetHeights(0, 0, TerrainHeightMap);
    }
}
