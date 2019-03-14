using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
// this class should be okay, nothing needs to be set in editor mode, all runtime variables
public class NoiseParameterEditor {
    public int CurrentSelectedIndexNoise = 0;
    int DeleteFailsafe = 0;
    public string NoisePresetName = string.Empty;
    public List<NoiseParameters> SerializedNoiseParameters;
    public string[] AllNoiseParameterNames = new string[0];
    public NoiseParameters SerializedNoiseParameter;

    public NoiseParameterEditor(TerrainInfo info) {
        SerializedNoiseParameters = new List<NoiseParameters>();
        CurrentSelectedIndexNoise = EditorPrefs.GetInt("ParameterPresetNoiseIdx");
        TryGeneratingSavedNoiseParameters(info, true);
    }

    public void DrawNoiseParameterGUI(TerrainInfo info, Dictionary<string, bool> EditorWidgetFoldouts) {
        EditorWidgetFoldouts["TerrainGenerationWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["TerrainGenerationWidget"], "TerrainGenerationWidget");
        if (EditorWidgetFoldouts["TerrainGenerationWidget"]) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("DebugPlane coloring");
            SerializedNoiseParameter.TerrainTextureType = (TextureType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), SerializedNoiseParameter.TerrainTextureType);
            GUILayout.EndHorizontal();
            var noiseScale = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Noise Scale", SerializedNoiseParameter.NoiseScale);
            SerializedNoiseParameter.NoiseScale = noiseScale <= 0 ? 0.0001f : noiseScale;
            var baseFrequency = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Base Frequecny", SerializedNoiseParameter.BaseFrequency);
            SerializedNoiseParameter.BaseFrequency = baseFrequency <= 0 ? 0.0001f : baseFrequency;
            var persistance = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Persistance", SerializedNoiseParameter.Persistance);
            SerializedNoiseParameter.Persistance = persistance <= 0 ? 0.0001f : persistance;
            var lacunarity = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Lacunarity", SerializedNoiseParameter.Lacunarity);
            SerializedNoiseParameter.Lacunarity = lacunarity <= 0 ? 0.0001f : lacunarity;
            var octaves = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "Number of octaves", SerializedNoiseParameter.NumberOfOctaves);
            SerializedNoiseParameter.NumberOfOctaves = octaves <= 0 ? 1 : octaves;
            SerializedNoiseParameter.Seed = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Seed", SerializedNoiseParameter.Seed);
            SerializedNoiseParameter.UserOffset = EditorGUI.Vector2Field(EditorGUILayout.GetControlRect(), "User Offset", SerializedNoiseParameter.UserOffset);
            SerializedNoiseParameter.GlobalNoiseAddition = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Global noise add", SerializedNoiseParameter.GlobalNoiseAddition);
            SerializedNoiseParameter.CustomFunction = (CustomFunctionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), SerializedNoiseParameter.CustomFunction);
            if (SerializedNoiseParameter.CustomFunction == CustomFunctionType.kCustom) {
                var customExponent = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Custom Exponent", SerializedNoiseParameter.CustomExponent);
                SerializedNoiseParameter.CustomExponent = customExponent <= 0 ? 0.0001f : customExponent;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pick a generation type");
            info.GenerationType = (GenerationType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), info.GenerationType);
            GUILayout.EndHorizontal();

            if (GUI.Button(EditorGUILayout.GetControlRect(), "Paint the terrain!")) {
                if (info.GenerationType != GenerationType.kSingleRun) {
                    Debug.LogWarningFormat("WARNING: Parameter _GenerationType is {0}, needs to be kSingleRun (can't paint terrain in runtime), changing to single run.", info.GenerationType.ToString());
                    info.GenerationType = GenerationType.kSingleRun;
                }
                AssignSplatMap.DoSplat(info);
            }
            EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "PARAMETER BOUNDRIES NEED TO BE IN ASCENDING ORDER!");
        }
        GUILayout.Label("Saved noise presets");
        GUILayout.BeginHorizontal(GUILayout.Width(250));
        var lastIndex = CurrentSelectedIndexNoise;
        CurrentSelectedIndexNoise = EditorGUILayout.Popup(CurrentSelectedIndexNoise, AllNoiseParameterNames, GUILayout.Width(250));
        DeleteFailsafe = lastIndex != CurrentSelectedIndexNoise ? 0 : DeleteFailsafe;
        if (GUILayout.Button(string.Format("Delete selected preset ({0})", DeleteFailsafe), GUILayout.MaxWidth(200))) {
            if (DeleteFailsafe == 2) {
                DeleteFailsafe = 0;
                SerializationManager.DeleteNoiseParameter(AllNoiseParameterNames[CurrentSelectedIndexNoise]);
                SerializedNoiseParameters.RemoveAt(CurrentSelectedIndexNoise);
                TryGeneratingSavedNoiseParameters(info, false);
            } else {
                DeleteFailsafe++;
            }
        }
        GUILayout.EndHorizontal();
        if (GUI.Button(EditorGUILayout.GetControlRect(), "Load preset")) {
            TryGeneratingSavedNoiseParameters(info, true);
        }
        EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "Terrain parameter serializer, saves the current terrain preset configuration.");
        NoisePresetName = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Terrain preset name: ", NoisePresetName);
        if (GUI.Button(EditorGUILayout.GetControlRect(), "Save preset")) {
            SerializedNoiseParameter.NoiseParameterName = NoisePresetName;
            SerializationManager.SaveNoiseParameters(NoisePresetName, SerializedNoiseParameter);
            TryGeneratingSavedNoiseParameters(info, false);
        }
        EditorUtils.GUILine();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        SetCorrectNoiseParams(SerializedNoiseParameter, ref info);
        EditorPrefs.SetInt("ParameterPresetNoiseIdx", CurrentSelectedIndexNoise);
    }
    public void TryGeneratingSavedNoiseParameters(TerrainInfo info, bool overrideSerialisedParam) {
        SerializationManager.InitializeManager();
        SerializedNoiseParameters = SerializationManager.ReadAllNoiseParameters();
        AllNoiseParameterNames = new string[SerializedNoiseParameters.Count];
        for (int i = 0; i < SerializedNoiseParameters.Count; i++) {
            AllNoiseParameterNames[i] = SerializedNoiseParameters[i].NoiseParameterName;
        }
        if (overrideSerialisedParam) {
            SerializedNoiseParameter = SerializedNoiseParameters[CurrentSelectedIndexNoise];
            var cp = SerializedNoiseParameters[CurrentSelectedIndexNoise];
            SetCorrectNoiseParams(cp, ref info);
        }
    }
    private void SetCorrectNoiseParams(NoiseParameters noiseParam, ref TerrainInfo info) {
        info.ErosionType = noiseParam.ErosionType;
        info.RuntimeErosion = noiseParam.RuntimeErosion;
        info.NoiseScale = noiseParam.NoiseScale;
        info.BaseFrequency = noiseParam.BaseFrequency;
        info.Persistance = noiseParam.Persistance;
        info.Lacunarity = noiseParam.Lacunarity;
        info.NumberOfOctaves = noiseParam.NumberOfOctaves;
        info.GlobalNoiseAddition = noiseParam.GlobalNoiseAddition;
        info.Seed = noiseParam.Seed;
        info.UserOffset = noiseParam.UserOffset;
        info.CustomFunction = noiseParam.CustomFunction;
        info.CustomExponent = noiseParam.CustomExponent;
        info.TerrainTextureType = noiseParam.TerrainTextureType;
    }
}