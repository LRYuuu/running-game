# Epic 2 回顾 - 基础地图系统

**完成日期**: 2026-03-17
**状态**: 完成 (8/8 Stories)

---

## 故事完成情况

| Story | 描述 | 状态 |
|-------|------|------|
| 2-1 | 作为系统，我可以按 Chunk 为单位生成地形 | ✅ done |
| 2-2 | 作为系统，我可以动态清理远离玩家的 Chunk | ✅ done |
| 2-3 | 作为玩家，我希望地形有自然起伏 | ✅ done |
| 2-4 | 作为玩家，我希望相邻地形高度差不会太大 | ✅ done |
| 2-5 | 作为玩家，我希望游戏开始时有平坦地形 | ✅ done |
| 2-6 | 作为开发者，我希望使用种子生成地图 | ✅ done |
| 2-7 | 作为开发者，我希望有地图配置 ScriptableObject | ✅ done |
| 2-8 | 作为开发者，我希望 Tile 可以有翻转变化 | ✅ done |

---

## 技术实现总结

### 核心组件

1. **TilemapEndlessMapGenerator.cs** - 无尽地图生成器
   - 基于 Unity Tilemap 的地形生成
   - Chunk 为单位的管理（20×5 瓦片）
   - 动态生成和清理机制
   - Perlin Noise 地形起伏算法
   - 障碍物生成系统

2. **TilemapMapConfig.asset** - 地图配置 ScriptableObject
   - Chunk 尺寸：`chunkWidth = 20`, `groundHeight = 5`
   - 地形起伏：`enableTerrainVariation = true`, `heightVariation = 3`
   - Perlin Noise：`noiseFrequency = 0.15`, `noiseStrength = 1.0`
   - 种子：`seed = 12345`
   - 起始平坦：`flatChunkCount = 2`
   - 障碍物配置：`obstacleSpawnChance = 0.3`, `minObstacleGap = 3`, `maxObstacleGap = 8`

3. **BackgroundManager.cs** + **ParallaxBackground.cs** - 三层视差背景系统
   - Far Background: 0.2x 速度
   - Mid Background: 0.5x 速度
   - Near Background: 1.0x 速度

### 关键功能实现

#### 1. Chunk 管理系统

```csharp
// 保持固定的 Chunk 总数为 4 个
int targetTotalChunks = 4;
// 右侧生成新 Chunk，左侧清理旧 Chunk
```

#### 2. 地形起伏算法

```csharp
// 使用 Perlin Noise 计算高度
float noiseX = (worldX + config.seed) * config.noiseFrequency;
float noiseValue = Mathf.PerlinNoise(noiseX, 0);
int targetHeight = Mathf.FloorToInt(config.baseHeight + noiseOffset);

// 限制高度差 ≤ 1
if (targetHeight > prevHeight + 1) targetHeight = prevHeight + 1;
else if (targetHeight < prevHeight - 1) targetHeight = prevHeight - 1;
```

#### 3. Tile 翻转效果

```csharp
// 基于位置计算翻转模式，创造视觉变化
int hash = Mathf.Abs((x * 17 + y * 31) % 4);
// 0=无翻转，1=水平，2=垂直，3=旋转 180 度
```

#### 4. 草坪自动匹配

```csharp
// 根据左右列高度差选择正确的草坪 Tile
bool leftExposed = (leftHeight < columnHeight);
bool rightExposed = (rightHeight < columnHeight);
// 自动选择 grassLeft / grassMiddle / grassRight
```

---

## 经验教训

### 做得好的

1. **程序化生成算法** - Perlin Noise 产生自然起伏，可重复性强
2. **高度差限制** - 确保相邻地形高度差≤1，保证可玩性
3. **起始保护区** - 前 2 个 Chunk 保持平坦，让玩家熟悉操作
4. **Tile 翻转系统** - 简单算法实现视觉多样性
5. **ScriptableObject 配置** - 所有参数可配置，便于调优

### 需要改进的

1. **高度缓存管理** - `_heightCache` 使用字典存储，清理逻辑可以更高效
2. **障碍物配置分离** - 当前障碍物生成逻辑与地图生成器耦合，建议分离为独立系统
3. **Chunk 预加载** - 当前只保留 4 个 Chunk，可以考虑根据玩家速度动态调整

### 踩过的坑

1. **地图偏移计算** - 初始 `_mapOffset` 和 `initialMapOffset` 概念混淆，导致玩家位置不正确
2. **Chunk 索引计算** - `ClearChunk` 方法中 Chunk 索引与世界坐标转换容易出错
3. **Tile 翻转实现** - 需要创建新的 Tile 实例来应用 transform，不能直接修改原 Tile
4. **草坪边界判断** - 需要根据左右列高度动态选择正确的边界 Tile

---

## Git 提交历史

| Commit | 说明 |
|--------|------|
| `e8e5309` | feat(map): 实现地形起伏生成系统 |
| `89991c3` | fix(map): 修复玩家位置与地图初始偏移 |
| `17fefee` | fix(map): 调整地图和玩家初始位置 |
| `f49f7f9` | feat(map): 实现无尽地图滚动系统 |

---

## 代码统计

| 文件 | 行数 | 说明 |
|------|------|------|
| `TilemapEndlessMapGenerator.cs` | ~540 行 | 主地图生成器 |
| `TilemapMapConfig.cs` | ~85 行 | 配置类 |
| `BackgroundManager.cs` | ~165 行 | 背景管理器 |
| `ParallaxBackground.cs` | ~130 行 | 视差背景组件 |

---

## 与 Epic 1 的集成

Epic 2 完成后，地图系统可以与 Epic 1 的玩家控制器无缝配合：

- ✅ 地图向左滚动，玩家位置固定
- ✅ 地面检测正常工作
- ✅ 跳跃机制在起伏地形上正常
- ✅ 障碍物已预留生成逻辑（待 Epic 3 实现碰撞）

---

## 下一步计划

Epic 2 完成后，地图系统已经具备完整的核心功能：
- ✅ Chunk 为基础的无尽生成
- ✅ 动态清理优化性能
- ✅ 自然起伏的地形
- ✅ 可配置的所有参数

接下来进入 **Epic 3: 障碍物与碰撞**，实现：
- 障碍物预制体和生成逻辑
- 空隙（Gap）生成
- 玩家与障碍物碰撞检测
- 死亡和重生逻辑

---

## 参考资源

- [Unity Tilemap 官方文档](https://docs.unity3d.com/Manual/class-Tilemap.html)
- [Perlin Noise 地形生成教程](https://catlikecoding.com/unity/tutorials/procedural-maps/perlin-noise/)
- [2D 平台跳跃地图设计最佳实践](https://learn.unity.com/project/2d-platformer-tutorial)
