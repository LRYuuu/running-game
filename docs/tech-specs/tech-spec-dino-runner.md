---
title: 'Chrome 小恐龙风格平台跳跃游戏'
slug: 'dino-runner-mini-game'
created: '2026-03-11'
status: 'in-progress'
stepsCompleted: [1, 2]
tech_stack:
  - Unity 2022.3.62f3 (LTS)
  - C#
  - Unity Physics2D
  - Unity UI (uGUI)
  - Unity 2D Tilemap
files_to_modify: []
code_patterns:
  - 单例模式
  - [SerializeField] Inspector 配置
  - Tooltip 属性说明
  - 事件委托模式
test_patterns: []
---

# Tech-Spec: Chrome 小恐龙风格平台跳跃游戏

**Created:** 2026-03-11

## Overview

### Problem Statement

需要一个简单、快速开发的 Chrome 小恐龙风格平台跳跃游戏，开发周期短，复杂度低，用于快速验证核心玩法机制。

### Solution

在现有 Runner's Journey Unity 项目中实现基础 Runner 游戏机制。使用 **Unity 2D Tilemap 系统**实现程序化地形生成（多层平台、起伏地形、坑洞陷阱），使用**对象池 + 预制体**实现障碍生成（地面障碍、飞鸟）。包括玩家奔跑（逐帧动画）、二段跳跃、随时间加速、碰撞检测、得分系统和游戏结束/重新开始功能。采用像素卡通可爱美术风格，支持全平台适配（PC、移动端）。

### Scope

**In Scope:**
- 玩家控制（奔跑逐帧动画、二段跳跃）
- **Tilemap 程序化地形生成**（多层平台、起伏地形、坑洞）
- 地面障碍生成（仙人掌类，需要跳跃）
- 飞鸟障碍生成（空中障碍，需要 timing 跳跃）
- 陷阱/坑洞生成（需要跳过）
- 随时间加速机制
- 碰撞检测（障碍、陷阱、掉落）
- 得分系统（生存时间/距离）
- 游戏结束处理
- 重新开始功能
- 简单的 UI（得分显示）

**Out of Scope:**
- 主菜单系统
- 音效和背景音乐
- 最高分记录（本地存储）
- 道具系统
- 下蹲机制
- 多人模式
- 社交分享功能

## Context for Development

### Codebase Patterns

**项目目录结构：**
- `Assets/Scripts/` - 脚本目录（空，待创建）
- `Assets/Prefabs/` - 预制体目录（空，待创建）
- `Assets/Scenes/` - 场景目录（空，待创建）
- `Assets/Art/` - 美术资源目录（包含 Animations/, Materials/, Models/, Sprites/, Textures/）
- `ProjectSettings/Physics2DSettings.asset` - 2D 物理配置（重力 Y=-9.81）

**项目状态：Confirmed Clean Slate**
- ✅ 所有游戏代码已清空，从零开始
- ✅ 无历史包袱，可自由设计架构
- ✅ Unity 2D 物理已配置，可直接使用

**技术环境：**
- Unity 版本：2022.3.62f3c1 (LTS 稳定版)
- 2D 物理已配置（重力 Y=-9.81）
- 无现有代码约束，可自由选择编码规范

### Files to Reference

| File | Purpose |
|------|---------|
| `Assets/Scenes/DinoRunnerGame.unity` | 新创建的游戏主场景（含 Tilemap） |
| `Assets/Scripts/PlayerController.cs` | 新创建的玩家控制器（奔跑 + 二段跳 + 动画）|
| `Assets/Scripts/GameManager.cs` | 新创建的游戏状态管理 |
| `Assets/Scripts/TerrainGenerator.cs` | 新创建的地形生成器（Tilemap 程序化生成）|
| `Assets/Scripts/ObstacleSpawner.cs` | 新创建的障碍物生成器 |
| `Assets/Scripts/ObstacleMove.cs` | 新创建的障碍物移动逻辑 |
| `Assets/Scripts/ScoreManager.cs` | 新创建的得分管理 |
| `Assets/Scripts/GameSpeedController.cs` | 新创建的游戏速度控制器（加速机制）|
| `Assets/Tileset/` | 新创建的 Tileset 资源目录 |
| `Assets/Prefabs/GroundObstacle.prefab` | 新创建的地面障碍预制体 |
| `Assets/Prefabs/BirdObstacle.prefab` | 新创建的飞鸟障碍预制体 |
| `Assets/Prefabs/PitTrap.prefab` | 新创建的陷阱预制体 |
| `Assets/Prefabs/Player.prefab` | 新创建的玩家预制体 |
| `Assets/Sprites/` | 美术素材目录（PNG 素材导入）|
| `ProjectSettings/Physics2DSettings.asset` | 2D 物理配置参考 |

