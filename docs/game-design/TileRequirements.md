# Tile 资产需求清单（2D 横板跳跃）

## 1. Tile 配置总览

### 1.1 地面结构

```
单个地面列的垂直结构（从下到上）：

Y 轴
↑
│  空气（玩家活动区域）
│  ────────────────────  ← 地表面（草坪顶层）
│  草坪层（1 格高）       ← 草皮，有装饰物
│  ────────────────────
│  土壤层 1              ← 单个土壤 Tile 通过翻转创造变化
│  土壤层 2              ← 翻转模式：无/水平/垂直/180 度旋转
│  土壤层 3
│  土壤层 4
│  ...（向下延伸）
└────────────────────────→ X 轴

水平结构（每排）：
┌─────┬─────┬─────┐
│ 左  │ 中  │ 右  │  ← 草坪层
│黑边 │无缝 │黑边 │
├─────┼─────┼─────┤
│ 土壤（翻转效果）│
└─────┴─────┴─────┘
```

---

## 2. Tile 清单

### 2.1 草坪 Tile（3 个，顶层）

| Tile 名称 | 用途 | 碰撞体 | 示意图 |
|----------|------|--------|--------|
| `grass_left` | 地面左边界 | 实心 | `▓▒░`（左侧黑边） |
| `grass_middle` | 地面中间 | 实心 | `░░░`（无缝衔接） |
| `grass_right` | 地面右边界 | 实心 | `░▒▓`（右侧黑边） |

**用途说明：**
- 左中右用于水平方向的拼接
- 黑边用于地块之间的视觉分界
- 每个草坪 Tile 上可以放置装饰性障碍物

### 2.2 土壤 Tile（1 个，地下层）

| Tile 名称 | 用途 | 碰撞体 | 动态效果 |
|----------|------|--------|----------|
| `dirt_tile` | 土壤颗粒样式 | 实心 | 4 种翻转模式 |

**技术方案：**
- 只使用 1 个 Tile 资产
- 通过 Transform 矩阵实现翻转效果
- 翻转模式：
  - 模式 0：无翻转
  - 模式 1：水平翻转（左右镜像）
  - 模式 2：垂直翻转（上下镜像）
  - 模式 3：水平 + 垂直翻转（旋转 180 度）
- 根据位置哈希自动选择翻转模式，公式：`(x * 17 + y * 31) % 4`
- 视觉上营造 4 种不同土壤的变化效果

### 2.3 障碍物 Tile（地面上的装饰，带碰撞）

| Tile 名称 | 用途 | 碰撞体 | 示意图 |
|----------|------|--------|--------|
| `obstacle_stone` | 石头障碍 | 实心 | `▲` |
| `obstacle_bush` | 草丛障碍 | 实心 | `♨` |
| `obstacle_box` | 木箱障碍 | 实心 | `■` |
| `obstacle_spike` | 尖刺障碍（致死） | 实心 | `△` |
| `obstacle_log` | 圆木障碍 | 实心 | `▄` |

**用途说明：**
- 放置在草坪层上方
- 玩家需要跳跃躲避
- 尖刺为致死障碍，其他为碰撞障碍

---

## 3. 边界情况详解

### 3.1 地面左右边界

```
情况 1: 地面左边界（起点）

    空气  空气  空气
    │     │     │
    │     │     │
    ┌─────┴─────┴─────┐
    │grass_left(黑边) │
    ├─────────────────┤
    │   dirt_随机      │
    ├─────────────────┤
    │   dirt_随机      │
    └─────────────────┘

情况 2: 地面右边界（无限延伸的前方）

    空气  空气  空气
    │     │     │
    │     │     │
    ┌─────┬─────┬─────┐
    │grass_right(黑边)│
    ├─────────────────┤
    │   dirt_随机      │
    ├─────────────────┤
    │   dirt_随机      │
    └─────────────────┘
```

### 3.2 草坪拼接规则

```
水平拼接（从左到右）：

Chunk 边界：
... │ grass_right ║ grass_left │ ...
    │   (黑边)    ║   (黑边)    │
    └─────────────┴─────────────┘
         Chunk N         Chunk N+1

Chunk 内部：
┌─────┬─────┬─────┬─────┐
│ left│ mid │ mid │right│
│黑边 │无缝 │无缝 │黑边 │
└─────┴─────┴─────┴─────┘
```

### 3.3 空隙（Gap）情况

```
情况 1: 小空隙（1 格宽）

    空气  空气  空气  空气
    │     │     │     │
───┘     │     │     └───
grass   空气  空气  grass
 dirts   无    无    dirts

情况 2: 大空隙（多格宽，需要跳跃）

    空气  空气  空气  空气  空气
    │     │     │     │     │
───┘     │     │     │     └───
grass   空气  空气  空气  grass
 dirts   无    无    无    dirts
         ←─── 跳跃 ───→
```

### 3.4 障碍物放置规则

