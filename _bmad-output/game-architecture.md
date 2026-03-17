---
title: 'Game Architecture'
project: 'Square Fireline'
date: '2026-03-16'
author: 'liruoyu'
version: '1.0'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9]
status: 'complete'

# Source Documents
gdd: '_bmad-output/gdd.md'
epics: '_bmad-output/epics.md'
brief: null
---

# Square Fireline - Game Architecture

## Document Status

This architecture document is being created through the GDS Architecture Workflow.

**Steps Completed:** 9 of 9 (Complete & Handoff)

---

## Engine & Framework

### Selected Engine

**Unity** v2022.3.62f3 LTS

**Rationale:**
- GDD 中明确指定的引擎版本
- LTS 长期支持版本，稳定性高，适合作品集项目
- 完整的 2D 工具链（Tilemap、Sprite Renderer、Physics 2D）
- 全平台发布支持（PC、移动端、主机）
- 成熟的 Asset Store 生态系统和社区资源
- Unity Test Framework 支持 EditMode + PlayMode 测试

### Engine-Provided Architecture

| 组件 | Unity 解决方案 | 说明 |
|------|---------------|------|
| **渲染** | URP (Universal Render Pipeline) | 支持 2D 光照、像素完美缩放、多平台适配 |
| **物理** | Unity Physics 2D (Box2D 封装) | 自动碰撞检测、刚体模拟、地面检测 |
| **音频** | AudioSource / AudioListener | 空间音频、音量控制、音频资源管理 |
| **输入** | Unity New Input System | 跨平台输入抽象（键盘/触屏/手柄） |
| **场景管理** | SceneManager + DontDestroyOnLoad | 多场景加载、持久化对象管理 |
| **动画** | Animator + Mecanim | 状态机驱动的角色动画系统 |
| **UI** | UI Toolkit | 基于 USS/UXML 的现代化 UI 系统 |
| **构建系统** | Unity Build Pipeline + IL2CPP | .NET Standard 2.1 后端，x86_64 架构 |
| **数据持久化** | PlayerPrefs / JSON 序列化 | 简单数据存储和配置文件管理 |
| **调试** | Debug 类 + Profiler | 日志输出、性能分析工具 |

## Architectural Decisions Log

以下架构决策已通过 GDS Architecture Workflow Step 4 确定：

| # | 决策类别 | 决策结果 | 优先级 | 理由摘要 |
|---|---------|---------|--------|---------|
| **1** | **代码架构** | ECS/DOTS | P0 | 数据导向设计，适合程序化生成 + 高性能需求 |
| **2** | **状态管理** | State Machine + ECS Integration | P0 | 清晰的状态流转，与 ECS 系统深度整合 |
| **3** | **数据流/事件系统** | C# 事件 + ScriptableObject 混合模式 | P0 | 高频事件用 C# 事件，游戏状态事件用 ScriptableObject |
| **4** | **数据持久化** | JSON 序列化 | P0 | 展示序列化能力、支持复杂结构、可读性好 |
| **5** | **资源加载策略** | Addressables | P1 | 异步加载、内存管理、依赖管理、扩展性强 |
| **6** | **对象池架构** | Unity 官方 ObjectPool<T> + 自定义封装 | P1 | 官方支持、线程安全、平衡性能和复杂度 |
| **7** | **UI 架构** | UI Toolkit + 简单数据绑定 | P1 | GDD 指定、Unity 推荐、样式分离 |
| **8** | **测试架构** | Unity Test Framework + NUnit 断言 | P0 | 官方标准、EditMode+PlayMode、覆盖率报告 |

---

## Architectural Decision Details

### 决策 1: 代码架构 - ECS/DOTS

**详细说明:**
- 采用 Unity DOTS (Data-Oriented Technology Stack) 架构
- 使用 ECS (Entity-Component-System) 模式组织游戏逻辑
- 利用 Burst Compiler + Jobs System 实现高性能

**适用场景:**
- 地图生成系统：使用 Job 并行生成 Chunk
- 障碍物系统：System 批量处理障碍物逻辑
- 玩家系统：传统 MonoBehaviour（输入响应）+ ECS（状态同步）

---

### 决策 2: 状态管理 - State Machine + ECS Integration

**详细说明:**
- 使用显式状态机管理游戏全局状态（Menu/Playing/Paused/GameOver）
- 状态变化通过 ECS 事件通知相关系统
- 状态数据存储在 Singleton Component 中

**状态流转:**
```
Menu → Playing → GameOver → Playing (Respawn)
              ↓
            Paused → Playing
```

---

### 决策 3: 数据流/事件系统 - C# 事件 + ScriptableObject 混合模式

**详细说明:**
- **C# 事件/委托**: 用于高频事件（每帧更新、Chunk 生成触发）
- **ScriptableObject 事件通道**: 用于游戏状态事件（游戏开始/结束、得分更新、UI 响应）

**使用场景划分:**
| 事件类型 | 实现方式 | 示例 |
|---------|---------|------|
| 高频事件 | C# 事件 | 玩家位置更新、Chunk 生成触发 |
| 游戏状态事件 | ScriptableObject | 游戏开始、游戏结束、得分更新 |
| UI 响应事件 | ScriptableObject | 显示死亡提示、更新分数显示 |
| 系统间通信 | 混合使用 | 障碍物生成完成通知 |

---

### 决策 4: 数据持久化 - JSON 序列化

**详细说明:**
- 使用 `JsonUtility` 进行序列化/反序列化
- 数据存储在 `Application.persistentDataPath`
- 支持版本控制（通过数据类版本号）

**持久化数据结构:**
```csharp
[System.Serializable]
public class PlayerData {
    public int version;
    public int highScore;
    public List<string> unlockedBiomes;
    public PlayerSettings settings;
}
```

---

### 决策 5: 资源加载策略 - Addressables

**详细说明:**
- 使用 `Addressables.LoadAssetAsync<T>` 异步加载 Chunk 预制体
- 引用计数管理资源生命周期
- 自动处理依赖资产（材质、纹理等）

**关键配置:**
| 配置项 | 设置 |
|--------|------|
| Profile | DefaultProfile（开发）+ ReleaseProfile（发布） |
| Content Update | 启用增量构建支持 |
| Build Path | `[Project]/AddressableBuilds/[BuildTarget]` |
| Load Path | `file://{Project}/AddressableBuilds/[BuildTarget]` |

---

### 决策 6: 对象池架构 - Unity 官方 ObjectPool<T> + 自定义封装

**详细说明:**
- 使用 `UnityEngine.Pool.ObjectPool<T>` 官方实现
- 封装游戏特定的预加载、容量监控逻辑
- Chunk 池化，障碍物/粒子直接创建销毁

**混合策略:**
| 对象类型 | 管理方式 | 原因 |
|---------|---------|------|
| Chunk | 对象池 | 高频创建/销毁，性能敏感 |
| 障碍物 | 直接创建销毁 | 数量少，GC 影响可忽略 |
| 粒子效果 | 简单池化 | 可选优化，非 P0 |

---

### 决策 7: UI 架构 - UI Toolkit + 简单数据绑定

**详细说明:**
- 使用 UI Toolkit 构建所有 UI 界面
- 通过封装 `UIDocument` 实现简单数据绑定
- USS 样式表分离视觉设计

**UI 架构分层:**
| 层级 | 职责 | 实现方式 |
|------|------|---------|
| View 层 | UI 元素引用、显示逻辑 | UIDocument + VisualElement |
| ViewModel 层 | 数据状态、业务逻辑 | POCO 类 + 事件通知 |
| Controller 层 | 用户输入处理 | MonoBehaviour + New Input System |

---

### 决策 8: 测试架构 - Unity Test Framework + NUnit 断言

