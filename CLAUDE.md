# Square Fireline - 项目开发规范

> **Unity**: 2022.3.62f3c1 | **平台**: Windows | **类型**: 2D 横板跳跃

---

## 🎯 AI 助手工作指南

### 执行任务前
1. 先查看「待完成功能」了解项目进度
2. 修改代码前参考「代码规范」和「项目结构」
3. 提交前检查「Git 提交规范」

### 代码修改原则
- 遵循现有命名空间划分（Map/Player/Obstacles/UI/Audio/Utils）
- 新增脚本时参考「代码模板」结构
- 日志格式：`[ClassName] 消息内容`

---

## 项目概述

2D 横板跳跃游戏，核心特性：
- **地图系统**: 基于 Tilemap 的无尽地图生成（Chunk 为单位）
- **背景系统**: 三层视差滚动（Far/Mid/Near）
- **物理系统**: Unity 2D 物理

---

## 📁 项目结构（核心文件）

```
Assets/
├── Scripts/
│   ├── Map/              # SquareFireline.Map - 地图生成
│   ├── Player/           # SquareFireline.Player - 玩家控制
│   ├── Obstacles/        # SquareFireline.Obstacles - 障碍物
│   ├── UI/               # SquareFireline.UI - 界面
│   ├── Audio/            # SquareFireline.Audio - 音频
│   └── Utils/            # SquareFireline.Utils - 工具类
├── Prefabs/              # 预制体
├── ScriptableObjects/    # 配置资产
├── Scenes/
│   ├── login.unity       # 主菜单
│   └── EndlessMapTest.unity  # 测试场景
└── Art/Textures/         # 美术资源
```

---

## 📝 代码规范（必须遵守）

### 命名空间
| 路径 | 命名空间 |
|------|----------|
| Scripts/Map/ | `SquareFireline.Map` |
| Scripts/Player/ | `SquareFireline.Player` |
| Scripts/Obstacles/ | `SquareFireline.Obstacles` |
| Scripts/UI/ | `SquareFireline.UI` |
| Scripts/Audio/ | `SquareFireline.Audio` |
| Scripts/Utils/ | `SquareFireline.Utils` |

### 命名规则
- 类/方法：`PascalCase`
- 私有字段：`_camelCase`（下划线前缀）
- 参数：`camelCase`

### 代码结构
- 公共方法：必须添加 `/// <summary>` 文档注释
- 序列化字段：必须添加 `[Header()]` 和 `[Tooltip()]`
- 日志格式：`Debug.Log($"[ClassName] 消息")`

---

## 🗺️ 核心系统

### 地图生成 (Map/)
- **Chunk 尺寸**: 20×5 瓦片
- **配置类**: `TilemapMapConfig` (ScriptableObject)
- **生成器**: `TilemapEndlessMapGenerator`

### 背景系统
- 三层视差：Far (0.2x) / Mid (0.5x) / Near (1.0x)
- 管理类：`BackgroundManager`

### 玩家系统 (待完善)
- 需要：移动/跳跃控制、动画系统

---

## 📋 代码模板

### MonoBehaviour
```csharp
using UnityEngine;

namespace SquareFireline.Map
{
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
        private void Update() { }
        #endregion
    }
}
```

### ScriptableObject
```csharp
using UnityEngine;

namespace SquareFireline.Map
{
    [CreateAssetMenu(fileName = "MapConfig", menuName = "Square Fireline/Map Config")]
    public class MapConfig : ScriptableObject
    {
        [Header("配置")]
        [Tooltip("说明")]
        public int value = 10;
    }
}
```

---

## 🏗️ 场景结构

### 标准 Hierarchy
```
Scene
├── Canvas (Screen Space - Overlay)
├── Managers (DontDestroyOnLoad) - GameManager, AudioManager, UIManager
├── Camera - Main Camera
├── Map - Grid + MapGenerator
├── Player
└── Lighting
```

### 场景列表
| 场景 | 用途 | 索引 |
|------|------|------|
| login.unity | 主菜单 | 0 |
| EndlessMapTest.unity | 测试 | 1 |
| Game.unity | 主游戏 (待创建) | 2 |

---

## ✅ 测试规范

测试目录：`Assets/Tests/`
- `EditMode/` - 纯逻辑测试
- `PlayMode/` - 场景测试

---

## 📦 构建设置

- **平台**: Windows (StandaloneWindows64)
- **架构**: x86_64
- **后端**: IL2CPP + .NET Standard 2.1
- **帧率**: 60 FPS

---

## ⚙️ 待完成功能

| 优先级 | 功能 |
|--------|------|
| P0 | 玩家控制器 (移动/跳跃)、玩家动画系统、UI 系统 (血条/分数) |
| P1 | 障碍物系统、收集品系统、音频系统、死亡/重生逻辑 |
| P2 | 难度曲线、存档系统、特效系统 |

---

## 📝 Git 提交规范

**格式**: `<type>(<scope>): <subject>`

| type | 说明 |
|------|------|
| feat | 新功能 |
| fix | 修复 |
| docs | 文档 |
| style | 格式 |
| refactor | 重构 |
| test | 测试 |
| chore | 构建 |

**scope**: map, player, ui, background, obstacles

**示例**:
```bash
git commit -m "feat(map): 添加空隙生成功能"
git commit -m "fix(player): 修复跳跃检测"
```

---

## 📚 文档索引 (docs/)

### 开发规范
| 文档 | 说明 |
|------|------|
| [docs/README.md](docs/README.md) | 项目文档总索引 |
| [docs/GIT_COMMIT_GUIDE.md](docs/GIT_COMMIT_GUIDE.md) | Git 提交规范详解 |

### 游戏设计
| 文档 | 说明 |
|------|------|
| [docs/game-design/MapGenerationPlan.md](docs/game-design/MapGenerationPlan.md) | 地图生成设计方案 |
| [docs/game-design/TileRequirements.md](docs/game-design/TileRequirements.md) | Tile 资产需求清单 |
| [docs/game-design/TerrainVariationPlan.md](docs/game-design/TerrainVariationPlan.md) | 地形起伏生成方案 |
| [docs/game-design/TerrainVariationPlan-Implementation.md](docs/game-design/TerrainVariationPlan-Implementation.md) | 地形起伏实施计划 |

### 技术规格
| 文档 | 说明 |
|------|------|
| [docs/tech-specs/tech-spec-dino-runner.md](docs/tech-specs/tech-spec-dino-runner.md) | 恐龙跑酷小游戏技术规格 |
