using UnityEngine;
using UnityEngine.Tilemaps;

namespace SquareFireline.Map
{
    /// <summary>
    /// 2D 横板跳跃 Tilemap 地图配置
    /// </summary>
    [CreateAssetMenu(fileName = "TilemapMapConfig", menuName = "Square Fireline/Tilemap Map Config")]
    public class TilemapMapConfig : ScriptableObject
    {
        [Header("Chunk 尺寸")]
        [Tooltip("Chunk 宽度（X 轴瓦片数量）")]
        public int chunkWidth = 20;

        [Tooltip("地面高度（从 y=0 到地面的瓦片数量）")]
        public int groundHeight = 5;

        [Header("生成配置")]
        [Tooltip("玩家右侧保留的 Chunk 数量")]
        public int aheadChunkCount = 3;

        [Tooltip("玩家左侧清理的 Chunk 数量")]
        public int behindChunkCount = 2;

        [Header("草坪 Tile（顶层）")]
        [Tooltip("左边界草坪 Tile（左侧有黑边）")]
        public TileBase grassLeft;

        [Tooltip("中间草坪 Tile（无缝衔接）")]
        public TileBase grassMiddle;

        [Tooltip("右边界草坪 Tile（右侧有黑边）")]
        public TileBase grassRight;

        [Header("土壤 Tile（地下层）")]
        [Tooltip("单个土壤 Tile，生成时通过翻转创造不同效果")]
        public TileBase dirtTile;

        [Header("障碍物 Tile 池")]
        [Tooltip("障碍物 Tile 池，至少包含一个障碍物 Tile")]
        public TileBase[] obstacleTiles;

        [Header("障碍物配置")]
        [Tooltip("障碍物生成概率 (0-1)")]
        [Range(0f, 1f)]
        public float obstacleSpawnChance = 0.3f;

        [Tooltip("障碍物之间的最小间隔（瓦片数量）")]
        public int minObstacleGap = 3;

        [Tooltip("障碍物之间的最大间隔（瓦片数量）")]
        public int maxObstacleGap = 8;

        [Tooltip("障碍物在 Y 轴上的生成层（通常为 groundHeight）")]
        public int obstacleLayerY = 5;

        [Header("地形起伏")]
        [Tooltip("启用起伏地形")]
        public bool enableTerrainVariation = false;

        [Tooltip("基准高度")]
        public int baseHeight = 5;

        [Tooltip("最小高度")]
        public int minHeight = 2;

        [Tooltip("最大高度")]
        public int maxHeight = 8;

        [Tooltip("高度变化幅度")]
        public int heightVariation = 3;

        [Tooltip("噪声频率")]
        public float noiseFrequency = 0.15f;

        [Tooltip("噪声强度")]
        public float noiseStrength = 1.0f;

        [Tooltip("随机种子")]
        public int seed = 12345;

        [Tooltip("前 N 个 Chunk 保持平坦")]
        public int flatChunkCount = 3;
    }
}