### Technical Decisions

1. **架构选择**：使用简单的组件模式，每个游戏元素独立脚本
2. **输入系统**：同时支持鼠标点击（PC）和触摸输入（移动端）+ 空格键（PC 测试）
3. **物理系统**：使用 Unity 内置 Rigidbody2D + Collider2D（项目已有 2D 物理配置）
4. **二段跳实现**：使用跳跃计数器（jumpCount），允许最多 2 次跳跃
5. **加速机制**：使用游戏速度控制器，随时间线性增加 obstacle moveSpeed
6. **障碍类型**：三种预制体（地面障碍、飞鸟、陷阱），对象池生成
7. **代码规范**：新代码采用 `[SerializeField]` + `Tooltip` 模式（Unity 标准）
8. **全平台适配**：使用 Unity Input 系统 + 响应式 UI
9. **场景管理**：创建独立游戏场景 `DinoRunnerGame.unity`
10. **美术风格**：像素卡通可爱风格，使用 SpriteRenderer + SpriteAtlas
11. **地形生成**：使用 Unity Tilemap 系统，程序化随机生成（有规则的随机）
12. **地形设计**：多层平台、起伏地形、坑洞陷阱，使用地形块序列生成
13. **玩家动画**：使用 Animator + 逐帧 Sprite 序列

## Implementation Plan

### Tasks

#### Phase 0: 美术资源导入（新增）

**Task 0: 导入 Tileset 和素材**
- [ ] 创建目录 `Assets/Sprites/` 和 `Assets/Tileset/`
- [ ] 导入 PNG 素材包到 `Assets/Sprites/`
- [ ] 切割 Tileset 并创建 Tiles：
  - `Assets/Tileset/GroundTile.asset` - 地面 Tile
  - `Assets/Tileset/GrassTile.asset` - 草地表面 Tile
  - `Assets/Tileset/DirtTile.asset` - 土壤 Tile
  - `Assets/Tileset/PlatformTile.asset` - 平台 Tile
  - `Assets/Tileset/SlopeUpTile.asset` - 上坡 Tile
  - `Assets/Tileset/SlopeDownTile.asset` - 下坡 Tile
  - `Assets/Tileset/PitTile.asset` - 坑洞边缘 Tile
- [ ] 导入玩家精灵序列：
  - `Assets/Sprites/Player/Run_00.png` ~ `Run_07.png`（8 帧奔跑动画）
  - `Assets/Sprites/Player/Jump_00.png` ~ `Jump_03.png`（4 帧跳跃动画）
- [ ] 导入障碍精灵：
  - `Assets/Sprites/Obstacles/Cactus.png`
  - `Assets/Sprites/Obstacles/Bird_00.png`, `Bird_01.png`（2 帧拍翅）
  - `Assets/Sprites/Obstacles/Trap.png`

#### Phase 1: 核心玩法 (基础循环)

**Task 1: 创建游戏场景（含 Tilemap）**
- [ ] 创建 `Assets/Scenes/DinoRunnerGame.unity`
- [ ] 设置 Camera 为正交相机 (Orthographic)
- [ ] 创建 Tilemap 层级：
  ```
  Grid (GameObject)
  └── GroundTilemap (Tilemap)
       └── Tilemap Collider 2D
       └── Tilemap Renderer
  ```
