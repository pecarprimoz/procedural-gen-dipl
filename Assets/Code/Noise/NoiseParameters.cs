using System.Collections.Generic;
using UnityEngine;

public class NoiseParameters {
    
    public string NoiseParameterName = string.Empty;

    public ErosionGeneration.ErosionType ErosionType;
    public int ErosionIterations;
    public bool RuntimeErosion;
    // Controls for the perlin generation
    public float NoiseScale;
    public float BaseFrequency;
    public float Persistance;
    public float Lacunarity;
    public int NumberOfOctaves;
    public float GlobalNoiseAddition;

    public string Seed;
    public Vector2 UserOffset;
    public CustomFunctionType CustomFunction;
    public float CustomExponent;

    public TextureType TerrainTextureType;
    public List<TerrainParameters> TerrainParameterList;

    public NoiseParameters(ErosionGeneration.ErosionType erosionType, bool runtimeErosion, int numberOfIterations, string name, List<TerrainParameters> terrainParameterList, Vector2 userOffset, float noiseScale, float baseFrequency,
        float persistance, float lacunarity, int numberOfOctaves, float globalNoiseAddition, string seed, CustomFunctionType customFunction,
        float customExponent, TextureType terrainTextureType) {
        ErosionType = erosionType;
        ErosionIterations = numberOfIterations;
        RuntimeErosion = runtimeErosion;
        NoiseParameterName = name;
        NoiseScale = noiseScale;
        BaseFrequency = baseFrequency;
        Persistance = persistance;
        Lacunarity = lacunarity;
        NumberOfOctaves = numberOfOctaves;
        GlobalNoiseAddition = globalNoiseAddition;
        Seed = seed;
        UserOffset = userOffset;
        CustomFunction = customFunction;
        CustomExponent = customExponent;
        TerrainTextureType = terrainTextureType;
        TerrainParameterList = terrainParameterList;
    }

}
