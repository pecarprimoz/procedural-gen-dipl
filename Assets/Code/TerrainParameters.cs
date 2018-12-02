using UnityEngine;

[System.Serializable]
public struct TerrainParameters {
    public string Name;
    public float ParameterBoundry;
    public Color TerrainColor;

    public TerrainParameters(string name, float parameterBoundry, Color c) {
        Name = name;
        ParameterBoundry = parameterBoundry;
        TerrainColor = c;
    }
}
