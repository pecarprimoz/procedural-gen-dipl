using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGeneration {
    public enum CustomFunctionType {
        kNone,
        kSin,
        kCos,
        kEps,
        kPi,
        kCustom
    }
    public static float[,] GenerateTerrain(int terrainWidth, int terrainHeight, string seed, float scale, float frequency, int numberOfOctaves,
        float persistance, float lacunarity, Vector2 userOffset, CustomFunctionType functionType = CustomFunctionType.kNone, float customExponent = 2.0f) {
        float[,] currentTerrain = new float[terrainWidth, terrainHeight];
        // localScale is used when calculating how big the hills will be in the same area (highter the localScale, the more even the terrain)
        float localScale = scale <= 0 ? 0.0001f : scale;
        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        Vector2[] octaveOffsets = GenerateOctaveOffsets(seed, numberOfOctaves, userOffset);

        for (int y = 0; y < terrainHeight; y++) {
            for (int x = 0; x < terrainWidth; x++) {
                // the amplitude is used to generate hills of different heights, the higher the amplitude the bigger the hill
                float amplitude = 1.0f;
                // noiseHeight tells us the current height ranging from -1 to 1 
                float noiseHeight = 0.0f;
                float localFrequency = frequency;
                for (int i = 0; i < numberOfOctaves; i++) {
                    // when combining this with frequency we now control the number of big hills per area
                    float nx = (float)(x - terrainWidth / 2) / localScale * localFrequency + octaveOffsets[i].x;
                    float ny = (float)(y - terrainHeight / 2) / localScale * localFrequency + octaveOffsets[i].y;

                    float perlinValue = GenerateTerrainPerlinNoise(nx, ny);
                    // the current height value in our area
                    noiseHeight += perlinValue * amplitude;
                    // persistance controlles the amplitude, tells us how many big hills we actually have (smaller persistance less big hills)
                    amplitude *= persistance;
                    // lacunarity controlls the frequency, tells us how many hills we have in an area (smaller lacunarity less dense hills)
                    localFrequency *= lacunarity;

                }
                if (maxNoiseHeight < noiseHeight) {
                    maxNoiseHeight = noiseHeight;
                } else if (minNoiseHeight > noiseHeight) {
                    minNoiseHeight = noiseHeight;
                }
                currentTerrain[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < terrainHeight; y++) {
            for (int x = 0; x < terrainWidth; x++) {
                switch (functionType) {
                    case CustomFunctionType.kSin:
                        currentTerrain[x, y] = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), Mathf.Sin(currentTerrain[x, y]));
                        break;
                    case CustomFunctionType.kCos:
                        currentTerrain[x, y] = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), Mathf.Cos(currentTerrain[x, y]));
                        break;
                    case CustomFunctionType.kEps:
                        currentTerrain[x, y] = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), Mathf.Epsilon);
                        break;
                    case CustomFunctionType.kCustom:
                        currentTerrain[x, y] = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), customExponent);
                        break;
                    case CustomFunctionType.kNone:
                        currentTerrain[x, y] = 0.2f + currentTerrain[x,y];
                        break;
                    default:
                        currentTerrain[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]);
                        break;
                }
            }
        }
        return currentTerrain;
    }

    public static int GenerateIntSeed(string s) {
        float endSeed = 0;
        char[] charArr = s.ToCharArray();
        for (int i = 0; i < charArr.Length; i++) {
            endSeed += charArr[i];
        }
        return (int)endSeed;
    }

    private static Vector2[] GenerateOctaveOffsets(string seed, int numberOfOctaves, Vector2 userOffset) {
        System.Random rnd = new System.Random(GenerateIntSeed(seed));
        Vector2[] octaveOffset = new Vector2[numberOfOctaves];
        for (int i = 0; i < numberOfOctaves; i++) {
            float offsetX = rnd.Next(-10000, 10000) + userOffset.x;
            float offsetY = rnd.Next(-10000, 10000) + userOffset.y;
            octaveOffset[i] = new Vector2(offsetX, offsetY);
        }
        return octaveOffset;
    }

    public static float GenerateTerrainPerlinNoise(float x, float y) {
        // we multiply by 2 and sub by 1 so we get a range from -1 to 1
        return Mathf.PerlinNoise(x, y) * 2 - 1;
    }
}
