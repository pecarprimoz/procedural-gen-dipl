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
    private GenerationType _GenerationType = GenerationType.kMultiPerlin;

    void Start() {
        // You can deserialize here and take the first NoiseParameter from the list if you dont want the default values
        // InitializeTerrain();
    }

    private void Update() {
        InitializeTerrain();
    }

    void InitializeTerrain() {
        // Currently we do one pass of perlin noise, need to do 3 generations at least for the weather and moisture map
        // One possible idea, generate 3 perlin noise maps, with different parameters somewhat resembling terrain/weather/moisture maps
        // Deserialize them here and pass them trough a limiter (since we want even terrain, we wont have huge peaks, stuff like this
        // needs to be normalised (take a base height ex. 0.5 then pass the terrain map trough it, then add the weather and moisture maps
        // with a lower weight (terrain weight 0.7, moisture 0.2, weather 0.1 not sure about this)
        _Terrain.terrainData.heightmapResolution = TerrainWidth < TerrainHeight ? TerrainWidth : TerrainHeight;
        _Terrain.terrainData.size = new Vector3(TerrainWidth, TerrainDepth, TerrainHeight);
        switch (_GenerationType) {
            case GenerationType.kDev:
                TerrainHeightMap = NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, Seed, NoiseScale,
                    BaseFrequency, NumberOfOctaves, Persistance, Lacunarity, UserOffset, CustomFunction, CustomExponent);
                _Terrain.terrainData.SetHeights(0, 0, TerrainHeightMap);
                break;
            case GenerationType.kMultiPerlin:
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
                _Terrain.terrainData.SetHeights(0, 0, endMap);
                break;
            default:
                Debug.LogError("Unknown generation type");
                break;
        }
    }
}
