public class TerrainPoint {
    public TerrainPoint(int x, int z) {
        X = x;
        Z = z;
        ContainsItem = false;
    }

    public int X { get; set; }
    public int Z { get; set; }
    public bool ContainsItem { get; set; }
}