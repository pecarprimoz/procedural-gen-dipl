using UnityEngine;

public class TerrainGenerationDebugger : MonoBehaviour {
    public TerrainGeneration tg;
    [SerializeField]
    public DebugPlaneType planeContent;
    private bool WasTextureCreated = false;

    private static Texture2D texture;
    private void Start() {
        planeContent = DebugPlaneType.kHeightMap;
    }

    public void GenerateTexture() {
        // TODO, CHECK THE WIDTH/HEIGHT AND UPDATE TYPE, THIS RAPES 1km * 1km TERRAINS ! FIX BY MAKING IT CALLABLE TROUGH EDITOR
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
        //GenerateTexture();
    }
}
