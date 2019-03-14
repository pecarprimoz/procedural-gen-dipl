using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorUtils {
    // Helper to draw a line in edtior, cuz Unity...
    public static void GUILine(int i_height = 2) {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
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

    public static void AddTerrainParametersFromEditorToTerrainInfoInRuntime(TerrainInfo info, List<TerrainParameters> terrainParameters) {
        info.TerrainParameterList = terrainParameters;
    }
}