using System.Collections.Generic;
using UnityEngine;

public class TerrainInfo {

    public TerrainInfo(Terrain terrain) {
        HeightMap = new float[TerrainWidth, TerrainHeight];
        MoistureMap = new float[TerrainWidth, TerrainHeight];
        TemperatureMap = new float[TerrainWidth, TerrainHeight];
        _Terrain = terrain;
    }
    public TerrainInfo() {
        HeightMap = new float[TerrainWidth, TerrainHeight];
        MoistureMap = new float[TerrainWidth, TerrainHeight];
        TemperatureMap = new float[TerrainWidth, TerrainHeight];
    }
    public ContentManager ContentManager {
        get; set;
    }

    // Terrain parameters, change this if you want a bigger/higher terrain
    public int TerrainWidth = 128;
    public int TerrainHeight = 128;
    public int TerrainDepth = 64;

    public ErosionGeneration.ErosionType ErosionType = ErosionGeneration.ErosionType.kThermalErosion;
    public int ErosionIterations = 50;
    public bool RuntimeErosion = false;

    // Parameters for perlin generations, overwritten with editor parameters (see Editor/ParameterEditorWidget)
    public float NoiseScale = 100.0f;
    public float BaseFrequency = 1.0f;
    public float Persistance = 0.5f;
    public float Lacunarity = 2.0f;
    public int NumberOfOctaves = 3;
    public float GlobalNoiseAddition = 0.0f;

    public string Seed = "";
    public Vector2 UserOffset = Vector2.zero;
    public CustomFunctionType CustomFunction = CustomFunctionType.kNone;
    public float CustomExponent = 2.0f;
    public TextureType TerrainTextureType = TextureType.kGrayscale;

    public Terrain _Terrain;

    public float[,] HeightMap;
    public float[,] MoistureMap;
    public float[,] TemperatureMap;

    public int[,] BiomeMap;
    // this could also be initialised when creating the BiomeType enum from editor, not sure if its worth it yet
    public Dictionary<int, List<TerrainPoint>> SeperatedBiomes = new Dictionary<int, List<TerrainPoint>>();

    // Used for creating the terrain in runtime, then switch to single run-s to paint the terrain
    public GenerationType GenerationType = GenerationType.kSingleRun;

    [SerializeField]
    public List<TerrainParameters> TerrainParameterList = new List<TerrainParameters>();

    // Seasonal changes boolean
    public bool AreSeasonsChanging = false;
    public SeasonType CurrentSeason = SeasonType.kSpring;


}