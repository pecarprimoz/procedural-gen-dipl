using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class TerrainParameterPresetEditor {
    public TerrainParameterPresetEditor() {
        SerializedTerrainParameters = new List<TerrainParameters>();
        CurrentSelectedIndex = EditorPrefs.GetInt("ParameterPresetIdx");
        TryGeneratingSavedParameterList();
    }
    public List<TerrainParameters> SerializedTerrainParameters;

    public bool FoldoutParameterPresets = false;
    private ReorderableList ReorderableParameterList;
    public string TerrainPresetName = string.Empty;
    public string[] AllParameterNames = new string[0];
    public int CurrentSelectedIndex = 0;
    private int DeleteFailsafe = 0;
    List<List<TerrainParameters>> AllParameters = new List<List<TerrainParameters>>();

    public void DrawLoadSaveGUI(TerrainInfo info, Dictionary<string, bool> EditorWidgetFoldouts) {
        // Draws the GUI widgets for picking and deleting parameters
        FoldoutParameterPresets = EditorGUILayout.Foldout(FoldoutParameterPresets, "Parameter Presets");
        if (FoldoutParameterPresets) {
            if (AllParameterNames.Length > 0) {
                GUILayout.Label("Saved parameter presets");
                GUILayout.BeginHorizontal(GUILayout.Width(250));
                var lastIndex = CurrentSelectedIndex;
                CurrentSelectedIndex = EditorGUILayout.Popup(CurrentSelectedIndex, AllParameterNames, GUILayout.Width(250));
                DeleteFailsafe = lastIndex != CurrentSelectedIndex ? 0 : DeleteFailsafe;
                if (GUILayout.Button(string.Format("Delete selected preset ({0})", DeleteFailsafe), GUILayout.MaxWidth(200))) {
                    if (DeleteFailsafe == 2) {
                        DeleteFailsafe = 0;
                        SerializationManager.DeleteTerrainParameter(AllParameterNames[CurrentSelectedIndex]);
                        AllParameters.RemoveAt(CurrentSelectedIndex);
                        TryGeneratingSavedParameterList();
                    } else {
                        DeleteFailsafe++;
                    }
                }
                GUILayout.EndHorizontal();
                if (GUI.Button(EditorGUILayout.GetControlRect(), "Load preset")) {
                    TryGeneratingSavedParameterList();
                    for (int i = 0; i < SerializedTerrainParameters.Count; i++) {
                        var parameter = SerializedTerrainParameters[i];
                        parameter.TerrainColor = new Color(parameter.TerrainColorVector.x, parameter.TerrainColorVector.y, parameter.TerrainColorVector.z, 1);
                        EditorUtils.ValidateTexture(ref parameter);
                        SerializedTerrainParameters[i] = parameter;
                    }
                    ReorderableParameterList = null;
                }
                EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "Terrain parameter serializer, saves the current terrain preset configuration.");
                TerrainPresetName = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Terrain preset name: ", TerrainPresetName);
                if (GUI.Button(EditorGUILayout.GetControlRect(), "Save preset")) {
                    SerializationManager.SaveTerrainPreset(TerrainPresetName, SerializedTerrainParameters);
                    TryGeneratingSavedParameterList();
                }
                EditorPrefs.SetInt("ParameterPresetIdx", CurrentSelectedIndex);
                if (info != null) {
                    info.TerrainParameterList = SerializedTerrainParameters;
                }
            }
        }
    }

    public ReorderableList DisplayParameterList() {
        if (ReorderableParameterList != null) {
            return ReorderableParameterList;
        }
        ReorderableParameterList = new ReorderableList(SerializedTerrainParameters, typeof(TerrainParameters), true, true, true, true);
        ReorderableParameterList.elementHeight = 22.0f * 18;
        ReorderableParameterList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var currentParameter = SerializedTerrainParameters[index];
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
            EditorUtils.ValidateTexture(ref currentParameter);
            var newTerrainTexture = (Texture2D)EditorGUI.ObjectField(rect, "Texture", currentParameter.TerrainTexture, typeof(Texture2D));
            if (newTerrainTexture != currentParameter.TerrainTexture) {
                currentParameter.TerrainTexture = newTerrainTexture;
                currentParameter.TexturePath = AssetDatabase.GetAssetPath(currentParameter.TerrainTexture);
            }
            ReorderableListDrawPresetObjecList(rect, currentParameter);
            SerializedTerrainParameters[index] = currentParameter;
            EditorGUILayout.Separator();
        };
        ReorderableParameterList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, string.Format("Parameter list"));
        };
        ReorderableParameterList.onSelectCallback = (ReorderableList list) => {
        };
        ReorderableParameterList.onAddCallback = (ReorderableList list) => {
            ReorderableParameterList.list.Add(new TerrainParameters());
        };
        ReorderableParameterList.onRemoveCallback = (ReorderableList list) => {
            SerializedTerrainParameters.RemoveAt(list.index);
        };
        return ReorderableParameterList;
    }

    private void ReorderableListDrawPresetObjecList(Rect rect, TerrainParameters tparam) {
        rect.height = 22.0f;
        rect.y += 22.0f;
        EditorGUI.LabelField(rect, "Terrain game objects");
        rect.height = 22.0f;
        rect.y += 22.0f;
        var tmpWidth = rect.width;
        rect.width = tmpWidth / 2;
        if (GUI.Button(rect, "+")) {
            tparam.ObjectListCount++;
            tparam.TerrainParameterObjectList.Add(null);
            tparam.ObjectListPath.Add(string.Empty);
        }
        rect.x += rect.width;
        if (GUI.Button(rect, "-")) {
            if (tparam.ObjectListCount - 1 >= 0) {
                tparam.ObjectListCount--;
                tparam.TerrainParameterObjectList.RemoveAt(tparam.ObjectListCount);
                tparam.ObjectListPath.RemoveAt(tparam.ObjectListCount);
            }
        }
        rect.x -= rect.width;
        rect.width = tmpWidth;
        rect.height = 22.0f;
        rect.y += 22.0f;
        for (int i = 0; i < tparam.ObjectListCount; i++) {
            var newTerrainObject = (GameObject)EditorGUI.ObjectField(rect, tparam.TerrainParameterObjectList[i], typeof(GameObject), false);
            if (newTerrainObject != tparam.TerrainParameterObjectList[i]) {
                tparam.TerrainParameterObjectList[i] = newTerrainObject;
                tparam.ObjectListPath[i] = AssetDatabase.GetAssetPath(newTerrainObject);
            }
            rect.height = 22.0f;
            rect.y += 22.0f;
        }
    }

    // we can do this shit in editor !
    public void TryGeneratingSavedParameterList() {
        List<string> allPresetNames;
        SerializationManager.InitializeManager();
        AllParameters = SerializationManager.ReadAllTerrainParameters(out allPresetNames);
        AllParameterNames = new string[AllParameters.Count];
        for (int i = 0; i < AllParameters.Count; i++) {
            AllParameterNames[i] = allPresetNames[i];
        }
        SerializedTerrainParameters = AllParameters[CurrentSelectedIndex];
    }
}