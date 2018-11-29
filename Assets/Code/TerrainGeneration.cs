using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {
    private int TerrainHeight = 128;
    private int TerrainWidth = 128;
    private int TerrainDepth = 20;

    [Range(0.1f, 10.0f)]
    public float Frequency = 3.5f;

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
        float[,] currentTerrain = new float[TerrainHeight, TerrainWidth];
        for (int x = 0; x < TerrainWidth; x++) {
            for (int y = 0; y < TerrainHeight; y++) {
                currentTerrain[x, y] = GenerateTerrainPerlinNoise((float)x / TerrainWidth, (float)y / TerrainHeight);
            }
        }
        return currentTerrain;
    }

    public float GenerateTerrainPerlinNoise(float x, float y) {
        return Mathf.PerlinNoise(Frequency * x, Frequency * y);
    }

}
