# 地形起伏生成 - 实施计划表

> 版本：1.0
> 创建日期：2026-03-13
> **状态：代码实施完成，待测试**

---

## 实施状态

| 阶段 | 任务数 | 状态 |
|------|--------|------|
| P1 - 配置修改 | 2 | ✓ 已完成 |
| P2 - 核心实现 | 4 | ✓ 已完成 |
| P3 - 边界处理 | 3 | ✓ 已完成 |
| P4 - 测试验证 | 4 | ⏳ 待测试 |
| **总计** | **13** | **9/13 完成** |

---

## 实施记录

### 2026-03-13 - 代码实施完成

**完成的修改**：

1. **TilemapMapConfig.cs** - 添加 9 个地形起伏参数
2. **TilemapEndlessMapGenerator.cs** - 核心实现：
   - 添加 `using System.Collections.Generic;`
   - 添加 `_heightCache` 字段
   - 添加 `GetColumnHeight()` 方法（Perlin Noise + 高度差限制）
   - 添加 `ClearHeightCache()` 方法
   - 修改 `GenerateGroundColumn()` 使用动态高度和边界判断
   - 修改 `ClearColumn()` 清理缓存
   - 修改 `SpawnObstacle()` 使用动态高度
   - 修改 `ShouldSpawnObstacle()` 禁止高度变化区域生成障碍物
   - 修改 `Initialize()` 和 `Cleanup()` 管理缓存

**编译状态**：✓ 成功，无错误

**待测试项目**：
- [ ] 在 Unity 中测试地形起伏效果
- [ ] 验证草坪边界无黑边接缝
- [ ] 验证土壤边界无黑边接缝
- [ ] 验证障碍物生成位置正确

---

## 阶段概览

| 阶段 | 任务数 | 预计工时 | 依赖 |
|------|--------|----------|------|
| P0 - 准备工作 | 3 | 0.5 小时 | - |
| P1 - 配置修改 | 2 | 0.5 小时 | P0 |
| P2 - 核心实现 | 4 | 2 小时 | P1 |
| P3 - 边界处理 | 3 | 1 小时 | P2 |
| P4 - 测试验证 | 4 | 1.5 小时 | P3 |
| **总计** | **16** | **5.5 小时** | - |

---

## P0 - 准备工作

### P0-1 | 备份当前代码
- [ ] 复制 `TilemapMapConfig.cs` 为 `TilemapMapConfig.cs.bak`
- [ ] 复制 `TilemapEndlessMapGenerator.cs` 为 `TilemapEndlessMapGenerator.cs.bak`
- [ ] 创建 Git 提交：`feat(terrain): backup before terrain variation`

### P0-2 | 确认 Tile 资源
- [ ] 检查 `grass_left`、`grass_middle`、`grass_right` 是否已配置
- [ ] 检查 `dirt_tile` 是否已配置
- [ ] 记录当前 Tile 的边界样式（确认黑边位置）

### P0-3 | 创建测试场景
- [ ] 复制 `EndlessMapTest.unity` 为 `TerrainTest.unity`
- [ ] 在测试场景中固定相机，方便观察地形

---

## P1 - 配置修改

### P1-1 | TilemapMapConfig - 新增地形起伏参数

**文件**: `Assets/Scripts/Map/TilemapMapConfig.cs`

在 `obstacleLayerY` 字段后添加：

```csharp
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
```

- [ ] 添加字段
- [ ] 保存后在 Unity 中检查 Inspector 是否显示新字段

### P1-2 | 创建测试用 MapConfig

- [ ] 在 `Assets/ScriptableObjects/Map/` 创建新配置 `TilemapMapConfig_Terrain.asset`
- [ ] 设置参数为推荐值：
  - `enableTerrainVariation`: true
  - `baseHeight`: 5
  - `minHeight`: 3
  - `maxHeight`: 7
  - `heightVariation`: 3
  - `noiseFrequency`: 0.15
  - `noiseStrength`: 1.0
  - `seed`: 12345
  - `flatChunkCount`: 3

---

## P2 - 核心实现

### P2-1 | TilemapEndlessMapGenerator - 添加高度缓存

