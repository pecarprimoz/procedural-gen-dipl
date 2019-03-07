using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentManager : MonoBehaviour {
    // Here we create our skeleton for placing objects for easier usage later on
    private void Start() {
        var balls = new GameObject("Balls");
        balls.transform.SetParent(ParentObjectForInstantiatedObjects.transform);
        PlaceableDict.Add(PlaceableObjectType.kBall, balls);

        var cubes = new GameObject("Cubes");
        cubes.transform.SetParent(ParentObjectForInstantiatedObjects.transform);
        PlaceableDict.Add(PlaceableObjectType.kCube, cubes);
    }
    
    public Dictionary<PlaceableObjectType, GameObject> PlaceableDict = new Dictionary<PlaceableObjectType, GameObject>();

    // Just keep all the data here
    public GameObject ParentObjectForInstantiatedObjects;

    // DEBUG PREFAB FOR PLACEMENTS
    public GameObject PlaceableObject;
    // until i have proper models
    public GameObject PlaceableObjectCube;



}
