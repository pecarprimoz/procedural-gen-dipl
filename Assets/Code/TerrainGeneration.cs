using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {
    private int TerrainHeight = 128;
    private int TerrainWidth = 128;
    private int TerrainDepth = 20;

    public float[,] currentTerrain;

    [Range(0.1f, 10.0f)]
    public float Frequency = 3.5f;

    [Range(1, 5)]
    public int NumberOfOctaves = 1;

    public Terrain _Terrain;
    void Start() {
        InitializeTerrain();
    }

    private void Update() {
        GenerateNewTerrain();
    }

    void InitializeTerrain() {
        _Terrain.terrainData.heightmapResolution = TerrainWidth + 1;
        _Terrain.terrainData.size = new Vector3(TerrainWidth, TerrainDepth, TerrainHeight);
        // GenerateNewTerrain()
    }

    private void GenerateNewTerrain() {
        var terrainArray = GenerateTerrain();
        _Terrain.terrainData.SetHeights(0, 0, terrainArray);
    }

    public float[,] GenerateTerrain() {
        currentTerrain = new float[TerrainWidth, TerrainHeight];
        for (int x = 0; x < TerrainWidth; x++) {
            for (int y = 0; y < TerrainHeight; y++) {
                float nx = (float)x / TerrainWidth;
                float ny = (float)y / TerrainHeight;
                // Generate multiple octaves and fiddle with frequencies to get more crumbly terrain
                currentTerrain[x, y] += 0.15f *Mathf.Clamp(GenerateTerrainPerlinNoise(nx, ny), 0.0f, 1.0f);
            }
        }
        return currentTerrain;
    }

    public float GenerateTerrainPerlinNoise(float x, float y) {
        return Mathf.PerlinNoise(3* Frequency * x, 3* Frequency * y);
    }


}
