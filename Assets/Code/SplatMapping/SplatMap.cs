using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AssignSplatMap : MonoBehaviour
{
    private static bool ValidParameterCheck(List<TerrainParameters> terrainParameterList)
    {
        foreach (var parameter in terrainParameterList)
        {
            if (parameter.TerrainTextureSpring == null ||
                parameter.TerrainTextureSummer == null ||
                parameter.TerrainTextureAutumn == null ||
                parameter.TerrainTextureWinter == null)
            {
                return false;
            }
        }
        return true;
    }

    public static void DoRoadSplat(ref float[,,] splatmapData, int x, int y)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int h = 0; h < 6; h++)
                {
                    splatmapData[x + i, y + j, h] = 0f;
                    splatmapData[x - i, y - j, h] = 0f;
                    splatmapData[x + i, y - j, h] = 0f;
                    splatmapData[x - i, y + j, h] = 0f;
                    splatmapData[x, y + j, h] = 0f;
                    splatmapData[x + j, y, h] = 0f;
                    splatmapData[x, y - j, h] = 0f;
                    splatmapData[x - j, y, h] = 0f;
                    splatmapData[x, y, h] = 0f;

                    splatmapData[x + i, y + j, 7] = 1f;
                    splatmapData[x - i, y - j, 7] = 1f;
                    splatmapData[x + i, y - j, 7] = 1f;
                    splatmapData[x - i, y + j, 7] = 1f;
                    splatmapData[x, y + j, 7] = 1f;
                    splatmapData[x + j, y, 7] = 1f;
                    splatmapData[x, y - j, 7] = 1f;
                    splatmapData[x - j, y, 7] = 1f;
                    splatmapData[x, y, 7] = 1f;
                }
            }
        }
    }
    // everything looks moved one idx down, baisicly water is one idx up
    public static void DoSplat(TerrainInfo info, SeasonType seasonType = SeasonType.kUndefined, float seasonAmmount = 1.0f)
    {
        if (!ValidParameterCheck(info.TerrainParameterList))
        {
            Debug.LogError("Invalid textures for splat mapping, make sure you have textures set in the reorderable list!");
            return;
        }

        float[,,] splatmapData = new float[info.TerrainWidth, info.TerrainHeight, info.TerrainParameterList.Count + 1];
        TerrainLayer[] terrain_layers = new TerrainLayer[info.TerrainParameterList.Count + 1];
        for (int i = 0; i < terrain_layers.Length - 1; i++)
        {
            terrain_layers[i] = new TerrainLayer();
            switch (seasonType)
            {
                case SeasonType.kSpring:
                    terrain_layers[i].diffuseTexture = info.TerrainParameterList[i].TerrainTextureSpring;
                    break;
                case SeasonType.kSummer:
                    terrain_layers[i].diffuseTexture = info.TerrainParameterList[i].TerrainTextureSummer;
                    break;
                case SeasonType.kAutumn:
                    terrain_layers[i].diffuseTexture = info.TerrainParameterList[i].TerrainTextureAutumn;
                    break;
                case SeasonType.kWinter:
                    terrain_layers[i].diffuseTexture = info.TerrainParameterList[i].TerrainTextureWinter;
                    break;
                case SeasonType.kUndefined:
                    terrain_layers[i].diffuseTexture = info.TerrainParameterList[i].TerrainTextureSpring;
                    break;
            }
        }
        terrain_layers[terrain_layers.Length - 1] = new TerrainLayer();
        terrain_layers[terrain_layers.Length - 1].diffuseTexture = info.RoadGenerator.RoadTexture;
        info._Terrain.terrainData.terrainLayers = terrain_layers;
        var tmpRoad = new List<(int, int)>();
        for (int y = 0; y < info._Terrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < info._Terrain.terrainData.alphamapWidth; x++)
            {
                float[] splatWeights = new float[info._Terrain.terrainData.alphamapLayers];
                if (info.RoadGenerator.IsRoadOnCoordinates(y, x))
                {
                    tmpRoad.Add((x, y));
                    //DoRoadSplat(ref splatmapData, x, y);
                    continue;
                }
                else
                {
                    // Setup an array to record the mix of texture weights at this point
                    int idx = BiomeGeneration.GetCorrectBiomeIndex(info, x, y);
                    //splatWeights[idx] = 1f;
                    // blending, if we arnt at ocean/snow level
                    if (idx >= 1 && idx < info.TerrainParameterList.Count - 1)
                    {
                        splatWeights[idx + 1] = 0.5f;
                        splatWeights[idx - 1] = 0.5f;
                    }
                    // we are at ocean level, blend the sand
                    else if (idx == 0)
                    {
                        splatWeights[idx + 1] = 0.5f;
                    }
                    // we are at snow level, blend the snow
                    else if (idx == info.TerrainParameterList.Count - 1)
                    {
                        splatWeights[idx - 1] = 0.5f;
                    }
                    // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                }
                float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < info._Terrain.terrainData.alphamapLayers; i++)
                {
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
        for (int i = 0; i < tmpRoad.Count; i++)
        {
            var seg = tmpRoad[i];
            DoRoadSplat(ref splatmapData, seg.Item1, seg.Item2);
        }
        // Finally assign the new splatmap to the terrainData:
        info._Terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}