```
障碍物只能放置在草坪层上方：

    空气  空气  空气
    │     │     │
    │    stone  │  ← 障碍物（带碰撞）
    │     ▲     │
    ┌─────┴─────┬─────┐
    │ grass_mid │grass│
    ├───────────┼─────┤
    │  dirt_随机 │ ... │
    └───────────┴─────┘

障碍物的碰撞体：
- 使用 BoxCollider2D 或 PolygonCollider2D
- 与玩家 collider 触发跳跃检测
```

---

## 4. 生成规则

### 4.1 地面列生成逻辑

```csharp
// 每列地面生成
for (int x = 0; x < chunkWidth; x++)
{
    // 1. 确定草坪类型（左/中/右）
    TileBase grassTile;
    if (x == 0)
        grassTile = grass_left;    // 左边界
    else if (x == chunkWidth - 1)
        grassTile = grass_right;   // 右边界
    else
        grassTile = grass_middle;  // 中间

    // 2. 设置草坪层（y = groundHeight - 1）
    tilemap.SetTile(new Vector3Int(x, groundHeight - 1, 0), grassTile);

    // 3. 设置土壤层（y = 0 到 groundHeight - 2）
    for (int y = 0; y < groundHeight - 1; y++)
    {
        // 使用单个 Tile 通过翻转创造变化
        int flipMode = (x * 17 + y * 31) % 4;
        var flippedTile = CreateFlippedTile(dirt_tile, flipMode);
        tilemap.SetTile(new Vector3Int(x, y, 0), flippedTile);
    }
}

// 创建翻转 Tile
Tile CreateFlippedTile(TileBase original, int flipMode)
{
    var tile = ScriptableObject.CreateInstance<Tile>();
    tile.sprite = original.sprite;

    Matrix4x4 transform = Matrix4x4.identity;
    if (flipMode == 1) transform = Matrix4x4.Scale(new Vector3(-1, 1, 1));      // 水平
    else if (flipMode == 2) transform = Matrix4x4.Scale(new Vector3(1, -1, 1));  // 垂直
    else if (flipMode == 3) transform = Matrix4x4.Scale(new Vector3(-1, -1, 1)); // 180 度
    tile.transform = transform;

    return tile;
}
```

### 4.2 空隙生成逻辑

```csharp
// 生成空隙时，清除对应位置的 Tile
for (int x = gapStart; x < gapEnd; x++)
{
    // 清除草坪层
    tilemap.SetTile(new Vector3Int(x, groundHeight - 1, 0), null);

    // 清除土壤层
    for (int y = 0; y < groundHeight; y++)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), null);
    }
}
```

### 4.3 障碍物生成逻辑

```csharp
// 在草坪层上方生成障碍物
if (ShouldSpawnObstacle(x))
{
    Vector3 spawnPos = new Vector3(x, groundHeight, 0);
    GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);

    // 障碍物需要碰撞体
    obstacle.AddComponent<BoxCollider2D>();
}
```

---

## 5. 最小可用集合

| 优先级 | Tile 名称 | 数量 | 用途 |
|--------|----------|------|------|
| **P0** | `grass_left` | 1 | 左边界草坪 |
| **P0** | `grass_middle` | 1 | 中间草坪 |
| **P0** | `grass_right` | 1 | 右边界草坪 |
| **P0** | `dirt_tile` | 1 | 土壤样式（通过翻转创造 4 种变化） |
| **P1** | `obstacle_stone` | 1 | 石头障碍 |
| **P1** | `obstacle_spike` | 1 | 尖刺障碍 |

**Tile 总数：最少 5 个 Tile 即可开始游戏**

---

## 6. Hierarchy 结构建议

```
Hierarchy:
├── MapGenerator (GameObject)
│   └── TilemapEndlessMapGenerator (组件)
├── Grid (Tilemap 根节点)
│   ├── Ground (Tilemap)
│   │   - 草坪层 + 土壤层
│   │   - Tilemap Collider 2D
│   │   - Tilemap Renderer
│   └── Obstacles (Tilemap)
│       - 障碍物层
│       - Tilemap Collider 2D
│       - Tilemap Renderer
├── Player
│   ├── SpriteRenderer
│   ├── Rigidbody2D
│   └── BoxCollider2D
└── Camera (固定位置，跟随玩家 X 轴)
```

---

## 7. 美术规格建议

| 属性 | 建议值 |
|------|--------|
| Tile 尺寸 | 128x128 px（或 64x64） |
| 单位 | 1 Tile = 1 世界单位 |
| 草坪黑边 | 8-16 px 宽（用于地块分界） |
| 土壤颗粒 | 单个 Tile，翻转后视觉差异明显 |
| 碰撞体 | Tilemap Collider 2D |
| 物理材质 | Physics Material 2D（摩擦力 0.4，弹力 0） |

**土壤 Tile 设计建议：**
- 土壤颗粒图案应该是非对称的，这样翻转后能产生明显的视觉差异
- 避免中心对称的图案，否则翻转效果不明显

---

*文档版本：3.0（单个土壤 Tile 通过翻转创造变化）*
*创建日期：2026-03-12*
*最后更新：2026-03-13*
*项目：Runner's Journey*
