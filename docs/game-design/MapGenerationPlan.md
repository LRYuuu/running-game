# 2D 横板跳跃地图生成方案

## 1. 概述

### 游戏配置
| 特性 | 配置 |
|------|------|
| 游戏类型 | 2D 横板跳跃 |
| 相机方案 | 地图向左滚动（玩家 X 轴移动） |
| 前进方向 | X 轴正方向 |
| 技术方案 | Unity Tilemap |

---

## 2. Tile 配置

### 2.1 草坪 Tile（3 个，顶层 y=groundHeight-1）

| Tile 名称 | 用途 | 边界 |
|----------|------|------|
| `grass_left` | Chunk 左边界 | 左侧黑边 |
| `grass_middle` | Chunk 中间 | 无缝衔接 |
| `grass_right` | Chunk 右边界 | 右侧黑边 |

### 2.2 土壤 Tile（1 个，地下层 y=0~groundHeight-2）

| Tile 名称 | 用途 | 动态效果 |
|----------|------|----------|
| `dirt_tile` | 土壤颗粒样式 | 通过 Transform 翻转创造 4 种变化 |

**技术方案：**
- 只使用 1 个土壤 Tile 资产
- 生成时通过 Tile 的 Transform 矩阵实现翻转
- 翻转模式：无翻转、水平翻转、垂直翻转、水平 + 垂直翻转
- 根据位置哈希自动选择翻转模式，营造视觉变化

### 2.3 障碍物（待配置）

放置在草坪层上方（y=groundHeight），需要碰撞体。

---

## 3. 地图结构

```
Y 轴（高度）
↑
│                    空气（玩家活动区）
│  障碍物？
│     ▲
│     │
├─────┴─────┴─────┴─────┤  ← 草坪层 (y=groundHeight-1)
│ grass_left │ grass_mid │ ... │
│  (黑边)    │  (无缝)   │     │
├────────────┼───────────┼─────┤
│  dirt_翻转  │  dirt_翻转 │ ... │  ← 土壤层 (y=0~groundHeight-2)
│  (翻转效果) │  (翻转效果) │     │
└────────────┴───────────┴─────┘
     X=0        X=1       X=2
```

### Chunk 拼接

```
Chunk 0          Chunk 1          Chunk 2
┌─────┬─────┐   ┌─────┬─────┐   ┌─────┬─────┐
│ L │ M │ R │ ║ │ L │ M │ R │ ║ │ L │ M │ R │
└───┴─────┴───┘   └───┴─────┴───┘   └───┴─────┴───┘
    黑边║黑边        黑边║黑边
    Chunk 交界       Chunk 交界
```

---

## 4. 生成规则

### 4.1 地面列生成

```csharp
// 每列从下到上
for (int y = 0; y < groundHeight; y++)
{
    if (y == groundHeight - 1)
        // 草坪层
        tile = grass_left/middle/right;
    else
        // 土壤层（通过翻转创造不同效果）
        tile = dirt_tile;
        flipMode = (x * 17 + y * 31) % 4;  // 4 种翻转模式
        tile.transform = GetFlipMatrix(flipMode);

    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
}
```

**翻转模式说明：**
- `0` = 无翻转
- `1` = 水平翻转
- `2` = 垂直翻转
- `3` = 水平 + 垂直翻转（旋转 180 度）

### 4.2 空隙生成

```csharp
// 空隙配置
minGapStart = 5;      // 前 5 个 Chunk 不生成空隙
gapChance = 0.1f;     // 10% 概率生成空隙
minGapWidth = 1;      // 最小空隙宽度
maxGapWidth = 3;      // 最大空隙宽度
```

### 4.3 障碍物生成

```csharp
// 障碍物配置
obstacleSpawnChance = 0.3f;  // 30% 概率
minObstacleGap = 3;          // 最小间隔
maxObstacleGap = 8;          // 最大间隔
```

---

## 5. 代码文件

### 5.1 TilemapMapConfig.cs

```csharp
[CreateAssetMenu(fileName = "TilemapMapConfig", menuName = "Runner's Journey/Tilemap Map Config")]
public class TilemapMapConfig : ScriptableObject
{
    // Chunk 尺寸
    public int chunkWidth = 20;
    public int groundHeight = 5;

    // 生成配置
    public int aheadChunkCount = 3;
    public int behindChunkCount = 2;

    // Tile 资产
    public TileBase grassLeft;
    public TileBase grassMiddle;
    public TileBase grassRight;
    public TileBase dirtTile;  // 单个土壤 Tile，通过翻转创造变化

    // 障碍物配置
    public TileBase[] obstacleTiles;  // 障碍物 Tile 池
    public float obstacleSpawnChance = 0.3f;
    public int minObstacleGap = 3;
    public int maxObstacleGap = 8;
    public int obstacleLayerY = 5;
}
```

