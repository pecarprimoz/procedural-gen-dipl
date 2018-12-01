using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {

    public readonly int TerrainWidth = 128;
    public readonly int TerrainHeight = 128;

    [Range(0.0f, 100.0f)]
    public float NoiseScale = 100.0f;
    [Range(0.0f, 100.0f)]
    public float BaseFrequency = 1.0f;
    [Range(0.0f, 1f)]
    public float Persistance = 0.5f;
    [Range(0.0f, 5f)]
    public float Lacunarity = 2.0f;
    [Range(1, 10)]
    public int NumberOfOctaves = 3;

    public string Seed = "";
    public Vector2 UserOffset = Vector2.zero;

    public float[,] CurrentTerrain;
    public Terrain _Terrain;

    public NoiseGeneration.CustomFunctionType CustomFunction = NoiseGeneration.CustomFunctionType.kNone;

    private int TerrainDepth = 20;

    void Start() {
        // InitializeTerrain();
    }

    private void Update() {
        InitializeTerrain();
    }

    void InitializeTerrain() {
        _Terrain.terrainData.heightmapResolution = TerrainWidth < TerrainHeight ? TerrainWidth : TerrainHeight;
        _Terrain.terrainData.size = new Vector3(TerrainWidth, TerrainDepth, TerrainHeight);
        CurrentTerrain = NoiseGeneration.GenerateTerrain(TerrainWidth, TerrainHeight, Seed, NoiseScale, BaseFrequency, NumberOfOctaves, Persistance, Lacunarity, UserOffset, CustomFunction);
        _Terrain.terrainData.SetHeights(0, 0, CurrentTerrain);
    }


}