- [ ] 设置背景颜色或添加背景 Sprite
- [ ] 保存场景

**Task 2: 创建玩家控制器**
- [ ] 创建 `Assets/Scripts/PlayerController.cs`
  ```csharp
  - [SerializeField] float jumpForce = 7f
  - [SerializeField] float gravityScale = 2.5f
  - [SerializeField] int maxJumpCount = 2  // 二段跳
  - private int jumpCount = 0
  - private bool isGrounded = false
  - Update(): 检测 Input.GetMouseButtonDown(0) 或 Input.GetKeyDown(KeyCode.Space)
  - Jump(): 当 jumpCount < maxJumpCount 时施加向上力，jumpCount++
  - OnCollisionEnter2D(): 检测接地，重置 jumpCount = 0
  - 添加 Rigidbody2D + BoxCollider2D/CircleCollider2D
  - 添加 Animator 组件，控制奔跑/跳跃动画
  ```
- [ ] 创建玩家 Prefab `Assets/Prefabs/Player.prefab`
  - 创建空 GameObject，添加 SpriteRenderer + Rigidbody2D + Collider2D + Animator
  - 挂载 PlayerController.cs 脚本
  - 设置 Rigidbody2D: gravityScale = 2.5, freezeRotationZ = true
  - 创建 Animator Controller，设置奔跑/跳跃动画状态机

**Task 3: 创建地形生成器（核心）**
- [ ] 创建 `Assets/Scripts/TerrainGenerator.cs`
  ```csharp
  - 单例模式
  - [SerializeField] Tilemap groundTilemap
  - [SerializeField] TileBase[] groundTiles  // 各种地面 Tile
  - [SerializeField] float chunkLength = 20f  // 每次生成的长度
  - [SerializeField] float minPlatformHeight = 2f
  - [SerializeField] float maxPlatformHeight = 6f
  - 地形块类型枚举：Flat, SlopeUp, SlopeDown, Platform, Pit
  - 生成规则：
    * 不能连续 2 个 Pit
    * SlopeUp 后必须跟 Flat 或 SlopeDown
    * 平台高度变化不超过 maxPlatformHeight
  - GenerateChunk(float startX, float length): 生成一段地形
  - ClearTerrain(): 清除旧地形（用于重新开始）
  ```

**Task 4: 创建障碍物系统**
- [ ] 创建 `Assets/Scripts/ObstacleMove.cs`
  ```csharp
  - [SerializeField] float moveSpeed = -5f
  - [SerializeField] float speedMultiplier = 1.0f  // 用于加速
  - Update(): transform.Translate(moveSpeed * speedMultiplier * Time.deltaTime, 0, 0)
  - 当超出屏幕左边界时销毁 GameObject
  ```
- [ ] 创建 `Assets/Scripts/ObstacleSpawner.cs`
  ```csharp
  - [SerializeField] GameObject[] obstaclePrefabs  // 3 种预制体
  - [SerializeField] float minSpawnInterval = 1.5f
  - [SerializeField] float maxSpawnInterval = 3.0f
  - Coroutine: 随机间隔生成随机障碍类型
  - 权重控制：地面障碍 60%, 飞鸟 25%, 陷阱 15%
  - 生成位置需要考虑当前地形高度
  ```

**Task 5: 创建三种障碍预制体**
- [ ] 地面障碍 `Assets/Prefabs/GroundObstacle.prefab`
  - GameObject + SpriteRenderer + BoxCollider2D
  - 像素卡通风格（仙人掌/石头等）
  - 高度适中，需要跳跃越过
- [ ] 飞鸟障碍 `Assets/Prefabs/BirdObstacle.prefab`
  - GameObject + SpriteRenderer + BoxCollider2D + Animator
  - 生成位置在空中（玩家跳跃高度附近）
  - 2 帧拍翅动画
- [ ] 陷阱/坑洞 `Assets/Prefabs/PitTrap.prefab`
  - 可见陷阱精灵 + BoxCollider2D
  - 或设计为地面缺口（需要 Tilemap 配合）

