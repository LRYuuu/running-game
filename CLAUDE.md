# Square Fireline - 项目开发规范

> **Unity 版本**: 2022.3.62f3c1 | **平台**: Windows | **类型**: 2D 横板跳跃

---

## 项目概述

Square Fireline 是一款 2D 横板跳跃游戏，采用程序化地图生成和多层视差滚动背景。

### 核心技术
- **地图系统**: 基于 Tilemap 的无尽地图生成（Chunk 为单位）
- **背景系统**: 三层视差滚动（Far/Mid/Near）
- **物理系统**: Unity 2D 物理 (Rigidbody2D + Collider2D)

---

## 完整项目结构

```
Assets/
├── Art/
│   ├── Textures/GroundTile/    # 地面 Tile (grass_left/middle/right, dirt_tile)
│   └── Textures/Obstacles/     # 障碍物 Tile
├── Documentation/              # 设计文档
├── Editor/                     # 编辑器扩展
├── Plugins/                    # 第三方插件
├── Prefabs/
│   ├── Player/
│   ├── Obstacles/
│   └── Collectibles/
├── Scenes/
│   ├── login.unity             # 登录/主菜单
│   └── EndlessMapTest.unity    # 地图测试
├── Scripts/
│   ├── Map/                    # 地图生成 (Namespace: SquareFireline.Map)
│   │   ├── TilemapMapConfig.cs
│   │   └── TilemapEndlessMapGenerator.cs
│   ├── Player/                 # 玩家控制 (Namespace: SquareFireline.Player)
│   ├── Obstacles/              # 障碍物 (Namespace: SquareFireline.Obstacles)
│   ├── UI/                     # 界面 (Namespace: SquareFireline.UI)
│   ├── Audio/                  # 音频 (Namespace: SquareFireline.Audio)
│   ├── Utils/                  # 工具类
│   ├── ParallaxBackground.cs
│   ├── BackgroundManager.cs
│   ├── InfiniteBackgroundTiler.cs
│   └── ProceduralBackgroundGenerator.cs
└── ScriptableObjects/          # 配置资产
    └── Map/
        └── TilemapMapConfig.asset
```

---

## 核心系统架构

### 1. 地图生成系统 (Map/)

#### Chunk 结构 (20×5 瓦片)
```
Y 轴 ↑
│     [障碍物层 y=5]
│     ──────────────────── ← 草坪层 (y=4)
│     │ L │ M │ ... │ R │
│     ──────────────────── ← 土壤层 (y=0~3, 4 种翻转效果)
│     │   土壤 (翻转)     │
└──────────────────────────→ X 轴
```

#### 配置参数 (TilemapMapConfig.cs)
| 参数 | 默认值 | 说明 |
|------|--------|------|
| chunkWidth | 20 | Chunk 宽度 |
| groundHeight | 5 | 地面高度 |
| aheadChunkCount | 3 | 前方保留 Chunk 数 |
| behindChunkCount | 2 | 后方清理 Chunk 数 |
| obstacleSpawnChance | 0.3f | 障碍物概率 |
| minObstacleGap | 3 | 最小间隔 |
| maxObstacleGap | 8 | 最大间隔 |

#### 土壤翻转机制
```csharp
// 4 种模式：0=无翻转，1=水平，2=垂直，3=双重
int flipMode = Mathf.Abs((x * 17 + y * 31) % 4);
Matrix4x4 transform = Matrix4x4.Scale(new Vector3(
    flipMode == 1 || flipMode == 3 ? -1 : 1,
    flipMode == 2 || flipMode == 3 ? -1 : 1,
    1
));
```

#### 空隙生成规则
- 前 5 个 Chunk 不生成空隙
- 空隙概率：10%
- 宽度：1~3 瓦片

### 2. 背景系统

#### 三层视差结构
```
Hierarchy:
├── Backgrounds
│   ├── FarBackground   (速度系数 0.2)
│   ├── MidBackground   (速度系数 0.5)
│   └── NearBackground  (速度系数 1.0)
```

#### 视差计算公式
```csharp
视差偏移 = 相机移动 × 视差系数
```

### 3. 玩家系统 (待实现)

#### 预期 Hierarchy
```
Player
├── SpriteRenderer
├── Rigidbody2D
├── BoxCollider2D
├── PlayerController
├── PlayerAnimation
└── Animator
```

### 4. 障碍物系统

| 类型 | 行为 | 碰撞体 |
|------|------|--------|
| 石头 | 阻挡 | BoxCollider2D |
| 尖刺 | 致死 | BoxCollider2D |
| 木箱 | 可破坏 | BoxCollider2D |
| 移动平台 | 单向通过 | PlatformEffector2D |

---

## 开发规范

### 命名空间划分
| 命名空间 | 路径 |
|---------|------|
| `SquareFireline.Map` | Assets/Scripts/Map/ |
| `SquareFireline.Player` | Assets/Scripts/Player/ |
| `SquareFireline.Obstacles` | Assets/Scripts/Obstacles/ |
| `SquareFireline.UI` | Assets/Scripts/UI/ |
| `SquareFireline.Audio` | Assets/Scripts/Audio/ |
| `SquareFireline.Utils` | Assets/Scripts/Utils/ |

### 代码模板

#### MonoBehaviour 标准结构
```csharp
using UnityEngine;

namespace SquareFireline.Map
{
    /// <summary>
    /// 类说明
    /// </summary>
    public class ClassName : MonoBehaviour
    {
        #region 序列化字段
        [Header("分类")]
        [Tooltip("说明")]
        public int value;
        #endregion

        #region 私有字段
        private int _privateField;
        #endregion

        #region Unity 生命周期
        private void Awake() { }
        private void Start() { }
        private void Update() { }
        #endregion

        #region 公共方法
        /// <summary>
        /// 方法说明
        /// </summary>
        public void PublicMethod() { }
        #endregion

        #region 私有方法
        private void PrivateMethod() { }
        #endregion

        #if UNITY_EDITOR
        private void OnDrawGizmos() { }
        #endif
    }
}
```

