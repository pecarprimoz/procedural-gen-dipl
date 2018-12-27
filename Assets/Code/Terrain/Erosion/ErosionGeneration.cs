//  source http://micsymposium.org/mics_2011_proceedings/mics2011_submission_30.pdf
public class ErosionGeneration {
    public enum ErosionType {
        kNone,
        kThermalErosion,
        kHydraulicErosion,
        kImprovedErosion
    }

    public static void ThermalErosion(ref float[,] terrainMap, int width, int height, int iter) {
        float talus = 4.0f / width;
        int lowestX = -1, lowestY = -1;

        float currentDifference, currentHeight,
              maxDifference,
              newHeight;
        // run the erosion iter times to get an eroded terrain
        for (int o = 0; o < iter; o++) {
            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    currentHeight = terrainMap[x, y];
                    maxDifference = -float.MaxValue;
                    // using von neumann neighbourhood (faster) 
                    for (int i = -1; i < 2; i += 2) {
                        for (int j = -1; j < 2; j += 2) {
                            currentDifference = currentHeight - terrainMap[x + i, y + j];
                            if (currentDifference > maxDifference) {
                                maxDifference = currentDifference;
                                lowestX = i;
                                lowestY = j;
                            }
                        }
                    }
                    // redestribute the biggest height to the lowest memeber
                    if (maxDifference > talus) {
                        newHeight = currentHeight - maxDifference / 2.0f;
                        terrainMap[x, y] = newHeight;
                        terrainMap[x + lowestX, y + lowestY] = newHeight;
                    }
                }
            }
        }
    }
    public static void ImprovedThermalErosion(ref float[,] terrainMap, int width, int height, int iter) {
        float talus = 4.0f / width;
        int lowestX = -1, lowestY = -1;

        float currentDifference, currentHeight,
              maxDifference,
              newHeight;
        // iter one for simplicity
        for (int o = 0; o < iter; o++) {
            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    currentHeight = terrainMap[x, y];
                    maxDifference = -float.MaxValue;
                    for (int i = -1; i < 2; i += 1) {
                        for (int j = -1; j < 2; j += 1) {
                            currentDifference = currentHeight - terrainMap[x + i, y + j];
                            if (currentDifference > maxDifference) {
                                maxDifference = currentDifference;
                                lowestX = i;
                                lowestY = j;
                            }
                        }
                    }
                    // we make sure the max difference is bigger than 0
                    if (maxDifference > 0.0f && maxDifference <= talus) {
                        newHeight = currentHeight - maxDifference / 2.0f;
                        terrainMap[x, y] = newHeight;
                        terrainMap[x + lowestX, y + lowestY] = newHeight;
                    }
                }
            }
        }
    }
    public static void HydraulicErosion(ref float[,] terrainMap, int width, int height, int iter) {
        int x, y, i, j, iterCount,
        lowestX = -1, lowestY = -1;

        float[,] water_map = new float[width, height];
        float rainAmmount = 0.01f, //amount of rain dropped per pixel each iteration
          solubility = 0.01f, //how much sediment a unit of water will erode
          evaporation = 0.9f, //how much water evaporates from each pixel each iteration
          capacity = solubility, //how much sediment a unit of water can hold
          waterLost, currentHeight, currentDifference, maxDifference; //temporary variables
        // initialise water map
        for (i = 0; i < height; ++i) {
            for (j = 0; j < width; ++j)
                water_map[i, j] = 0.0f;
        }
        for (iterCount = 0; iterCount < iter; ++iterCount) {
            //step 1: rain
            for (x = 0; x < height; ++x) {
                for (y = 0; y < width; ++y)
                    water_map[x, y] += rainAmmount;
            }
            //step 2: erosion
            for (x = 0; x < height; ++x) {
                for (y = 0; y < width; ++y) {
                    terrainMap[x, y] -= water_map[x, y] * solubility;
                }
            }
            //step 3: movement
            // again neumann neighbours, smallest member gets height redistributed from largest member
            for (x = 1; x < (height - 1); ++x) {
                for (y = 1; y < (width - 1); ++y) {
                    //find the lowest neighbor
                    currentHeight = terrainMap[x, y] + water_map[x, y];
                    maxDifference = -float.MaxValue;
                    for (i = -1; i < 2; i += 1) {
                        for (j = -1; j < 2; j += 1) {
                            currentDifference = currentHeight - terrainMap[x + i, y + j] - water_map[x + i, y + i];
                            if (currentDifference > maxDifference) {
                                maxDifference = currentDifference;
                                lowestX = i;
                                lowestY = j;
                            }
                        }
                    }
                    //now either do nothing, level off, or move all the water
                    if (maxDifference > 0.0f) {
                        //move it all...
                        if (water_map[x, y] < maxDifference) {
                            water_map[x + lowestX, y + lowestY] += water_map[x, y];
                            water_map[x, y] = 0.0f;
                        }
                        //level off...
                        else {
                            water_map[x + lowestX, y + lowestY] += maxDifference / 2.0f;
                            water_map[x, y] -= maxDifference / 2.0f;
                        }
                    }
                }
            }
            //step 4: evaporation / deposition
            for (x = 0; x < height; ++x) {
                for (y = 0; y < width; ++y) {
                    waterLost = water_map[x, y] * evaporation;
                    water_map[x, y] -= waterLost;
                    terrainMap[x, y] += waterLost * capacity;
                }
            }
        }
    }
}