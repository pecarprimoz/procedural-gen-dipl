using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainParameters {
    [JsonIgnore]
    public Color _TerrainColor;
    // Newtonsoft and Unity colors dont mix well
    //serializing color results in a stack overflow due to color.linear having different values every time you go deeper
    [JsonIgnore]
    public Color TerrainColor {
        get { return _TerrainColor; }
        set { _TerrainColor = value; TerrainColorVector = value; }
    }
    [JsonIgnore]
    public Texture2D TerrainTexture;
    [JsonIgnore]
    public List<GameObject> TerrainParameterObjectList;
    public List<int> TerrainParameterObjectCount;

    public List<string> ObjectListPath;

    public int ObjectListCount;

    public string Name;
    public float ParameterBoundry;
    public float TemperatureParameterBoundry;
    public float MoistureParameterBoundry;
    public Vector4 TerrainColorVector;
    public string TexturePath;
    public TerrainParameters(string name, float moistureParameterBoundry, float temperatureParameterBoundry, float parameterBoundry, Color c, string texturePath) {
        Name = name;
        MoistureParameterBoundry = moistureParameterBoundry;
        TemperatureParameterBoundry = temperatureParameterBoundry;
        ParameterBoundry = parameterBoundry;
        _TerrainColor = TerrainColorVector = c;
        TexturePath = texturePath;
        TerrainTexture = Resources.Load(texturePath) as Texture2D;
        TerrainParameterObjectList = new List<GameObject>();
        TerrainParameterObjectCount = new List<int>();
        ObjectListPath = new List<string>();
        ObjectListCount = 0;
    }
    public TerrainParameters() {
        Name = string.Empty;
        ParameterBoundry = 0.0f;
        TemperatureParameterBoundry = 0.0f;
        MoistureParameterBoundry = 0.0f;
        _TerrainColor = TerrainColorVector = Color.white;
        TexturePath = string.Empty;
        TerrainTexture = null;
        TerrainParameterObjectList = new List<GameObject>();
        TerrainParameterObjectCount = new List<int>();

        ObjectListPath = new List<string>();
        ObjectListCount = 0;
    }
}