#### Phase 2: 游戏状态管理

**Task 6: 创建游戏管理器**
- [ ] 创建 `Assets/Scripts/GameManager.cs`
  ```csharp
  - 单例模式
  - 枚举 GameState: Playing, GameOver
  - [SerializeField] PlayerController player
  - [SerializeField] TerrainGenerator terrainGenerator
  - [SerializeField] ObstacleSpawner spawner
  - [SerializeField] GameSpeedController speedController
  - OnCollisionEnter2D(): 检测碰撞，设置 GameState.GameOver
  - Restart(): 清除地形缓存，重新加载场景
  ```

**Task 7: 创建游戏速度控制器**
- [ ] 创建 `Assets/Scripts/GameSpeedController.cs`
  ```csharp
  - [SerializeField] float startSpeed = 5f
  - [SerializeField] float maxSpeed = 15f
  - [SerializeField] float speedIncreasePerSecond = 0.1f
  - public float CurrentSpeed { get; private set; }
  - Update(): CurrentSpeed = Mathf.Min(startSpeed + speedIncreasePerSecond * Time.time, maxSpeed)
  - 通知 terrainGenerator 和 obstacleSpawner 更新速度
  ```

**Task 8: 创建得分系统**
- [ ] 创建 `Assets/Scripts/ScoreManager.cs`
  ```csharp
  - 单例模式
  - int score (基于生存时间或距离)
  - AddScore(int amount): score += amount
  - ResetScore(): score = 0
  - 事件 OnScoreChanged
  - Update(): 每帧或定时增加分数
  ```

#### Phase 3: UI 系统

**Task 9: 创建 UI Canvas**
- [ ] 创建 Canvas `Assets/Scenes/DinoRunnerGame.unity` 中
  - 创建 UI -> Canvas
  - 设置 Canvas Scaler: Scale With Screen Size
  - 参考分辨率：1920x1080

**Task 10: 创建 UI 元素**
- [ ] 创建得分文本 `ScoreText` (TextMeshPro 或 UI.Text)
  - 锚点：顶部居中
  - 字体大小：72
  - 像素字体（可选）
- [ ] 创建游戏结束面板 `GameOverPanel`
  - 初始隐藏
  - 包含 "Game Over" 文本
  - 包含当前得分显示
  - 包含 "Restart" 按钮
- [ ] 创建 `Assets/Scripts/RunnerUI.cs`
  ```csharp
  - [SerializeField] Text scoreText
  - [SerializeField] GameObject gameOverPanel
  - [SerializeField] Button restartButton
  - Update(): 更新分数显示
  - OnGameOver(): 显示 GameOverPanel
  - OnRestartButton(): 调用 GameManager.Restart()
  ```

#### Phase 4: 地形生成规则细化

**Task 11: 实现地形生成算法**

地形块序列生成示例：
```
起始地形：[Flat] × 5  // 确保玩家有安全的起始区域

游戏进行中动态生成：
[Flat] → [Flat] → [SlopeUp] → [Platform] → [Platform] → [SlopeDown] → [Flat] → [Pit] → [Flat] ...

约束规则：
- pitProbability = 0.15f  // 15% 概率生成坑洞
- 不能连续 Pit
- SlopeUp 后必须跟 Platform 或 Flat
- 平台高度变化限制在 [minHeight, maxHeight]
```

伪代码：
```csharp
TileType lastTile = TileType.Flat;
TileType GenerateNextTile() {
    if (lastTile == TileType.Pit) {
        // 坑洞后必须是 Flat
        lastTile = TileType.Flat;
        return lastTile;
    }
    if (lastTile == TileType.SlopeUp) {
        // 上坡后生成平台或平地
        lastTile = Random.value > 0.5f ? TileType.Platform : TileType.Flat;
        return lastTile;
    }

    float rand = Random.value;
    if (rand < pitProbability) {
        lastTile = TileType.Pit;
    } else if (rand < 0.3f) {
        lastTile = TileType.SlopeUp;
    } else if (rand < 0.5f) {
        lastTile = TileType.SlopeDown;
    } else {
        lastTile = TileType.Flat;
    }
    return lastTile;
}
```

