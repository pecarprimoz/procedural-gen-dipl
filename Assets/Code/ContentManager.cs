using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentManager : MonoBehaviour {
    public void InitializeBiomePlacementObjects(TerrainInfo info) {
        // need to make a choice here, keep object when user generates and dont clear, or clear always ?
        BiomeParentGameObjects.Clear();
        var paramList = info.TerrainParameterList;
        // here we generate the parent object for every type of biome
        for (int i = 0; i < paramList.Count; i++) {
            var obj = new GameObject(string.Format("{0} - {1}", paramList[i].Name, i));
            obj.transform.SetParent(ParentObjectForInstantiatedObjects.transform);
            BiomeParentGameObjects.Add(i, obj);
        }
    }

    public Dictionary<int, GameObject> BiomeParentGameObjects = new Dictionary<int, GameObject>();
    // Just keep all the data here
    public GameObject ParentObjectForInstantiatedObjects;
}
