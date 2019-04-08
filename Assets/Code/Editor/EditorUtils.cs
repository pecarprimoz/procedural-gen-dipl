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
        if (currentParameter.TexturePathSpring == null && currentParameter.TerrainTextureSpring != null) {
            currentParameter.TexturePathSpring = AssetDatabase.GetAssetPath(currentParameter.TerrainTextureSpring);
        }
        if (currentParameter.TexturePathSummer == null && currentParameter.TerrainTextureSummer != null) {
            currentParameter.TexturePathSummer = AssetDatabase.GetAssetPath(currentParameter.TerrainTextureSummer);
        }
        if (currentParameter.TexturePathAutumn == null && currentParameter.TerrainTextureAutumn != null) {
            currentParameter.TexturePathAutumn = AssetDatabase.GetAssetPath(currentParameter.TerrainTextureAutumn);
        }
        if (currentParameter.TexturePathWinter == null && currentParameter.TerrainTextureWinter != null) {
            currentParameter.TexturePathWinter = AssetDatabase.GetAssetPath(currentParameter.TerrainTextureWinter);
        }
        // if we dont have the texture loaded but we have the path
        if (currentParameter.TerrainTextureSpring == null && currentParameter.TexturePathSpring != null) {
            currentParameter.TerrainTextureSpring = (Texture2D)AssetDatabase.LoadAssetAtPath(currentParameter.TexturePathSpring, typeof(Texture2D));
        }
        if (currentParameter.TerrainTextureSummer == null && currentParameter.TexturePathSummer != null) {
            currentParameter.TerrainTextureSummer = (Texture2D)AssetDatabase.LoadAssetAtPath(currentParameter.TexturePathSummer, typeof(Texture2D));
        }
        if (currentParameter.TerrainTextureAutumn == null && currentParameter.TexturePathAutumn != null) {
            currentParameter.TerrainTextureAutumn = (Texture2D)AssetDatabase.LoadAssetAtPath(currentParameter.TexturePathAutumn, typeof(Texture2D));
        }
        if (currentParameter.TerrainTextureWinter == null && currentParameter.TexturePathWinter != null) {
            currentParameter.TerrainTextureWinter = (Texture2D)AssetDatabase.LoadAssetAtPath(currentParameter.TexturePathWinter, typeof(Texture2D));
        }
    }

    public static void AddTerrainParametersFromEditorToTerrainInfoInRuntime(TerrainInfo info, List<TerrainParameters> terrainParameters) {
        info.TerrainParameterList = terrainParameters;
    }
}