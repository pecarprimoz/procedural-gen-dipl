using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseParameters {
    public enum TextureType {
        kGrayscale,
        kColored
    }
    // Controls for the perlin generation
    public float NoiseScale;
    public float BaseFrequency;
    public float Persistance;
    public float Lacunarity;
    public int NumberOfOctaves;

    public string Seed;
    public Vector2 UserOffset;
    public NoiseGeneration.CustomFunctionType CustomFunction;
    public float CustomExponent;

    public TextureType TerrainTextureType;
    public List<TerrainParameters> TerrainParameterList;

    public NoiseParameters(List<TerrainParameters> terrainParameterList, Vector2 userOffset, float noiseScale, float baseFrequency,
        float persistance, float lacunarity, int numberOfOctaves, string seed, NoiseGeneration.CustomFunctionType customFunction,
        float customExponent, TextureType terrainTextureType) {
        NoiseScale = noiseScale;
        BaseFrequency = baseFrequency;
        Persistance = persistance;
        Lacunarity = lacunarity;
        NumberOfOctaves = numberOfOctaves;
        Seed = seed;
        UserOffset = userOffset;
        CustomFunction = customFunction;
        CustomExponent = customExponent;
        TerrainTextureType = terrainTextureType;
        TerrainParameterList = terrainParameterList;
    }

}