#### Phase 5: 美术与动画

**Task 12: 设置玩家动画**
- [ ] 创建 Animator Controller `Assets/Animations/PlayerAnimator.controller`
- [ ] 创建动画状态：
  - `Idle` - 待机动画（可选）
  - `Run` - 奔跑动画（8 帧循环）
  - `Jump` - 跳跃动画（4 帧单次）
- [ ] 设置状态转换条件：
  - `isGrounded` (bool) - 地面/空中
  - `speed` (float) - 移动速度
- [ ] 在 PlayerController 中控制动画参数

**Task 13: 配置输入系统**
- [ ] 确认输入检测支持：
  - PC: 鼠标左键 (Input.GetMouseButtonDown(0)) + 空格键 (Input.GetKeyDown(KeyCode.Space))
  - 移动端：触摸输入 (Input.GetMouseButtonDown(0) 同样适用)
  - 二段跳：同一输入，在空中未达最大跳跃次数时再次触发

---

### Acceptance Criteria

**AC1: 玩家控制**
- [ ] Given 玩家点击屏幕/按下空格，When 玩家在地面上，Then 角色向上跳跃
- [ ] Given 玩家在空中且跳跃次数 < 2，When 再次点击，Then 执行二段跳
- [ ] Given 玩家跳跃次数 = 2，When 在空中再次点击，Then 不响应跳跃
- [ ] Given 玩家在地面移动，When Animator 检测速度，Then 播放奔跑动画

**AC2: 地形生成**
- [ ] Given 游戏开始，When 生成地形，Then 生成至少 5 个安全平地块
- [ ] Given 地形生成中，When 生成坑洞，Then 下一个地形块不能是坑洞
- [ ] Given 上坡地形，When 生成下一个地形，Then 是平台或平地
- [ ] Given 地形生成，Then 平台高度变化不超过设定范围

**AC3: 障碍物生成**
- [ ] Given 游戏开始，When 计时器触发，Then 生成随机类型的障碍（地面/飞鸟/陷阱）
- [ ] Given 障碍生成，Then 从屏幕右侧向左移动
- [ ] Given 障碍生成位置，Then 需要匹配当前地形高度

**AC4: 碰撞检测**
- [ ] Given 玩家碰到地面障碍或飞鸟，When 碰撞发生，Then 游戏结束
- [ ] Given 玩家碰到陷阱/掉落坑洞，When 触发条件，Then 游戏结束
- [ ] Given 玩家掉出屏幕底部，When 超出边界，Then 游戏结束

**AC5: 加速机制**
- [ ] Given 游戏进行中，When 时间流逝，Then 障碍移动速度逐渐增加
- [ ] Given 速度达到最大值，When 继续游戏，Then 速度不再增加（上限保护）

**AC6: 得分系统**
- [ ] Given 游戏进行中，When 时间流逝，Then 分数持续增加（基于时间/距离）
- [ ] Given 游戏结束，When 查看 UI，Then 显示最终得分

**AC7: 重新开始**
- [ ] Given 游戏结束，When 点击重新开始按钮，Then 清除地形缓存，重置游戏状态，重新开始

**AC8: 边界情况**
- [ ] Given 障碍移出屏幕左方，When 超出视野，Then 自动销毁障碍对象（内存管理）
- [ ] Given 玩家在二段跳最高点，When 开始下落，Then 正常受重力影响
- [ ] Given Tilemap 地形，When 玩家站在 Tile 上，Then Tilemap Collider 正确检测碰撞

## Additional Context

## Additional Context

### Dependencies

| 依赖 | 类型 | 说明 |
|------|------|------|
| Unity Physics2D | 内置 | Rigidbody2D, Collider2D, Physics2D |
| Unity UI (uGUI) | 内置 | Canvas, Text, Button |
| Unity Input System | 内置 | Input.GetMouseButtonDown, Input.GetKeyDown |
| Unity 2D Tilemap | 内置 | Tilemap, TilemapRenderer, TilemapCollider2D, RuleTile |

