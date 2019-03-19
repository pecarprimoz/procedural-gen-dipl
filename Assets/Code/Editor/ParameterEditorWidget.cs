using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
[InitializeOnLoad]
[CustomEditor(typeof(TerrainGeneration), true)]
public class ParameterEditorWidget : Editor {
    [SerializeField]
    private TerrainParameterPresetEditor _ParameterPresetEditor;
    NoiseParameterEditor _NoiseParameterEditor;
    private TerrainInfo TerrainInfo;
    private Dictionary<string, bool> EditorWidgetFoldouts = new Dictionary<string, bool>() {
        { "TerrainSettingsWidget", false },
        { "DevelWidget", false },
        { "ErosionWidget", false },
        { "ParameterPresetWidget", false },
        { "TerrainGenerationWidget", false },
        { "ParameterListWidget", false }
    };
    public List<NoiseParameters> AllParameters = new List<NoiseParameters>();
    public TerrainGeneration Script;
    public bool WasInitialised = false;
    public bool EditorInitialized = false;

    private void OnActivate() {
        // Parameter preset editor works in editor
        if (_ParameterPresetEditor == null) {
            _ParameterPresetEditor = new TerrainParameterPresetEditor();
        }
        Script = (TerrainGeneration)target;
        // Noise parameter editor works in runtime
        if (Script.TerrainInfo != null) {
            // We are in runtime, means we can get all the info from runtime
            if (TerrainInfo == null) {
                TerrainInfo = Script.TerrainInfo;
                _NoiseParameterEditor = new NoiseParameterEditor(TerrainInfo);
                // now just overrwrite this terrain infos parameter property list (biomes) with our serialized editor list
                TerrainInfo.TerrainParameterList = _ParameterPresetEditor.SerializedTerrainParameters;
            }
        }
        WasInitialised = true;
    }

    /// <summary>
    /// The main InspectorGUI code, every widget drawer gets called here
    /// </summary>
    public override void OnInspectorGUI() {
        if (!WasInitialised) {
            OnActivate();
        }
        if (target != null) {
            if (!EditorInitialized) {
                OnActivate();
            }
            if (Script.TerrainInfo != null) {
                DrawDevelWidget();
                DrawErosionTypeProperties();
                _NoiseParameterEditor.DrawNoiseParameterGUI(TerrainInfo, EditorWidgetFoldouts);
            }
        }
        _ParameterPresetEditor.DrawLoadSaveGUI(TerrainInfo, EditorWidgetFoldouts);
        _ParameterPresetEditor.DisplayParameterList().DoLayoutList();

        EditorGUILayout.LabelField("Run the project to initialise the controls!");
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
        EditorGUILayout.Space();
        EditorGUILayout.Space();
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
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}
