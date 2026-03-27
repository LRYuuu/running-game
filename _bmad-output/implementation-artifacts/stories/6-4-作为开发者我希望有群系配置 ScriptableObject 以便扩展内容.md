# Story 6-4: 作为开发者，我希望有群系配置 ScriptableObject，以便扩展内容

**Epic:** Epic 6 - 地图生成扩展
**优先级:** P0
**估时:** 2h
**状态:** done

---

## Story

作为开发者，
我希望有群系配置 ScriptableObject，
以便扩展内容。

---

## Acceptance Criteria

### 功能验收标准

1. **群系配置可扩展**
   - Given 新的群系需求（如丛林、火山等）
   - When 开发者创建新的 BiomeConfig 资产
   - Then 无需修改代码即可支持新群系

2. **配置参数完整**
   - Given BiomeConfig ScriptableObject
   - When 开发者在 Inspector 中查看
   - Then 可以配置所有必要的视觉和游戏元素

3. **群系序列支持**
   - Given 需要群系随距离自动切换
   - When 开发者创建 BiomeSequence 资产
   - Then 可以配置多个群系阶段和切换距离

4. **默认回退机制**
   - Given 未指定群系配置或配置丢失
   - When 游戏运行时
   - Then 系统回退到默认群系（草坪）

### 技术验收标准

1. **ScriptableObject 规范**
   - Given BiomeConfig 和 BiomeSequence
   - When 使用 CreateAssetMenu 创建
   - Then 遵循项目命名和目录规范

2. **配置验证**
   - Given 开发者在 Inspector 中配置
   - When 配置值为空或不合理
   - Then 有 Tooltip 提示和默认值保护

3. **运行时验证**
   - Given 游戏启动
   - When 加载群系配置
   - Then 验证配置完整性并记录日志

4. **调试支持**
   - Given 开发模式下运行
   - When 群系切换时
   - Then 在 Console 中输出调试信息

---

## Tasks / Subtasks

### Task 1: 验证 BiomeConfig 扩展性 (AC: 1, 2)
- [ ] 检查 BiomeConfig.cs 是否包含所有必要字段
- [ ] 确认字段包括：
  - 基本信息：biomeName
  - Tile 配置：groundTile, dirtTile, groundDecorationTile
  - 视觉配置：skyColor, backgroundColor
  - 障碍物：obstaclePrefabs[]
- [ ] 验证 [CreateAssetMenu] 路径正确
- [ ] 验证所有字段有 [Tooltip] 注释

### Task 2: 验证 BiomeSequence 扩展性 (AC: 3)
- [ ] 检查 BiomeSequence.cs 是否支持多阶段配置
- [ ] 确认 BiomeStage 结构包含：
  - biome: BiomeConfig 引用
  - transitionDistance: 切换距离阈值
- [ ] 验证 useSequence 开关功能
- [ ] 测试添加/删除群系阶段

### Task 3: 创建示例群系配置 (AC: 1, 4)
- [ ] 创建 GrasslandBiome.asset - 草坪群系（默认）
- [ ] 创建 DesertBiome.asset - 沙漠群系
- [ ] 创建 SnowlandBiome.asset - 雪地群系
- [ ] 创建 FullSequence.asset - 完整序列（草地→沙漠→雪地）
- [ ] 在 Inspector 中配置所有参数

### Task 4: 配置验证和回退 (AC: 4)
- [ ] 在 BiomeManager 中添加配置验证
- [ ] 实现默认群系回退逻辑
- [ ] 添加配置丢失时的警告日志
- [ ] 确保不会因配置问题导致崩溃

### Task 5: 调试和日志 (AC: 4)
- [ ] 在 BiomeManager 中添加详细日志
- [ ] 群系切换时输出新旧群系名称
- [ ] 在开发模式下显示当前配置状态
- [ ] 记录配置验证结果

---

## Architecture & Design

### 群系配置系统架构

