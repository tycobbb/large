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
        Graphics.Blit(
            null,
            m_Terrain.terrainData.heightmapTexture,
            m_TerrainHeight
        );
    }
}
