using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AssignSplatMap : MonoBehaviour {
    private static bool ValidParameterCheck(List<TerrainParameters> terrainParameterList) {
        foreach (var parameter in terrainParameterList) {
            if (parameter.TerrainTextureSpring == null ||
                parameter.TerrainTextureSummer == null ||
                parameter.TerrainTextureAutumn == null ||
                parameter.TerrainTextureWinter == null) {
                return false;
            }
        }
        return true;
    }
    public static void DoSplat(TerrainInfo info) {
        if (!ValidParameterCheck(info.TerrainParameterList)) {
            Debug.LogError("Invalid textures for splat mapping, make sure you have textures set in the reorderable list!");
            return;
        }
        float[,,] splatmapData = new float[info.TerrainWidth, info.TerrainHeight, info.TerrainParameterList.Count];
        SplatPrototype[] splat_lists = new SplatPrototype[info.TerrainParameterList.Count];
        for (int i = 0; i < splat_lists.Length; i++) {
            splat_lists[i] = new SplatPrototype();
            // @ TODO, SEASONAL CHANGES WILL HAPPEN HERE, ONLY WORKING WITH SPRING FOR TESTING, IMPL !!!
            splat_lists[i].texture = info.TerrainParameterList[i].TerrainTextureSpring;
        }
        info._Terrain.terrainData.splatPrototypes = splat_lists;
        for (int y = 0; y < info._Terrain.terrainData.alphamapHeight; y++) {
            for (int x = 0; x < info._Terrain.terrainData.alphamapWidth; x++) {
                // float height = terrainHeightMap[x, y];

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[info._Terrain.terrainData.alphamapLayers];
                int idx = BiomeGeneration.GetCorrectBiomeIndex(info, x, y);
                // if the current parameterBoundry is smaller or equal to the height means we found our boundry
                splatWeights[idx] = 0.5f;
                // blending, if we arnt at ocean/snow level
                if (idx >= 1 && idx < info.TerrainParameterList.Count - 1) {
                    splatWeights[idx + 1] = 0.25f;
                    splatWeights[idx - 1] = 0.25f;
                }
                // we are at ocean level, blend the sand
                else if (idx == 0) {
                    splatWeights[idx + 1] = 0.5f;
                }
                // we are at snow level, blend the snow
                else if (idx == info.TerrainParameterList.Count - 1) {
                    splatWeights[idx - 1] = 0.5f;
                }
                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                // float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < info._Terrain.terrainData.alphamapLayers; i++) {
                    // Normalize so that sum of all texture weights = 1
                    // splatWeights[i] /= z;
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
        // Finally assign the new splatmap to the terrainData:
        info._Terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}