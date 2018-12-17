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
    [JsonIgnore]
    public Texture2D TerrainTexture;

    public string Name;
    public float ParameterBoundry;
    public Vector4 TerrainColorVector;
    public string TexturePath; 
    public TerrainParameters(string name, float parameterBoundry, Color c, string texturePath) {
        Name = name;
        ParameterBoundry = parameterBoundry;
        _TerrainColor = TerrainColorVector = c;
        TexturePath = texturePath;
        TerrainTexture = Resources.Load(texturePath) as Texture2D;
    }
}