#### ScriptableObject 配置
```csharp
using UnityEngine;

namespace SquareFireline.Map
{
    [CreateAssetMenu(fileName = "MapConfig", menuName = "Square Fireline/Map Config")]
    public class MapConfig : ScriptableObject
    {
        [Header("分类标题")]
        [Tooltip("字段说明")]
        public int value = 10;
    }
}
```

### 命名约定
| 类型 | 规则 | 示例 |
|------|------|------|
| 命名空间 | PascalCase | `SquareFireline.Map` |
| 类名 | PascalCase | `TilemapEndlessMapGenerator` |
| 方法 | PascalCase | `GenerateChunk` |
| 私有字段 | camelCase + 下划线前缀 | `_mainCamera` |
| 公有字段 | `[SerializeField]` + camelCase | `[SerializeField] private float scrollSpeed` |
| 参数 | camelCase | `chunkX` |

### 注释规范
- **公共方法**: 必须 XML 文档注释 `/// <summary>`
- **序列化字段**: 必须 `[Header()]` + `[Tooltip()]`
- **日志格式**: `[ClassName] 消息内容`

### 日志示例
```csharp
Debug.Log($"[TilemapMapGenerator] 生成 Chunk #{chunkX} @ X={startX}");
Debug.LogWarning($"[TilemapMapGenerator] grassTile 为 null!");
Debug.LogError($"[TilemapMapGenerator] 缺少 MapConfig 配置！");
```

---

## 场景结构

### 标准场景 Hierarchy
```
Scene
├── ─── Canvas (Screen Space - Overlay)
│   ├── UI_EventSystem
│   └── [UI Panels]
├── ─── Managers (DontDestroyOnLoad)
│   ├── GameManager
│   ├── AudioManager
│   └── UIManager
├── ─── Camera
│   └── Main Camera
├── ─── Map
│   ├── Grid
│   │   ├── Ground (Tilemap + Collider2D + Renderer)
│   │   ├── Obstacles (Tilemap + Collider2D + Renderer)
│   │   └── Background (Tilemap)
│   └── MapGenerator
│       └── TilemapEndlessMapGenerator
├── ─── Player
└── ─── Lighting
    └── Directional Light
```

### 场景列表
| 场景 | 用途 | 索引 |
|------|------|------|
| login.unity | 主菜单 | 0 |
| EndlessMapTest.unity | 测试场景 | 1 |
| Game.unity (待创建) | 主游戏 | 2 |

---

## 资源规范

### Tile 导入设置
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 128
Filter Mode: Point (像素风格)
Compression: None
```

### Prefab 规范
- 存放路径：`Assets/Prefabs/[类别]/`
- 命名：`[功能]Prefab` 或 `[功能].prefab`
- 使用嵌套 Prefab 减少重复

### ScriptableObject 创建
1. Project 窗口右键 → `Create > Square Fireline > [类型]`
2. 存放路径：`Assets/ScriptableObjects/[功能]/`

---

## 测试规范

### 测试目录
```
Assets/Tests/
├── EditMode/           # 纯逻辑测试
│   └── SquareFireline.Tests.asmdef
└── PlayMode/           # 需要场景的测试
    └── SquareFireline.PlayMode.asmdef
```

### EditMode 测试模板
```csharp
using NUnit.Framework;
using UnityEngine;

namespace SquareFireline.Tests.Map
{
    [TestFixture]
    public class MapConfigTests
    {
        [Test]
        public void ChunkWidth_ShouldBePositive()
        {
            var config = ScriptableObject.CreateInstance<MapConfig>();
            Assert.Greater(config.chunkWidth, 0);
        }
    }
}
```

---

## UnityMCP 使用

### 资源监控
- `mcpforunity://editor/state` - 编辑器状态
- `mcpforunity://project/info` - 项目信息
- `mcpforunity://instances` - Unity 实例列表

### 常用工具
- `manage_gameobject` - GameObject 增删改
- `manage_components` - 组件管理
- `manage_asset` - 资产管理
- `manage_scene` - 场景管理
- `manage_material` - 材质管理

---

## 待完成功能

### 高优先级 (P0)
- [ ] 玩家控制器 (移动/跳跃)
- [ ] 玩家动画系统
- [ ] UI 系统 (血条/分数)
- [ ] 主菜单场景完善

### 中优先级 (P1)
- [ ] 障碍物系统完善 (多种类型)
- [ ] 收集品系统 (金币/道具)
- [ ] 音频系统
- [ ] 死亡/重生逻辑

### 低优先级 (P2)
- [ ] 难度曲线 (距离越远障碍越密)
- [ ] 存档系统
- [ ] 特效系统

---

## 构建设置

```
Platform: Windows (StandaloneWindows64)
Architecture: x86_64
Scripting Backend: IL2CPP
API Compatibility: .NET Standard 2.1
Managed Stripping: Medium
Color Space: Gamma
Target Frame Rate: 60
```

---

## Git 提交规范

```
<type>(<scope>): <subject>

# type: feat(新功能), fix(修复), docs(文档), style(格式),
#       refactor(重构), test(测试), chore(构建)
# scope: map, player, ui, background, obstacles
```

示例：
```bash
git commit -m "feat(map): 添加空隙生成功能"
git commit -m "fix(background): 修复视差滚动速度计算"
```
