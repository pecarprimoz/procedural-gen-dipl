using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SerializationManager {
    private static string NoiseParameterLocationWindows = @"Assets\Resources\NoiseParameterPresets\";
    private static string NoiseParameterLocationOSX = @"Assets/Resources/NoiseParameterPresets";
    private static string NoiseParameterLocation = "";

    private static string TerrainParameterLocationWindows = @"Assets\Resources\TerrainParameterPresets\";
    private static string TerrainParameterLocationOSX = @"Assets/Resources/TerrainParameterPresets";
    private static string TerrainParameterLocation = "";

    public static void InitializeManager() {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            NoiseParameterLocation = NoiseParameterLocationWindows;
            TerrainParameterLocation = TerrainParameterLocationWindows;
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
            NoiseParameterLocation = NoiseParameterLocationOSX;
            TerrainParameterLocation = TerrainParameterLocationOSX;
        }
    }
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

    public static void SaveTerrainPreset(string name, List<TerrainParameters> parameters) {
        if (name.Length == 0) {
            Debug.LogError(string.Format("Name cannot be empty."));
            return;
        }
        string path = string.Format("{0}{1}.json", TerrainParameterLocation, name);
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
            allNoiseParameters.Add(parameter);
        }
        return allNoiseParameters;
    }

    public static List<List<TerrainParameters>> ReadAllTerrainParameters(out List<string> paramNames) {
        paramNames = new List<string>();
        List<List<TerrainParameters>> allTerrainParameters = new List<List<TerrainParameters>>();
        foreach (string file in Directory.GetFiles(TerrainParameterLocation, "*.json")) {
            paramNames.Add(Path.GetFileName(file));
            string jsonContent = File.ReadAllText(file);
            List<TerrainParameters> parameters = JsonConvert.DeserializeObject<List<TerrainParameters>>(jsonContent);
            for (int i = 0; i < parameters.Count; i++) {
                var cp = parameters[i];
                cp.TerrainColor = new Color(cp.TerrainColorVector.x, cp.TerrainColorVector.y, cp.TerrainColorVector.z, 1);
                parameters[i] = cp;
            }
            allTerrainParameters.Add(parameters);
        }
        return allTerrainParameters;
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

    public static void DeleteTerrainParameter(string name) {
        string path = string.Format("{0}{1}.json", TerrainParameterLocation, name);
        string pathMeta = string.Format("{0}{1}.json.meta", TerrainParameterLocation, name);
        if (File.Exists(path)) {
            File.Delete(path);
            File.Delete(pathMeta);
        } else {
            Debug.LogErrorFormat("{0} does not exist !", path);
        }
    }

}
