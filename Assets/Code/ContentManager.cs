using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentManager : MonoBehaviour {
    // Here we create our skeleton for placing objects for easier usage later on
    private void Start() {
        var balls = new GameObject("Balls");
        balls.transform.SetParent(ParentObjectForInstantiatedObjects.transform);
        PlaceableDict.Add(PlaceableObjectType.kBall, balls);
    }

    public enum PlaceableObjectType {
        kBall = 0
    }

    public Dictionary<PlaceableObjectType, GameObject> PlaceableDict = new Dictionary<PlaceableObjectType, GameObject>();

    // Just keep all the data here
    public GameObject ParentObjectForInstantiatedObjects;

    // DEBUG PREFAB FOR PLACEMENTS
    public GameObject PlaceableObject;


}
