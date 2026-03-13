using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace SquareFireline.Map
{
    /// <summary>
    /// 基于 Unity Tilemap 的 2D 横板无尽地图生成器
    /// 玩家位置固定，地图向左滚动（类似背景系统）
    /// </summary>
    public class TilemapEndlessMapGenerator : MonoBehaviour
    {
        [Header("引用")]
        [Tooltip("目标 Tilemap 组件（地面）")]
        public Tilemap groundTilemap;

        [Tooltip("目标 Tilemap 组件（障碍物）")]
        public Tilemap obstacleTilemap;

        [Header("地图配置")]
        [Tooltip("地图配置资产")]
        public TilemapMapConfig config;

        [Header("运行时状态")]
        [Tooltip("玩家 Transform")]
        public Transform playerTransform;

        [Header("滚动设置")]
        [Tooltip("地图滚动速度（负值表示向左）")]
        public float scrollSpeed = -2f;

        [Header("初始位置设置")]
        [Tooltip("角色所在的 Chunk 索引（从 0 开始计数，建议设为 1 让角色靠左）")]
        public int playerChunkIndex = 1;

        [Tooltip("初始地图偏移（用于调整玩家视觉位置，负值向左）")]
        public float initialMapOffset = -12f;

        // 地图整体偏移（累计滚动距离）
        private float _mapOffset = 0f;

        // 已生成的 Chunk 范围（X 轴方向，世界坐标）
        private int minGeneratedChunk = 0;
        private int maxGeneratedChunk = 0;

        // 上一个障碍物的位置（世界坐标）
        private int lastObstacleWorldX = -999;
        private int currentObstacleGap = 3;

        // 地形起伏高度缓存
        private Dictionary<int, int> _heightCache = new Dictionary<int, int>();

        // 空隙配置（暂时禁用）
        // [System.Serializable]
        // public class GapConfig
        // {
        //     public int minGapWidth = 1;
        //     public int maxGapWidth = 3;
        //     public float gapChance = 0.1f;
        //     [Range(0, 10)]
        //     public int minGapStart = 5;
        // }

        // public GapConfig gapConfig = new GapConfig();

        // 当前空隙范围（暂时禁用）
        // private int gapStartWorldX = -1;
        // private int gapEndWorldX = -1;

        private void Awake()
        {
            if (groundTilemap == null)
            {
                groundTilemap = GetComponent<Tilemap>();
            }

            if (config == null)
            {
                Debug.LogError("[TilemapMapGenerator] 缺少 MapConfig 配置！");
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (playerTransform == null || groundTilemap == null || config == null) return;

            // 更新地图偏移（地图向左移动）
            _mapOffset += scrollSpeed * Time.deltaTime;

            // 检查并生成新的 Chunk（右侧）
            CheckAndGenerateChunks();

            // 检查是否需要清理 Chunk（左侧）- 清理后立即生成新的补充
            CheckAndClearChunks();

            // 应用地图偏移
            ApplyMapOffset();
        }

        /// <summary>
        /// 检查并生成新的 Chunk
        /// </summary>
        private void CheckAndGenerateChunks()
        {
            // 保持固定的 Chunk 总数为 4 个
            int targetTotalChunks = 4;
            int currentChunkCount = maxGeneratedChunk - minGeneratedChunk;
            int chunksToGenerate = targetTotalChunks - currentChunkCount;

            // 在右侧生成新的 Chunk
            for (int i = 0; i < chunksToGenerate; i++)
            {
                GenerateChunkAtEnd();
            }
        }

        /// <summary>
        /// 检查并清理移出的 Chunk
        /// </summary>
        private void CheckAndClearChunks()
        {
            // 计算需要清理到的 Chunk 索引
            int targetMinChunk = Mathf.FloorToInt(-_mapOffset / config.chunkWidth) - config.behindChunkCount;

            while (minGeneratedChunk < targetMinChunk)
            {
                ClearChunk(minGeneratedChunk);
                minGeneratedChunk++;
            }
        }

        /// <summary>
        /// 在末尾生成一个 Chunk（基于已生成 Chunk 的末尾位置）
        /// </summary>
        private void GenerateChunkAtEnd()
        {
            // 计算起始 X 位置：基于 minGeneratedChunk 的起始位置 + 偏移
            int startX = minGeneratedChunk * config.chunkWidth + (maxGeneratedChunk - minGeneratedChunk) * config.chunkWidth;

            Debug.Log($"[TilemapMapGenerator] 生成 Chunk #{maxGeneratedChunk} @ X={startX}");

            for (int x = 0; x < config.chunkWidth; x++)
            {
                int worldX = startX + x;

                // 生成地面列
                GenerateGroundColumn(worldX, x);

                // 尝试生成障碍物
                if (ShouldSpawnObstacle(worldX))
                {
                    SpawnObstacle(worldX);
                }
            }

            maxGeneratedChunk++;
        }

        /// <summary>
        /// 应用地图偏移（移动整个 Tilemap）
        /// </summary>
        private void ApplyMapOffset()
        {
            if (groundTilemap != null)
            {
                groundTilemap.transform.localPosition = new Vector3(_mapOffset, 0, 0);
            }
            if (obstacleTilemap != null)
            {
                obstacleTilemap.transform.localPosition = new Vector3(_mapOffset, 0, 0);
            }
        }

        /// <summary>
        /// 初始化地图
        /// </summary>
        public void Initialize()
        {
            Debug.Log($"[TilemapMapGenerator] 初始化 - Chunk 宽度：{config.chunkWidth}, 基准高度：{config.baseHeight}");
            Debug.Log($"[TilemapMapGenerator] Tile 检查 - grassLeft: {(config.grassLeft != null ? "OK" : "NULL")}, grassMiddle: {(config.grassMiddle != null ? "OK" : "NULL")}, grassRight: {(config.grassRight != null ? "OK" : "NULL")}");
            Debug.Log($"[TilemapMapGenerator] Tile 检查 - dirtTile: {(config.dirtTile != null ? "OK" : "NULL")}");
            Debug.Log($"[TilemapMapGenerator] groundTilemap: {(groundTilemap != null ? "OK" : "NULL")}");
            Debug.Log($"[TilemapMapGenerator] 起始 Chunk 索引：{playerChunkIndex}");
            Debug.Log($"[TilemapMapGenerator] 地形起伏：{config.enableTerrainVariation}");

            // 设置初始 minGeneratedChunk
            minGeneratedChunk = 0;
            maxGeneratedChunk = 0;

            // 清空缓存
            _heightCache.Clear();

            // 设置初始地图偏移，让左侧填满
            _mapOffset = initialMapOffset;

            // 生成初始 4 个 Chunk
            int initialChunkCount = 4;
            for (int i = 0; i < initialChunkCount; i++)
            {
                GenerateChunkAtEnd();
            }

            // 应用初始偏移
            ApplyMapOffset();
        }

        /// <summary>
        /// 判断是否需要生成空隙（暂时禁用，始终返回 false）
        /// </summary>
        private bool ShouldGenerateGap(int chunkIndex)
        {
            return false;
        }

        /// <summary>
        /// 生成单个地面列
        /// </summary>
        private void GenerateGroundColumn(int worldX, int localX)
        {
            // 1. 获取实际高度
            int columnHeight = GetColumnHeight(worldX);

            // 2. 获取左右列高度（用于边界判断）
            int leftHeight = GetColumnHeight(worldX - 1);
            int rightHeight = GetColumnHeight(worldX + 1);

            // 3. 判断是否需要黑边（暴露的侧面）
            bool leftExposed = (leftHeight < columnHeight);   // 左侧低，当前列左侧暴露
            bool rightExposed = (rightHeight < columnHeight); // 右侧低，当前列右侧暴露

            // 4. 选择草坪 Tile
            TileBase grassTile;
            if (leftExposed && rightExposed)
            {
                // 两侧都暴露：使用 grass_left 或 grass_right（或用专门的孤立 Tile）
                grassTile = config.grassLeft;
            }
            else if (leftExposed)
            {
                // 仅左侧暴露：使用 grass_left
                grassTile = config.grassLeft;
            }
            else if (rightExposed)
            {
                // 仅右侧暴露：使用 grass_right
                grassTile = config.grassRight;
            }
            else
            {
                // 无暴露：使用 grass_middle
                grassTile = config.grassMiddle;
            }

            // 5. 设置草坪层（y = columnHeight - 1）
            Vector3Int grassPos = new Vector3Int(worldX, columnHeight - 1, 0);

            if (grassTile == null)
            {
                Debug.LogWarning($"[TilemapMapGenerator] grassTile 为 null! localX={localX}, worldX={worldX}");
                return;
            }

            groundTilemap.SetTile(grassPos, grassTile);

            // 6. 设置土壤层（y = 0 到 columnHeight - 2）
            // 土壤层不会暴露（因为高度差≤1），统一使用 dirtTile
            for (int y = 0; y < columnHeight - 1; y++)
            {
                if (config.dirtTile == null)
                {
                    Debug.LogWarning("[TilemapMapGenerator] dirtTile 为 null，跳过土壤层生成");
                    continue;
                }

                Vector3Int dirtPos = new Vector3Int(worldX, y, 0);

                // 根据位置生成不同的翻转效果
                int flipMode = GetDirtFlipMode(worldX, y);

                // 设置 Tile 并应用翻转
                SetTileWithFlip(dirtPos, config.dirtTile, flipMode, worldX, y);
            }
        }

        /// <summary>
        /// 根据位置计算翻转模式
        /// 返回值：0=无翻转，1=水平翻转，2=垂直翻转，3=水平 + 垂直翻转
        /// </summary>
        private int GetDirtFlipMode(int x, int y)
        {
            // 使用位置和随机数组合，创造看似随机的翻转效果
            int hash = Mathf.Abs((x * 17 + y * 31) % 4);
            return hash;
        }

        /// <summary>
        /// 设置 Tile 并应用翻转效果
        /// </summary>
        private void SetTileWithFlip(Vector3Int position, TileBase tile, int flipMode, int worldX, int y)
        {
            if (tile is Tile tileData)
            {
                // 创建一个新的 Tile 实例用于设置
                var newTile = ScriptableObject.CreateInstance<Tile>();
                newTile.sprite = tileData.sprite;
                newTile.colliderType = tileData.colliderType;

                // 根据 flipMode 设置 Transform 矩阵
                Matrix4x4 transform = Matrix4x4.identity;

                if (flipMode == 1) // 水平翻转
                {
                    transform = Matrix4x4.Scale(new Vector3(-1, 1, 1));
                }
                else if (flipMode == 2) // 垂直翻转
                {
                    transform = Matrix4x4.Scale(new Vector3(1, -1, 1));
                }
                else if (flipMode == 3) // 水平 + 垂直翻转（相当于旋转 180 度）
                {
                    transform = Matrix4x4.Scale(new Vector3(-1, -1, 1));
                }

                // 应用变换
                newTile.transform = transform;

                groundTilemap.SetTile(position, newTile);
            }
            else
            {
                // 如果不是 Tile 类型，直接设置
                groundTilemap.SetTile(position, tile);
            }
        }

        /// <summary>
        /// 清除一个地面列
        /// </summary>
        private void ClearColumn(int x)
        {
            // 获取实际高度
            int columnHeight = GetColumnHeight(x);

            // 清除地面（到实际高度）
            for (int y = 0; y < columnHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                groundTilemap.SetTile(pos, null);
            }

            // 清除障碍物（在草坪层上方）
            if (obstacleTilemap != null)
            {
                Vector3Int obsPos = new Vector3Int(x, columnHeight, 0);
                obstacleTilemap.SetTile(obsPos, null);
            }

            // 清除高度缓存
            ClearHeightCache(x);
        }

        /// <summary>
        /// 判断是否应该生成障碍物
        /// </summary>
        private bool ShouldSpawnObstacle(int worldX)
        {
            // 检查当前列高度与前一列的高度差
            if (worldX > 0)
            {
                int currentHeight = GetColumnHeight(worldX);
                int prevHeight = GetColumnHeight(worldX - 1);

                // 高度差 > 0 时不生成障碍物（避免障碍物在空中或地下）
                if (Mathf.Abs(currentHeight - prevHeight) > 0)
                {
                    return false;
                }
            }

            // 检查与上一个障碍物的间隔
            if (worldX - lastObstacleWorldX < currentObstacleGap)
            {
                return false;
            }

            // 概率判定
            if (Random.value > config.obstacleSpawnChance)
            {
                return false;
            }

            // 更新间隔
            currentObstacleGap = Random.Range(config.minObstacleGap, config.maxObstacleGap + 1);
            return true;
        }

        /// <summary>
        /// 生成障碍物
        /// </summary>
        private void SpawnObstacle(int worldX)
        {
            if (obstacleTilemap == null)
            {
                Debug.LogWarning("[TilemapMapGenerator] 障碍物 Tilemap 未设置，跳过障碍物生成");
                return;
            }

            if (config.obstacleTiles == null || config.obstacleTiles.Length == 0)
            {
                Debug.LogWarning("[TilemapMapGenerator] 障碍物 Tile 池为空，请在 MapConfig 中配置障碍物 Tile");
                return;
            }

            // 获取当前列高度，障碍物生成在草坪层上方
            int columnHeight = GetColumnHeight(worldX);
            Vector3Int obsPos = new Vector3Int(worldX, columnHeight, 0);

            // 从障碍物池中随机选择一个 Tile
            TileBase obstacleTile = config.obstacleTiles[Random.Range(0, config.obstacleTiles.Length)];

            obstacleTilemap.SetTile(obsPos, obstacleTile);
            lastObstacleWorldX = worldX;
            Debug.Log($"[TilemapMapGenerator] 生成障碍物 @ X={worldX}, Y={columnHeight}, Tile={obstacleTile.name}");
        }

        /// <summary>
        /// 获取指定 X 坐标的地面高度
        /// </summary>
        private int GetColumnHeight(int worldX)
        {
            // 检查缓存
            if (_heightCache.TryGetValue(worldX, out int cached))
                return cached;

            // 前 N 个 Chunk 保持平坦
            int chunkIndex = Mathf.FloorToInt(worldX / (float)config.chunkWidth);
            if (chunkIndex < config.flatChunkCount)
            {
                _heightCache[worldX] = config.baseHeight;
                return config.baseHeight;
            }

            // 使用 Perlin Noise 计算基础高度
            float noiseX = (worldX + config.seed) * config.noiseFrequency;
            float noiseValue = Mathf.PerlinNoise(noiseX, 0);

            // 计算目标高度
            float noiseOffset = (noiseValue - 0.5f) * 2 * config.heightVariation * config.noiseStrength;
            int targetHeight = Mathf.FloorToInt(config.baseHeight + noiseOffset);

            // 获取前一列高度（用于限制高度差）
            if (worldX > 0 && _heightCache.TryGetValue(worldX - 1, out int prevHeight))
            {
                // 限制高度差 ≤ 1
                if (targetHeight > prevHeight + 1)
                    targetHeight = prevHeight + 1;
                else if (targetHeight < prevHeight - 1)
                    targetHeight = prevHeight - 1;
            }

            // 限制在最小/最大范围内
            targetHeight = Mathf.Clamp(targetHeight, config.minHeight, config.maxHeight);

            // 存入缓存
            _heightCache[worldX] = targetHeight;

            return targetHeight;
        }

        /// <summary>
        /// 清除指定列的高度缓存
        /// </summary>
        private void ClearHeightCache(int worldX)
        {
            _heightCache.Remove(worldX);
        }

        /// <summary>
        /// 清理一个 Chunk
        /// </summary>
        private void ClearChunk(int chunkIndex)
        {
            // 计算相对于 minGeneratedChunk 的起始位置
            int startX = minGeneratedChunk * config.chunkWidth + (chunkIndex - minGeneratedChunk) * config.chunkWidth;

            for (int x = 0; x < config.chunkWidth; x++)
            {
                ClearColumn(startX + x);
            }

            Debug.Log($"[TilemapMapGenerator] 清理 Chunk #{chunkIndex} @ X={startX}");
        }

        /// <summary>
        /// 清理所有
        /// </summary>
        public void Cleanup()
        {
            if (groundTilemap != null)
                groundTilemap.ClearAllTiles();

            if (obstacleTilemap != null)
                obstacleTilemap.ClearAllTiles();

            minGeneratedChunk = 0;
            maxGeneratedChunk = 0;
            lastObstacleWorldX = -999;

            // 清空高度缓存
            _heightCache.Clear();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下显示调试信息
        /// </summary>
        private void OnDrawGizmos()
        {
            if (config == null) return;

            // 绘制 Chunk 边界
            Gizmos.color = Color.yellow;
            for (int i = minGeneratedChunk; i <= maxGeneratedChunk; i++)
            {
                float x = i * config.chunkWidth;
                Gizmos.DrawLine(new Vector3(x, 0, 0), new Vector3(x, config.groundHeight + 2, 0));
            }

            // 绘制地面高度线
            Gizmos.color = Color.green;
            float groundY = config.groundHeight - 1;
            Gizmos.DrawLine(
                new Vector3(minGeneratedChunk * config.chunkWidth, groundY, 0),
                new Vector3(maxGeneratedChunk * config.chunkWidth, groundY, 0)
            );
        }
#endif
    }
}
