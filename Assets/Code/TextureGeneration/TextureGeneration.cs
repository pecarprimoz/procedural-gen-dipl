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
                        terrainTexture[y * terrainGeneration.TerrainInfo.TerrainWidth + x] = terrainGeneration.TerrainInfo.TerrainParameterList[GetCorrectBiomeIndex(terrainGeneration, x, y)].TerrainColor;
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
    public static int GetCorrectBiomeIndex(TerrainGeneration terrainGeneration, int x, int y) {
        return GetCorrectBiomeIndex(terrainGeneration.TerrainInfo.HeightMap, terrainGeneration.TerrainInfo.TemperatureMap, terrainGeneration.TerrainInfo.MoistureMap, terrainGeneration.TerrainInfo.TerrainParameterList, x, y);
    }

    public static int GetCorrectBiomeIndex(float[,] heightMap, float[,] tempMap, float[,] moistMap, List<TerrainParameters> terrainParameterList, int x, int y) {
        List<int> validHeightsIndicies = new List<int>();
        List<int> validTemperatureIndicies = new List<int>();
        List<int> validMoistureIndicies = new List<int>();
        var tparams = terrainParameterList;
        // iterate first time to figure out where we are on the height map
        float hmapValue = heightMap[x, y];
        for (int i = 0; i < tparams.Count; i++) {
            var tparam = tparams[i];
            if (hmapValue <= tparam.ParameterBoundry) {
                validHeightsIndicies.Add(i);
            }
        }
        float tmapValue = tempMap[x, y];
        for (int i = 0; i < validHeightsIndicies.Count; i++) {
            var tparam = tparams[i];
            if (tmapValue <= tparam.TemperatureParameterBoundry) {
                validTemperatureIndicies.Add(i);
            }
        }

        float mmapValue = moistMap[x, y];
        for (int i = 0; i < validTemperatureIndicies.Count; i++) {
            var tparam = tparams[i];
            if (mmapValue <= tparam.MoistureParameterBoundry) {
                validMoistureIndicies.Add(i);
            }
        }
        if (validMoistureIndicies.Count > 0) {
            return validMoistureIndicies.Last();
        } else if (validTemperatureIndicies.Count > 0) {
            return validTemperatureIndicies.Last();
        } else if (validHeightsIndicies.Count > 0) {
            return validHeightsIndicies.Last();
        } else {
            // this should not happen
            string debugStr = string.Format("HM: {0}, MM: {1}, TM: {2}", hmapValue, mmapValue, tmapValue);
            Debug.LogError(debugStr);
            return -1;
        }
    }
}
