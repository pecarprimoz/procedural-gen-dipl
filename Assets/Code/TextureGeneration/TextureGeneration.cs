using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TextureGeneration {
    public static Color[] GenerateHeightmapTexture(TerrainGeneration _tg, TerrainGenerationDebugger.DebugPlaneContent planeContent) {
        Color[] terrainTexture = new Color[_tg.TerrainWidth * _tg.TerrainHeight];
        for (int y = 0; y < _tg.TerrainHeight; y++) {
            for (int x = 0; x < _tg.TerrainWidth; x++) {
                if (_tg.TerrainTextureType == NoiseParameters.TextureType.kColored) {
                    // do check if textureType is all
                    terrainTexture[y * _tg.TerrainWidth + x] = _tg.TerrainParameterList[GetCorrectBiomeIndex(_tg, x, y)].TerrainColor;
                } else if (_tg.TerrainTextureType == NoiseParameters.TextureType.kGrayscale) {
                    if (planeContent == TerrainGenerationDebugger.DebugPlaneContent.kHeightMap) {
                        terrainTexture[y * _tg.TerrainWidth + x] = Color.Lerp(Color.white, Color.black, _tg.TerrainHeightMap[x, y]);
                    } else if (planeContent == TerrainGenerationDebugger.DebugPlaneContent.kMoistureMap) {
                        terrainTexture[y * _tg.TerrainWidth + x] = Color.Lerp(Color.white, Color.black, _tg.TerrainMoistureMap[x, y]);
                    } else if (planeContent == TerrainGenerationDebugger.DebugPlaneContent.kTemperatureMap) {
                        terrainTexture[y * _tg.TerrainWidth + x] = Color.Lerp(Color.white, Color.black, _tg.TerrainTemperatureMap[x, y]);
                    }
                }
            }
        }
        return terrainTexture;
    }
    public static int GetCorrectBiomeIndex(TerrainGeneration _tg, int x, int y) {
        return GetCorrectBiomeIndex(_tg.TerrainHeightMap, _tg.TerrainTemperatureMap, _tg.TerrainMoistureMap, _tg.TerrainParameterList, x, y);
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
