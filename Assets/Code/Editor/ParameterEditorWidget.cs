using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[InitializeOnLoad]
[CustomEditor(typeof(TerrainGeneration), true)]
public class ParameterEditorWidget : Editor {
    private ReorderableList ReorderableParameterList;
    private TerrainInfo TerrainInfo;
    private int CurrentSelectedIndex = 0;
    private string NoisePresetName = string.Empty;
    private int DeleteFailsafe = 0;
    private Dictionary<string, bool> EditorWidgetFoldouts = new Dictionary<string, bool>();
    private List<string> EditorWidgetNames = new List<string> { "TerrainSettingsWidget", "DevelWidget", "ErosionWidget", "ParameterPresetWidget", "TerrainGenerationWidget", "ParameterListWidget" };
    public List<NoiseParameters> AllParameters = new List<NoiseParameters>();
    public string[] AllParameterNames = new string[0];
    public TerrainGeneration Script;

    public bool EditorInitialised = false;

    private void OnActivate() {
        Script = (TerrainGeneration)target;
        if (Script.TerrainInfo != null) {
            TerrainInfo = Script.TerrainInfo;
            CurrentSelectedIndex = EditorPrefs.GetInt("ParameterPresetIdx");
            foreach (var widgetName in EditorWidgetNames) {
                EditorWidgetFoldouts.Add(widgetName, true);
            }
            TryGeneratingSavedParameterList();
            // This is to have the parameter list already loaded
            if (AllParameters.Count > 0) {
                TerrainInfo.TerrainParameterList = AllParameters[CurrentSelectedIndex].TerrainParameterList;
            }
        }
        EditorInitialised = true;
    }

