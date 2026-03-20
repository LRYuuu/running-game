# Story 4-7: 作为开发者，我希望有游戏配置 ScriptableObject，以便调整参数

**Epic:** Epic 4 - 游戏流程管理
**优先级:** P1
**估算:** 1h
**状态:** ready-for-dev
**创建日期:** 2026-03-20

---

## Story

作为开发者，
我希望有游戏配置 ScriptableObject，
以便调整参数。

---

## Acceptance Criteria

### 功能性验收标准

1. **GameManager 配置**
   - Given 需要调整游戏状态参数
   - When 修改 GameConfig ScriptableObject
   - Then 能够配置 respawnDelay、initialState 等参数

2. **ScoreManager 配置**
   - Given 需要调整分数系统参数
   - When 修改 ScoreConfig ScriptableObject
   - Then 能够配置 scorePerSecond、scoreInterval 等参数

3. **Checkpoint 配置**
   - Given 需要调整检查点参数
   - When 修改 CheckpointConfig ScriptableObject
   - Then 能够配置 checkpoint 相关参数

### 技术性验收标准

1. **使用 ScriptableObject 存储配置**
   - Given 创建配置文件
   - When 使用 `[CreateAssetMenu]` 特性
   - Then 能在 Unity Editor 中创建和编辑配置

2. **配置与代码分离**
   - Given 游戏脚本需要读取配置
   - When 通过 `[SerializeField]` 引用 ScriptableObject
   - Then 配置值在 Inspector 中可调整

3. **默认值处理**
   - Given ScriptableObject 未赋值
   - When 脚本运行
   - Then 有合理的默认值避免错误

---

## Tasks / Subtasks

### Task 1: 创建 GameConfig ScriptableObject
- [x] 创建 `GameConfig.cs` 类
- [x] 添加 respawnDelay、initialState 等字段
- [x] 添加 `[CreateAssetMenu]` 特性

### Task 2: 创建 ScoreConfig ScriptableObject
- [x] 创建 `ScoreConfig.cs` 类
- [x] 添加 scorePerSecond、scoreInterval 等字段
- [x] 迁移 ScoreManager 中的现有配置字段

### Task 3: 修改 GameManager 使用配置
- [x] 添加 `_gameConfig` 序列化字段
- [x] 在 StartRespawn 中使用 GameConfig.respawnDelay
- [x] 确保空值处理

### Task 4: 修改 ScoreManager 使用配置
- [x] 添加 `_scoreConfig` 序列化字段
- [x] 移除硬编码的 `_scorePerSecond` 和 `_scoreInterval`
- [x] 从配置对象读取参数

### Task 5: 创建配置资源
- [ ] 在 Unity Editor 中创建 GameConfig.asset
- [ ] 在 Unity Editor 中创建 ScoreConfig.asset
- [ ] 设置合理的默认值

### Task 6: 编写测试用例
- [x] 创建 GameConfigTests.cs
- [x] 创建 ScoreConfigTests.cs

---

## Architecture & Design

### 配置系统架构图

```
┌─────────────────────────────────────────────────────────────┐
│                    ScriptableObjects                         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────┐              ┌─────────────────┐       │
│  │   GameConfig    │              │  ScoreConfig    │       │
│  │ ScriptableObject│              │ ScriptableObject│       │
│  ├─────────────────┤              ├─────────────────┤       │
│  │ + respawnDelay  │              │ + scorePerSecond│       │
│  │ + initialState  │              │ + scoreInterval │       │
│  │ + ...           │              │ + ...           │       │
│  └────────┬────────┘              └────────┬────────┘       │
│           │                                │                 │
│           │ [SerializeField]               │ [SerializeField]│
│           ▼                                ▼                 │
│  ┌─────────────────┐              ┌─────────────────┐       │
│  │   GameManager   │              │  ScoreManager   │       │
│  └─────────────────┘              └─────────────────┘       │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 文件位置

| 文件 | 路径 | 操作 |
|------|------|------|
| `GameConfig.cs` | `Assets/Scripts/Game/GameConfig.cs` | 新建 |
| `ScoreConfig.cs` | `Assets/Scripts/Game/ScoreConfig.cs` | 新建 |
| `GameManager.cs` | `Assets/Scripts/Game/GameManager.cs` | 修改 |
| `ScoreManager.cs` | `Assets/Scripts/Game/ScoreManager.cs` | 修改 |

### 命名空间规范

- 游戏管理：`SquareFireline.Game`

---

## Implementation Notes

### GameConfig.cs 示例

```csharp
using UnityEngine;

namespace SquareFireline.Game
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Square Fireline/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("重生配置")]
        [Tooltip("重生延迟时间（秒）")]
        public float respawnDelay = 1.0f;

        [Header("游戏状态")]
        [Tooltip("初始游戏状态")]
        public GameState initialState = GameState.Waiting;
    }
}
```

### ScoreConfig.cs 示例

```csharp
using UnityEngine;

namespace SquareFireline.Game
{
    [CreateScriptableObject(fileName = "ScoreConfig", menuName = "Square Fireline/Score Config")]
    public class ScoreConfig : ScriptableObject
    {
        [Header("分数配置")]
        [Tooltip("每秒增加的分数")]
        public int scorePerSecond = 1;

        [Tooltip("分数累加间隔（秒）")]
        public float scoreInterval = 1f;
    }
}
```

### 修改 GameManager

```csharp
// 添加字段
[SerializeField] private GameConfig _gameConfig;