**详细说明:**
- **EditMode 测试**: 纯逻辑测试（地图生成算法、数据结构）
- **PlayMode 测试**: 场景集成测试（玩家控制、碰撞检测）
- 使用 NUnit 断言语法（`Assert.That()`）
- 目标：核心系统 100% 测试覆盖

**测试组织:**
```
Assets/Tests/
├── EditMode/
│   ├── Map/
│   ├── Player/
│   └── Obstacles/
└── PlayMode/
    ├── Map/
    └── Player/
```

---

## Cross-cutting Concerns

以下跨领域关注点已通过 GDS Architecture Workflow Step 5 确定：

| # | 关注点 | 决策结果 | 优先级 | 理由摘要 |
|---|--------|---------|--------|---------|
| **1** | **错误处理** | 全局异常处理 + 信号通知 | P0 | 错误不应该暂停游戏，但需要通知 UI 显示 |
| **2** | **日志系统** | 结构化 JSON 日志 | P1 | 便于日志分析工具解析，支持远程错误报告 |
| **3** | **配置管理** | ScriptableObject + JSON 配置文件 | P0 | 平衡数值可调整，支持运行时热重载 |
| **4** | **事件系统** | Event Bus (集中式事件分发器) | P0 | 统一的事件管理，支持同步/异步分发 |
| **5** | **调试工具** | 调试控制台 + 性能监控 | P1 | 开发效率工具，支持运行时参数调整 |

---

## Cross-cutting Concern Details

### 1. 错误处理策略 - 全局异常处理 + 信号通知

**详细说明:**
- 错误不应该中断游戏流程，而是通过事件系统通知相关模块
- 使用全局异常处理器捕获未处理的异常
- 错误信息通过 ScriptableObject 事件通道通知 UI 显示
- 关键系统错误记录到日志，但不中断游戏

**错误分类:**
| 错误类型 | 处理方式 | 示例 |
|---------|---------|------|
| **致命错误** | 记录日志 + 通知 UI + 安全降级 | 资源加载失败 → 使用默认资源 |
| **可恢复错误** | 记录日志 + 自动重试 | 网络请求失败 → 重试 3 次 |
| **警告** | 记录日志，不影响流程 | 配置值超出推荐范围 |

**错误处理流程:**
```
错误发生 → 全局异常处理器捕获 → 分类处理
    ├─ 记录日志（结构化格式）
    ├─ 触发错误事件（通知 UI）
    └─ 执行安全降级（如适用）
```

**代码示例:**
```csharp
public class GlobalErrorHandler : MonoBehaviour
{
    private void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type) {
        if (type == LogType.Error || type == LogType.Exception) {
            // 记录结构化日志
            Logger.Error("GameError", new {
                condition = condition,
                stackTrace = stackTrace,
                timestamp = DateTime.Now
            });

            // 通知 UI 显示错误提示（不暂停游戏）
            GameErrorEvent.Raise(new GameError {
                message = GetFriendlyMessage(condition),
                isCritical = type == LogType.Exception
            });
        }
    }
}
```

---

### 2. 日志系统 - 结构化 JSON 日志

**详细说明:**
- 所有日志以结构化 JSON 格式输出
- 支持 5 级日志级别：ERROR, WARN, INFO, DEBUG, TRACE
- 日志输出到控制台（开发）和文件（发布）
- 便于日志分析工具解析和远程错误报告

**日志级别定义:**
| 级别 | 说明 | 使用场景 |
|------|------|---------|
| **ERROR** | 错误 | 系统异常、资源加载失败 |
| **WARN** | 警告 | 非致命问题、配置异常 |
| **INFO** | 信息 | 游戏状态变化、重要事件 |
| **DEBUG** | 调试 | 开发调试信息、逻辑追踪 |
| **TRACE** | 追踪 | 详细的执行路径追踪 |

**日志格式:**
```json
{
  "timestamp": "2026-03-16T10:30:00.000Z",
  "level": "INFO",
  "category": "MapGenerator",
  "message": "Chunk generated",
  "data": {
    "chunkId": "chunk_001",
    "position": {"x": 100, "y": 0},
    "generationTimeMs": 15.2
  }
}
```

**代码示例:**
```csharp
public static class Logger
{
    public static void Info(string category, object data = null) {
        Log("INFO", category, data);
    }

    public static void Error(string category, object data = null) {
        Log("ERROR", category, data);
    }

    private static void Log(string level, string category, object data) {
        var logEntry = new {
            timestamp = DateTime.UtcNow.ToString("O"),
            level = level,
            category = category,
            message = data?.ToString() ?? "",
            data = data
        };

        string jsonLog = JsonUtility.ToJson(logEntry);
        UnityEngine.Debug.Log(jsonLog);
    }
}

// 使用示例
Logger.Info("MapGenerator", new {
    chunkId = "chunk_001",
    generationTimeMs = 15.2
});
```

---

### 3. 配置管理 - ScriptableObject + JSON 配置文件

**详细说明:**
- **平衡数值**使用 ScriptableObject 资产管理
- 支持运行时热重载，无需重新编译
- 可选 JSON 配置文件用于更复杂的配置场景
- 版本控制友好，支持不同配置的版本追踪

**配置分类:**
| 配置类型 | 存储方式 | 示例 |
|---------|---------|------|
| **游戏平衡数值** | ScriptableObject | 跳跃高度、移动速度、障碍物密度 |
| **系统配置** | JSON 文件 | 图形质量、音频设置、输入映射 |
| **调试配置** | ScriptableObject | 调试开关、性能监控选项 |

**ScriptableObject 配置示例:**
```csharp
[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Square Fireline/Game Balance Config")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("玩家配置")]
    [Tooltip("玩家跳跃力")]
    public float playerJumpForce = 10f;

    [Tooltip("玩家移动速度")]
    public float playerMoveSpeed = 5f;

    [Header("地图配置")]
    [Tooltip("障碍物生成概率 (0-1)")]
    public float obstacleSpawnRate = 0.3f;

    [Tooltip("空隙生成概率 (0-1)")]
    public float gapSpawnRate = 0.1f;
}
```

**配置热重载支持:**
```csharp
public class ConfigManager : MonoBehaviour
{
    private GameBalanceConfig _config;

    // 编辑器模式下支持热重载
    #if UNITY_EDITOR
    private void OnValidate() {
        if (_config != null) {
            // 配置值改变时自动更新
            ApplyConfigChanges();
        }
    }
    #endif

    private void ApplyConfigChanges() {
        PlayerController.Instance.UpdateJumpForce(_config.playerJumpForce);
        PlayerController.Instance.UpdateMoveSpeed(_config.playerMoveSpeed);
    }
}
```

---

### 4. 事件系统 - Event Bus (集中式事件分发器)

**详细说明:**
- 集中式事件分发器，统一管理游戏内所有事件
- 支持同步和异步事件分发
- 事件订阅者无需直接引用，降低耦合度
- 与 ScriptableObject 事件通道结合使用

**事件分类:**
| 事件类型 | 分发方式 | 使用场景 |
|---------|---------|---------|
| **高频事件** | 同步分发 | 每帧更新、碰撞检测 |
| **游戏状态事件** | 同步分发 | 游戏开始/结束、得分更新 |
| **异步事件** | 异步分发 | 资源加载完成、网络请求 |

