using UnityEngine;

public class TerrainGenerationDebugger : MonoBehaviour {
    public TerrainGeneration tg;
    [SerializeField]
    public DebugPlaneType planeContent;
    public bool AlwaysUpdate = false;

    private static Texture2D texture;
    private void Start() {
        planeContent = DebugPlaneType.kHeightMap;
    }

    public void GenerateTexture() {
        // Fun fact, Unity does not GC new Terrains, which results in a memory leak, to prevent this we call destroy and have the texture as static
        Destroy(texture);
        // The debug terrain is driven by the TerrainGeneration (the main terrain that we are working on)
        texture = new Texture2D(tg.TerrainInfo.TerrainWidth, tg.TerrainInfo.TerrainHeight);
        GetComponent<Renderer>().material.mainTexture = texture;
        if (tg.TerrainInfo.HeightMap == null) {
            return;
        }
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(TextureGeneration.GenerateHeightmapTexture(tg, planeContent));
        texture.Apply();
    }

    private void Update() {
        if (AlwaysUpdate) {
            GenerateTexture();
        }
    }
}
