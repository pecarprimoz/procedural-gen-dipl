using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerationDebugger : MonoBehaviour {
    public TerrainGeneration tg;
    private static Texture2D texture;
    private void Update() {
        // Fun fact, Unity does not GC new Terrains, which results in a memory leak, to prevent this we call destroy and have the texture as static
        Destroy(texture);
        // The debug terrain is driven by the TerrainGeneration (the main terrain that we are working on)
        texture = new Texture2D(tg.TerrainWidth, tg.TerrainHeight);
        GetComponent<Renderer>().material.mainTexture = texture;
        float[,] terrainMap = tg.CurrentTerrain;
        if (terrainMap == null) {
            return;
        }
        for (int y = 0; y < texture.height; y++) {
            for (int x = 0; x < texture.width; x++) {
                Color c = Color.Lerp(Color.white, Color.black, terrainMap[x, y]);
                texture.SetPixel(x, y, c);
            }
        }
        texture.Apply();
    }
}
