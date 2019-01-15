using System.Collections.Generic;
using UnityEngine;

public static class TextureGeneration {
    public static Color[] GenerateHeightmapTexture(int terrainWidth, int terrainHeight,
        float[,] currentTerrain, List<TerrainParameters> terrainParameterList, NoiseParameters.TextureType textureType) {
        Color[] terrainTexture = new Color[terrainWidth * terrainHeight];
        for (int y = 0; y < terrainHeight; y++) {
            for (int x = 0; x < terrainWidth; x++) {
                float currentHeight = currentTerrain[x, y];
                switch (textureType) {
                    case NoiseParameters.TextureType.kColored:
                        for (int i = 0; i < terrainParameterList.Count; i++) {
                            var parameter = terrainParameterList[i];
                            // use biome generation to get the color of the biome, TODO implemen for terrain with textures !
                            if (currentHeight <= parameter.ParameterBoundry) {
                                terrainTexture[y * terrainWidth + x] = parameter.TerrainColor;
                                break;
                            }
                        }
                        break;
                    case NoiseParameters.TextureType.kGrayscale:
                        terrainTexture[y * terrainWidth + x] = Color.Lerp(Color.white, Color.black, currentHeight);
                        break;
                    default:
                        Debug.LogError("Unknown texture type.");
                        break;
                }
            }
        }
        return terrainTexture;
    }
}