```
┌────────────────────────────────────────────────────────────┐
│                   群系配置系统                              │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  ┌──────────────────────────┐                             │
│  │   BiomeConfig            │                             │
│  │   (ScriptableObject)     │                             │
│  ├──────────────────────────┤                             │
│  │ biomeName: string        │◄─── 群系名称                │
│  │ groundTile: TileBase     │◄─── 地表 Tile               │
│  │ dirtTile: TileBase       │◄─── 土壤 Tile               │
│  │ skyColor: Color          │◄─── 天空颜色                │
│  │ backgroundColor: Color   │◄─── 背景颜色                │
│  │ obstaclePrefabs[]: GameObject     │◄─── 障碍物列表    │
│  └──────────────────────────┘                             │
│           │                                                │
│           │ referenced by                                 │
│           ▼                                                │
│  ┌──────────────────────────┐                             │
│  │   BiomeStage             │                             │
│  ├──────────────────────────┤                             │
│  │ biome: BiomeConfig       │                             │
│  │ transitionDistance: float│                             │
│  └──────────────────────────┘                             │
│           │                                                │
│           │ contained in                                  │
│           ▼                                                │
│  ┌──────────────────────────┐                             │
│  │   BiomeSequence          │                             │
│  │   (ScriptableObject)     │                             │
│  ├──────────────────────────┤                             │
│  │ sequenceName: string     │                             │
│  │ biomeStages[]: List      │                             │
│  │ useSequence: bool        │                             │
│  └──────────────────────────┘                             │
│                                                            │
│  ┌─────────────────────────────────────────────────────┐  │
│  │              BiomeConfig Assets                     │  │
│  │  - GrasslandBiome.asset (默认)                      │  │
│  │  - DesertBiome.asset                                │  │
│  │  - SnowlandBiome.asset                              │  │
│  │  - JungleBiome.asset (扩展示例)                     │  │
│  │  - VolcanoBiome.asset (扩展示例)                    │  │
│  └─────────────────────────────────────────────────────┘  │
│  ┌─────────────────────────────────────────────────────┐  │
│  │           BiomeSequence Assets (可选)               │  │
│  │  - FullSequence.asset (草地→沙漠→雪地)              │  │
│  │  - ShortSequence.asset (草地→沙漠)                  │  │
│  │  - CustomSequence.asset (自定义序列）               │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### 扩展新群系的步骤

```
1. 创建新的 BiomeConfig 资产
   │
   ├─ 右键点击 Assets/Resources/Biomes 文件夹
   │
   ├─ 选择 Create → Runner's Journey → Biome Config
   │
   ├─ 命名为 NewBiome.asset
   │
   └─ 在 Inspector 中配置参数

2. 配置群系参数
   │
   ├─ biomeName: "丛林"
   │
   ├─ groundTile: 引用丛林地表 Tile
   │
   ├─ dirtTile: 引用丛林土壤 Tile
   │
   ├─ skyColor: 设置丛林天空颜色
   │
   ├─ backgroundColor: 设置丛林背景颜色
   │
   └─ obstaclePrefabs: 添加丛林特有的障碍物

3. （可选）添加到群系序列
   │
   ├─ 打开 BiomeSequence 资产
   │
   ├─ 在 biomeStages 列表中添加新阶段
   │
   ├─ 设置新的 biome 和 transitionDistance
   │
   └─ 保存配置

4. 验证
   │
   ├─ 运行游戏
   │
   ├─ 选择新群系或使用序列
   │
   └─ 验证视觉效果正常
```

### 配置文件位置

| 文件 | 路径 | 状态 |
|------|------|------|
| `BiomeConfig.cs` | `Assets/Scripts/Map/BiomeConfig.cs` | ✅ 已创建 (Story 6-1) |
| `BiomeSequence.cs` | `Assets/Scripts/Map/BiomeSequence.cs` | ✅ 已创建 (Story 6-1) |
| `BiomeManager.cs` | `Assets/Scripts/Map/BiomeManager.cs` | ✅ 已创建 (Story 6-1) |
| `GrasslandBiome.asset` | `Assets/Resources/Biomes/GrasslandBiome.asset` | ✅ 已创建 |
| `DesertBiome.asset` | `Assets/Resources/Biomes/DesertBiome.asset` | ✅ 已创建 |
| `SnowlandBiome.asset` | `Assets/Resources/Biomes/SnowlandBiome.asset` | ✅ 已创建 |
| `FullSequence.asset` | `Assets/Resources/Biomes/Sequences/FullSequence.asset` | ⬜ 待创建 |

### BiomeConfig 字段说明

```csharp
[CreateAssetMenu(fileName = "NewBiome",
                 menuName = "Runner's Journey/Biome Config",
                 order = 1)]