    /// <summary>
    /// The main InspectorGUI code, every widget drawer gets called here
    /// </summary>
    public override void OnInspectorGUI() {
        if (target != null) {
            if (!EditorInitialised) {
                OnActivate();
            }
            if (Script.TerrainInfo != null) {
                DrawTerrainSettingsWidget();
                DrawDevelWidget();
                DrawErosionTypeProperties();
                DrawTerrainGenerationProperties();
                serializedObject.Update();
                EditorWidgetFoldouts["ParameterListWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["ParameterListWidget"], "ParameterListWidget");
                if (EditorWidgetFoldouts["ParameterListWidget"]) {
                    DisplayParameterList().DoLayoutList();
                }
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
        EditorGUILayout.LabelField("Run the project to initialise the controls!");
    }

    public void DrawTerrainSettingsWidget() {
        EditorWidgetFoldouts["TerrainSettingsWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["TerrainSettingsWidget"], "TerrainSettingsWidget");
        if (EditorWidgetFoldouts["TerrainSettingsWidget"]) {
            GUILayout.Label("ONLY EDIT THESE PARAMETERS WHEN THE GAME IS NOT RUNNING!");
            var width = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "TerrainWidth", TerrainInfo.TerrainWidth);
            TerrainInfo.TerrainWidth = width <= 0 ? 128 : width;
            var height = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "TerrainHeight", TerrainInfo.TerrainHeight);
            TerrainInfo.TerrainHeight = height <= 0 ? 128 : height;
            var depth = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "TerrainDepth", TerrainInfo.TerrainDepth);
            TerrainInfo.TerrainDepth = depth <= 0 ? 128 : depth;
        }
    }
    // Used for devel stuff
    public void DrawDevelWidget() {
        EditorWidgetFoldouts["DevelWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["DevelWidget"], "DevelWidget");
        if (EditorWidgetFoldouts["DevelWidget"]) {
            if (GUILayout.Button("Gen. H, M, T maps")) {
                if (TerrainInfo.GenerationType != GenerationType.kSingleRun) {
                    TerrainInfo.GenerationType = GenerationType.kSingleRun;
                }
                Script.GenerateTerrainOnDemand();
            }
        }
    }

    /// <summary>
    /// Draws the ErosionTypeWidget
    /// </summary>
    public void DrawErosionTypeProperties() {
        EditorWidgetFoldouts["ErosionWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["ErosionWidget"], "ErosionWidget");
        if (EditorWidgetFoldouts["ErosionWidget"]) {
            GUILayout.Label("Pick an erosion type, try different iter numbers !");
            TerrainInfo.ErosionType = (ErosionGeneration.ErosionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainInfo.ErosionType);
            var iters = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "Number of erosion iterations", TerrainInfo.ErosionIterations);
            TerrainInfo.ErosionIterations = iters <= 0 ? 1 : iters;
            TerrainInfo.RuntimeErosion = EditorGUI.Toggle(EditorGUILayout.GetControlRect(), "Toggle runtime erosion? (will lag)", TerrainInfo.RuntimeErosion);
            if (!TerrainInfo.RuntimeErosion) {
                if (GUILayout.Button("Apply erosion!")) {
                    Script.ApplyErosion();
                }
            }
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
                    TerrainInfo.TerrainTextureType = loadedNoiseParameterPreset.TerrainTextureType;
                    TerrainInfo.NoiseScale = loadedNoiseParameterPreset.NoiseScale;
                    TerrainInfo.BaseFrequency = loadedNoiseParameterPreset.BaseFrequency;
                    TerrainInfo.Persistance = loadedNoiseParameterPreset.Persistance;
                    TerrainInfo.Lacunarity = loadedNoiseParameterPreset.Lacunarity;
                    TerrainInfo.NumberOfOctaves = loadedNoiseParameterPreset.NumberOfOctaves;
                    TerrainInfo.GlobalNoiseAddition = loadedNoiseParameterPreset.GlobalNoiseAddition;
                    TerrainInfo.Seed = loadedNoiseParameterPreset.Seed;
                    TerrainInfo.UserOffset = loadedNoiseParameterPreset.UserOffset;
                    TerrainInfo.CustomFunction = loadedNoiseParameterPreset.CustomFunction;
                    TerrainInfo.CustomExponent = loadedNoiseParameterPreset.CustomExponent;
                    TerrainInfo.TerrainParameterList = loadedNoiseParameterPreset.TerrainParameterList;
                    TerrainInfo.ErosionType = loadedNoiseParameterPreset.ErosionType;
                    TerrainInfo.ErosionIterations = loadedNoiseParameterPreset.ErosionIterations;
                    TerrainInfo.RuntimeErosion = loadedNoiseParameterPreset.RuntimeErosion;
                    for (int i = 0; i < TerrainInfo.TerrainParameterList.Count; i++) {
                        var parameter = TerrainInfo.TerrainParameterList[i];
                        ValidateTexture(ref parameter);
                        TerrainInfo.TerrainParameterList[i] = parameter;
                    }
                    ReorderableParameterList = null;
                    Script.GenerateTerrainOnDemand();
                    AssignSplatMap.DoSplat(TerrainInfo.HeightMap, TerrainInfo.TemperatureMap, TerrainInfo.MoistureMap, TerrainInfo._Terrain,
                        TerrainInfo._Terrain.terrainData, TerrainInfo.TerrainParameterList,
                        TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight);
                }
            }
            // Draws the GUI widgets for saving presets
            EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "Noise parameter serializer, saves the current configuration.");
            NoisePresetName = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Noise preset name: ", NoisePresetName);
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Save preset")) {
                NoiseParameters currentNoiseParameters = new NoiseParameters(TerrainInfo.ErosionType, TerrainInfo.RuntimeErosion, TerrainInfo.ErosionIterations, NoisePresetName, TerrainInfo.TerrainParameterList, TerrainInfo.UserOffset, TerrainInfo.NoiseScale,
                    TerrainInfo.BaseFrequency, TerrainInfo.Persistance, TerrainInfo.Lacunarity, TerrainInfo.NumberOfOctaves, TerrainInfo.GlobalNoiseAddition, TerrainInfo.Seed,
                    TerrainInfo.CustomFunction, TerrainInfo.CustomExponent, TerrainInfo.TerrainTextureType);
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
            TerrainInfo.TerrainTextureType = (NoiseParameters.TextureType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainInfo.TerrainTextureType);
            GUILayout.EndHorizontal();
            var noiseScale = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Noise Scale", TerrainInfo.NoiseScale);
            TerrainInfo.NoiseScale = noiseScale <= 0 ? 0.0001f : noiseScale;
            var baseFrequency = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Base Frequecny", TerrainInfo.BaseFrequency);
            TerrainInfo.BaseFrequency = baseFrequency <= 0 ? 0.0001f : baseFrequency;
            var persistance = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Persistance", TerrainInfo.Persistance);
            TerrainInfo.Persistance = persistance <= 0 ? 0.0001f : persistance;
            var lacunarity = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Lacunarity", TerrainInfo.Lacunarity);
            TerrainInfo.Lacunarity = lacunarity <= 0 ? 0.0001f : lacunarity;
            var octaves = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "Number of octaves", TerrainInfo.NumberOfOctaves);
            TerrainInfo.NumberOfOctaves = octaves <= 0 ? 1 : octaves;
            TerrainInfo.Seed = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Seed", TerrainInfo.Seed);
            TerrainInfo.UserOffset = EditorGUI.Vector2Field(EditorGUILayout.GetControlRect(), "User Offset", TerrainInfo.UserOffset);
            TerrainInfo.GlobalNoiseAddition = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Global noise add", TerrainInfo.GlobalNoiseAddition);
            TerrainInfo.CustomFunction = (NoiseGeneration.CustomFunctionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainInfo.CustomFunction);
            if (TerrainInfo.CustomFunction == NoiseGeneration.CustomFunctionType.kCustom) {
                var customExponent = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Custom Exponent", TerrainInfo.CustomExponent);
                TerrainInfo.CustomExponent = customExponent <= 0 ? 0.0001f : customExponent;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pick a generation type");
            TerrainInfo.GenerationType = (GenerationType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainInfo.GenerationType);
            GUILayout.EndHorizontal();
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Paint the terrain!")) {
                if (TerrainInfo.GenerationType != GenerationType.kSingleRun) {
                    Debug.LogWarningFormat("WARNING: Parameter _GenerationType is {0}, needs to be kSingleRun (can't paint terrain in runtime), changing to single run.", TerrainInfo.GenerationType.ToString());
                    TerrainInfo.GenerationType = GenerationType.kSingleRun;
                }
                AssignSplatMap.DoSplat(TerrainInfo.HeightMap, TerrainInfo.TemperatureMap, TerrainInfo.MoistureMap, TerrainInfo._Terrain, TerrainInfo._Terrain.terrainData, TerrainInfo.TerrainParameterList, TerrainInfo.TerrainWidth, TerrainInfo.TerrainHeight);
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
        ReorderableParameterList = new ReorderableList(TerrainInfo.TerrainParameterList, typeof(TerrainParameters), true, true, true, true);
        ReorderableParameterList.elementHeight = 22.0f * 7;
        ReorderableParameterList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var currentParameter = TerrainInfo.TerrainParameterList[index];
            rect.height = 20.0f;
            currentParameter.Name = EditorGUI.TextField(rect, "Name", currentParameter.Name);
            rect.y += 22.0f;
            rect.height = 20.0f;
            currentParameter.MoistureParameterBoundry = EditorGUI.FloatField(rect, "Moisture Boundry", currentParameter.MoistureParameterBoundry);
            rect.y += 22.0f;
            rect.height = 20.0f;
            currentParameter.TemperatureParameterBoundry = EditorGUI.FloatField(rect, "Temp Boundry", currentParameter.TemperatureParameterBoundry);
            rect.height = 22.0f;
            rect.y += 22.0f;
            currentParameter.ParameterBoundry = EditorGUI.FloatField(rect, "HM Boundry", currentParameter.ParameterBoundry);
            rect.height = 22.0f;
            rect.y += 22.0f;
            currentParameter.TerrainColor = EditorGUI.ColorField(rect, "Color", currentParameter.TerrainColor);
            rect.height = 22.0f;
            rect.y += 22.0f;
            EditorGUI.LabelField(rect, currentParameter.TexturePath);
            rect.height = 22.0f;
            rect.y += 22.0f;
            ValidateTexture(ref currentParameter);
            var newTerrainTexture = (Texture2D)EditorGUI.ObjectField(rect, "Texture", currentParameter.TerrainTexture, typeof(Texture2D));
            if (newTerrainTexture != currentParameter.TerrainTexture) {
                currentParameter.TerrainTexture = newTerrainTexture;
                currentParameter.TexturePath = AssetDatabase.GetAssetPath(currentParameter.TerrainTexture);
            }
            TerrainInfo.TerrainParameterList[index] = currentParameter;
            EditorGUILayout.Separator();
        };
        ReorderableParameterList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, string.Format("Parameter list"));
        };
        ReorderableParameterList.onSelectCallback = (ReorderableList list) => {
        };
        ReorderableParameterList.onRemoveCallback = (ReorderableList list) => {
            TerrainInfo.TerrainParameterList.RemoveAt(list.index);
        };
        return ReorderableParameterList;
    }

    public static void ValidateTexture(ref TerrainParameters currentParameter) {
        // Edge case if textures exist in runtime
        if (currentParameter.TexturePath == null && currentParameter.TerrainTexture != null) {
            currentParameter.TexturePath = AssetDatabase.GetAssetPath(currentParameter.TerrainTexture);
        }
        if (currentParameter.TerrainTexture == null && currentParameter.TexturePath != null) {
            currentParameter.TerrainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(currentParameter.TexturePath, typeof(Texture2D));
        }
    }

    private void TryGeneratingSavedParameterList() {
        SerializationManager.InitializeManager();
        AllParameters = SerializationManager.ReadAllNoiseParameters();
        AllParameterNames = new string[AllParameters.Count];
        for (int i = 0; i < AllParameters.Count; i++) {
            AllParameterNames[i] = AllParameters[i].NoiseParameterName;
        }
    }

    private void OnDisable() {
        EditorPrefs.SetInt("ParameterPresetIdx", CurrentSelectedIndex);
    }

    // Helper to draw a line in edtior, cuz Unity...
    void GUILine(int i_height = 2) {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}
