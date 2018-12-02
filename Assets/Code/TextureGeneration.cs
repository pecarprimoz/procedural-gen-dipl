using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGeneration {
    public enum TextureType {
        kGrayscale,
        kColored
    }

    public static Texture2D GenerateHeightmapTexture(int terrainWidth, int terrainHeight, float[,] currentTerrain) {
        for (int y = 0; y < terrainHeight; y++) {
            for (int x = 0; x < terrainWidth; x++) {
                float currentHeight = currentTerrain[x, y];

            }
        }
        return new Texture2D(1,1);
    }
}