**Event Bus 实现:**
```csharp
public class EventBus
{
    private static EventBus _instance;
    private Dictionary<Type, List<Action<object>>> _subscribers = new();

    public static EventBus Instance {
        get {
            if (_instance == null) _instance = new EventBus();
            return _instance;
        }
    }

    public void Subscribe<T>(Action<T> handler) {
        var type = typeof(T);
        if (!_subscribers.ContainsKey(type)) {
            _subscribers[type] = new List<Action<object>>();
        }
        _subscribers[type].Add(data => handler((T)data));
    }

    public void Publish<T>(T eventData) {
        var type = typeof(T);
        if (_subscribers.ContainsKey(type)) {
            foreach (var handler in _subscribers[type]) {
                handler(eventData);
            }
        }
    }
}

// 使用示例
public class GameStartEvent {
    public int seed;
    public string biome;
}

// 订阅
EventBus.Instance.Subscribe<GameStartEvent>(OnGameStart);

// 发布
EventBus.Instance.Publish(new GameStartEvent { seed = 12345, biome = "grassland" });
```

---

### 5. 调试工具 - 调试控制台 + 性能监控

**详细说明:**
- 开发模式下启用调试工具
- 实时性能监控（FPS、内存、Draw Calls）
- 运行时参数调整（平衡数值、调试开关）
- 日志过滤和搜索功能

**调试工具功能:**
| 功能 | 说明 | 快捷键 |
|------|------|--------|
| **调试控制台** | 显示日志、执行命令 | ~ (波浪号) |
| **性能监控** | FPS 计数器、内存占用 | F1 |
| **参数调整器** | 运行时修改平衡数值 | F2 |
| **游戏时间控制** | 慢动作、暂停、加速 | F3 |

**调试控制台实现:**
```csharp
public class DebugConsole : MonoBehaviour
{
    private bool _isVisible;
    private List<string> _logs = new();
    private string _inputBuffer = "";

    private void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            _isVisible = !_isVisible;
        }

        if (_isVisible) {
            RenderConsole();
        }
    }

    public static void Log(string message) {
        Instance._logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public static void ExecuteCommand(string command) {
        // 支持命令：set_param, spawn_obstacle, god_mode, etc.
        var parts = command.Split(' ');
        switch (parts[0]) {
            case "set_param":
                ConfigManager.Instance.SetParameter(parts[1], float.Parse(parts[2]));
                break;
            case "god_mode":
                PlayerController.Instance.IsInvincible = true;
                break;
        }
    }
}
```

**性能监控面板:**
```csharp
public class PerformanceMonitor : MonoBehaviour
{
    [Header("监控设置")]
    public float updateInterval = 0.5f;

    private int _frameCount;
    private float _fps;
    private float _memoryUsage;

    private void Update() {
        _frameCount++;

        if (Time.time > _nextUpdate) {
            _fps = _frameCount / (Time.time - _lastUpdate);
            _memoryUsage = GC.GetTotalMemory(false) / 1024f / 1024f; // MB

            Logger.Debug("Performance", new { fps = _fps, memoryMB = _memoryUsage });

            _frameCount = 0;
            _lastUpdate = Time.time;
            _nextUpdate = Time.time + updateInterval;
        }
    }
}
```

---

## Project Structure

采用 **Hybrid 模式** (混合模式) - 顶层按类型组织，子层按功能组织

### Organization Pattern Selection

| 模式 | 选择 | 理由 |
|------|------|------|
| **Hybrid 模式** | ✓ 已选 | 平衡类型清晰度与功能模块化，适合作品集展示 |
| By Feature | ✗ | 功能间耦合度高，不利于代码审查 |
| By Type | ✗ | 功能分散，不利于快速定位 |
| Domain-Driven | ✗ | 复杂度过高，不适合本项目规模 |

### Root Directory Structure

```
Square Fireline/
├── Assets/
│   ├── Scripts/           # 源代码
│   ├── Prefabs/           # 预制体
│   ├── Scenes/            # 场景
│   ├── ScriptableObjects/ # 配置数据
│   ├── Art/               # 美术资源
│   ├── Audio/             # 音频资源
│   └── Tests/             # 测试代码
├── _bmad/                 # BMad 工作流配置
├── _bmad-output/          # 生成的文档
└── docs/                  # 项目文档
```

### Source Code Organization

```
Assets/Scripts/
├── Core/                  # 核心系统 (跨模块通用)
│   ├── EventBus.cs
│   ├── Logger.cs
│   ├── GlobalErrorHandler.cs
│   └── ConfigManager.cs
├── Game/                  # 游戏逻辑层
│   ├── GameStateMachine.cs
│   ├── ScoreSystem.cs
│   └── CheckpointSystem.cs
├── Player/                # 玩家系统
│   ├── PlayerController.cs
│   ├── PlayerAnimation.cs
│   └── PlayerInput.cs
├── Map/                   # 地图系统
│   ├── TilemapMapConfig.cs
│   ├── TilemapEndlessMapGenerator.cs
│   ├── ChunkManager.cs
│   └── Chunk.cs
├── Obstacles/             # 障碍物系统
│   ├── ObstacleSpawner.cs
│   ├── ObstacleConfig.cs
│   └── Obstacle.cs
├── UI/                    # UI 系统
│   ├── GameUIController.cs
│   ├── MenuUIController.cs
│   └── UIDataBinder.cs
├── Audio/                 # 音频系统 (P2)
├── Background/            # 背景系统
└── Debug/                 # 调试工具 (仅开发版)
```

### System Mapping

| 系统 | 目录 | 命名空间 | 架构模式 |
|------|------|---------|---------|
| 核心系统 | Scripts/Core/ | SquareFireline.Core | Singleton + Event Bus |
| 游戏流程 | Scripts/Game/ | SquareFireline.Game | State Machine |
| 玩家控制 | Scripts/Player/ | SquareFireline.Player | MonoBehaviour + ECS |
| 地图生成 | Scripts/Map/ | SquareFireline.Map | ECS/DOTS + Job System |
| 障碍物 | Scripts/Obstacles/ | SquareFireline.Obstacles | ECS/DOTS |
| UI 系统 | Scripts/UI/ | SquareFireline.UI | UI Toolkit + Data Binding |
| 音频系统 | Scripts/Audio/ | SquareFireline.Audio | Singleton |
| 调试工具 | Scripts/Debug/ | SquareFireline.Debug | Static + Singleton |

### Naming Conventions

| 类型 | 命名规范 | 示例 |
|------|---------|------|
| **C# Scripts** | `PascalCase.cs` | `PlayerController.cs` |
| **ScriptableObjects** | `[功能]Config.asset` | `MapConfig.asset` |
| **Prefabs** | `PascalCase.prefab` | `Player.prefab` |
| **Namespaces** | `SquareFireline.[系统名]` | `SquareFireline.Map` |
| **Private fields** | `_camelCase` | `_playerSpeed` |
| **Public fields** | `camelCase` | `playerSpeed` |
| **Methods** | `PascalCase` | `CalculateJumpForce()` |
| **Layers** | `LayerName=Number` | `Player=8, Ground=9, Obstacle=10, Checkpoint=11` |

### Layer Configuration

```yaml
Physics Layers:
  - Player: 8
  - Ground: 9
  - Obstacle: 10
  - Checkpoint: 11

Collision Matrix:
  - Player ↔ Ground: ✓
  - Player ↔ Obstacle: ✓
  - Player ↔ Checkpoint: ✓
  - Player ↔ Player: ✗
```

---

## Implementation Patterns

以下实现模式已通过 GDS Architecture Workflow Step 7 确定，为代码实现提供具体的技术指导和代码结构规范。

### 1. Component Patterns - MonoBehaviour vs ECS 指南

根据系统职责选择合适的组件模式，平衡性能与开发效率。

#### System Architecture Map

