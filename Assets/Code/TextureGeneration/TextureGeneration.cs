using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TextureGeneration {
    public static Color[] GenerateHeightmapTexture(TerrainGeneration terrainGeneration, DebugPlaneType planeContent) {
        Color[] terrainTexture = new Color[terrainGeneration.TerrainInfo.TerrainWidth * terrainGeneration.TerrainInfo.TerrainHeight];
        for (int y = 0; y < terrainGeneration.TerrainInfo.TerrainHeight; y++) {
            for (int x = 0; x < terrainGeneration.TerrainInfo.TerrainWidth; x++) {
                if (planeContent == DebugPlaneType.kAll) {
                    if (terrainGeneration.TerrainInfo.TerrainTextureType == TextureType.kColored) {
                        terrainTexture[y * terrainGeneration.TerrainInfo.TerrainWidth + x] = terrainGeneration.TerrainInfo.TerrainParameterList[BiomeGeneration.GetCorrectBiomeIndex(terrainGeneration.TerrainInfo, x, y)].TerrainColor;
                    } else {
                        var valsTogether = terrainGeneration.TerrainInfo.HeightMap[x, y] + terrainGeneration.TerrainInfo.TemperatureMap[x, y] + terrainGeneration.TerrainInfo.MoistureMap[x, y];
                        valsTogether = valsTogether / 3;
                        terrainTexture[y * terrainGeneration.TerrainInfo.TerrainWidth + x] = Color.Lerp(Color.white, Color.black, valsTogether);
                    }
                } else if (terrainGeneration.TerrainInfo.TerrainTextureType == TextureType.kGrayscale) {
                    if (planeContent == DebugPlaneType.kHeightMap) {
                        terrainTexture[y * terrainGeneration.TerrainInfo.TerrainWidth + x] = Color.Lerp(Color.white, Color.black, terrainGeneration.TerrainInfo.HeightMap[x, y]);
                    } else if (planeContent == DebugPlaneType.kMoistureMap) {
                        terrainTexture[y * terrainGeneration.TerrainInfo.TerrainWidth + x] = Color.Lerp(Color.white, Color.black, terrainGeneration.TerrainInfo.MoistureMap[x, y]);
                    } else if (planeContent == DebugPlaneType.kTemperatureMap) {
                        terrainTexture[y * terrainGeneration.TerrainInfo.TerrainWidth + x] = Color.Lerp(Color.white, Color.black, terrainGeneration.TerrainInfo.TemperatureMap[x, y]);
                    }
                }
            }
        }
        return terrainTexture;
    }
}