**文件**: `Assets/Scripts/Map/TilemapEndlessMapGenerator.cs`

在类成员变量区域添加（`currentObstacleGap` 之后）：

```csharp
// 地形起伏高度缓存
private Dictionary<int, int> _heightCache = new Dictionary<int, int>();
```

- [ ] 添加缓存字段

### P2-2 | 实现 GetColumnHeight 方法

在 `SpawnObstacle` 方法后添加新方法：

```csharp
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
```

- [ ] 添加 `GetColumnHeight` 方法
- [ ] 添加 `ClearHeightCache` 方法

### P2-3 | 修改 GenerateGroundColumn 方法

**修改前**：
```csharp
private void GenerateGroundColumn(int worldX, int localX)
{
    // 1. 确定草坪类型（左/中/右）
    TileBase grassTile;
    if (localX == 0) grassTile = config.grassLeft;
    else if (localX == config.chunkWidth - 1) grassTile = config.grassRight;
    else grassTile = config.grassMiddle;

    // 2. 设置草坪层（y = config.groundHeight - 1）
    Vector3Int grassPos = new Vector3Int(worldX, config.groundHeight - 1, 0);
    // ...
}
```

**修改后**：
```csharp
private void GenerateGroundColumn(int worldX, int localX)
{
    // 1. 获取实际高度
    int columnHeight = GetColumnHeight(worldX);

    // 2. 确定草坪类型（左/中/右）
    TileBase grassTile;
    if (localX == 0) grassTile = config.grassLeft;
    else if (localX == config.chunkWidth - 1) grassTile = config.grassRight;
    else grassTile = config.grassMiddle;

    // 3. 设置草坪层（y = columnHeight - 1）
    Vector3Int grassPos = new Vector3Int(worldX, columnHeight - 1, 0);
    // ...
}
```

完整代码：

```csharp
private void GenerateGroundColumn(int worldX, int localX)
{
    // 1. 获取实际高度
    int columnHeight = GetColumnHeight(worldX);

    // 2. 确定草坪类型（左/中/右）
    TileBase grassTile;
    if (localX == 0)
    {
        grassTile = config.grassLeft;
    }
    else if (localX == config.chunkWidth - 1)
    {
        grassTile = config.grassRight;
    }
    else
    {
        grassTile = config.grassMiddle;
    }

    // 3. 设置草坪层（y = columnHeight - 1）
    Vector3Int grassPos = new Vector3Int(worldX, columnHeight - 1, 0);

    if (grassTile == null)
    {
        Debug.LogWarning($"[TilemapMapGenerator] grassTile 为 null! localX={localX}, worldX={worldX}");
        return;
    }

    groundTilemap.SetTile(grassPos, grassTile);

    // 4. 设置土壤层（y = 0 到 columnHeight - 2）
    for (int y = 0; y < columnHeight - 1; y++)
    {
        if (config.dirtTile == null)
        {
            Debug.LogWarning("[TilemapMapGenerator] dirtTile 为 null，跳过土壤层生成");
            continue;
        }

        Vector3Int dirtPos = new Vector3Int(worldX, y, 0);
        int flipMode = GetDirtFlipMode(worldX, y);
        SetTileWithFlip(dirtPos, config.dirtTile, flipMode, worldX, y);
    }
}
```

- [ ] 修改 `GenerateGroundColumn` 方法

### P2-4 | 修改 ClearColumn 方法

**修改前**：
```csharp
private void ClearColumn(int x)
{
    for (int y = 0; y < config.groundHeight; y++)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        groundTilemap.SetTile(pos, null);
    }
    // 清除障碍物...
}
```

**修改后**：
```csharp
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
```

- [ ] 修改 `ClearColumn` 方法

---

## P3 - 边界处理

### P3-1 | 修改 SpawnObstacle 方法

**修改前**：
```csharp
private void SpawnObstacle(int worldX)
{
    // ...
    int obstacleY = config.obstacleLayerY > 0 ? config.obstacleLayerY : config.groundHeight;
    Vector3Int obsPos = new Vector3Int(worldX, obstacleY, 0);
    // ...
}
```