public class BiomeConfig : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("群系名称，用于 UI 显示和调试")]
    public string biomeName = "Grassland";

    [Header("地形 Tile")]
    [Tooltip("用于地表层的 Tile")]
    public TileBase groundTile;

    [Tooltip("用于地表上层的装饰 Tile（可选）")]
    public TileBase groundDecorationTile;

    [Tooltip("用于填充地下的 Tile")]
    public TileBase dirtTile;

    [Header("视觉配置")]
    [Tooltip("天空颜色")]
    public Color skyColor = Color.cyan;

    [Tooltip("背景颜色")]
    public Color backgroundColor = Color.green;

    [Header("障碍物")]
    [Tooltip("该群系特有的障碍物预制体列表")]
    public GameObject[] obstaclePrefabs;
}
```

### BiomeSequence 字段说明

```csharp
[CreateAssetMenu(fileName = "NewBiomeSequence",
                 menuName = "Runner's Journey/Biome Sequence",
                 order = 2)]
public class BiomeSequence : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("序列名称")]
    public string sequenceName = "FullProgression";

    [Header("群系阶段")]
    [Tooltip("群系阶段列表（按距离排序）")]
    public List<BiomeStage> biomeStages = new List<BiomeStage>();

    [Header("模式开关")]
    [Tooltip("是否启用序列模式，false 则使用固定单一群系")]
    public bool useSequence = true;
}

[System.Serializable]
public class BiomeStage
{
    [Tooltip("该阶段使用的群系配置")]
    public BiomeConfig biome;

    [Tooltip("切换到下一个群系的距离阈值（米）")]
    public float transitionDistance = 200f;
}
```

### 配置验证逻辑

```csharp
// BiomeManager.cs 中的验证逻辑
private void ValidateBiomeConfig(BiomeConfig config)
{
    if (config == null)
    {
        Debug.LogWarning("[BiomeManager] 群系配置为 null，使用默认值");
        return;
    }

    if (string.IsNullOrEmpty(config.biomeName))
    {
        Debug.LogWarning("[BiomeManager] 群系名称为空");
    }

    if (config.groundTile == null)
    {
        Debug.LogWarning($"[BiomeManager] 群系'{config.biomeName}'未配置地表 Tile");
    }

    if (config.dirtTile == null)
    {
        Debug.LogWarning($"[BiomeManager] 群系'{config.biomeName}'未配置土壤 Tile");
    }

    if (config.obstaclePrefabs == null || config.obstaclePrefabs.Length == 0)
    {
        Debug.Log($"[BiomeManager] 群系'{config.biomeName}'未配置障碍物，将使用默认障碍物");
    }
}