// 在 Awake 或 Start 中使用
private void Start()
{
    if (_gameConfig != null)
    {
        respawnDelay = _gameConfig.respawnDelay;
        ChangeState(_gameConfig.initialState);
    }
}
```

### 修改 ScoreManager

```csharp
// 添加字段
[SerializeField] private ScoreConfig _scoreConfig;

// 在 Awake 中读取配置
private void Awake()
{
    // ...单例初始化...

    if (_scoreConfig != null)
    {
        _scorePerSecond = _scoreConfig.scorePerSecond;
        _scoreInterval = _scoreConfig.scoreInterval;
    }
}
```

---

## Testing Requirements

### EditMode 测试

创建 `Assets/Tests/EditMode/Game/GameConfigTests.cs`：

```csharp
using NUnit.Framework;
using UnityEngine;
using SquareFireline.Game;

namespace SquareFireline.Tests.EditMode.Game
{
    [TestFixture]
    public class GameConfigTests
    {
        [Test]
        public void GameConfig_CreateAsset_HasDefaultValues()
        {
            // Arrange & Act
            var config = ScriptableObject.CreateInstance<GameConfig>();

            // Assert
            Assert.AreEqual(1.0f, config.respawnDelay);
            Assert.AreEqual(GameState.Waiting, config.initialState);
        }
    }
}
```

---

## Developer Context

### 与 Epic 4 其他故事的关系

| Story | 关系 |
|-------|------|
| 4-1 | GameManager 使用 GameConfig 配置初始状态 |
| 4-2 | ScoreManager 使用 ScoreConfig 配置分数参数 |
| 4-3/4-4 | GameConfig 配置 respawnDelay |

### 扩展点（未来 Epic）

- **Epic 5**: UI 配置（面板动画时长、分数显示格式等）
- **Epic 6**: 地图生成配置（Chunk 尺寸、生成概率等）
- **Epic 7**: 音频配置（音量、音效列表等）

---

## References

- [Unity ScriptableObject 官方文档](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [GameManager.cs](C:\shiyou-workspace\Square Fireline\Assets\Scripts\Game\GameManager.cs)
- [ScoreManager.cs](C:\shiyou-workspace\Square Fireline\Assets\Scripts\Game\ScoreManager.cs)

---

## 下一步行动

1. 创建 GameConfig.cs 和 ScoreConfig.cs
2. 修改 GameManager 和 ScoreManager 使用配置
3. 在 Unity Editor 中创建配置资源
4. 编写测试用例
5. 更新 sprint-status.yaml 标记 4-7 为 done

**Story 状态**: done

---

*文档创建时间*: 2026-03-20
*文档版本*: 1.1
*状态*: Done ✅

---

## Dev Agent Record

### Agent Model Used
qwen3.5-plus

### Implementation Plan
1. 创建 GameConfig.cs 和 ScoreConfig.cs ScriptableObject 类
2. 修改 GameManager 使用 GameConfig.respawnDelay
3. 修改 ScoreManager 从 ScoreConfig 读取配置参数
4. 创建 EditMode 测试用例验证配置默认值
5. 更新 sprint-status.yaml 标记 4-7 为 done

### Debug Log
- ✅ GameConfig.cs 创建完成，包含 respawnDelay、initialState、checkpointActivateRadius 字段
- ✅ ScoreConfig.cs 创建完成，包含 scorePerSecond、scoreInterval 字段
- ✅ GameManager.cs 修改完成，从 GameConfig 读取 respawnDelay
- ✅ ScoreManager.cs 修改完成，从 ScoreConfig 读取配置参数
- ✅ GameConfigTests.cs 创建完成，4 个测试用例
- ✅ ScoreConfigTests.cs 创建完成，3 个测试用例

### Completion Notes List
- Story 4-7 实现完成
- 配置与代码分离，可在 Unity Editor 中调整参数
- 空值处理已添加，未配置时使用默认值
- Task 5（在 Unity Editor 中创建配置资源）需要在 Editor 中手动执行

### File List

| 文件 | 状态 | 修改说明 |
|------|------|----------|
| `Assets/Scripts/Game/GameConfig.cs` | ✅ 新建 | GameConfig ScriptableObject 类 |
| `Assets/Scripts/Game/ScoreConfig.cs` | ✅ 新建 | ScoreConfig ScriptableObject 类 |
| `Assets/Scripts/Game/GameManager.cs` | ✅ 已修改 | 添加 _gameConfig 字段，使用 GameConfig.respawnDelay |
| `Assets/Scripts/Game/ScoreManager.cs` | ✅ 已修改 | 添加 _scoreConfig 字段，从 ScoreConfig 读取参数 |
| `Assets/Tests/EditMode/Game/GameConfigTests.cs` | ✅ 新建 | GameConfig 测试用例 |
| `Assets/Tests/EditMode/Game/ScoreConfigTests.cs` | ✅ 新建 | ScoreConfig 测试用例 |
| `_bmad-output/implementation-artifacts/stories/4-7-*.md` | ✅ 已更新 | 任务完成标记 |

### Change Log
- Story 4-7 实现完成：游戏配置 ScriptableObject 功能 (2026-03-20)
- 测试用例添加：7 个新测试用例（GameConfig 4 个，ScoreConfig 3 个）
- sprint-status.yaml 更新：Story 4-7 状态更新为 done
