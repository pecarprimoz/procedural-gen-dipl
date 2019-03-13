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
    // this fucked should be serializable allready, we'll see :D
    List<TerrainParameters> SerializedTerrainParameters;

    private ReorderableList ReorderableParameterList;
    public string NoisePresetName = string.Empty;
    public string[] AllParameterNames = new string[0];
    public int CurrentSelectedIndex = 0;
    private int DeleteFailsafe = 0;
    List<NoiseParameters> AllParameters = new List<NoiseParameters>();


    // first issue here, noise parameters are coupled with terrain parameters (biomes), need to seperate this THIS COULD BE FINE IN RUNTIME, ALSO IN EDITOR BUT USELESS SINCE WE DO
    // NOT WANT TO EXPOSE THESE VALUES TO THE USER WHILE THE GAME IS NOT RUNNING (just causes complications)
    // turn this into trow different draw load save guis, since this one does not need to deserialize terrain info, only terrainpresets (biomes)
    public void DrawLoadSaveGUI(Dictionary<string, bool> EditorWidgetFoldouts) {
        // Draws the GUI widgets for picking and deleting parameters
        //EditorWidgetFoldouts["ParameterPresetWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["ParameterPresetWidget"], "ParameterPresetWidget");
        //if (EditorWidgetFoldouts["ParameterPresetWidget"]) {
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
                for (int i = 0; i < SerializedTerrainParameters.Count; i++) {
                    var parameter = SerializedTerrainParameters[i];
                    EditorUtils.ValidateTexture(ref parameter);
                    SerializedTerrainParameters[i] = parameter;
                }
                ReorderableParameterList = null;
                //    }
                //}
                //// Draws the GUI widgets for saving presets, MAKE THIS SHIT ONLY AVALIABLE IN RUN TIME, IF YOU WANNA SAVE UR TERRAIN PRESETS, DO IT SEPERATLY !!! dont do shit for now since i dont plan on saving in editor when testing !!!
                EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "Noise parameter serializer, saves the current configuration.");
                NoisePresetName = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Noise preset name: ", NoisePresetName);
                if (GUI.Button(EditorGUILayout.GetControlRect(), "Save preset")) {
                    //    NoiseParameters currentNoiseParameters = new NoiseParameters(info.ErosionType, info.RuntimeErosion, info.ErosionIterations, NoisePresetName, info.TerrainParameterList, info.UserOffset, info.NoiseScale,
                    //        info.BaseFrequency, info.Persistance, info.Lacunarity, info.NumberOfOctaves, info.GlobalNoiseAddition, info.Seed,
                    //        info.CustomFunction, info.CustomExponent, info.TerrainTextureType);
                    //    SerializationManager.SaveNoiseParameters(NoisePresetName, currentNoiseParameters);
                    //    TryGeneratingSavedParameterList();
                }
            }
            EditorUtils.GUILine();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            // lol
            EditorPrefs.SetInt("ParameterPresetIdx", CurrentSelectedIndex);

        }
    }

    public ReorderableList DisplayParameterList() {
        if (ReorderableParameterList != null) {
            return ReorderableParameterList;
        }
        ReorderableParameterList = new ReorderableList(SerializedTerrainParameters, typeof(TerrainParameters), true, true, true, true);
        ReorderableParameterList.elementHeight = 22.0f * 7;
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
            SerializedTerrainParameters[index] = currentParameter;
            EditorGUILayout.Separator();
        };
        ReorderableParameterList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, string.Format("Parameter list"));
        };
        ReorderableParameterList.onSelectCallback = (ReorderableList list) => {
        };
        ReorderableParameterList.onRemoveCallback = (ReorderableList list) => {
            SerializedTerrainParameters.RemoveAt(list.index);
        };
        return ReorderableParameterList;
    }

    // we can do this shit in editor !
    public void TryGeneratingSavedParameterList() {
        SerializationManager.InitializeManager();
        AllParameters = SerializationManager.ReadAllNoiseParameters();
        AllParameterNames = new string[AllParameters.Count];
        for (int i = 0; i < AllParameters.Count; i++) {
            AllParameterNames[i] = AllParameters[i].NoiseParameterName;
        }
        SerializedTerrainParameters = AllParameters[CurrentSelectedIndex].TerrainParameterList;
    }
}