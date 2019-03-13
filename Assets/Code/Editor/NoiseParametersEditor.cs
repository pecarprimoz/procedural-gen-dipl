using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
// this class should be okay, nothing needs to be set in editor mode, all runtime variables
public class NoiseParameterEditor {
    public void DrawNoiseParameterGUI(TerrainInfo info, Dictionary<string, bool> EditorWidgetFoldouts) {
        EditorWidgetFoldouts["TerrainGenerationWidget"] = EditorGUILayout.Foldout(EditorWidgetFoldouts["TerrainGenerationWidget"], "TerrainGenerationWidget");
        if (EditorWidgetFoldouts["TerrainGenerationWidget"]) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("DebugPlane coloring");
            info.TerrainTextureType = (TextureType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), info.TerrainTextureType);
            GUILayout.EndHorizontal();
            var noiseScale = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Noise Scale", info.NoiseScale);
            info.NoiseScale = noiseScale <= 0 ? 0.0001f : noiseScale;
            var baseFrequency = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Base Frequecny", info.BaseFrequency);
            info.BaseFrequency = baseFrequency <= 0 ? 0.0001f : baseFrequency;
            var persistance = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Persistance", info.Persistance);
            info.Persistance = persistance <= 0 ? 0.0001f : persistance;
            var lacunarity = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Lacunarity", info.Lacunarity);
            info.Lacunarity = lacunarity <= 0 ? 0.0001f : lacunarity;
            var octaves = EditorGUI.IntField(EditorGUILayout.GetControlRect(), "Number of octaves", info.NumberOfOctaves);
            info.NumberOfOctaves = octaves <= 0 ? 1 : octaves;
            info.Seed = EditorGUI.TextField(EditorGUILayout.GetControlRect(), "Seed", info.Seed);
            info.UserOffset = EditorGUI.Vector2Field(EditorGUILayout.GetControlRect(), "User Offset", info.UserOffset);
            info.GlobalNoiseAddition = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Global noise add", info.GlobalNoiseAddition);
            info.CustomFunction = (CustomFunctionType)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), info.CustomFunction);
            if (info.CustomFunction == CustomFunctionType.kCustom) {
                var customExponent = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Custom Exponent", info.CustomExponent);
                info.CustomExponent = customExponent <= 0 ? 0.0001f : customExponent;
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
        EditorUtils.GUILine();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}