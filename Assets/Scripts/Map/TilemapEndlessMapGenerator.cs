using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using RunnersJourney.Game;

namespace RunnersJourney.Map
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

        [Header("调试设置")]
        [Tooltip("是否启用详细日志")]
        public bool enableDebugLog = false;

        [Header("难度系统")]
        [Tooltip("难度计算器引用（用于动态难度）")]
        public DifficultyCalculator difficultyCalculator;

        [Header("生物群系")]
        [Tooltip("生物群系管理器引用")]
        private BiomeManager _biomeManager;

        [Header("默认 Tile（回退用）")]
        [Tooltip("默认地表 Tile（当群系未配置时使用）")]
        public TileBase defaultGroundTile;

        [Tooltip("默认土壤 Tile（当群系未配置时使用）")]
        public TileBase defaultDirtTile;

        [Tooltip("默认障碍物 Tile（当群系未配置时使用）")]
        public TileBase[] defaultObstacleTiles;

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

        // 当前空隙范围（世界坐标）
        private int gapStartWorldX = -1;
        private int gapEndWorldX = -1; // exclusive: gapStartWorldX <= x < gapEndWorldX

        // 上一个空隙的结束位置（用于间隔控制）
        private int lastGapEndWorldX = -999;

        // 滚动暂停状态
        private bool _isScrollingPaused = false;

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
            else
            {
                // 验证配置可玩性
                MapPlayabilityValidator.ValidateConfig(config);
            }

            // 获取 BiomeManager 引用
            _biomeManager = BiomeManager.Instance;
            if (_biomeManager != null)
            {
                _biomeManager.OnBiomeChanged.AddListener(OnBiomeChanged);
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            // 暂停时不更新地图滚动
            if (_isScrollingPaused || playerTransform == null || groundTilemap == null || config == null) return;

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
        /// 获取地图滚动距离（用于群系切换等）
        /// </summary>
        public float GetScrollDistance()
        {
            // 初始偏移是负值（向左），滚动距离是相对于初始位置的绝对值
            return Mathf.Max(0f, initialMapOffset - _mapOffset);
        }

        /// <summary>
        /// 获取已生成的 Chunk 总数（用于群系切换）
        /// </summary>
        public int GetGeneratedChunkCount()
        {
            return maxGeneratedChunk;
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
            // 在生成 Chunk 之前，先更新群系（基于即将生成的 Chunk 索引）
            if (_biomeManager != null)
            {
                _biomeManager.UpdateBiomeByChunkCount(maxGeneratedChunk);
            }

            // 计算起始 X 位置：基于 minGeneratedChunk 的起始位置 + 偏移
            int startX = minGeneratedChunk * config.chunkWidth + (maxGeneratedChunk - minGeneratedChunk) * config.chunkWidth;

            // 检查是否生成空隙
            bool shouldGenerateGap = ShouldGenerateGap(maxGeneratedChunk, startX);
            int gapWidth = 0;
            int gapStartLocalX = -1;
            int gapGroundHeight = -1;

            if (shouldGenerateGap)
            {
                // 随机选择空隙宽度
                gapWidth = Random.Range(config.minGapWidth, config.maxGapWidth + 1);
                // 空隙开始位置（Chunk 内部，留出边界）
                gapStartLocalX = Random.Range(2, config.chunkWidth - gapWidth - 2);

                // 检查空隙位置的地形高度
                int candidateStartX = startX + gapStartLocalX;
                int candidateEndX = candidateStartX + gapWidth;

                // 获取空隙左右两侧的地块高度
                int leftGroundHeight = GetColumnHeight(candidateStartX - 1);
                int rightGroundHeight = GetColumnHeight(candidateEndX);

                // 检查是否为无效位置（左右两侧有空隙或高度为 0）
                if (leftGroundHeight <= 0 || rightGroundHeight <= 0)
                {
                    // 空隙旁边已经是空隙，取消生成
                    gapWidth = 0;
                    gapStartLocalX = -1;
                    shouldGenerateGap = false;
                    if (enableDebugLog)
                    {
                        Debug.Log($"[TilemapMapGenerator] 空隙位置无效（旁边是空隙），取消生成 @ Chunk #{maxGeneratedChunk}");
                    }
                }
                else
                {
                    // 可玩性验证：空隙宽度检查
                    if (!MapPlayabilityValidator.IsGapWidthPlayable(config, gapWidth))
                    {
                        if (enableDebugLog)
                        {
                            Debug.LogWarning($"[TilemapMapGenerator] 空隙宽度过大 ({gapWidth} > {config.maxGapWidthPlayable})，取消生成 @ Chunk #{maxGeneratedChunk}");
                        }
                        gapWidth = 0;
                        gapStartLocalX = -1;
                        shouldGenerateGap = false;
                    }
                    else
                    {
                        // 可玩性验证：空隙两侧需要有足够的落地空间
                        // 检查空隙左侧是否有足够的障碍物间隔
                        if (candidateStartX - 1 - lastObstacleWorldX < config.minPlayableObstacleGap)
                        {
                            if (enableDebugLog)
                            {
                                Debug.LogWarning($"[TilemapMapGenerator] 空隙左侧障碍物间隔不足 ({candidateStartX - 1 - lastObstacleWorldX} < {config.minPlayableObstacleGap})，取消生成 @ Chunk #{maxGeneratedChunk}");
                            }
                            gapWidth = 0;
                            gapStartLocalX = -1;
                            shouldGenerateGap = false;
                        }
                    }

                    if (shouldGenerateGap)
                    {
                        // 使用左右两侧较低的地块高度作为空隙基准高度
                        gapGroundHeight = Mathf.Min(leftGroundHeight, rightGroundHeight);

                        if (leftGroundHeight != rightGroundHeight)
                        {
                            if (enableDebugLog)
                            {
                                Debug.Log($"[TilemapMapGenerator] 空隙两侧高度不同 (左={leftGroundHeight}, 右={rightGroundHeight})，使用较低高度 {gapGroundHeight} @ Chunk #{maxGeneratedChunk}");
                            }
                        }
                    }
                }
            }

            if (enableDebugLog && shouldGenerateGap)
            {
                Debug.Log($"[TilemapMapGenerator] 生成空隙 @ Chunk #{maxGeneratedChunk}, 宽度={gapWidth}, 起始={gapStartWorldX}, 高度={gapGroundHeight}");
            }

            if (enableDebugLog)
            {
                Debug.Log($"[TilemapMapGenerator] 生成 Chunk #{maxGeneratedChunk} @ X={startX}");
            }

            for (int x = 0; x < config.chunkWidth; x++)
            {
                int worldX = startX + x;

                // 检查是否在空隙范围内
                if (gapStartLocalX >= 0 && x >= gapStartLocalX && x < gapStartLocalX + gapWidth)
                {
                    // 空隙列：生成空隙 Tile 并清除高度缓存
                    GenerateGapColumn(worldX, gapGroundHeight);
                    _heightCache[worldX] = 0; // 标记为空隙
                    continue;
                }

                // 生成地面列
                GenerateGroundColumn(worldX, x);

                // 生成障碍物（空隙上方不生成障碍物）
                if (gapStartLocalX < 0 && ShouldSpawnObstacle(worldX))
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
            if (enableDebugLog)
            {
                Debug.Log($"[TilemapMapGenerator] 初始化 - Chunk 宽度：{config.chunkWidth}, 基准高度：{config.baseHeight}");
                Debug.Log($"[TilemapMapGenerator] Tile 检查 - grassLeft: {(config.grassLeft != null ? "OK" : "NULL")}, grassMiddle: {(config.grassMiddle != null ? "OK" : "NULL")}, grassRight: {(config.grassRight != null ? "OK" : "NULL")}");
                Debug.Log($"[TilemapMapGenerator] Tile 检查 - dirtTile: {(config.dirtTile != null ? "OK" : "NULL")}");
                Debug.Log($"[TilemapMapGenerator] groundTilemap: {(groundTilemap != null ? "OK" : "NULL")}");
                Debug.Log($"[TilemapMapGenerator] 起始 Chunk 索引：{playerChunkIndex}");
                Debug.Log($"[TilemapMapGenerator] 地形起伏：{config.enableTerrainVariation}");
            }

            // 设置初始 minGeneratedChunk
            minGeneratedChunk = 0;
            maxGeneratedChunk = 0;

            // 重置障碍物状态
            lastObstacleWorldX = -999;
            currentObstacleGap = Random.Range(config.minObstacleGap, config.maxObstacleGap + 1);

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
        /// 判断是否需要生成空隙
        /// </summary>
        private bool ShouldGenerateGap(int chunkIndex, int startX)
        {
            // 起始保护区检查：前 N 个 Chunk 不生成空隙
            if (chunkIndex < config.minGapStartChunk)
            {
                return false;
            }

            // 间隔检查：与上一个空隙的距离不能太近
            int gapStartCandidate = chunkIndex * config.chunkWidth;
            if (gapStartCandidate - lastGapEndWorldX < config.minGapInterval * config.chunkWidth)
            {
                return false;
            }

            // 概率判定 - 使用难度系统计算的当前概率（如果有难度计算器）
            float gapChance = config.gapSpawnChance;
            if (difficultyCalculator != null && difficultyCalculator.CurrentGapChance > 0f)
            {
                gapChance = difficultyCalculator.CurrentGapChance;
            }

            if (Random.value > gapChance)
            {
                return false;
            }

            // 可玩性验证：最大空隙宽度超过可玩值时不生成
            if (config.maxGapWidth > config.maxGapWidthPlayable)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning($"[TilemapMapGenerator] 配置警告：maxGapWidth ({config.maxGapWidth}) > maxGapWidthPlayable ({config.maxGapWidthPlayable})，已禁用空隙生成");
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断世界坐标是否在空隙范围内
        /// </summary>
        private bool IsInGap(int worldX)
        {
            return worldX >= gapStartWorldX && worldX < gapEndWorldX;
        }

        /// <summary>
        /// 获取空隙另一侧的地块高度
        /// </summary>
        /// <param name="currentX">当前位置</param>
        /// <param name="direction">方向：-1=向左查找，1=向右查找</param>
        /// <returns>空隙另一侧的地块高度，如果找不到则返回 0</returns>
        private int GetGapOppositeHeight(int currentX, int direction)
        {
            // 从当前位置向指定方向查找，跳过空隙列
            int searchX = currentX;
            while (IsInGap(searchX))
            {
                searchX += direction;
            }
            // 返回空隙另一侧的地块高度
            return GetColumnHeight(searchX);
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

            // 3. 判断是否在空隙旁边
            bool leftIsGap = IsInGap(worldX - 1);
            bool rightIsGap = IsInGap(worldX + 1);

            // 4. 获取空隙另一侧的地块高度（如果需要）
            int leftGroundHeight = leftHeight;
            int rightGroundHeight = rightHeight;

            if (leftIsGap)
            {
                // 左侧是空隙，获取空隙左侧的地块高度（从左侧相邻列开始向左查找）
                leftGroundHeight = GetGapOppositeHeight(worldX - 1, -1);
            }

            if (rightIsGap)
            {
                // 右侧是空隙，获取空隙右侧的地块高度（从右侧相邻列开始向右查找）
                rightGroundHeight = GetGapOppositeHeight(worldX + 1, 1);
            }

            // 5. 判断是否需要黑边（暴露的侧面）
            // 核心逻辑：只要当前列比相邻列高，该侧就显示黑边（因为与空气接触）

            bool leftExposed = false;
            bool rightExposed = false;

            // 左侧暴露：当前列严格高于左侧（且左侧不是空隙）
            if (!leftIsGap && columnHeight > leftHeight)
            {
                leftExposed = true;
            }
            // 右侧暴露：当前列严格高于右侧（且右侧不是空隙）
            if (!rightIsGap && columnHeight > rightHeight)
            {
                rightExposed = true;
            }

            // 保存土壤层的暴露状态（不受空隙影响，用于后续土壤 Tile 选择）
            bool dirtLeftExposed = leftExposed;
            bool dirtRightExposed = rightExposed;

            // 特殊情况：空隙相邻时，靠近空隙的一侧根据空隙另一侧的地块高度决定是否显示黑边
            // 逻辑：靠近空隙的一侧，如果比空隙另一侧的地块高，则显示黑边；如果相等或更低，则不显示黑边（无缝衔接）
            // 注意：这个逻辑只影响草坪层，不影响土壤层
            if (rightIsGap)
            {
                // 右侧是空隙，判断右侧（靠近空隙）是否需要黑边
                // 比较：当前列 vs 空隙右侧的地块高度（rightGroundHeight）
                if (columnHeight > rightGroundHeight)
                {
                    // 当前列比空隙右侧高，右侧（靠近空隙）应该显示黑边
                    rightExposed = true;
                }
                else
                {
                    // 当前列比空隙右侧低或相等，右侧（靠近空隙）不显示黑边（无缝衔接）
                    rightExposed = false;
                }
            }
            if (leftIsGap)
            {
                // 左侧是空隙，判断左侧（靠近空隙）是否需要黑边
                // 比较：当前列 vs 空隙左侧的地块高度（leftGroundHeight）
                if (columnHeight > leftGroundHeight)
                {
                    // 当前列比空隙左侧高，左侧（靠近空隙）应该显示黑边
                    leftExposed = true;
                }
                else
                {
                    // 当前列比空隙左侧低或相等，左侧（靠近空隙）不显示黑边（无缝衔接）
                    leftExposed = false;
                }
            }

            // 6. 选择草坪 Tile（从群系配置获取，根据边界状态）
            TileBase grassTile = GetGroundTile(worldX, leftExposed, rightExposed);

            // 6.5 选择土壤 Tile
            // 注意：土壤层始终使用普通 Tile，不显示黑边
            // 黑边只在草坪层的边界显示
            TileBase dirtTileToUse = GetDirtTile(worldX, false, false);
            int dirtFlipMode = 0; // 0=无翻转，1=水平翻转，2=垂直翻转，3=水平 + 垂直翻转

            // 6. 设置草坪层（y = columnHeight - 1）
            Vector3Int grassPos = new Vector3Int(worldX, columnHeight - 1, 0);

            if (grassTile == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning($"[TilemapMapGenerator] grassTile 为 null! localX={localX}, worldX={worldX}");
                }
                return;
            }

            groundTilemap.SetTile(grassPos, grassTile);

            // 7. 设置土壤层（y = 0 到 columnHeight - 2）
            // 土壤层根据暴露状态使用不同的 Tile
            for (int y = 0; y < columnHeight - 1; y++)
            {
                if (dirtTileToUse == null)
                {
                    if (enableDebugLog)
                    {
                        Debug.LogWarning("[TilemapMapGenerator] dirtTile 为 null，跳过土壤层生成");
                    }
                    continue;
                }

                Vector3Int dirtPos = new Vector3Int(worldX, y, 0);

                // 设置 Tile 并应用翻转
                SetTileWithFlip(dirtPos, dirtTileToUse, dirtFlipMode, worldX, y);
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
        /// 生成空隙列（填充水流 Tile）
        /// </summary>
        /// <param name="worldX">世界 X 坐标</param>
        /// <param name="groundHeight">地面高度（水流顶部位置）</param>
        private void GenerateGapColumn(int worldX, int groundHeight)
        {
            if (config.gapTopTile == null || config.gapCenterTile == null)
            {
                // 如果没有配置空隙 Tile，则不生成（保持空白）
                if (enableDebugLog)
                {
                    Debug.LogWarning($"[TilemapMapGenerator] 空隙 Tile 未配置！gapTopTile={(config.gapTopTile != null ? "OK" : "NULL")}, gapCenterTile={(config.gapCenterTile != null ? "OK" : "NULL")}");
                }
                return;
            }

            // 在空隙位置生成填充 Tile
            // 顶部水流 Tile（地面高度位置）
            if (groundHeight > 0)
            {
                Vector3Int gapTopPos = new Vector3Int(worldX, groundHeight - 1, 0);
                groundTilemap.SetTile(gapTopPos, config.gapTopTile);
            }

            // 中间水流 Tile（从 y=0 到 y=groundHeight-2）
            for (int y = 0; y < groundHeight - 1; y++)
            {
                Vector3Int gapCenterPos = new Vector3Int(worldX, y, 0);
                groundTilemap.SetTile(gapCenterPos, config.gapCenterTile);
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

                // 可玩性验证：高度差超过可玩限制时不生成
                if (!MapPlayabilityValidator.IsHeightDifferencePlayable(config, currentHeight - prevHeight))
                {
                    return false;
                }
            }

            // 检查与上一个障碍物的间隔
            if (worldX - lastObstacleWorldX < currentObstacleGap)
            {
                return false;
            }

            // 可玩性验证：障碍物间隔小于可玩最小值时不生成
            if (!MapPlayabilityValidator.IsObstacleGapPlayable(config, worldX - lastObstacleWorldX))
            {
                return false;
            }

            // 概率判定 - 使用难度系统计算的当前概率（如果有难度计算器）
            float obstacleChance = config.obstacleSpawnChance;
            if (difficultyCalculator != null && difficultyCalculator.CurrentObstacleChance > 0f)
            {
                obstacleChance = difficultyCalculator.CurrentObstacleChance;
            }

            if (Random.value > obstacleChance)
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
            Debug.Log($"[SpawnObstacle] 开始生成障碍物 @ X={worldX}, CurrentBiome={(_biomeManager != null && _biomeManager.CurrentBiome != null ? _biomeManager.CurrentBiome.biomeName : "NULL")}");

            // 获取当前列高度，障碍物生成在草坪层上方
            int columnHeight = GetColumnHeight(worldX);

            // 优先使用群系障碍物 Tile
            TileBase biomeObstacleTile = GetBiomeObstacleTile();
            if (biomeObstacleTile != null)
            {
                Vector3Int obsPos = new Vector3Int(worldX, columnHeight, 0);
                obstacleTilemap.SetTile(obsPos, biomeObstacleTile);
                lastObstacleWorldX = worldX;

                if (enableDebugLog)
                {
                    Debug.Log($"[TilemapMapGenerator] 生成障碍物 @ X={worldX}, Y={columnHeight}, Tile={biomeObstacleTile.name}");
                }
                return;
            }

            // 回退到使用 MapConfig 中的默认障碍物 Tile
            SpawnObstacleTile(worldX, columnHeight);
        }

        /// <summary>
        /// 获取当前群系的障碍物 Tile
        /// </summary>
        /// <returns>障碍物 Tile，如果没有配置则返回 null</returns>
        private TileBase GetBiomeObstacleTile()
        {
            Debug.Log($"[GetBiomeObstacleTile] _biomeManager={(_biomeManager != null)}");
            if (_biomeManager != null)
            {
                Debug.Log($"[GetBiomeObstacleTile] CurrentBiome={(_biomeManager.CurrentBiome != null ? _biomeManager.CurrentBiome.biomeName : "NULL")}");
                if (_biomeManager.CurrentBiome != null)
                {
                    Debug.Log($"[GetBiomeObstacleTile] obstacleTiles={(_biomeManager.CurrentBiome.obstacleTiles != null ? _biomeManager.CurrentBiome.obstacleTiles.Length.ToString() : "NULL")}");
                    if (_biomeManager.CurrentBiome.obstacleTiles != null && _biomeManager.CurrentBiome.obstacleTiles.Length > 0)
                    {
                        int randomIndex = Random.Range(0, _biomeManager.CurrentBiome.obstacleTiles.Length);
                        var selected = _biomeManager.CurrentBiome.obstacleTiles[randomIndex];
                        Debug.Log($"[GetBiomeObstacleTile] 选择障碍物：{selected.name}");
                        return selected;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 生成障碍物（Tile 方式 - 向后兼容）
        /// </summary>
        private void SpawnObstacleTile(int worldX, int columnHeight)
        {
            if (obstacleTilemap == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[TilemapMapGenerator] 障碍物 Tilemap 未设置，跳过障碍物生成");
                }
                return;
            }

            if (config.obstacleTiles == null || config.obstacleTiles.Length == 0)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[TilemapMapGenerator] 障碍物 Tile 池为空，请在 MapConfig 中配置障碍物 Tile");
                }
                return;
            }

            Vector3Int obsPos = new Vector3Int(worldX, columnHeight, 0);

            // 从障碍物池中随机选择一个 Tile
            TileBase obstacleTile = config.obstacleTiles[Random.Range(0, config.obstacleTiles.Length)];

            obstacleTilemap.SetTile(obsPos, obstacleTile);
            lastObstacleWorldX = worldX;

            if (enableDebugLog)
            {
                Debug.Log($"[TilemapMapGenerator] 生成障碍物 @ X={worldX}, Y={columnHeight}, Tile={obstacleTile.name}");
            }
        }

        /// <summary>
        /// 获取指定 X 坐标的地面高度
        /// </summary>
        private int GetColumnHeight(int worldX)
        {
            // 检查是否在空隙范围内
            if (IsInGap(worldX))
            {
                return 0; // 空隙没有地面
            }

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

            // 如果清除的是空隙范围内的列，检查是否需要清除空隙状态
            if (worldX >= gapStartWorldX && worldX < gapEndWorldX)
            {
                // 当 Chunk 被清理时，如果空隙起始位置在该 Chunk 内，清除空隙状态
                if (worldX == gapStartWorldX)
                {
                    gapStartWorldX = -1;
                    gapEndWorldX = -1;
                }
            }
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

            if (enableDebugLog)
            {
                Debug.Log($"[TilemapMapGenerator] 清理 Chunk #{chunkIndex} @ X={startX}");
            }
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
            currentObstacleGap = 3; // 重置障碍物间隔为初始值

            // 清空高度缓存
            _heightCache.Clear();

            // 重置空隙状态
            gapStartWorldX = -1;
            gapEndWorldX = -1;
            lastGapEndWorldX = -999;

            if (enableDebugLog)
            {
                Debug.Log("[TilemapMapGenerator] Cleanup 完成");
            }
        }

        /// <summary>
        /// 设置地图滚动暂停状态
        /// </summary>
        /// <param name="paused">true=暂停，false=恢复</param>
        public void SetScrollPaused(bool paused)
        {
            _isScrollingPaused = paused;
            if (enableDebugLog)
            {
                Debug.Log($"[TilemapMapGenerator] 滚动已{(paused ? "暂停" : "恢复")}");
            }
        }

        /// <summary>
        /// 群系切换时的回调
        /// </summary>
        private void OnBiomeChanged(BiomeConfig newBiome)
        {
            Debug.Log($"[TilemapMapGenerator] 群系已切换为 {newBiome.biomeName}，清除已生成的障碍物并使用新配置");

            // 清除已生成的障碍物，让后续生成的障碍物使用新群系配置
            if (obstacleTilemap != null)
            {
                obstacleTilemap.ClearAllTiles();
                Debug.Log("[TilemapMapGenerator] 障碍物 Tilemap 已清空");
            }

            // 重置障碍物状态，让新障碍物立即开始生成
            lastObstacleWorldX = -999;
            currentObstacleGap = Random.Range(config.minObstacleGap, config.maxObstacleGap + 1);
        }

        /// <summary>
        /// 获取当前群系配置
        /// </summary>
        private BiomeConfig GetCurrentBiome()
        {
            if (_biomeManager != null && _biomeManager.CurrentBiome != null)
            {
                return _biomeManager.CurrentBiome;
            }
            return null;
        }

        /// <summary>
        /// 获取地表 Tile（根据群系和边界状态）
        /// </summary>
        private TileBase GetGroundTile(int worldX, bool leftExposed, bool rightExposed)
        {
            var biome = GetCurrentBiome();
            if (biome != null)
            {
                return biome.GetGrassTile(leftExposed, rightExposed);
            }
            // 回退到默认配置或原配置
            if (defaultGroundTile != null)
            {
                return defaultGroundTile;
            }

            // 根据暴露状态返回对应的 config Tile
            if (leftExposed && rightExposed)
            {
                return config.grassIsolated;
            }
            else if (leftExposed)
            {
                return config.grassLeft;
            }
            else if (rightExposed)
            {
                return config.grassRight;
            }
            return config.grassMiddle;
        }

        /// <summary>
        /// 获取土壤 Tile（根据群系和边界状态）
        /// </summary>
        private TileBase GetDirtTile(int worldX, bool leftExposed, bool rightExposed)
        {
            var biome = GetCurrentBiome();
            if (biome != null)
            {
                return biome.GetDirtTile(leftExposed, rightExposed);
            }
            // 回退到默认配置或原配置
            if (defaultDirtTile != null)
            {
                return defaultDirtTile;
            }

            // 根据暴露状态返回对应的 config Tile
            if (leftExposed && !rightExposed)
            {
                return config.dirtLeft;
            }
            else if (!leftExposed && rightExposed)
            {
                return config.dirtRight;
            }
            return config.dirtTile;
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

            // 绘制空隙范围（红色）
            if (gapStartWorldX >= 0 && gapEndWorldX > gapStartWorldX)
            {
                Gizmos.color = new Color(1, 0, 0, 0.5f); // 半透明红色
                float gapWidth = gapEndWorldX - gapStartWorldX;
                Gizmos.DrawCube(
                    new Vector3(gapStartWorldX + gapWidth / 2f, config.baseHeight / 2f, 0),
                    new Vector3(gapWidth, config.baseHeight, 1)
                );

                // 绘制空隙边界标记
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    new Vector3(gapStartWorldX, 0, 0),
                    new Vector3(gapStartWorldX, config.baseHeight + 1, 0)
                );
                Gizmos.DrawLine(
                    new Vector3(gapEndWorldX, 0, 0),
                    new Vector3(gapEndWorldX, config.baseHeight + 1, 0)
                );
            }

            // ========== 可玩性调试可视化 ==========

            // 1. 绘制可玩性区域指示器
            int startX = minGeneratedChunk * config.chunkWidth;
            int endX = maxGeneratedChunk * config.chunkWidth;

            for (int x = startX; x < endX; x++)
            {
                int currentHeight = GetColumnHeight(x);
                if (currentHeight <= 0) continue; // 跳过空隙列

                // 检查高度差警告
                if (x > 0)
                {
                    int prevHeight = GetColumnHeight(x - 1);
                    int heightDiff = Mathf.Abs(currentHeight - prevHeight);

                    if (heightDiff > config.maxHeightDifference)
                    {
                        // 高度差过大 - 绘制红色警告
                        Gizmos.color = new Color(1, 0.5f, 0, 0.7f);
                        Gizmos.DrawCube(
                            new Vector3(x, currentHeight / 2f, 0),
                            new Vector3(0.8f, currentHeight, 0.8f)
                        );
                    }
                    else if (heightDiff > 0)
                    {
                        // 有高度差但在可玩范围内 - 绘制黄色提示
                        Gizmos.color = new Color(1, 1, 0, 0.3f);
                        Gizmos.DrawCube(
                            new Vector3(x, currentHeight / 2f, 0),
                            new Vector3(0.8f, currentHeight, 0.8f)
                        );
                    }
                }

                // 检查障碍物可玩性
                if (x - lastObstacleWorldX >= config.minObstacleGap)
                {
                    // 可以生成障碍物的位置 - 绘制蓝色半透明框
                    Gizmos.color = new Color(0, 0.5f, 1, 0.2f);
                    Gizmos.DrawCube(
                        new Vector3(x, currentHeight + 0.5f, 0),
                        new Vector3(0.5f, 0.5f, 0.5f)
                    );
                }
            }

            // 2. 绘制障碍物间隔安全区域（上一个障碍物右侧）
            if (lastObstacleWorldX >= 0)
            {
                // 最小可玩间隔区域 - 绿色
                Gizmos.color = new Color(0, 1, 0, 0.1f);
                int safeZoneEnd = lastObstacleWorldX + config.minPlayableObstacleGap;
                Gizmos.DrawCube(
                    new Vector3((lastObstacleWorldX + safeZoneEnd) / 2f, config.baseHeight / 2f, 0),
                    new Vector3(config.minPlayableObstacleGap, config.baseHeight, 0.5f)
                );

                // 标记最小可玩间隔边界
                Gizmos.color = Color.green;
                Gizmos.DrawLine(
                    new Vector3(safeZoneEnd, 0, 0),
                    new Vector3(safeZoneEnd, config.baseHeight + 1, 0)
                );
            }

            // 3. 绘制空隙可玩性指示器
            if (gapStartWorldX >= 0 && gapEndWorldX > gapStartWorldX)
            {
                int gapWidth = gapEndWorldX - gapStartWorldX;

                if (gapWidth > config.maxGapWidthPlayable)
                {
                    // 空隙宽度过大 - 红色警告
                    Gizmos.color = new Color(1, 0, 0, 0.8f);
                    Gizmos.DrawCube(
                        new Vector3(gapStartWorldX + gapWidth / 2f, config.baseHeight + 2f, 0),
                        new Vector3(gapWidth, 0.5f, 0.5f)
                    );
                }
                else
                {
                    // 空隙宽度可玩 - 绿色
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    Gizmos.DrawCube(
                        new Vector3(gapStartWorldX + gapWidth / 2f, config.baseHeight + 2f, 0),
                        new Vector3(gapWidth, 0.3f, 0.3f)
                    );
                }
            }

            // 4. 显示配置参数 HUD
            Vector3 labelPos = transform.position + Vector3.up * (config.baseHeight + 5);
            UnityEditor.Handles.Label(labelPos,
                $"Gap: {config.minGapWidth}-{config.maxGapWidth} (max playable: {config.maxGapWidthPlayable}) | " +
                $"Obs Gap: {config.minObstacleGap}-{config.maxObstacleGap} (min playable: {config.minPlayableObstacleGap}) | " +
                $"Max Height Diff: {config.maxHeightDifference}");
        }
#endif
    }
}
