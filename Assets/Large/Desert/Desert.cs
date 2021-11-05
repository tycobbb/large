using UnityEngine;

/// the desert terrain
[ExecuteAlways]
public class Desert: MonoBehaviour {
    // -- nodes --
    [Header("nodes")]
    [Tooltip("the terrain")]
    [SerializeField] Terrain m_Terrain;

    [Tooltip("a material for the height shader")]
    [SerializeField] Material m_TerrainHeight;

    // -- lifecycle --
    void Update() {
        var td = m_Terrain.terrainData;

        // render a new heightmap
        Graphics.Blit(
            null,
            td.heightmapTexture,
            m_TerrainHeight
        );

        // mark the entire heightmap as dirty
        var tr = new RectInt(
            0,
            0,
            td.heightmapResolution,
            td.heightmapResolution
        );

        td.DirtyHeightmapRegion(
            tr,
            TerrainHeightmapSyncControl.HeightOnly
        );

        // sync it
        td.SyncHeightmap();
    }
}
