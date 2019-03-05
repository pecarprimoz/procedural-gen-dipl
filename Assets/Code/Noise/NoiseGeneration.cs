using UnityEngine;

public static class NoiseGeneration {

    public static float[,] GenerateTerrain(TerrainInfo info) {
        float[,] currentTerrain = new float[info.TerrainWidth, info.TerrainHeight];
        // localScale is used when calculating how big the hills will be in the same area (highter the localScale, the more even the terrain)
        float localScale = info.NoiseScale <= 0 ? 0.0001f : info.NoiseScale;
        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        Vector2[] octaveOffsets = GenerateOctaveOffsets(info.Seed, info.NumberOfOctaves, info.UserOffset);
        // generate the terrain using perlin noise
        for (int y = 0; y < info.TerrainHeight; y++) {
            for (int x = 0; x < info.TerrainWidth; x++) {
                // the amplitude is used to generate hills of different heights, the higher the amplitude the bigger the hill
                float amplitude = 1.0f;
                // noiseHeight tells us the current height ranging from -1 to 1 
                float noiseHeight = 0.0f;
                float localFrequency = info.BaseFrequency;
                for (int i = 0; i < info.NumberOfOctaves; i++) {
                    // when combining this with frequency we now control the number of big hills per area
                    float nx = (float)(x - info.TerrainWidth / 2) / localScale * localFrequency + octaveOffsets[i].x;
                    float ny = (float)(y - info.TerrainHeight / 2) / localScale * localFrequency + octaveOffsets[i].y;

                    float perlinValue = GenerateTerrainPerlinNoise(nx, ny);
                    // the current height value in our area
                    noiseHeight += perlinValue * amplitude;
                    // persistance controlles the amplitude, tells us how many big hills we actually have (smaller persistance less big hills)
                    amplitude *= info.Persistance;
                    // lacunarity controlls the frequency, tells us how many hills we have in an area (smaller lacunarity less dense hills)
                    localFrequency *= info.Lacunarity;

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
        for (int y = 0; y < info.TerrainHeight; y++) {
            for (int x = 0; x < info.TerrainWidth; x++) {
                float final_value = 0.0f;
                switch (info.CustomFunction) {
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
                        final_value = Mathf.Pow(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]), info.CustomExponent) + info.GlobalNoiseAddition;
                        break;
                    case CustomFunctionType.kNone:
                        final_value = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]) + info.GlobalNoiseAddition;
                        break;
                    default:
                        final_value = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentTerrain[x, y]) + info.GlobalNoiseAddition;
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
        if (info.RuntimeErosion) {
            switch (info.ErosionType) {
                case ErosionGeneration.ErosionType.kThermalErosion:
                    ErosionGeneration.ThermalErosion(info);
                    break;
                case ErosionGeneration.ErosionType.kHydraulicErosion:
                    ErosionGeneration.ImprovedThermalErosion(info);
                    break;
                case ErosionGeneration.ErosionType.kImprovedErosion:
                    ErosionGeneration.HydraulicErosion(info);
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
        float[,] baseNoiseMap = GenerateTerrain(new TerrainInfo() {
            TerrainWidth = terrainWidth,
            TerrainHeight = terrainHeight,
            NoiseScale = 175,
            BaseFrequency = 3,
            NumberOfOctaves = 5,
            Persistance = 0.3f,
            Lacunarity = 4.5f,
            UserOffset = Vector2.zero,
            ErosionIterations = 50,
            GlobalNoiseAddition = 0.2f,
        });
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