| 系统 | 架构模式 | 理由 | 关键组件 |
|------|---------|------|---------|
| **Player System** | MonoBehaviour + ECS Hybrid | 输入响应需要 MonoBehaviour，状态同步使用 ECS | `PlayerController`, `PlayerInput`, `PlayerStateComponent` |
| **Map System** | 纯 ECS/DOTS + Jobs | 程序化生成需要高性能并行计算 | `MapGeneratorSystem`, `ChunkSpawnerJob`, `TerrainDataComponent` |
| **Obstacles System** | 纯 ECS/DOTS | 批量障碍物逻辑处理，数据驱动 | `ObstacleSpawnSystem`, `ObstacleComponent` |
| **UI System** | MonoBehaviour + UI Toolkit | Unity UI 系统基于 MonoBehaviour | `GameUIController`, `MenuUIController` |
| **Core Systems** | Singleton + Static | 全局访问、跨场景持久化 | `EventBus`, `Logger`, `ConfigManager` |
| **Audio System** | Singleton MonoBehaviour | 需要 MonoBehaviour 的 AudioSource 组件 | `AudioManager` |
| **Game State System** | Singleton + ScriptableObject | 状态持久化 + 编辑器可视化 | `GameStateMachine`, `GameBalanceConfig` |

#### Player System 详细设计

```csharp
// MonoBehaviour - 输入响应层
namespace SquareFireline.Player
{
    public class PlayerInput : MonoBehaviour
    {
        private PlayerInputActions _inputActions;

        public event Action JumpPressed;
        public event Action JumpReleased;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        private void Update()
        {
            if (_inputActions.Player.Jump.triggered)
                JumpPressed?.Invoke();

            if (_inputActions.Player.Jump.WasReleased())
                JumpReleased?.Invoke();
        }
    }
}

// ECS Component - 数据层
namespace SquareFireline.Player
{
    [System.Serializable]
    public struct PlayerStateComponent : IComponentData
    {
        public float MoveSpeed;
        public float JumpForce;
        public bool IsGrounded;
        public bool CanDoubleJump;
        public float CoyoteTimeRemaining;
    }

    // System - 逻辑层
    public class PlayerMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            Entities
                .WithAll<PlayerStateComponent>()
                .ForEach((ref PlayerStateComponent state) =>
                {
                    // 更新土狼时间
                    if (!state.IsGrounded && state.CoyoteTimeRemaining > 0)
                        state.CoyoteTimeRemaining -= deltaTime;
                }).WithoutBurst().Run();
        }
    }
}
```

#### Map System 详细设计

```csharp
// ECS Component - Chunk 数据
namespace SquareFireline.Map
{
    [System.Serializable]
    public struct ChunkDataComponent : IComponentData
    {
        public int2 ChunkCoordinates;
        public int Seed;
        public bool IsGenerated;
        public float2 NoiseValue;
    }

    // Job - 并行地形生成
    [BurstCompile]
    public struct GenerateTerrainJob : IJob
    {
        [ReadOnly] public int Seed;
        [ReadOnly] public int2 ChunkCoords;
        [ReadOnly] public float NoiseScale;
        [WriteOnly] public NativeArray<TileData> TileBuffer;

        public void Execute()
        {
            var hash = Seed ^ (ChunkCoords.x * 73856093) ^ (ChunkCoords.y * 19349663);

            for (int x = 0; x < 20; x++)
            {
                float noise = Mathf.PerlinNoise(
                    (ChunkCoords.x * 20 + x) * NoiseScale,
                    Seed * 0.01f
                );

                int height = Mathf.FloorToInt(noise * 3);
                TileBuffer[x] = new TileData { Height = height, Type = TileType.Grass };
            }
        }
    }

    // System - 调度器
    public class ChunkGenerationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ChunkDataComponent>()
                .WithNone<GeneratedTag>()
                .ForEach((int entityInQueryIndex, ref ChunkDataComponent chunk) =>
                {
                    var job = new GenerateTerrainJob
                    {
                        Seed = chunk.Seed,
                        ChunkCoords = chunk.ChunkCoordinates,
                        NoiseScale = 0.1f,
                        TileBuffer = chunk.TileBuffer
                    };
                    job.Schedule();
                }).WithoutBurst().Run();
        }
    }
}
```

---

### 2. Data Flow Patterns - 系统通信方法

#### Event Bus - 解耦系统通信

```csharp
namespace SquareFireline.Core
{
    /// <summary>
    /// 集中式事件分发器，统一管理游戏内所有事件
    /// 支持同步和异步事件分发
    /// </summary>
    public class EventBus
    {
        private static EventBus _instance;
        private Dictionary<Type, List<Action<object>>> _subscribers = new();

        public static EventBus Instance
        {
            get
            {
                if (_instance == null) _instance = new EventBus();
                return _instance;
            }
        }

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Action<object>>();

            _subscribers[type].Add(data => handler((T)data));
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
                _subscribers[type].RemoveAll(h => h.Target == handler.Target);
        }

        public void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                foreach (var handler in _subscribers[type].ToList())
                    handler(eventData);
            }
        }
    }

    // 事件数据结构
    public class GameStartEvent
    {
        public int Seed { get; set; }
        public string Biome { get; set; }
    }

    public class PlayerDeathEvent
    {
        public int FinalScore { get; set; }
        public Vector3 DeathPosition { get; set; }
    }

    public class ChunkGeneratedEvent
    {
        public int2 ChunkCoords { get; set; }
        public float GenerationTimeMs { get; set; }
    }
}
```

#### ScriptableObject 事件通道 - 游戏状态变化

```csharp
namespace SquareFireline.Core
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "Square Fireline/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private event Action _onRaise;

        public void Raise() => _onRaise?.Invoke();
        public void Subscribe(Action listener) => _onRaise += listener;
        public void Unsubscribe(Action listener) => _onRaise -= listener;
    }

    // 使用示例 - 游戏状态事件
    public class GameStartChannel : GameEvent { }
    public class GameOverChannel : GameEvent { }
    public class ScoreUpdateChannel : GameEvent { }
}
```

#### 直接引用 - 紧密耦合场景

```csharp
namespace SquareFireline.Player
{
    public class PlayerController : MonoBehaviour
    {
        // 紧密耦合：玩家与物理系统
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Collider2D _groundDetector;

        // 通过事件总线与 UI 通信（解耦）
        private void OnDeath()
        {
            EventBus.Instance.Publish(new PlayerDeathEvent
            {
                FinalScore = ScoreSystem.Instance.CurrentScore,
                DeathPosition = transform.position
            });
        }
    }
}
```

#### 数据流图示

```
┌─────────────┐     EventBus      ┌──────────────┐
│   Player    │ ────────────────→ │     UI       │
│ Controller  │  PlayerDeathEvent │ Controller   │
└─────────────┘                   └──────────────┘
       │                                  ▲
       │ Direct Reference                 │ ScriptableObject
       ▼                                  │
┌─────────────┐                   ┌──────────────┐
│ Rigidbody2D │                   │  GameEvent   │
│ Collider2D  │                   │  Channel     │
└─────────────┘                   └──────────────┘
```

---

### 3. Initialization Patterns - 系统启动顺序

#### Bootstrap Scene 架构

```csharp
namespace SquareFireline.Core
{
    /// <summary>
    /// 游戏启动引导场景
    /// 所有 DontDestroyOnLoad managers 的创建点
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCoreSystems();
        }

        private async void InitializeCoreSystems()
        {
            // 1. 初始化全局事件总线
            var eventBus = EventBus.Instance;

            // 2. 初始化配置管理器
            await ConfigManager.Instance.LoadAllConfigs();

            // 3. 初始化日志系统
            Logger.Initialize("game_log", LogLevel.Info);

            // 4. 加载主菜单场景
            await SceneManager.LoadSceneAsync("login");

            Logger.Info("Bootstrap", new { status = "complete" });
        }
    }
}
```

#### 系统启动顺序

