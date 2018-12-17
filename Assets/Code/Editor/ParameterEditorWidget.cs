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
    private int DeleteFailsafe = 0;
    public List<NoiseParameters> AllParameters = new List<NoiseParameters>();
    public string[] AllParameterNames = new string[0];

    private void OnEnable() {
        TerrainGenerationScript = (TerrainGeneration)target;
        TryGeneratingSavedParameterList();
        // This is to have the parameter list already loaded
        if (AllParameters.Count > 0) {
            TerrainGenerationScript.TerrainParameterList = AllParameters[0].TerrainParameterList;
        }
    }

    private ReorderableList DisplayParameterList() {
        if (ReorderableParameterList != null) {
            return ReorderableParameterList;
        }
        ReorderableParameterList = new ReorderableList(TerrainGenerationScript.TerrainParameterList, typeof(TerrainParameters), true, true, true, true);
        ReorderableParameterList.elementHeight = 22.0f * 5;
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
            rect.height = 22.0f;
            rect.y += 22.0f;
            EditorGUI.LabelField(rect, currentParameter.TexturePath);
            rect.height = 22.0f;
            rect.y += 22.0f;
            // Edge case if textures exist in runtime
            if (currentParameter.TexturePath == null && currentParameter.TerrainTexture != null) {
                currentParameter.TexturePath = AssetDatabase.GetAssetPath(currentParameter.TerrainTexture);
            }
            if (currentParameter.TerrainTexture == null && currentParameter.TexturePath != null) {
                currentParameter.TerrainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(currentParameter.TexturePath, typeof(Texture2D));
            }
            var newTerrainTexture = (Texture2D)EditorGUI.ObjectField(rect, "Texture", currentParameter.TerrainTexture, typeof(Texture2D));
            if (newTerrainTexture != currentParameter.TerrainTexture) {
                currentParameter.TerrainTexture = newTerrainTexture;
                currentParameter.TexturePath = AssetDatabase.GetAssetPath(currentParameter.TerrainTexture);
            }
            TerrainGenerationScript.TerrainParameterList[index] = currentParameter;
            EditorGUILayout.Separator();
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

    private void DrawLoadSaveGUI() {
        // Draws the GUI widgets for picking and deleting parameters
        if (AllParameterNames.Length > 0) {
            GUILayout.Label("Saved parameter presets");
            GUILayout.BeginHorizontal(GUILayout.Width(250));
            var lastIndex = CurrentSelectedIndex;
            CurrentSelectedIndex = EditorGUILayout.Popup(CurrentSelectedIndex, AllParameterNames, GUILayout.Width(250));
            DeleteFailsafe = lastIndex != CurrentSelectedIndex ? 0 : DeleteFailsafe;
            if (GUILayout.Button(string.Format("Delete selected preset ({0})", DeleteFailsafe), GUILayout.MaxWidth(200))) {
                if (DeleteFailsafe == 2) {
                    DeleteFailsafe = 0;
                    SerializationManager.DeleteNoiseParameter(AllParameterNames[CurrentSelectedIndex]);
                    AllParameters.RemoveAt(CurrentSelectedIndex);
                    TryGeneratingSavedParameterList();
                } else {
                    DeleteFailsafe++;
                }
            }
            GUILayout.EndHorizontal();
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Load preset")) {
                TryGeneratingSavedParameterList();
                var loadedNoiseParameterPreset = AllParameters[CurrentSelectedIndex];
                TerrainGenerationScript.TerrainTextureType = loadedNoiseParameterPreset.TerrainTextureType;
                TerrainGenerationScript.NoiseScale = loadedNoiseParameterPreset.NoiseScale;
                TerrainGenerationScript.BaseFrequency = loadedNoiseParameterPreset.BaseFrequency;
                TerrainGenerationScript.Persistance = loadedNoiseParameterPreset.Persistance;
                TerrainGenerationScript.Lacunarity = loadedNoiseParameterPreset.Lacunarity;
                TerrainGenerationScript.NumberOfOctaves = loadedNoiseParameterPreset.NumberOfOctaves;
                TerrainGenerationScript.GlobalNoiseAddition = loadedNoiseParameterPreset.GlobalNoiseAddition;
                TerrainGenerationScript.Seed = loadedNoiseParameterPreset.Seed;
                TerrainGenerationScript.UserOffset = loadedNoiseParameterPreset.UserOffset;
                TerrainGenerationScript.CustomFunction = loadedNoiseParameterPreset.CustomFunction;
                TerrainGenerationScript.CustomExponent = loadedNoiseParameterPreset.CustomExponent;
                TerrainGenerationScript.TerrainParameterList = loadedNoiseParameterPreset.TerrainParameterList;
                ReorderableParameterList = null;
            }
        }
        // Draws the GUI widgets for saving presets
        EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "Noise parameter serializer, saves the current configuration.");
        NoisePresetName = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Noise preset name: ", NoisePresetName);
        if (GUI.Button(EditorGUILayout.GetControlRect(), "Save preset")) {
            NoiseParameters currentNoiseParameters = new NoiseParameters(NoisePresetName, TerrainGenerationScript.TerrainParameterList, TerrainGenerationScript.UserOffset, TerrainGenerationScript.NoiseScale,
                TerrainGenerationScript.BaseFrequency, TerrainGenerationScript.Persistance, TerrainGenerationScript.Lacunarity, TerrainGenerationScript.NumberOfOctaves, TerrainGenerationScript.GlobalNoiseAddition, TerrainGenerationScript.Seed,
                TerrainGenerationScript.CustomFunction, TerrainGenerationScript.CustomExponent, TerrainGenerationScript.TerrainTextureType);
            SerializationManager.SaveNoiseParameters(NoisePresetName, currentNoiseParameters);
            TryGeneratingSavedParameterList();
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    public void DrawNoiseParameterGUI() {
        TerrainGenerationScript.TerrainTextureType = (NoiseParameters.TextureType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript.TerrainTextureType);
        var noiseScale = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Noise Scale", TerrainGenerationScript.NoiseScale);
        TerrainGenerationScript.NoiseScale = noiseScale <= 0 ? 0.0001f : noiseScale;
        var baseFrequency = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Base Frequecny", TerrainGenerationScript.BaseFrequency);
        TerrainGenerationScript.BaseFrequency = baseFrequency <= 0 ? 0.0001f : baseFrequency;
        var persistance = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Persistance", TerrainGenerationScript.Persistance);
        TerrainGenerationScript.Persistance = persistance <= 0 ? 0.0001f : persistance;
        var lacunarity = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Lacunarity", TerrainGenerationScript.Lacunarity);
        TerrainGenerationScript.Lacunarity = lacunarity <= 0 ? 0.0001f : lacunarity;
        var octaves = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "Number of octaves", TerrainGenerationScript.NumberOfOctaves);
        TerrainGenerationScript.NumberOfOctaves = octaves <= 0 ? 1 : octaves;
        TerrainGenerationScript.Seed = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Seed", TerrainGenerationScript.Seed);
        TerrainGenerationScript.UserOffset = EditorGUI.Vector2Field(EditorGUILayout.GetControlRect(), "User Offset", TerrainGenerationScript.UserOffset);
        TerrainGenerationScript.GlobalNoiseAddition = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Global noise add", TerrainGenerationScript.GlobalNoiseAddition);
        TerrainGenerationScript.CustomFunction = (NoiseGeneration.CustomFunctionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript.CustomFunction);
        if (TerrainGenerationScript.CustomFunction == NoiseGeneration.CustomFunctionType.kCustom) {
            var customExponent = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Custom Exponent", TerrainGenerationScript.CustomExponent);
            TerrainGenerationScript.CustomExponent = customExponent <= 0 ? 0.0001f : customExponent;
        }
        if (GUI.Button(EditorGUILayout.GetControlRect(), "Paint the terrain!")) {
            AssignSplatMap.DoSplat(TerrainGenerationScript._Terrain, TerrainGenerationScript._Terrain.terrainData, TerrainGenerationScript.TerrainParameterList);
        }
        EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "PARAMETER BOUNDRIES NEED TO BE IN ASCENDING ORDER!");
    }

    public void DrawTerrainGenerationProperties() {
        GUILayout.Label("Pick a generation type");
        TerrainGenerationScript._GenerationType = (TerrainGeneration.GenerationType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript._GenerationType);
        EditorGUILayout.Space();
        DrawLoadSaveGUI();
        DrawNoiseParameterGUI();
    }

    private void TryGeneratingSavedParameterList() {
        AllParameters = SerializationManager.ReadAllNoiseParameters();
        AllParameterNames = new string[AllParameters.Count];
        for (int i = 0; i < AllParameters.Count; i++) {
            AllParameterNames[i] = AllParameters[i].NoiseParameterName;
        }
    }
}