### Testing Strategy

**手动测试清单：**
- [ ] 玩家一段跳跃手感测试（调整 jumpForce 和 gravityScale）
- [ ] 玩家二段跳时机测试（空中再次跳跃窗口）
- [ ] 接地检测准确性测试（确保落地后重置跳跃次数）
- [ ] Tilemap 碰撞检测测试（确保玩家站在 Tile 上）
- [ ] 地形生成规则测试（无连续坑洞、坡度约束）
- [ ] 三种障碍生成权重测试（60%/25%/15%）
- [ ] 障碍碰撞检测准确性测试
- [ ] 加速机制平滑性测试（从 startSpeed 到 maxSpeed）
- [ ] 移动端触摸输入响应测试
- [ ] PC 空格键输入响应测试
- [ ] 游戏结束后重新开始流程测试
- [ ] 地形清除和重新生成测试

**性能测试：**
- [ ] 长时间运行无内存泄漏（Tilemap 动态生成/清除）
- [ ] 移动端帧率稳定 60FPS
- [ ] 大量障碍同时存在时无卡顿

### Notes

**风险提示：**
- Tilemap 动态生成需要正确管理，避免内存泄漏
- 二段跳的接地检测需要精确，否则可能出现无限跳跃或无法二段跳
- 坑洞设计需要视觉提示，否则玩家无法预判
- 加速机制需要平衡，过快会导致无法反应
- 地形生成规则需要充分测试，避免产生无法通过的地形

**关键数值调优：**
- jumpForce: 影响跳跃高度，需要与平台高度匹配
- gravityScale: 影响下落速度，影响游戏手感
- spawnInterval: 影响障碍密度，需要随速度动态调整
- speedIncreasePerSecond: 影响难度曲线
- platformHeightRange: 影响地形起伏程度
- pitProbability: 影响坑洞频率（建议 0.1~0.2）

**Tilemap 使用建议：**
- 使用 RuleTile 可以自动处理 Tile 之间的拼接（如草地边缘）
- Tilemap Collider 2D 可以优化为 Used By Composite + Composite Collider 2D 以提升性能
- 动态生成时使用 `tilemap.SetTilesBlock()` 批量设置比逐个设置性能更好

**后续迭代建议：**
- 音效系统（跳跃音效、得分音效、碰撞音效）
- 主菜单系统（开始按钮、设置）
- 最高分记录（PlayerPrefs 本地存储）
- 下蹲机制（躲避飞鸟）
- 连击/成就系统
- 更多地形块类型和组合
- 生物群系（森林、沙漠、雪地等）

**开发时间估算：**
- Phase 0 (美术导入): 1-2 小时（取决于素材包大小）
- Phase 1 (核心玩法：玩家控制 + 地形生成): 4-5 小时
- Phase 2 (游戏状态 + 加速机制): 1-2 小时
- Phase 3 (UI 系统): 1 小时
- Phase 4 (地形生成规则细化): 2-3 小时
- Phase 5 (美术动画): 1-2 小时
- **总计**: 约 10-15 小时

---

**技术规格已更新 - 使用 Tilemap 程序化地形生成方案**

**主要变更：**
- 地形方案：纯预制体生成 → Unity Tilemap 程序化生成
- 新增 Phase 0：美术资源导入和 Tileset 配置
- 新增 Task：TerrainGenerator（地形生成器）
- 新增地形规则：约束随机生成（不能连续坑洞、坡度约束等）
- 新增玩家动画：Animator + 逐帧 Sprite 序列
- 预计时间：5-7 小时 → 10-15 小时

**下一步选项：**
```
[a] Advanced Elicitation - 深入挖掘需求（如地形规则细化）
[c] Continue - 继续审查步骤，确认最终规格
[q] Questions - 询问任何问题
[p] Party Mode - 邀请其他专家参与审查
```
