using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(TerrainGeneration), true)]
public class ParameterEditorWidget : Editor {
    private ReorderableList ReorderableParameterList;
    private TerrainGeneration TerrainGenerationScript;
    private int CurrentSelectedIndex = 0;
    private string NoisePresetName = string.Empty;
    public List<NoiseParameters> AllParameters = new List<NoiseParameters>();
    public string[] AllParameterNames;

    private void OnEnable() {
        TerrainGenerationScript = (TerrainGeneration)target;
        // You dont have deserialize every frame, very costly.
        AllParameters = SerializationManager.ReadAllNoiseParameters();
        AllParameterNames = new string[AllParameters.Count];
        for (int i = 0; i < AllParameters.Count; i++) {
            AllParameterNames[i] = AllParameters[i].NoiseParameterName;
        }
    }
    private void OnSceneGUI() {
    }

    private ReorderableList DisplayParameterList() {
        if (ReorderableParameterList != null) {
            return ReorderableParameterList;
        }
        ReorderableParameterList = new ReorderableList(TerrainGenerationScript.TerrainParameterList, typeof(TerrainParameters), true, true, true, true);
        ReorderableParameterList.elementHeight = 22.0f * 3;
        ReorderableParameterList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var currentParameter = TerrainGenerationScript.TerrainParameterList[index];
            rect.height = 20.0f;
            currentParameter.Name = EditorGUI.TextField(rect, "Name", currentParameter.Name);
            rect.y += 22.0f;
            rect.height = 20.0f;
            currentParameter.ParameterBoundry = EditorGUI.FloatField(rect, "Boundry", currentParameter.ParameterBoundry);
            rect.height = 22.0f;
            rect.y += 22.0f;
            currentParameter.TerrainColor = EditorGUI.ColorField(rect, "Color", currentParameter.TerrainColor);
            TerrainGenerationScript.TerrainParameterList[index] = currentParameter;
        };
        ReorderableParameterList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, string.Format("Parameter list"));
        };
        ReorderableParameterList.onSelectCallback = (ReorderableList list) => {
        };
        ReorderableParameterList.onRemoveCallback = (ReorderableList list) => {
            TerrainGenerationScript.TerrainParameterList.RemoveAt(list.index);
        };
        return ReorderableParameterList;
    }

    public override void OnInspectorGUI() {
        DrawTerrainGenerationProperties();
        serializedObject.Update();
        DisplayParameterList().DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(TerrainGenerationScript);
    }

    public void DrawTerrainGenerationProperties() {
        if (AllParameterNames.Length > 0) {
            CurrentSelectedIndex = EditorGUILayout.Popup(CurrentSelectedIndex, AllParameterNames);
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Load preset")) {
                var loadedNoiseParameterPreset = AllParameters[CurrentSelectedIndex];
                TerrainGenerationScript.TerrainTextureType = loadedNoiseParameterPreset.TerrainTextureType;
                TerrainGenerationScript.NoiseScale = loadedNoiseParameterPreset.NoiseScale;
                TerrainGenerationScript.BaseFrequency = loadedNoiseParameterPreset.BaseFrequency;
                TerrainGenerationScript.Persistance = loadedNoiseParameterPreset.Persistance;
                TerrainGenerationScript.Lacunarity = loadedNoiseParameterPreset.Lacunarity;
                TerrainGenerationScript.Seed = loadedNoiseParameterPreset.Seed;
                TerrainGenerationScript.UserOffset = loadedNoiseParameterPreset.UserOffset;
                TerrainGenerationScript.CustomFunction = loadedNoiseParameterPreset.CustomFunction;
                TerrainGenerationScript.CustomExponent = loadedNoiseParameterPreset.CustomExponent;
                TerrainGenerationScript.TerrainParameterList = loadedNoiseParameterPreset.TerrainParameterList;
            }
        }
        EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "Noise parameter serializer (for runtime use):");
        NoisePresetName = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Noise preset name: ", NoisePresetName);
        if (GUI.Button(EditorGUILayout.GetControlRect(), "Save preset")) {
            NoiseParameters currentNoiseParameters = new NoiseParameters(NoisePresetName, TerrainGenerationScript.TerrainParameterList, TerrainGenerationScript.UserOffset, TerrainGenerationScript.NoiseScale,
                TerrainGenerationScript.BaseFrequency, TerrainGenerationScript.Persistance, TerrainGenerationScript.Lacunarity, TerrainGenerationScript.NumberOfOctaves, TerrainGenerationScript.Seed,
                TerrainGenerationScript.CustomFunction, TerrainGenerationScript.CustomExponent, TerrainGenerationScript.TerrainTextureType);
            SerializationManager.SaveNoiseParameters(NoisePresetName, currentNoiseParameters);
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        TerrainGenerationScript.TerrainTextureType = (NoiseParameters.TextureType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript.TerrainTextureType);
        var noiseScale = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Noise Scale", TerrainGenerationScript.NoiseScale);
        TerrainGenerationScript.NoiseScale = noiseScale <= 0 ? 0.0001f : noiseScale;
        var baseFrequency = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Base Frequecny", TerrainGenerationScript.BaseFrequency);
        TerrainGenerationScript.BaseFrequency = baseFrequency <= 0 ? 0.0001f : baseFrequency;
        var persistance = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Persistance", TerrainGenerationScript.Persistance);
        TerrainGenerationScript.Persistance = persistance <= 0 ? 0.0001f : persistance;
        var lacunarity = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Lacunarity", TerrainGenerationScript.Lacunarity);
        TerrainGenerationScript.Lacunarity = lacunarity <= 0 ? 0.0001f : lacunarity;
        TerrainGenerationScript.Seed = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Seed", TerrainGenerationScript.Seed);
        TerrainGenerationScript.UserOffset = EditorGUI.Vector2Field(EditorGUILayout.GetControlRect(), "User Offset", TerrainGenerationScript.UserOffset);
        TerrainGenerationScript.CustomFunction = (NoiseGeneration.CustomFunctionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript.CustomFunction);
        if (TerrainGenerationScript.CustomFunction == NoiseGeneration.CustomFunctionType.kCustom) {
            var customExponent = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Custom Exponent", TerrainGenerationScript.CustomExponent);
            TerrainGenerationScript.CustomExponent = customExponent <= 0 ? 0.0001f : customExponent;
        }
        EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "PARAMETER BOUNDRIES NEED TO BE IN ASCENDING ORDER!");
        // @TODO, add a button that saves the current preset of all parameters (serialize or just put as default values in TerrainGeneration)
    }
}
