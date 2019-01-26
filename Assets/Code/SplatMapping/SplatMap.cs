using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AssignSplatMap : MonoBehaviour {
    private static bool ValidParameterCheck(List<TerrainParameters> terrainParameterList) {
        foreach (var parameter in terrainParameterList) {
            if (parameter.TerrainTexture == null) {
                return false;
            }
        }
        return true;
    }
    public static void DoSplat(float[,] terrainHeightMap, float[,] terrainTemperatureMap, float[,] terrainMoistureMap, Terrain terrain, TerrainData terrainData, List<TerrainParameters> terrainParameterList, int terrainWidth, int terrainHeight) {
        if (!ValidParameterCheck(terrainParameterList)) {
            Debug.LogError("Invalid textures for splat mapping, make sure you have textures set in the reorderable list!");
            return;
        }
        float[,,] splatmapData = new float[terrainWidth, terrainHeight, terrainParameterList.Count];
        SplatPrototype[] splat_lists = new SplatPrototype[terrainParameterList.Count];
        for (int i = 0; i < splat_lists.Length; i++) {
            splat_lists[i] = new SplatPrototype();
            splat_lists[i].texture = terrainParameterList[i].TerrainTexture;
        }
        terrainData.splatPrototypes = splat_lists;
        for (int y = 0; y < terrainData.alphamapHeight; y++) {
            for (int x = 0; x < terrainData.alphamapWidth; x++) {
                float height = terrainHeightMap[x, y];

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];
                int idx = TextureGeneration.GetCorrectBiomeIndex(terrainHeightMap, terrainTemperatureMap, terrainMoistureMap, terrainParameterList, x, y);
                // if the current parameterBoundry is smaller or equal to the height means we found our boundry
                splatWeights[idx] = 0.5f;
                // blending, if we arnt at ocean/snow level
                if (idx >= 1 && idx < terrainParameterList.Count - 1) {
                    splatWeights[idx + 1] = 0.25f;
                    splatWeights[idx - 1] = 0.25f;
                }
                // we are at ocean level, blend the sand
                else if (idx == 0) {
                    splatWeights[idx + 1] = 0.5f;
                }
                // we are at snow level, blend the snow
                else if (idx == terrainParameterList.Count - 1) {
                    splatWeights[idx - 1] = 0.5f;
                }


                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                // float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++) {
                    // Normalize so that sum of all texture weights = 1
                    // splatWeights[i] /= z;
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}