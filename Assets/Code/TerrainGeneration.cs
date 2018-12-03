using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {
    public enum TextureType {
        kGrayscale,
        kColored
    }
    public readonly int TerrainWidth = 128;
    public readonly int TerrainHeight = 128;
    public readonly int TerrainDepth = 20;
    // Controls for the perlin generation
    public float NoiseScale = 100.0f;
    public float BaseFrequency = 1.0f;
    public float Persistance = 0.5f;
    public float Lacunarity = 2.0f;
    public int NumberOfOctaves = 3;

    public string Seed = "";
    public Vector2 UserOffset = Vector2.zero;
    public NoiseGeneration.CustomFunctionType CustomFunction = NoiseGeneration.CustomFunctionType.kNone;

    public float[,] TerrainHeightMap;
    public Terrain _Terrain;
    public TextureType TerrainTextureType = TextureType.kGrayscale;

    [SerializeField]
    public List<TerrainParameters> TerrainParameterList = new List<TerrainParameters>();

    void Start() {
        // InitializeTerrain();
    }

    private void Update() {
        InitializeTerrain();
    }

    void InitializeTerrain() {
        _Terrain.terrainData.heightmapResolution = TerrainWidth < TerrainHeight ? TerrainWidth : TerrainHeight;
        _Terrain.terrainData.size = new Vector3(TerrainWidth, TerrainDepth, TerrainHeight);
        TerrainHeightMap = NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, Seed, NoiseScale, BaseFrequency, NumberOfOctaves, Persistance, Lacunarity, UserOffset, CustomFunction);
        _Terrain.terrainData.SetHeights(0, 0, TerrainHeightMap);
    }
    private void OnDestroy() {
        // figure out how to save properties from runtime
    }
}
