using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SerializationManager {

    private static string NoiseParameterLocation = @"Assets\Resources\NoiseParameterPresets\";

    public static void SaveNoiseParameters(string name, NoiseParameters parameters) {
        if (name.Length == 0) {
            Debug.LogError(string.Format("Name cannot be empty."));
            return;
        }
        string path = string.Format("{0}{1}.json", NoiseParameterLocation, name);
        string jsonObject = JsonConvert.SerializeObject(parameters, Formatting.None, new JsonSerializerSettings() {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 1
        });
        // User can overwrite his file
        using (StreamWriter file = new StreamWriter(path)) {
            file.Write(jsonObject);
        }
    }

    public static List<NoiseParameters> ReadAllNoiseParameters() {
        List<NoiseParameters> allNoiseParameters = new List<NoiseParameters>();
        foreach (string file in Directory.GetFiles(NoiseParameterLocation, "*.json")) {
            string jsonContent = File.ReadAllText(file);
            NoiseParameters parameter = JsonConvert.DeserializeObject<NoiseParameters>(jsonContent);
            for (int i = 0; i < parameter.TerrainParameterList.Count; i++) {
                parameter.TerrainParameterList[i] = new TerrainParameters(
                    parameter.TerrainParameterList[i].Name,
                    parameter.TerrainParameterList[i].MoistureParameterBoundry,
                    parameter.TerrainParameterList[i].TemperatureParameterBoundry,
                    parameter.TerrainParameterList[i].ParameterBoundry,
                    parameter.TerrainParameterList[i].TerrainColorVector,
                    parameter.TerrainParameterList[i].TexturePath);
            }
            allNoiseParameters.Add(parameter);
        }
        return allNoiseParameters;
    }

    public static void DeleteNoiseParameter(string name) {
        string path = string.Format("{0}{1}.json", NoiseParameterLocation, name);
        string pathMeta = string.Format("{0}{1}.json.meta", NoiseParameterLocation, name);
        if (File.Exists(path)) {
            File.Delete(path);
            File.Delete(pathMeta);
        } else {
            Debug.LogErrorFormat("{0} does not exist !", path);
        }
    }

}