**修改后**：
```csharp
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

    // 获取当前列高度
    int columnHeight = GetColumnHeight(worldX);
    Vector3Int obsPos = new Vector3Int(worldX, columnHeight, 0);

    // 从障碍物池中随机选择一个 Tile
    TileBase obstacleTile = config.obstacleTiles[Random.Range(0, config.obstacleTiles.Length)];

    obstacleTilemap.SetTile(obsPos, obstacleTile);
    lastObstacleWorldX = worldX;
    Debug.Log($"[TilemapMapGenerator] 生成障碍物 @ X={worldX}, Y={columnHeight}, Tile={obstacleTile.name}");
}
```

- [ ] 修改 `SpawnObstacle` 方法

### P3-2 | 修改 ShouldSpawnObstacle 方法

**修改前**：
```csharp
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
```

**修改后**：
```csharp
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
```

- [ ] 修改 `ShouldSpawnObstacle` 方法

### P3-3 | 修改初始化和清理方法

**Initialize 方法** - 添加缓存清理：
```csharp
public void Initialize()
{
    // ... 原有代码 ...

    // 清空缓存
    _heightCache.Clear();

    // ... 原有代码 ...
}
```

**Cleanup 方法** - 添加缓存清理：
```csharp
public void Cleanup()
{
    // ... 原有代码 ...

    // 清空高度缓存
    _heightCache.Clear();
}
```

- [ ] 修改 `Initialize` 方法
- [ ] 修改 `Cleanup` 方法

---

## P4 - 测试验证

### P4-1 | 编译检查

- [ ] 在 Unity 中等待编译完成
- [ ] 检查 Console 是否有编译错误
- [ ] 如有错误，根据错误信息修复

### P4-2 | 基础功能测试

- [ ] 在 Unity 中打开 `TerrainTest` 场景
- [ ] 将 `MapConfig` 替换为 `TilemapMapConfig_Terrain`
- [ ] 进入 Play 模式
- [ ] 观察地形是否有起伏
- [ ] 检查 Console 是否有错误日志

### P4-3 | 视觉验证

- [ ] 检查草坪层边界是否有黑边接缝
- [ ] 检查土壤层边界是否有黑边接缝
- [ ] 检查 Chunk 交界处是否正常
- [ ] 截图记录测试结果

### P4-4 | 参数调整测试

测试以下配置：

| 配置 | noiseFrequency | 预期效果 | 测试结果 |
|------|----------------|----------|----------|
| 平缓 | 0.05 | 大跨度平缓起伏 | [ ] |
| 标准 | 0.15 | 中等起伏 | [ ] |
| 密集 | 0.3 | 频繁小起伏 | [ ] |

- [ ] 调整参数并记录效果
- [ ] 选择最适合游戏的参数

---

## P5 - 收尾工作（可选）

### P5-1 | 代码清理

- [ ] 删除或注释掉调试日志
- [ ] 添加必要的代码注释
- [ ] 检查代码格式是否符合规范

### P5-2 | 文档更新

- [ ] 更新 `docs/game-design/MapGenerationPlan.md`
- [ ] 记录最终使用的参数配置
- [ ] 添加测试截图到文档

### P5-3 | Git 提交

- [ ] 创建提交：`feat(map): 实现地形起伏生成系统`
- [ ] 在提交信息中列出主要修改点
- [ ] 推送到远程仓库

---

## 验收清单

完成所有任务后，确认以下项目：

- [ ] 地形高度在 `[minHeight, maxHeight]` 范围内
- [ ] 相邻列高度差 ≤ 1
- [ ] 起始 N 个 Chunk 保持平坦
- [ ] 草坪层边界无黑边接缝
- [ ] 土壤层边界无黑边接缝
- [ ] 障碍物生成在草坪层上方
- [ ] 高度变化区域不生成障碍物
- [ ] 清理 Chunk 时同步清理缓存
- [ ] 无明显帧率下降
- [ ] 无内存泄漏

---

## 问题记录

| 问题描述 | 解决方案 | 日期 |
|----------|----------|------|
|          |          |      |
|          |          |      |

---

**文档版本**: v1.0
**创建日期**: 2026-03-13
**项目**: Runner's Journey