```csharp
namespace SquareFireline.Core
{
    public enum SystemInitOrder
    {
        Core = 0,      // EventBus, Logger, ConfigManager
        Game = 10,     // GameStateMachine, ScoreSystem
        Player = 20,   // PlayerController, PlayerInput
        Map = 30,      // MapGenerator, ChunkManager
        Obstacles = 40,// ObstacleSpawner
        UI = 50,       // UI Controllers
        Audio = 60     // AudioManager
    }

    public interface IInitializableSystem
    {
        SystemInitOrder InitOrder { get; }
        Task InitializeAsync();
    }

    public class SystemInitializer : MonoBehaviour
    {
        private List<IInitializableSystem> _systems = new();

        private void Awake()
        {
            // 收集所有可初始化系统
            _systems.AddRange(FindObjectsOfType<MonoBehaviour>()
                .OfType<IInitializableSystem>());

            // 按优先级排序
            _systems = _systems.OrderBy(s => s.InitOrder).ToList();
        }

        public async Task InitializeAllAsync()
        {
            foreach (var system in _systems)
            {
                Logger.Info("SystemInit", new { system = system.GetType().Name });
                await system.InitializeAsync();
            }
        }
    }
}
```

#### 依赖注入模式

```csharp
namespace SquareFireline.Core
{
    /// <summary>
    /// 简单服务定位器模式
    /// 用于系统间依赖解析
    /// </summary>
    public class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new();

        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
        }
    }

    // 使用示例
    public class MapGenerator : MonoBehaviour, IInitializableSystem
    {
        private ChunkManager _chunkManager;
        private TilemapMapConfig _config;

        public SystemInitOrder InitOrder => SystemInitOrder.Map;

        public async Task InitializeAsync()
        {
            _chunkManager = ServiceLocator.Get<ChunkManager>();
            _config = ConfigManager.Instance.GetConfig<TilemapMapConfig>();
            await PreloadChunkPrefabs();
        }
    }
}
```

---

### 4. Update Loop Patterns - 每帧更新处理

#### ECS System Update

```csharp
namespace SquareFireline.Map
{
    /// <summary>
    /// ECS 系统通过 SystemGroup 组织更新顺序
    /// Unity 默认执行顺序：SimulationSystemGroup → FixedStepSimulationSystemGroup
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class ChunkCleanupSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var playerPos = PlayerController.Instance?.transform.position ?? Vector3.zero;

            Entities
                .WithAll<ChunkDataComponent>()
                .ForEach((ref ChunkDataComponent chunk, int entityInQueryIndex) =>
                {
                    float distance = Vector2.Distance(
                        new Vector2(chunk.ChunkCoordinates.x * 20, 0),
                        new Vector2(playerPos.x, 0)
                    );

                    if (distance > 100f) // 清理距离玩家过远的 Chunk
                        PostUpdateCommands.DestroyEntity(entityInQueryIndex);
                }).WithoutBurst().Run();
        }
    }
}
```

#### MonoBehaviour Update 分工

```csharp
namespace SquareFireline.Player
{
    public class PlayerController : MonoBehaviour
    {
        // Update: 输入响应（每帧）
        private void Update()
        {
            HandleJumpInput();
            CheckGrounded();
        }

        // FixedUpdate: 物理更新（固定时间步长）
        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyJumpForce();
        }

        // LateUpdate: 跟随/相机更新（在 Update 之后）
        private void LateUpdate()
        {
            UpdateCameraFollow();
        }

        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump") && CanJump())
            {
                _wantsJump = true;
            }
        }

        private void ApplyMovement()
        {
            _rigidbody2D.velocity = new Vector2(
                _config.MoveSpeed,
                _rigidbody2D.velocity.y
            );
        }
    }
}
```

#### 更新循环时序

```
Frame N (16.67ms @ 60FPS)
├─ FixedUpdate (if needed)
│  └─ Physics Simulation, Player Movement
├─ Update
│  ├─ Input Processing (PlayerInput)
│  ├─ ECS SystemGroup Execution
│  │  ├─ Map Generation Systems
│  │  ├─ Obstacle Spawn Systems
│  │  └─ Cleanup Systems
│  └─ Game Logic Updates
├─ LateUpdate
│  └─ Camera Follow, UI Updates
└─ Rendering
   └─ UI Toolkit Render
```

---

### 5. Error Handling Implementation - 具体模式

#### 全局异常处理器

```csharp
namespace SquareFireline.Core
{
    /// <summary>
    /// 全局错误处理，不中断游戏流程
    /// 通过事件系统通知相关模块
    /// </summary>
    public class GlobalErrorHandler : MonoBehaviour
    {
        private static GlobalErrorHandler _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                // 记录结构化日志
                Logger.Error("GameError", new
                {
                    condition = condition,
                    stackTrace = stackTrace,
                    timestamp = DateTime.Now.ToString("O")
                });

                // 通知 UI 显示错误提示（不暂停游戏）
                var friendlyMessage = GetFriendlyMessage(condition);
                EventBus.Instance.Publish(new GameErrorEvent
                {
                    Message = friendlyMessage,
                    IsCritical = type == LogType.Exception
                });

                // 致命错误时执行安全降级
                if (type == LogType.Exception)
                {
                    ExecuteSafeFallback();
                }
            }
        }

        private string GetFriendlyMessage(string condition)
        {
            // 将技术性错误信息转换为用户友好的提示
            if (condition.Contains("MissingReferenceException"))
                return "资源加载异常，请重试";
            if (condition.Contains("NullReferenceException"))
                return "发生未知错误，已记录日志";
            return "发生错误，但游戏将继续";
        }

        private void ExecuteSafeFallback()
        {
            // 安全降级逻辑
            Logger.Warn("Fallback", new { action = "executing_safe_mode" });
        }
    }

    public class GameErrorEvent
    {
        public string Message { get; set; }
        public bool IsCritical { get; set; }
    }
}
```

#### 资源加载错误处理

```csharp
namespace SquareFireline.Core
{
    public class SafeAssetLoader
    {
        private const string FallbackPrefabPath = "Prefabs/Fallback";

        public static async Task<T> LoadAssetWithFallback<T>(string assetPath) where T : UnityEngine.Object
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<T>(assetPath);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                    return handle.Result;
                else
                    throw new AssetLoadException($"Failed to load: {assetPath}");
            }
            catch (Exception e)
            {
                Logger.Error("AssetLoader", new { path = assetPath, error = e.Message });

                // 返回备用资源
                var fallbackHandle = Addressables.LoadAssetAsync<T>(FallbackPrefabPath);
                await fallbackHandle.Task;
                return fallbackHandle.Result;
            }
        }
    }

    public class AssetLoadException : Exception
    {
        public AssetLoadException(string message) : base(message) { }
    }
}
```

---

### 6. Testing Patterns - EditMode vs PlayMode 结构

#### EditMode 测试 - 纯逻辑

```csharp
// Assets/Tests/EditMode/Map/ChunkGenerationTests.cs
using NUnit.Framework;
using SquareFireline.Map;

namespace Tests.EditMode.Map
{
    [TestFixture]
    public class ChunkGenerationTests
    {
        private TilemapEndlessMapGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new TilemapEndlessMapGenerator();
        }

        [Test]
        public void GenerateChunk_WithSameSeed_ReturnsIdenticalTerrain()
        {
            // Arrange
            int seed = 12345;
            int2 chunkCoords = new int2(5, 0);

            // Act
            var chunk1 = _generator.GenerateChunk(seed, chunkCoords);
            var chunk2 = _generator.GenerateChunk(seed, chunkCoords);

            // Assert
            Assert.That(chunk1.Tiles, Is.EqualTo(chunk2.Tiles));
        }

        [Test]
        public void GenerateChunk_AdjacentChunks_HeightDifferenceAtMostOne()
        {
            // Arrange
            int seed = 54321;

            // Act
            var chunk1 = _generator.GenerateChunk(seed, new int2(0, 0));
            var chunk2 = _generator.GenerateChunk(seed, new int2(1, 0));

            // Assert
            int heightDiff = Mathf.Abs(chunk1.AverageHeight - chunk2.AverageHeight);
            Assert.That(heightDiff, Is.LessThanOrEqualTo(1));
        }

        [Test]
        public void GenerateChunk_StartProtectionZone_IsFlat()
        {
            // Arrange
            int seed = 99999;

            // Act
            var chunk1 = _generator.GenerateChunk(seed, new int2(0, 0));
            var chunk2 = _generator.GenerateChunk(seed, new int2(1, 0));
            var chunk3 = _generator.GenerateChunk(seed, new int2(2, 0));

            // Assert
            Assert.That(chunk1.IsFlat, Is.True);
            Assert.That(chunk2.IsFlat, Is.True);
            Assert.That(chunk3.IsFlat, Is.True);
        }
    }
}
```

