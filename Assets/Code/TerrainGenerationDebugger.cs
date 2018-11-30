using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerationDebugger : MonoBehaviour {
    public TerrainGeneration tg;
    private void Start() {
    }
    private void Update() {

        Texture2D texture = new Texture2D(128, 128);
        GetComponent<Renderer>().material.mainTexture = texture;
        float[,] terrainMap = tg.currentTerrain;
        if (terrainMap == null) {
            return;
        }
        for (int y = 0; y < texture.height; y++) {
            for (int x = 0; x < texture.width; x++) {
                Color c = new Color(1 * terrainMap[x, y], 1 * terrainMap[x, y], 1 * terrainMap[x, y], 1);
                texture.SetPixel(x, y, c);
            }
        }
        texture.Apply();
    }
}
