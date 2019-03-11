using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[InitializeOnLoad]
[CustomEditor(typeof(TerrainGenerationDebugger), true)]
public class TerrainGenerationDebuggerWidget : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        TerrainGenerationDebugger tgb = (TerrainGenerationDebugger)target;
        if (GUILayout.Button("Generate texture")) {
            tgb.GenerateTexture();
        }
    }
}