### 5.2 TilemapEndlessMapGenerator.cs

主要功能：
- 沿 X 轴动态生成 Chunk
- 随机生成空隙（Gap）
- 随机生成障碍物
- 清理后方 Chunk 优化性能

---

## 6. Unity 场景设置

### 6.1 Hierarchy 结构

```
Hierarchy:
├── MapGenerator (GameObject)
│   └── TilemapEndlessMapGenerator (组件)
│       - groundTilemap: Ground Tilemap
│       - obstacleTilemap: Obstacles Tilemap
│       - config: TilemapMapConfig 资产
│       - playerTransform: Player
├── Grid (Tilemap 根节点)
│   ├── Ground (Tilemap)
│   │   - Tilemap Collider 2D
│   │   - Tilemap Renderer
│   └── Obstacles (Tilemap)
│       - Tilemap Collider 2D
│       - Tilemap Renderer
├── Player
│   ├── SpriteRenderer
│   ├── Rigidbody2D
│   └── BoxCollider2D
└── Main Camera
    └── Camera (跟随玩家 X 轴)
```

### 6.2 创建 MapConfig 资产

1. 在 Project 窗口右键：`Assets > Create > Runner's Journey > Tilemap Map Config`
2. 命名：`MapConfig`
3. 配置参数：
   - `chunkWidth`: 20
   - `groundHeight`: 5
   - `grassLeft`: 拖入左边界草坪 Tile
   - `grassMiddle`: 拖入中间草坪 Tile
   - `grassRight`: 拖入右边界草坪 Tile
   - `dirtTile`: 拖入单个土壤 Tile（会自动通过翻转创造变化）
   - `obstacleTiles`: 拖入障碍物 Tile 池（可选）
   - `aheadChunkCount`: 3
   - `behindChunkCount`: 2

---

## 7. 使用步骤

### 7.1 准备 Tile 资产

1. 创建/导入以下 Tile：
   - `grass_left`、`grass_middle`、`grass_right`
   - `dirt_tile`（单个土壤 Tile，通过翻转创造 4 种变化）

### 7.2 创建 MapConfig

1. 右键创建 `TilemapMapConfig` 资产
2. 分配 Tile 引用

### 7.3 设置场景

1. 创建空 GameObject `MapGenerator`
2. 添加 `TilemapEndlessMapGenerator` 组件
3. 分配 `groundTilemap`、`config`、`playerTransform`

### 7.4 运行测试

1. 将玩家放置在起点附近（x=2, y=groundHeight）
2. 进入 Play 模式
3. 玩家向右移动，观察地图生成

---

## 8. 调试

### 8.1 控制台日志

```
[TilemapMapGenerator] 初始化 - Chunk 宽度：20, 地面高度：5
[TilemapMapGenerator] 生成 Chunk #0 @ X=0
[TilemapMapGenerator] 生成 Chunk #1 @ X=20
[TilemapMapGenerator] 生成空隙：25 ~ 28 (宽度：3)
[TilemapMapGenerator] 清理 Chunk #0 @ X=0
```

### 8.2 Gizmos 调试

在 Scene 视图中：
- 黄色线：Chunk 边界
- 绿色线：地面高度

---

## 9. 待完成事项

### 障碍物系统

1. 创建障碍物 Tile（或 GameObject Prefab）
2. 在 `TilemapEndlessMapGenerator.cs` 中添加障碍物 Tile 配置
3. 实现 `SpawnObstacle` 方法中的 Tile 选择逻辑

### 扩展功能

- [ ] 多种障碍物类型（石头、尖刺、木箱等）
- [ ] 障碍物动画（如移动平台）
- [ ] 收集品（金币、道具等）
- [ ] 难度曲线（距离越远障碍越密集）

---

## 10. 相关文档

- [地形起伏生成方案](./TerrainVariationPlan.md) - 地势高度变化的详细设计方案

---

*文档版本：3.0（单个土壤 Tile 通过翻转创造变化）*
*最后更新：2026-03-13*
*项目：Runner's Journey*
