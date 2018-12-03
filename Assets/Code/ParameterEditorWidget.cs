using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(TerrainGeneration), true)]
public class ParameterEditorWidget : Editor {
    private ReorderableList ReorderableParameterList;
    private TerrainGeneration TerrainGenerationScript;
    private void OnEnable() {
        TerrainGenerationScript = (TerrainGeneration)target;
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
        TerrainGenerationScript.TerrainTextureType = (TerrainGeneration.TextureType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript.TerrainTextureType);
        TerrainGenerationScript.NoiseScale = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Noise Scale", TerrainGenerationScript.NoiseScale);
        TerrainGenerationScript.BaseFrequency = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Base Frequecny", TerrainGenerationScript.BaseFrequency);
        TerrainGenerationScript.Persistance = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Persistance", TerrainGenerationScript.Persistance);
        TerrainGenerationScript.Lacunarity = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Lacunarity", TerrainGenerationScript.Lacunarity);
        TerrainGenerationScript.Seed = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Seed", TerrainGenerationScript.Seed);
        TerrainGenerationScript.UserOffset = EditorGUI.Vector2Field(EditorGUILayout.GetControlRect(), "User Offset", TerrainGenerationScript.UserOffset);
        TerrainGenerationScript.CustomFunction = (NoiseGeneration.CustomFunctionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), TerrainGenerationScript.CustomFunction);
        EditorGUI.LabelField(EditorGUILayout.GetControlRect(), "PARAMETER BOUNDRIES NEED TO BE IN ASCENDING ORDER!");
        // @TODO, add a button that saves the current preset of all parameters (serialize or just put as default values in TerrainGeneration)
    }
}
