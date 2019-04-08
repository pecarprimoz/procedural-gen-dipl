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
    public Texture2D TerrainTextureSpring;
    [JsonIgnore]
    public Texture2D TerrainTextureSummer;
    [JsonIgnore]
    public Texture2D TerrainTextureAutumn;
    [JsonIgnore]
    public Texture2D TerrainTextureWinter;

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

    public string TexturePathSpring;
    public string TexturePathSummer;
    public string TexturePathAutumn;
    public string TexturePathWinter;



    public TerrainParameters(string name, float moistureParameterBoundry, float temperatureParameterBoundry, float parameterBoundry, Color c, string texturePathSpring,
        string texturePathSummer, string texturePathAutumn, string texturePathWinter) {
        Name = name;
        MoistureParameterBoundry = moistureParameterBoundry;
        TemperatureParameterBoundry = temperatureParameterBoundry;
        ParameterBoundry = parameterBoundry;
        _TerrainColor = TerrainColorVector = c;
        TexturePathSpring = texturePathSpring;
        TexturePathSummer = texturePathSummer;
        TexturePathAutumn = texturePathAutumn;
        TexturePathWinter = texturePathWinter;
        TerrainTextureSpring = Resources.Load(texturePathSpring) as Texture2D;
        TerrainTextureSummer = Resources.Load(texturePathSummer) as Texture2D;
        TerrainTextureAutumn = Resources.Load(texturePathAutumn) as Texture2D;
        TerrainTextureWinter = Resources.Load(texturePathWinter) as Texture2D;

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
        TexturePathSpring = TexturePathSummer = TexturePathAutumn = TexturePathWinter = string.Empty;
        TerrainTextureSpring = TerrainTextureSummer = TerrainTextureAutumn = TerrainTextureWinter = null;
        TerrainParameterObjectList = new List<GameObject>();
        TerrainParameterObjectCount = new List<int>();

        ObjectListPath = new List<string>();
        ObjectListCount = 0;
    }
}
