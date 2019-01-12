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
    private Dictionary<string, bool> EditorWidgetFoldouts = new Dictionary<string, bool>();
    private List<string> EditorWidgetNames = new List<string> { "ErosionWidget", "ParameterPresetWidget", "TerrainGenerationWidget", "ParameterListWidget" };
    public List<NoiseParameters> AllParameters = new List<NoiseParameters>();
    public string[] AllParameterNames = new string[0];

    private void OnEnable() {
        foreach (var widgetName in EditorWidgetNames) {
            EditorWidgetFoldouts.Add(widgetName, true);
        }
        TerrainGenerationScript = (TerrainGeneration)target;
        TryGeneratingSavedParameterList();
        // This is to have the parameter list already loaded
        if (AllParameters.Count > 0) {
            TerrainGenerationScript.TerrainParameterList = AllParameters[0].TerrainParameterList;
        }
    }

    /// <summary>
    /// The main InspectorGUI code, every widget drawer gets called here
    /// </summary>
    public override void OnInspectorGUI() {
        DrawErosionTypeProperties();
        DrawTerrainGenerationProperties();
        serializedObject.Update();
        EditorWidgetFoldouts["ParameterListWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["ParameterListWidget"], "ParameterListWidget");
        if (EditorWidgetFoldouts["ParameterListWidget"]) {
            DisplayParameterList().DoLayoutList();
        }
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(TerrainGenerationScript);
    }

    /// <summary>
    /// Draws the ErosionTypeWidget
    /// </summary>
    public void DrawErosionTypeProperties() {
        EditorWidgetFoldouts["ErosionWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["ErosionWidget"], "ErosionWidget");
        if (EditorWidgetFoldouts["ErosionWidget"]) {
            GUILayout.Label("Pick an erosion type, try different iter numbers !");
            TerrainGenerationScript._ErosionType = (ErosionGeneration.ErosionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript._ErosionType);
            var iters = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "Number of erosion iterations", TerrainGenerationScript.ErosionIterations);
            TerrainGenerationScript.ErosionIterations = iters <= 0 ? 1 : iters;
        }
        GUILine();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    public void DrawTerrainGenerationProperties() {
        DrawLoadSaveGUI();
        DrawNoiseParameterGUI();
    }

    /// <summary>
    /// Draws the ParameterPresetWidget
    /// </summary>
    private void DrawLoadSaveGUI() {
        // Draws the GUI widgets for picking and deleting parameters
        EditorWidgetFoldouts["ParameterPresetWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["ParameterPresetWidget"], "ParameterPresetWidget");
        if (EditorWidgetFoldouts["ParameterPresetWidget"]) {
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
                    NoiseParameters loadedNoiseParameterPreset = AllParameters[CurrentSelectedIndex];
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
                    TerrainGenerationScript._ErosionType = loadedNoiseParameterPreset.ErosionType;
                    TerrainGenerationScript.ErosionIterations = loadedNoiseParameterPreset.ErosionIterations;
                    TerrainGenerationScript.GenerateTerrainFromPreset();
                    ReorderableParameterList = null;
                }
            }
            // Draws the GUI widgets for saving presets
            EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "Noise parameter serializer, saves the current configuration.");
            NoisePresetName = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Noise preset name: ", NoisePresetName);
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Save preset")) {
                NoiseParameters currentNoiseParameters = new NoiseParameters(TerrainGenerationScript._ErosionType, TerrainGenerationScript.ErosionIterations, NoisePresetName, TerrainGenerationScript.TerrainParameterList, TerrainGenerationScript.UserOffset, TerrainGenerationScript.NoiseScale,
                    TerrainGenerationScript.BaseFrequency, TerrainGenerationScript.Persistance, TerrainGenerationScript.Lacunarity, TerrainGenerationScript.NumberOfOctaves, TerrainGenerationScript.GlobalNoiseAddition, TerrainGenerationScript.Seed,
                    TerrainGenerationScript.CustomFunction, TerrainGenerationScript.CustomExponent, TerrainGenerationScript.TerrainTextureType);
                SerializationManager.SaveNoiseParameters(NoisePresetName, currentNoiseParameters);
                TryGeneratingSavedParameterList();
            }
        }
        GUILine();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Draws TerrainGenerationWidget
    /// </summary>
    public void DrawNoiseParameterGUI() {
        EditorWidgetFoldouts["TerrainGenerationWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["TerrainGenerationWidget"], "TerrainGenerationWidget");
        if (EditorWidgetFoldouts["TerrainGenerationWidget"]) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("DebugPlane coloring");
            TerrainGenerationScript.TerrainTextureType = (NoiseParameters.TextureType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript.TerrainTextureType);
            GUILayout.EndHorizontal();
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pick a generation type");
            TerrainGenerationScript._GenerationType = (TerrainGeneration.GenerationType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript._GenerationType);
            GUILayout.EndHorizontal();
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Paint the terrain!")) {
                if (TerrainGenerationScript._GenerationType != TerrainGeneration.GenerationType.kSingleRun) {
                    Debug.LogErrorFormat("WARNING: Parameter _GenerationType is {0}, needs to be kSingleRun (can't paint terrain in runtime), change the generation type then try again!", TerrainGenerationScript._GenerationType.ToString());
                }
                AssignSplatMap.DoSplat(TerrainGenerationScript._Terrain, TerrainGenerationScript._Terrain.terrainData, TerrainGenerationScript.TerrainParameterList, TerrainGenerationScript.TerrainWidth, TerrainGenerationScript.TerrainHeight);
            }
            EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "PARAMETER BOUNDRIES NEED TO BE IN ASCENDING ORDER!");
        }
        GUILine();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
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

    private void TryGeneratingSavedParameterList() {
        AllParameters = SerializationManager.ReadAllNoiseParameters();
        AllParameterNames = new string[AllParameters.Count];
        for (int i = 0; i < AllParameters.Count; i++) {
            AllParameterNames[i] = AllParameters[i].NoiseParameterName;
        }
    }

    // Helper to draw a line in edtior, cuz Unity...
    void GUILine(int i_height = 2) {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}