#### PlayMode 测试 - 场景集成

```csharp
// Assets/Tests/PlayMode/Player/PlayerJumpTests.cs
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;
using SquareFireline.Player;

namespace Tests.PlayMode.Player
{
    public class PlayerJumpTests
    {
        private GameObject _player;
        private PlayerController _controller;

        [SetUp]
        public void SetUp()
        {
            // 加载测试场景
            yield return SceneManager.LoadSceneAsync("EndlessMapTest", LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("EndlessMapTest"));

            // 创建测试玩家
            _player = new GameObject("TestPlayer");
            _player.AddComponent<Rigidbody2D>();
            _player.AddComponent<CapsuleCollider2D>();
            _controller = _player.AddComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_player);
            SceneManager.UnloadSceneAsync("EndlessMapTest");
        }

        [UnityTest]
        public IEnumerator Player_WhenJumpPressed_JumpsWithFixedHeight()
        {
            // Arrange
            float expectedHeight = 5f;
            _controller.SetupTestMode(jumpForce: 10f);

            // Act
            _controller.TriggerJump();
            yield return new WaitForSeconds(0.5f);

            // Assert
            float actualHeight = _player.transform.position.y;
            Assert.That(actualHeight, Is.GreaterThan(expectedHeight - 0.5f));
        }

        [UnityTest]
        public IEnumerator Player_CanDoubleJump_WhenInAir()
        {
            // Arrange
            _controller.SetupTestMode();

            // Act - 第一次跳跃
            _controller.TriggerJump();
            yield return new WaitForSeconds(0.3f);

            // Act - 第二次跳跃（空中）
            float heightBeforeSecondJump = _player.transform.position.y;
            _controller.TriggerJump();
            yield return new WaitForSeconds(0.3f);
            float heightAfterSecondJump = _player.transform.position.y;

            // Assert
            Assert.That(heightAfterSecondJump, Is.GreaterThan(heightBeforeSecondJump));
        }

        [UnityTest]
        public IEnumerator Player_InputResponse_Under100ms()
        {
            // Arrange
            var stopwatch = new System.Diagnostics.Stopwatch();

            // Act
            stopwatch.Start();
            _controller.TriggerJump();
            bool jumped = _controller.IsJumping;
            stopwatch.Stop();

            // Assert
            Assert.That(jumped, Is.True);
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
        }
    }
}
```

#### 测试覆盖目标

| 系统 | EditMode 测试 | PlayMode 测试 | 目标覆盖率 |
|------|-------------|-------------|-----------|
| **Map** | Chunk 生成算法、高度差验证 | 场景加载、动态生成 | 100% |
| **Player** | 跳跃计算、地面检测逻辑 | 输入响应、物理交互 | 100% |
| **Obstacles** | 生成概率、位置验证 | 碰撞检测、死亡触发 | 90% |
| **Game** | 状态机流转、分数计算 | 流程集成、重生逻辑 | 95% |
| **UI** | 数据绑定逻辑 | UI 响应、显示更新 | 80% |
| **Core** | EventBus、Logger | 无 | 100% |

---

### 7. Code Organization Patterns - 系统内文件组织

#### 命名空间结构

```
SquareFireline/
├── Core/                  # 全局通用系统
│   ├── EventBus
│   ├── Logger
│   ├── ConfigManager
│   └── ServiceLocator
├── Game/                  # 游戏流程
│   ├── GameStateMachine
│   ├── ScoreSystem
│   └── CheckpointSystem
├── Player/                # 玩家控制
│   ├── PlayerController
│   ├── PlayerInput
│   └── PlayerAnimation
├── Map/                   # 地图生成
│   ├── TilemapMapConfig
│   ├── TilemapEndlessMapGenerator
│   ├── ChunkManager
│   └── Chunk
├── Obstacles/             # 障碍物
│   ├── ObstacleSpawner
│   ├── ObstacleConfig
│   └── Obstacle
├── UI/                    # 界面
│   ├── GameUIController
│   ├── MenuUIController
│   └── UIDataBinder
├── Audio/                 # 音频
│   └── AudioManager
└── Debug/                 # 调试工具（开发版）
    ├── DebugConsole
    └── PerformanceMonitor
```

#### 文件命名约定

| 类型 | 命名规范 | 示例 |
|------|---------|------|
| **MonoBehaviour** | `[功能] + .cs` | `PlayerController.cs`, `ChunkManager.cs` |
| **ScriptableObject** | `[功能]Config.cs` | `MapConfig.cs`, `GameBalanceConfig.cs` |
| **ECS Components** | `[功能]Component.cs` | `PlayerStateComponent.cs`, `ChunkDataComponent.cs` |
| **ECS Systems** | `[功能]System.cs` | `ChunkGenerationSystem.cs`, `CleanupSystem.cs` |
| **ECS Jobs** | `[功能]Job.cs` | `GenerateTerrainJob.cs`, `SpawnObstacleJob.cs` |
| **Events** | `[事件名]Event.cs` | `GameStartEvent.cs`, `PlayerDeathEvent.cs` |
| **Interfaces** | `I + [功能]` | `IInitializableSystem.cs`, `IDamageable.cs` |
| **Enums** | `[功能]Type.cs` | `GameState.cs`, `TileType.cs` |
| **ScriptableObject 资产** | `[功能]Config.asset` | `MapConfig.asset`, `ObstacleConfig.asset` |
| **Prefabs** | `[功能].prefab` | `Player.prefab`, `Chunk_01.prefab` |

#### 类内文件组织

```csharp
// 标准类结构模板
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquareFireline.SystemName
{
    /// <summary>
    /// 类的功能说明
    /// </summary>
    public class ClassName : MonoBehaviour
    {
        #region 单例模式（如适用）

        private static ClassName _instance;
        public static ClassName Instance => _instance;

        #endregion

        #region 序列化字段

        [Header("分类说明")]
        [Tooltip("字段用途说明")]
        [SerializeField] private int _configValue = 10;

        #endregion

        #region 事件

        public event Action OnStateChange;
        private event Action<int> _internalEvent;

        #endregion

        #region 属性

        public int PublicValue { get; private set; }
        private bool _isInitialized;

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateLogic();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 方法功能说明
        /// </summary>
        /// <param name="param">参数说明</param>
        /// <returns>返回值说明</returns>
        public void PublicMethod(int param)
        {
            // 实现
        }

        #endregion

        #region 私有方法

        private void Initialize()
        {
            // 初始化逻辑
        }

        private void UpdateLogic()
        {
            // 更新逻辑
        }

        private void Cleanup()
        {
            // 清理逻辑
        }

        #endregion
    }
}
```

#### ScriptableObject 资产组织

