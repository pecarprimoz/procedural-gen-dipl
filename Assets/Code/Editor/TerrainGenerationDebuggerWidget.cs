using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
[CustomEditor(typeof(TerrainGenerationDebugger), true)]
public class TerrainGenerationDebuggerWidget : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        TerrainGenerationDebugger tgb = (TerrainGenerationDebugger)target;
        if (!tgb.AlwaysUpdate) {
            if (GUILayout.Button("Generate texture")) {
                tgb.GenerateTexture();
            }
        }
    }
}
