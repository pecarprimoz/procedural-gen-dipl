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
    public static void DoSplat(float[,] terrainHeightMap, Terrain terrain, TerrainData terrainData, List<TerrainParameters> terrainParameterList, int terrainWidth, int terrainHeight) {
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
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                float height = terrainHeightMap[x, y];

                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01, x_01);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];


                for (int i = 0; i < terrainParameterList.Count; i++) {
                    if (height <= terrainParameterList[i].ParameterBoundry) {
                        splatWeights[i] = 1;
                        break;
                    }
                }


                //// Texture[0] has constant influence
                //splatWeights[0] = 0.5f;

                //// Texture[1] is stronger at lower altitudes
                //splatWeights[1] = Mathf.Clamp01((terrainData.heightmapHeight - height));

                //// Texture[2] stronger on flatter terrain
                //// Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                //// Subtract result from 1.0 to give greater weighting to flat surfaces
                //splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 5.0f));

                //// Texture[3] increases with height but only on surfaces facing positive Z axis 
                //splatWeights[3] = height * Mathf.Clamp01(normal.z);

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++) {

                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}