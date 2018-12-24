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
    public enum ErosionType {
        kThermalErosion,
        kHydraulicErosion,
        kImprovedErosion
    }

    public ErosionType _ErosionType = ErosionType.kThermalErosion;
    public int NumberOfIterations = 50;

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
        switch (_ErosionType) {
            case ErosionType.kThermalErosion:
                NormalErosion(ref TerrainHeightMap, TerrainWidth, TerrainHeight, NumberOfIterations);
                break;
            case ErosionType.kHydraulicErosion:
                ImprovedErosion(ref TerrainHeightMap, TerrainWidth, TerrainHeight, NumberOfIterations);
                break;
            case ErosionType.kImprovedErosion:
                HydraulicErosion(ref TerrainHeightMap, TerrainWidth, TerrainHeight, NumberOfIterations);
                break;
        }
        _Terrain.terrainData.SetHeights(0, 0, TerrainHeightMap);
    }

    //  source http://micsymposium.org/mics_2011_proceedings/mics2011_submission_30.pdf
    private void NormalErosion(ref float[,] terrainMap, int width, int height, int iter) {
        float talus = 4.0f / width;
        int
        lowest_x = -1, lowest_y = -1;

        float current_difference, current_height,
              max_dif,
              new_height;
        // iter one for simplicity
        for (int o = 0; o < iter; o++) {
            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    current_height = terrainMap[x, y];
                    max_dif = -float.MaxValue;
                    for (int i = -1; i < 2; i += 2) {
                        for (int j = -1; j < 2; j += 2) {
                            current_difference = current_height - terrainMap[x + i, y + j];
                            if (current_difference > max_dif) {
                                max_dif = current_difference;
                                lowest_x = i;
                                lowest_y = j;
                            }
                        }
                    }
                    if (max_dif > talus) {
                        new_height = current_height - max_dif / 2.0f;
                        terrainMap[x, y] = new_height;
                        terrainMap[x + lowest_x, y + lowest_y] = new_height;
                    }
                }
            }
        }
    }
    private void ImprovedErosion(ref float[,] terrainMap, int width, int height, int iter) {
        float talus = 4.0f / width;
        int lowest_x = -1, lowest_y = -1;
        float current_difference, current_height,
              max_dif,
              new_height;
        // iter one for simplicity
        for (int o = 0; o < iter; o++) {
            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    current_height = terrainMap[x, y];
                    max_dif = -float.MaxValue;
                    for (int i = -1; i < 2; i += 1) {
                        for (int j = -1; j < 2; j += 1) {
                            current_difference = current_height - terrainMap[x + i, y + j];
                            if (current_difference > max_dif) {
                                max_dif = current_difference;

                                lowest_x = i;
                                lowest_y = j;
                            }
                        }
                    }
                    if (max_dif > 0.0f && max_dif <= talus) {
                        new_height = current_height - max_dif / 2.0f;
                        terrainMap[x, y] = new_height;
                        terrainMap[x + lowest_x, y + lowest_y] = new_height;
                    }
                }
            }
        }
    }

    private void HydraulicErosion(ref float[,] terrainMap, int width, int height, int iter) {
        int x, y, i, j, iter_count,
        lowest_x = -1, lowest_y = -1;

        float[,] water_map = new float[width, height];
        float rain_amount = 0.01f, //amount of rain dropped per pixel each iteration
          solubility = 0.01f, //how much sediment a unit of water will erode
          evaporation = 0.9f, //how much water evaporates from each pixel each iteration
          capacity = solubility, //how much sediment a unit of water can hold
          water_lost, current_height, current_difference, max_dif; //temporary variables
        for (i = 0; i < height; ++i) {
            for (j = 0; j < width; ++j)
                water_map[i, j] = 0.0f;
        }
        for (iter_count = 0; iter_count < iter; ++iter_count) {
            //step 1: rain
            for (x = 0; x < height; ++x) {
                for (y = 0; y < width; ++y)
                    water_map[x, y] += rain_amount;
            }
            //step 2: erosion
            for (x = 0; x < height; ++x) {
                for (y = 0; y < width; ++y) {
                    terrainMap[x, y] -= water_map[x, y] * solubility;
                }
            }
            //step 3: movement
            for (x = 1; x < (height - 1); ++x) {
                for (y = 1; y < (width - 1); ++y) {
                    //find the lowest neighbor
                    current_height = terrainMap[x, y] + water_map[x, y];
                    max_dif = -float.MaxValue;
                    for (i = -1; i < 2; i += 1) {
                        for (j = -1; j < 2; j += 1) {
                            current_difference = current_height - terrainMap[x + i, y + j] - water_map[x + i, y + i];
                            if (current_difference > max_dif) {
                                max_dif = current_difference;
                                lowest_x = i;
                                lowest_y = j;
                            }
                        }
                    }
                    //now either do nothing, level off, or move all the water
                    if (max_dif > 0.0f) {
                        //move it all...
                        if (water_map[x, y] < max_dif) {
                            water_map[x + lowest_x, y + lowest_y] += water_map[x, y];
                            water_map[x, y] = 0.0f;
                        }
                        //level off...
                        else {
                            water_map[x + lowest_x, y + lowest_y] += max_dif / 2.0f;
                            water_map[x, y] -= max_dif / 2.0f;
                        }
                    }
                }
            }
            //step 4: evaporation / deposition
            for (x = 0; x < height; ++x) {
                for (y = 0; y < width; ++y) {
                    water_lost = water_map[x, y] * evaporation;
                    water_map[x, y] -= water_lost;
                    terrainMap[x, y] += water_lost * capacity;
                }
            }
        }
    }


    // @TODO, this does not work right, remove or rewrite
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