```
Assets/ScriptableObjects/
├── Configs/
│   ├── Map/
│   │   ├── MapConfig.asset
│   │   └── TileConfig.asset
│   ├── Player/
│   │   └── PlayerConfig.asset
│   ├── Obstacles/
│   │   └── ObstacleConfig.asset
│   └── Game/
│       ├── GameBalanceConfig.asset
│       └── DifficultyConfig.asset
└── Events/
    ├── GameStartEvent.asset
    ├── GameOverEvent.asset
    └── ScoreUpdateEvent.asset
```

#### Prefab 组织

```
Assets/Prefabs/
├── Characters/
│   ├── Player.prefab
│   └── Enemies/
├── Map/
│   ├── Chunks/
│   │   ├── Chunk_Grass_01.prefab
│   │   ├── Chunk_Grass_02.prefab
│   │   └── Chunk_Desert_01.prefab
│   ├── Tiles/
│   │   ├── Tile_Grass.prefab
│   │   └── Tile_Dirt.prefab
│   └── Decorations/
├── Obstacles/
│   ├── Obstacle_Rock.prefab
│   ├── Obstacle_Spike.prefab
│   └── Obstacle_Bird.prefab
└── UI/
    ├── GameUI.prefab
    └── MenuUI.prefab
```

---

## Step 8: Validation

**状态：** 完成
**目标：** 验证架构文档的完整性、一致性和需求覆盖率

---

### 8.1 根据 GDD 需求审查架构

#### 游戏支柱验证

| GDD 支柱 | 架构支持方式 | 验证状态 |
|---------|------------|---------|
| **简单易上手** | - 单按钮输入设计（New Input System 抽象）<br>- 快速加载（Addressables 异步加载）<br>- 直观 UI（UI Toolkit） | ✅ 支持 |
| **流畅操作** | - 输入响应 <100ms（MonoBehaviour 直接输入处理）<br>- 60FPS 稳定（ECS/DOTS + Burst Compiler）<br>- 物理系统（Unity Physics 2D） | ✅ 支持 |
| **无限重玩价值** | - 程序化地图生成（ECS ChunkGenerationSystem）<br>- Perlin Noise 地形（Job 并行计算）<br>- 种子生成系统（可复现测试） | ✅ 支持 |
| **快速重试** | - 立即重生（GameStateMachine 状态管理）<br>- 检查点系统（CheckpointSystem）<br>- 对象池优化（ObjectPool<Chunk>） | ✅ 支持 |

#### GDD 功能需求映射

| GDD 需求类别 | 架构系统 | 实现模式 | 验证状态 |
|-------------|---------|---------|---------|
| **FR-PC01~06 玩家控制** | PlayerController, PlayerInput | MonoBehaviour + ECS 混合 | ✅ 支持 |
| **FR-MG01~07 地图生成** | TilemapEndlessMapGenerator, ChunkManager | ECS + Jobs | ✅ 支持 |
| **FR-OB01~05 障碍物系统** | ObstacleSpawner, ObstacleConfig | ECS + ScriptableObject | ✅ 支持 |
| **FR-CP01~05 碰撞检测** | Unity Physics 2D | 引擎内置 + 事件监听 | ✅ 支持 |
| **FR-DR01~04 死亡重生** | GameStateMachine, CheckpointSystem | 状态机 + 数据持久化 | ✅ 支持 |
| **FR-UI01~06 UI 反馈** | GameUIController, UIDataBinder | UI Toolkit + 数据绑定 | ✅ 支持 |
| **FR-GP01~05 流程管理** | GameBootstrap, SystemInitializer | Bootstrap + 依赖注入 | ✅ 支持 |
| **FR-DS01~03 数据持久化** | SaveSystem (JSON) | 序列化 + PlayerPrefs | ✅ 支持 |
| **FR-TS01~04 测试验证** | EditMode + PlayMode 测试 | Unity Test Framework | ✅ 支持 |

---

### 8.2 Epic 需求验证

#### Epic 1: 核心玩家控制器

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| 玩家 GameObject 设置 | Player.prefab (Rigidbody2D + CapsuleCollider2D) | ✅ 支持 |
| 地面检测系统 | PlayerController.IsGrounded (Physics2D.Raycast) | ✅ 支持 |
| 一段跳实现 | PlayerController.Jump() (固定高度计算) | ✅ 支持 |
| 二段跳实现 | PlayerController.DoubleJump() (空中状态检测) | ✅ 支持 |
| 跳跃手感调优 | 可配置 JumpForce/GravityScale (ScriptableObject) | ✅ 支持 |
| 落地缓冲期 | PlayerStateComponent.CoyoteTime (浮点计时器) | ✅ 支持 |
| 土狗时间 | PlayerInput 缓冲窗口 (InputAction.performed) | ✅ 支持 |

#### Epic 2: 基础地图系统

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| Tilemap 系统设置 | Grid + Tilemap GameObject | ✅ 支持 |
| Chunk 预制体设计 | Chunk_XX.prefab (20×5 瓦片) | ✅ 支持 |
| 程序化生成算法 | ChunkGenerationSystem (Perlin Noise + Job) | ✅ 支持 |
| 动态 Chunk 生成 | ChunkManager.SpawnChunk() (对象池) | ✅ 支持 |
| 动态 Chunk 清理 | ChunkCleanupSystem (距离检测) | ✅ 支持 |
| 基础地形生成 | TilemapMapConfig (ScriptableObject 配置) | ✅ 支持 |
| 相邻高度差限制 | ValidateHeightDifference Job (≤1 约束) | ✅ 支持 |
| 起始保护区 | MapGenerator.SafeZoneChunkCount = 3 | ✅ 支持 |

#### Epic 3: 障碍物与碰撞

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| 障碍物预制体 | Obstacle_Rock/Spike.prefab | ✅ 支持 |
| 障碍物生成算法 | ObstacleSpawner (概率控制) | ✅ 支持 |
| 空隙生成 | MapGenerator.GenerateGap() (10% 概率) | ✅ 支持 |
| 碰撞检测 | Physics2D.OnCollisionEnter2D | ✅ 支持 |
| 死亡触发逻辑 | EventBus.Publish<PlayerDeathEvent>() | ✅ 支持 |
| 障碍物密度配置 | ObstacleConfig.SpawnProbability | ✅ 支持 |

#### Epic 4: 游戏流程管理

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| 游戏状态机 | GameStateMachine (Menu/Playing/GameOver) | ✅ 支持 |
| 分数系统 | ScoreSystem (生存时间/距离) | ✅ 支持 |
| 检查点系统 | CheckpointSystem (LastSafePosition) | ✅ 支持 |
| 重生逻辑 | PlayerController.Respawn(Vector3 position) | ✅ 支持 |
| 分数重置 | ScoreSystem.Reset() (死亡时调用) | ✅ 支持 |
| 历史最高分 | SaveSystem.Load/SaveBestScore() | ✅ 支持 |

#### Epic 5: UI 系统

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| 主界面场景 | MenuUI.prefab (UI Toolkit) | ✅ 支持 |
| 游戏内 UI | GameUI.prefab (分数显示) | ✅ 支持 |
| 最高分显示 | UIDataBinder.Bind(SaveSystem.BestScore) | ✅ 支持 |
| 新纪录提示 | EventBus.Subscribe<NewRecordEvent>() | ✅ 支持 |
| 数据持久化 | SaveSystem (JSON + PlayerPrefs) | ✅ 支持 |

#### Epic 6: 地图生成扩展

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| 生物群系定义 | BiomeConfig (ScriptableObject 数组) | ✅ 支持 |
| 群系视觉差异 | TileReplacerSystem (Tile 替换) | ✅ 支持 |
| 群系解锁条件 | SaveSystem.UnlockedBiomes (PlayerPrefs) | ✅ 支持 |
| 难度曲线系统 | DifficultyCurveConfig (AnimationCurve) | ✅ 支持 |
| 动态难度调整 | ObstacleSpawner.AdjustDifficulty(float time) | ✅ 支持 |

