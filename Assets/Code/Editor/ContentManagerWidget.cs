using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
// this will need to be tied to TerrainParameters
// need to expose terrain parameter list to editor, then disable changing of the current terrain parameter list in runtime
// when this will be implemented, user will define biomes in editor, then will sync with runtime
// same will be done for biome objects, since we have the biomes in editor, the user will be able to drag prefabs directly
// baisicly TerrainParameterList -> holds all biome data, as well as which object can be placed
// every biome will have a list of gameobjects, which will be set in editor, then fetched in runtime for placement
// maybe TerrainParameres could hold that list, issue with serialization (path to object, then use Resources.Load<GameObject())
[InitializeOnLoad]
[CustomEditor(typeof(ContentManager), true)]
public class ContentManagerWidget : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

    }


}
