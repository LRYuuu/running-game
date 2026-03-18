# Story 3-6: 作为开发者，我希望有障碍物配置 ScriptableObject，以便调整参数

**Epic:** Epic 3 - 障碍物与碰撞
**优先级:** P1
**估算:** 1h
**状态:** Done

---

## 任务列表

- [x] 1. 在 TilemapMapConfig 中添加障碍物配置参数
- [x] 2. 配置障碍物 Tile 池支持
- [x] 3. 更新 MapConfig.asset 配置文件
- [x] 4. 文档化配置参数说明

---

## 实现摘要

### 配置文件位置

**文件:** `Assets/Scripts/Map/TilemapMapConfig.cs`

### 障碍物配置参数

```csharp
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
```

### 参数说明

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `obstacleTiles` | TileBase[] | - | 障碍物 Tile 池，从中随机选择生成 |
| `obstacleSpawnChance` | float | 0.3f | 生成概率 (0-1)，0 表示从不生成，1 表示每次满足条件时都生成 |
| `minObstacleGap` | int | 3 | 障碍物之间的最小间隔（瓦片数量） |
| `maxObstacleGap` | int | 8 | 障碍物之间的最大间隔（瓦片数量） |
| `obstacleLayerY` | int | 5 | 障碍物在 Y 轴上的生成层，通常等于 groundHeight |

### MapConfig.asset 配置示例

```yaml
obstacleTiles:
  - {fileID: 11400000, guid: cc23b4c4f5f2c7849ac5161102d10fe7, type: 2}
  - {fileID: 11400000, guid: 5c1aeb9d3f21f434588b0d1e6c6e3bd9, type: 2}
  - {fileID: 11400000, guid: 31e28ed86a5cd6e4c9214f745703f855, type: 2}
obstacleSpawnChance: 0.3
minObstacleGap: 3
maxObstacleGap: 8
obstacleLayerY: 5
```

---

## 验收标准

- [x] TilemapMapConfig 包含障碍物生成相关配置参数
- [x] 配置参数有清晰的 Tooltip 说明
- [x] 配置文件 MapConfig.asset 包含示例值
- [x] 开发者可以通过 Inspector 调整参数
- [x] 参数变化会实时影响游戏中的障碍物生成

---

## 备注

Story 3-6 的配置系统在实现 Story 3-1 时已同步完成。所有障碍物生成参数都通过 ScriptableObject 配置，开发者可以在 Unity Inspector 中直接调整，无需修改代码。