#### Epic 7: 音频系统

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| 音频管理器 | AudioManager (Singleton) | ✅ 支持 |
| 背景音乐播放 | AudioManager.PlayBGM(AudioClip) | ✅ 支持 |
| 音效播放 | AudioManager.PlaySFX(AudioClip) | ✅ 支持 |
| 音量控制 | AudioManager.SetVolume(float value) | ✅ 支持 |

#### Epic 8: Polish 与优化

| 需求 | 架构组件 | 实现状态 |
|------|---------|---------|
| 玩家动画 | PlayerAnimation (Animator + Mecanim) | ✅ 支持 |
| 粒子效果 | ParticleSystem (Jump/Dust/Death) | ✅ 支持 |
| 性能优化 | ECS + Burst Compiler + Jobs | ✅ 支持 |
| 内存优化 | ObjectPool<Chunk> + Addressables | ✅ 支持 |

---

### 8.3 PRD 非功能需求验证

| NFR 编号 | 需求 | 架构支持 | 验证方式 | 状态 |
|---------|------|---------|---------|------|
| **NFR-P01** | 60FPS 稳定 | ECS/DOTS + Burst Compiler | Profiler 监测 | ✅ 支持 |
| **NFR-P02** | 输入延迟 <100ms | MonoBehaviour 直接输入处理 | InputSystem 测试 | ✅ 支持 |
| **NFR-P03** | 加载时间 <3 秒 | Addressables 异步加载 | 加载时间测试 | ✅ 支持 |
| **NFR-P04** | 内存合理占用 | ObjectPool + Chunk 动态清理 | 内存分析器 | ✅ 支持 |
| **NFR-R01** | 崩溃率 <1% | GlobalErrorHandler + 异常捕获 | 崩溃日志统计 | ✅ 支持 |
| **NFR-R02** | 数据不丢失 | JSON 持久化 + PlayerPrefs | 断电恢复测试 | ✅ 支持 |
| **NFR-M01** | XML 文档注释 | 代码模板强制要求 | 代码审查 | ✅ 支持 |
| **NFR-M02** | 100% 测试覆盖 | Unity Test Framework + NUnit | 覆盖率报告 | ✅ 支持 |
| **NFR-M03** | CLAUDE.md 规范 | 命名空间/命名规则强制执行 | 静态分析 | ✅ 支持 |
| **NFR-A01** | 色盲友好 | UI Toolkit 高对比度 USS 样式 | 视觉审查 | ✅ 支持 |

---

### 8.4 架构决策一致性检查

| 决策编号 | 决策内容 | 与 Epic 需求一致性 | 验证状态 |
|---------|---------|-----------------|---------|
| **1** | ECS/DOTS 架构 | ✅ 支持程序化生成 + 性能需求 | 一致 |
| **2** | State Machine + ECS | ✅ 支持游戏流程管理 | 一致 |
| **3** | C# 事件 + ScriptableObject | ✅ 支持系统间低耦合通信 | 一致 |
| **4** | JSON 序列化 | ✅ 支持数据持久化需求 | 一致 |
| **5** | Addressables | ✅ 支持异步加载 + 内存管理 | 一致 |
| **6** | ObjectPool<Chunk> | ✅ 支持 Chunk 动态管理 | 一致 |
| **7** | UI Toolkit | ✅ 支持现代化 UI 需求 | 一致 |
| **8** | Unity Test Framework | ✅ 支持 100% 测试覆盖目标 | 一致 |

---

### 8.5 待决问题与风险

#### 已识别风险

| 风险 ID | 描述 | 影响 | 缓解措施 | 状态 |
|--------|------|------|---------|------|
| **RISK-01** | ECS/DOTS 学习曲线 | 开发效率可能降低 | 已提供详细代码模板 + 示例 | 🟡 已缓解 |
| **RISK-02** | 二段跳手感调优难度大 | 影响核心操作体验 | ScriptableObject 配置支持快速迭代 | 🟡 已缓解 |
| **RISK-03** | 单人开发进度受限 | 开发周期可能延长 | AI 辅助开发 + 优先级管理 | 🟡 监控中 |

#### 技术债务预警

| 债务项 | 产生原因 | 计划偿还时间 |
|-------|---------|-------------|
| 混合架构复杂度 | MonoBehaviour + ECS 并存 | Epic 8 Polish 阶段重构评估 |
| 事件系统重复 | C# Events + ScriptableObject Events | 统一为单一事件总线（可选） |

---

### 8.6 验证结论

**架构验证状态：** ✅ **通过**

**验证摘要：**
- ✅ 所有 GDD 游戏支柱得到架构支持
- ✅ 所有 Epic 1-8 需求有对应架构组件
- ✅ 所有 PRD 非功能需求有技术保障措施
- ✅ 所有架构决策相互一致，无冲突
- ✅ 风险已识别并有缓解措施

**准备进入：** Step 9: Complete & Handoff

---

## Step 9: Complete & Handoff

**状态：** 完成
**目标：** 完成架构文档并准备进入实现阶段

### 9.1 文档完整性检查

- [x] Frontmatter 更新为 `stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]`
- [x] Document Status 更新为 "9 of 9"
- [x] 所有 9 个 Step 内容完整
- [x] 代码示例格式正确
- [x] 表格数据对齐

### 9.2 实施准备清单

| 准备项 | 状态 | 说明 |
|-------|------|------|
| **架构文档** | ✅ 完成 | 本文件 `_bmad-output/game-architecture.md` |
| **GDD 文档** | ✅ 完成 | `_bmad-output/gdd.md` |
| **Epic 文档** | ✅ 完成 | `_bmad-output/epics.md` |
| **PRD 文档** | ✅ 完成 | `_bmad-output/planning-artifacts/prd.md` |
| **项目规范** | ✅ 完成 | `CLAUDE.md` 包含完整开发规范 |
| **技术栈** | ✅ 确定 | Unity 2022.3.62f3 LTS + URP + ECS/DOTS |
| **测试框架** | ✅ 确定 | Unity Test Framework + NUnit |

### 9.3 推荐实施顺序

根据 Epic 依赖关系图，推荐以下实施顺序：

```
Phase 1 (基础系统):
  ├── Epic 1: 核心玩家控制器
  └── Epic 2: 基础地图系统

Phase 2 (核心循环):
  ├── Epic 3: 障碍物与碰撞
  └── Epic 4: 游戏流程管理

Phase 3 (用户体验):
  ├── Epic 5: UI 系统
  └── Epic 7: 音频系统

Phase 4 (扩展内容):
  └── Epic 6: 地图生成扩展

Phase 5 (Polish):
  └── Epic 8: Polish 与优化
```

### 9.4 下一步行动

1. **创建技术规格文档** - 为 Epic 1 创建详细的技术规格（Tech Spec）
2. **设置测试场景** - 配置 `Assets/Tests/` 目录结构
3. **创建 ScriptableObject 配置** - 建立 `MapConfig.asset` 等基础配置
4. **实施 Epic 1** - 开始玩家控制器开发

---

## 附录：架构文档索引

| 文档 | 路径 | 说明 |
|------|------|------|
| **Game Architecture** | `_bmad-output/game-architecture.md` | 本文件 - 技术架构设计 |
| **Game Design Document** | `_bmad-output/gdd.md` | 游戏设计文档 |
| **Development Epics** | `_bmad-output/epics.md` | Epic 和用户故事列表 |
| **Product Requirements** | `_bmad-output/planning-artifacts/prd.md` | 产品需求文档 |
| **Project Guidelines** | `CLAUDE.md` | 项目开发规范 |

---

**文档创建日期：** 2026-03-16
**文档版本：** 1.0
**作者：** liruoyu
**状态：** 完成 (9 of 9 Steps)
