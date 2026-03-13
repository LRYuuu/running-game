using UnityEngine;
using UnityEngine.Tilemaps;

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

        // 地图整体偏移（累计滚动距离）
        private float _mapOffset = 0f;

        // 已生成的 Chunk 范围（X 轴方向，世界坐标）
        private int minGeneratedChunk = 0;
        private int maxGeneratedChunk = 0;

        // 上一个障碍物的位置（世界坐标）
        private int lastObstacleWorldX = -999;
        private int currentObstacleGap = 3;

        // 空隙配置
        [System.Serializable]
        public class GapConfig
        {
            public int minGapWidth = 1;
            public int maxGapWidth = 3;
            public float gapChance = 0.1f;
            [Range(0, 10)]
            public int minGapStart = 5; // 至少生成 5 个 Chunk 后才开始出现空隙
        }

        public GapConfig gapConfig = new GapConfig();

        // 当前空隙范围（世界坐标）
        private int gapStartWorldX = -1;
        private int gapEndWorldX = -1;

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

            // 检查是否需要生成新的 Chunk（右侧）
            CheckAndGenerateChunks();

            // 检查是否需要清理 Chunk（左侧）
            CheckAndClearChunks();

            // 应用地图偏移
            ApplyMapOffset();
        }

        /// <summary>
        /// 检查并生成新的 Chunk
        /// </summary>
        private void CheckAndGenerateChunks()
        {
            // 计算当前最右侧需要生成到的 Chunk 索引
            // 玩家在 x=0，需要保证右侧有足够的 Chunk
            int targetMaxChunk = Mathf.CeilToInt(-_mapOffset / config.chunkWidth) + config.aheadChunkCount;

            while (maxGeneratedChunk < targetMaxChunk)
            {
                GenerateChunk(maxGeneratedChunk);
                maxGeneratedChunk++;
            }
        }

        /// <summary>
        /// 检查并清理移出的 Chunk
        /// </summary>
        private void CheckAndClearChunks()
        {
            // 计算需要清理到的 Chunk 索引
            // 视野左边再往后的 Chunk 可以清理
            int targetMinChunk = Mathf.FloorToInt(-_mapOffset / config.chunkWidth) - config.behindChunkCount;

            while (minGeneratedChunk < targetMinChunk)
            {
                ClearChunk(minGeneratedChunk);
                minGeneratedChunk++;
            }
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
            Debug.Log($"[TilemapMapGenerator] 初始化 - Chunk 宽度：{config.chunkWidth}, 地面高度：{config.groundHeight}");
            Debug.Log($"[TilemapMapGenerator] Tile 检查 - grassLeft: {(config.grassLeft != null ? "OK" : "NULL")}, grassMiddle: {(config.grassMiddle != null ? "OK" : "NULL")}, grassRight: {(config.grassRight != null ? "OK" : "NULL")}");
            Debug.Log($"[TilemapMapGenerator] Tile 检查 - dirtTile: {(config.dirtTile != null ? "OK" : "NULL")}");
            Debug.Log($"[TilemapMapGenerator] groundTilemap: {(groundTilemap != null ? "OK" : "NULL")}");

            // 生成初始 Chunk（从 0 到 aheadChunkCount + behindChunkCount + 1）
            int initialChunkCount = config.aheadChunkCount + config.behindChunkCount + 2;
            for (int i = 0; i < initialChunkCount; i++)
            {
                GenerateChunk(i);
                maxGeneratedChunk++;
            }
        }

        /// <summary>
        /// 生成一个 Chunk
        /// </summary>
        private void GenerateChunk(int chunkIndex)
        {
            int startX = chunkIndex * config.chunkWidth;

            // 检查是否需要生成空隙
            bool shouldGenerateGap = ShouldGenerateGap(chunkIndex);

            for (int x = 0; x < config.chunkWidth; x++)
            {
                int worldX = startX + x;

                // 如果当前位置在空隙范围内，跳过生成
                if (shouldGenerateGap && worldX >= gapStartWorldX && worldX < gapEndWorldX)
                {
                    ClearColumn(worldX);
                    continue;
                }

                // 生成地面列
                GenerateGroundColumn(worldX, x);

                // 尝试生成障碍物
                if (ShouldSpawnObstacle(worldX))
                {
                    SpawnObstacle(worldX);
                }
            }

            Debug.Log($"[TilemapMapGenerator] 生成 Chunk #{chunkIndex} @ X={startX}");
        }

        /// <summary>
        /// 判断是否需要生成空隙
        /// </summary>
        private bool ShouldGenerateGap(int chunkIndex)
        {
            // 起始区域不生成空隙
            if (chunkIndex < gapConfig.minGapStart)
            {
                gapStartWorldX = -1;
                gapEndWorldX = -1;
                return false;
            }

            // 如果当前 Chunk 包含空隙的延续部分，返回 true
            if (gapEndWorldX > chunkIndex * config.chunkWidth)
            {
                return true;
            }

            // 概率判定是否生成新空隙
            if (Random.value > gapConfig.gapChance)
            {
                gapStartWorldX = -1;
                gapEndWorldX = -1;
                return false;
            }

            // 生成新空隙
            int gapWidth = Random.Range(gapConfig.minGapWidth, gapConfig.maxGapWidth + 1);
            gapStartWorldX = chunkIndex * config.chunkWidth + Random.Range(2, config.chunkWidth - gapWidth - 2);
            gapEndWorldX = gapStartWorldX + gapWidth;

            Debug.Log($"[TilemapMapGenerator] 生成空隙：{gapStartWorldX} ~ {gapEndWorldX} (宽度：{gapWidth})");
            return true;
        }

        /// <summary>
        /// 生成单个地面列
        /// </summary>
        private void GenerateGroundColumn(int worldX, int localX)
        {
            // 1. 确定草坪类型（左/中/右）
            TileBase grassTile;
            if (localX == 0)
            {
                grassTile = config.grassLeft;    // 左边界
            }
            else if (localX == config.chunkWidth - 1)
            {
                grassTile = config.grassRight;   // 右边界
            }
            else
            {
                grassTile = config.grassMiddle;  // 中间
            }

            // 2. 设置草坪层（y = groundHeight - 1）
            Vector3Int grassPos = new Vector3Int(worldX, config.groundHeight - 1, 0);

            if (grassTile == null)
            {
                Debug.LogWarning($"[TilemapMapGenerator] grassTile 为 null! localX={localX}, worldX={worldX}");
                return;
            }

            groundTilemap.SetTile(grassPos, grassTile);

            // 3. 设置土壤层（y = 0 到 groundHeight - 2）
            // 使用单个 Tile 通过翻转创造不同效果
            for (int y = 0; y < config.groundHeight - 1; y++)
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
            for (int y = 0; y < config.groundHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                groundTilemap.SetTile(pos, null);
            }

            // 清除障碍物
            if (obstacleTilemap != null)
            {
                Vector3Int obsPos = new Vector3Int(x, config.groundHeight, 0);
                obstacleTilemap.SetTile(obsPos, null);
            }
        }

        /// <summary>
        /// 判断是否应该生成障碍物
        /// </summary>
        private bool ShouldSpawnObstacle(int worldX)
        {
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

            // 在草坪层上方生成障碍物（y = config.obstacleLayerY 或 groundHeight）
            int obstacleY = config.obstacleLayerY > 0 ? config.obstacleLayerY : config.groundHeight;
            Vector3Int obsPos = new Vector3Int(worldX, obstacleY, 0);

            // 从障碍物池中随机选择一个 Tile
            TileBase obstacleTile = config.obstacleTiles[Random.Range(0, config.obstacleTiles.Length)];

            obstacleTilemap.SetTile(obsPos, obstacleTile);
            lastObstacleWorldX = worldX;
            Debug.Log($"[TilemapMapGenerator] 生成障碍物 @ X={worldX}, Y={obstacleY}, Tile={obstacleTile.name}");
        }

        /// <summary>
        /// 清理一个 Chunk
        /// </summary>
        private void ClearChunk(int chunkIndex)
        {
            int startX = chunkIndex * config.chunkWidth;

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
            gapStartWorldX = -1;
            gapEndWorldX = -1;
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