private void ValidateBiomeSequence(BiomeSequence sequence)
{
    if (sequence == null)
    {
        Debug.LogWarning("[BiomeManager] 群系序列为 null");
        return;
    }

    if (sequence.biomeStages == null || sequence.biomeStages.Count == 0)
    {
        Debug.LogWarning("[BiomeManager] 群系序列为空，禁用序列模式");
        sequence.useSequence = false;
        return;
    }

    foreach (var stage in sequence.biomeStages)
    {
        if (stage.biome == null)
        {
            Debug.LogWarning("[BiomeManager] 群系序列中有 null 阶段");
        }
        if (stage.transitionDistance <= 0)
        {
            Debug.LogWarning("[BiomeManager] 群系切换距离必须大于 0");
        }
    }
}
```

---

## Implementation Notes

### 实现建议

1. **ScriptableObject 组织**
   - 所有 BiomeConfig 资产放在 `Assets/Resources/Biomes/` 目录下
   - 所有 BiomeSequence 资产放在 `Assets/Resources/Biomes/Sequences/` 目录下
   - 使用一致的命名规范：`{BiomeName}Biome.asset`

2. **配置验证**
   - 在 Awake 或 Start 时验证所有配置
   - 使用 `[Range]` 属性限制数值范围
   - 使用 `[Min]` 属性确保正值

3. **调试支持**
   - 在 BiomeManager 中添加 `_enableDebugLog` 字段
   - 群系切换时输出详细日志
   - 可在 Inspector 中启用/禁用调试日志

4. **扩展指南**
   - 在文档中添加新群系创建指南
   - 提供配置示例和截图
   - 说明 Tile 资源和障碍物预制体的创建方法

### 新群系创建示例

以下是创建"丛林"群系的完整步骤：

**步骤 1: 创建 BiomeConfig 资产**
```
1. 在 Unity 编辑器中，右键点击 Assets/Resources/Biomes
2. 选择 Create → Runner's Journey → Biome Config
3. 命名为 JungleBiome.asset
```

**步骤 2: 配置参数**
```yaml
biomeName: "丛林"
groundTile: [拖入丛林地表 Tile]
groundDecorationTile: [可选，丛林装饰 Tile]
dirtTile: [拖入丛林土壤 Tile]
skyColor: #40E0D0 (青绿色)
backgroundColor: #228B22 (森林绿)
obstaclePrefabs:
  - [丛林藤蔓障碍物]
  - [丛林石头]
  - [丛林树桩]
```

**步骤 3: 添加到序列（可选）**
```
1. 打开 FullSequence.asset
2. 在 biomeStages 列表中添加新元素
3. 设置 biome 为 JungleBiome
4. 设置 transitionDistance 为 600（600 米后切换到丛林）
```

### 与现有系统集成

1. **TilemapMapConfig**
   - 可选：将默认 Tile 引用移到 BiomeConfig
   - 或：保留原有配置作为回退值

2. **GameManager**
   - 通过 BiomeManager 管理群系
   - 支持玩家选择或序列模式

3. **BiomeSelectionPanel**
   - 显示可用的群系选项
   - 保存玩家选择

---

## References

### 相关文档

1. **Story 6-1 实现**
   - 群系系统核心代码
   - [Source: `_bmad-output/implementation-artifacts/stories/6-1-作为系统我可以生成不同生物群系的地形提供视觉变化.md`]

2. **GDD 生物群系设计**
   - 三种群系定义
   - [Source: `_bmad-output/gdd.md`]

### 相关文件

| 文件 | 用途 |
|------|------|
| `BiomeConfig.cs` | 群系配置 ScriptableObject 定义 |
| `BiomeSequence.cs` | 群系序列配置定义 |
| `BiomeManager.cs` | 群系管理器 |
| `TilemapEndlessMapGenerator.cs` | 使用群系配置的地图生成器 |
| `BiomeSelectionPanel.cs` | 群系选择 UI |

---

## Dev Agent Record

### Agent Model Used

- **Model**: qwen3.5-plus
- **文档创建时间**: 2026-03-27

### Story Status

**状态:** done ✅

**前置依赖:**
- Story 6-1: ✅ 已完成（核心代码已实现）
- Story 6-2: ✅ 已完成（群系选择 UI 已实现）
- Story 6-3: ✅ 已完成（难度系统已实现）

**实现要点:**
- Story 6-4 的主要工作在 Story 6-1 中已经完成
- 本 Story 重点是验证配置系统的扩展性和完善配置资产
- 需要创建 BiomeSequence 资产（草地→沙漠→雪地序列）

**完成情况:**
- ✅ Task 1: BiomeConfig 扩展性验证通过
- ✅ Task 2: BiomeSequence 扩展性验证通过
- ✅ Task 3: 创建示例群系配置（GrasslandBiome, DesertBiome, SnowlandBiome, FullSequence）
- ✅ Task 4: 配置验证和回退机制已实现
- ✅ Task 5: 调试日志已实现
- ✅ FullSequence.asset 已创建：Assets/Resources/Biomes/Sequences/FullSequence.asset
