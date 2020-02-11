using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiomeGeneration {
    public static int GetCorrectBiomeIndex(TerrainInfo info, int x, int y) {
        List<int> validHeightsIndicies = new List<int>();
        List<int> validTemperatureIndicies = new List<int>();
        List<int> validMoistureIndicies = new List<int>();
        var tparams = info.TerrainParameterList;
        // iterate first time to figure out where we are on the height map
        float hmapValue = info.HeightMap[x, y];
        for (int i = 0; i < tparams.Count; i++) {
            var tparam = tparams[i];
            if (hmapValue <= tparam.ParameterBoundry) {
                validHeightsIndicies.Add(i);
            }
        }
        float tmapValue = info.TemperatureMap[x, y];
        for (int i = 0; i < validHeightsIndicies.Count; i++) {
            var tparam = tparams[i];
            if (tmapValue <= tparam.TemperatureParameterBoundry) {
                validTemperatureIndicies.Add(i);
            }
        }

        float mmapValue = info.MoistureMap[x, y];
        for (int i = 0; i < validTemperatureIndicies.Count; i++) {
            var tparam = tparams[i];
            if (mmapValue <= tparam.MoistureParameterBoundry) {
                validMoistureIndicies.Add(i);
            }
        }
        if (validMoistureIndicies.Count > 0) {
            return validMoistureIndicies.Last();
        } else if (validTemperatureIndicies.Count > 0) {
            return validTemperatureIndicies.Last();
        } else if (validHeightsIndicies.Count > 0) {
            return validHeightsIndicies.Last();
        } else {
            // this should not happen
            string debugStr = string.Format("HM: {0}, MM: {1}, TM: {2}", hmapValue, mmapValue, tmapValue);
            Debug.LogError(debugStr);
            return -1;
        }
    }

    public static int[,] GenerateBiomeMap(TerrainInfo info) {
        int[,] biomes = new int[info.TerrainWidth, info.TerrainHeight];
        for (int y = 0; y < info.TerrainHeight; y++) {
            for (int x = 0; x < info.TerrainWidth; x++) {
                var biome_idx = GetCorrectBiomeIndex(info, x, y);
                biomes[x, y] = biome_idx;
                // initialise our seperated biomes dict, idx represents the index of the biome param (check the parameter list)
                if (!info.SeperatedBiomes.ContainsKey(biome_idx)) {
                    info.SeperatedBiomes.Add(biome_idx, new List<TerrainPoint>());
                }
                // add the point of our dict
                info.SeperatedBiomes[biome_idx].Add(new TerrainPoint(y, x));
            }
        }
        return biomes;
    }
}