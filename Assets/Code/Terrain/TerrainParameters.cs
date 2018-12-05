using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public struct TerrainParameters {
    [JsonIgnore]
    private Color _TerrainColor;
    // Newtonsoft and Unity colors dont mix well
    //serializing color results in a stack overflow due to color.linear having different values every time you go deeper
    [JsonIgnore]
    public Color TerrainColor {
        get { return _TerrainColor; }
        set { _TerrainColor = value; TerrainColorVector = value; }
    }

    public string Name;
    public float ParameterBoundry;
    public Vector4 TerrainColorVector;

    public TerrainParameters(string name, float parameterBoundry, Color c) {
        Name = name;
        ParameterBoundry = parameterBoundry;
        _TerrainColor = TerrainColorVector = c;
    }
}
