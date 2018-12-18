using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {
    public enum GenerationType {
        kDev,
        kMultiPerlin
    }

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
    public GenerationType _GenerationType = GenerationType.kMultiPerlin;

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
                MultiPerlinTerrainGeneration();
                break;
            default:
                Debug.LogError("Unknown generation type");
                break;
        }
    }

    private void GenerateTerrainFromPreset() {
        TerrainHeightMap = NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, Seed, NoiseScale,
                    BaseFrequency, NumberOfOctaves, Persistance, Lacunarity, UserOffset, CustomFunction, CustomExponent, GlobalNoiseAddition);
        ErodeTerrainMap(ref TerrainHeightMap, TerrainWidth, TerrainHeight, 6);
        _Terrain.terrainData.SetHeights(0, 0, TerrainHeightMap);
    }

    private void ErodeTerrainMap(ref float[,] terrainMap, int width, int height, int iter) {
        float heightTreshold = 0.9f;
        float sharePercent = 0.5f;
        for (int i = 0; i < iter; i++) {
            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    // using neummans neighbours
                    /*
                     *    h1
                     * h2 h0 h3
                     *    h4
                     * */
                    float h0 = terrainMap[x, y];
                    float h1 = terrainMap[x, y - 1];
                    float h2 = terrainMap[x - 1, y];
                    float h3 = terrainMap[x + 1, y];
                    float h4 = terrainMap[x, y + 1];
                    if (h0 > heightTreshold) {
                        float amtToShare = h0 * sharePercent;
                        terrainMap[x, y] = h0 - 4.0f * amtToShare;
                        terrainMap[x, y - 1] -= h1>heightTreshold ? 0 : amtToShare;
                        terrainMap[x - 1, y] -= h2>heightTreshold ? 0 : amtToShare;
                        terrainMap[x + 1, y] -= h3>heightTreshold ? 0 : amtToShare;
                        terrainMap[x, y + 1] -= h4>heightTreshold ? 0 : amtToShare;
                    }
                }
            }
        }
    }

    private void MultiPerlinTerrainGeneration() {
        // For now working with 3 noiseParameters, base > moisture > weather
        // This is subject to change, produces interesting results tho, need to play with diff weights and maps
        List<NoiseParameters> noiseParameterList = SerializationManager.ReadAllNoiseParameters();
        var baseParam = noiseParameterList[0];
        var moistureParam = noiseParameterList[1];
        var weatherParam = noiseParameterList[2];
        List<float[,]> heightMaps = new List<float[,]>();
        heightMaps.Add(NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, baseParam));
        heightMaps.Add(NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, moistureParam));
        heightMaps.Add(NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, weatherParam));
        float[,] endMap = new float[TerrainWidth, TerrainHeight];
        float weight = 0.6f;
        foreach (var heightMap in heightMaps) {
            for (int y = 0; y < TerrainHeight; y++) {
                for (int x = 0; x < TerrainWidth; x++) {
                    heightMap[x, y] *= weight;
                }
            }
            weight -= 0.2f;
            for (int y = 0; y < TerrainHeight; y++) {
                for (int x = 0; x < TerrainWidth; x++) {
                    endMap[x, y] += heightMap[x, y];
                }
            }
        }
        TerrainHeightMap = endMap;
        _Terrain.terrainData.SetHeights(0, 0, endMap);
    }
}
