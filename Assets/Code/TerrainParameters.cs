using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public struct TerrainParameters {
    [JsonIgnore]
    private Color _TerrainColor;
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
