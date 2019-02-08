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

    public static float[,] GenerateTerrain(int terrainWidth, int terrainHeight, NoiseParameters param) {
        return GenerateTerrain(terrainWidth, terrainHeight, param.NoiseScale, param.BaseFrequency, param.NumberOfOctaves, param.Persistance,
            param.Lacunarity, param.UserOffset, param.ErosionIterations, param.Seed, param.CustomFunction, param.CustomExponent, param.GlobalNoiseAddition, param.ErosionType, param.RuntimeErosion);
    }

    public static float[,] GenerateTerrain(int terrainWidth, int terrainHeight, float scale, float frequency, int numberOfOctaves,
        float persistance, float lacunarity, Vector2 userOffset, int erosionIterations, string seed = "", CustomFunctionType functionType = CustomFunctionType.kNone, float customExponent = 0.0f, float userAddition = 0.0f, ErosionGeneration.ErosionType erosionType = ErosionGeneration.ErosionType.kImprovedErosion, bool runtimeErosion = false) {
        float[,] currentTerrain = new float[terrainWidth, terrainHeight];
        // localScale is used when calculating how big the hills will be in the same area (highter the localScale, the more even the terrain)
        float localScale = scale <= 0 ? 0.0001f : scale;
        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        Vector2[] octaveOffsets = GenerateOctaveOffsets(seed, numberOfOctaves, userOffset);
        // generate the terrain using perlin noise
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
        // apply custom functions to the terrain heights
        for (int y = 0; y < terrainHeight; y++) {
            for (int x = 0; x < terrainWidth; x++) {
                float final_value = 0.0f;
                switch (functionType) {
                    case CustomFunctionType.kSin:
                        final_value = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), Mathf.Sin(currentTerrain[x, y]));
                        break;
                    case CustomFunctionType.kCos:
                        final_value = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), Mathf.Cos(currentTerrain[x, y]));
                        break;
                    case CustomFunctionType.kEps:
                        final_value = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), Mathf.Epsilon);
                        break;
                    case CustomFunctionType.kCustom:
                        final_value = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), customExponent) + userAddition;
                        break;
                    case CustomFunctionType.kNone:
                        final_value = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]) + userAddition;
                        break;
                    default:
                        final_value = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]) + userAddition;
                        break;
                }
                if (final_value > 1.0f) {
                    final_value = 1.0f;
                } else if (final_value < 0.0f) {
                    final_value = 0.0f;
                }
                currentTerrain[x, y] = final_value;
            }
        }
        // erode the terrain, this is currently in runtime
        if (runtimeErosion) {
            switch (erosionType) {
                case ErosionGeneration.ErosionType.kThermalErosion:
                    ErosionGeneration.ThermalErosion(ref currentTerrain, terrainWidth, terrainHeight, erosionIterations);
                    break;
                case ErosionGeneration.ErosionType.kHydraulicErosion:
                    ErosionGeneration.ImprovedThermalErosion(ref currentTerrain, terrainWidth, terrainHeight, erosionIterations);
                    break;
                case ErosionGeneration.ErosionType.kImprovedErosion:
                    ErosionGeneration.HydraulicErosion(ref currentTerrain, terrainWidth, terrainHeight, erosionIterations);
                    break;
                case ErosionGeneration.ErosionType.kNone:
                    break;
                default:
                    break;
            }
        }
        return currentTerrain;
    }

    public static float[,] GenerateTemperatureMap(int terrainWidth, int terrainHeight, float[,] heightMap) {
        // init boundries based on terrainHeight
        float[,] baseNoiseMap = GenerateTerrain(terrainWidth, terrainHeight, 175, 3, 5, 0.3f, 4.5f, Vector2.zero, 50, userAddition: 0.2f);
        float[,] temperatureMap = new float[terrainWidth, terrainHeight];
        for (int y = 0; y < terrainHeight; y++) {
            for (int x = 0; x < terrainWidth; x++) {
                temperatureMap[x, y] = (float)(terrainHeight - y) / terrainHeight;
                float endVal = baseNoiseMap[x, y] * temperatureMap[x, y];
                temperatureMap[x, y] = endVal > 1 ? 1 : endVal;
            }
        }
        return temperatureMap;
    }

    // SWAPPED MOISTURE AND TEMP MAPS CUZ I HAVE A BETTER IDEA FOR TEMP MAPS !
    public static float[,] GenerateMoistureMap(int terrainWidth, int terrainHeight, float[,] heightMap) {
        float[,] moistureMap = new float[terrainWidth, terrainHeight];
        // temperature map is basiclly 1 - heightMap[x,y]
        // means that points that are really hight have a low temperature value (near 0)
        // hot points have values closer to 1 (deserts, ocean etc)
        // @TODO, think about implementing smarter temperature generation, equators, south/north poles ...
        for (int y = 0; y < terrainHeight; y++) {
            for (int x = 0; x < terrainWidth; x++) {
                moistureMap[x, y] = 1 - heightMap[x, y];
            }
        }
        return moistureMap;
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
