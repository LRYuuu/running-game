# Runner's Journey

> 一款通过程序化生成提供无限重玩价值的 2D 横板跳跃游戏

[![Unity](https://img.shields.io/badge/Unity-2022.3.62f3%20LTS-000000?style=flat&logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20iOS%20%7C%20Android-lightgrey)]()
[![License](https://img.shields.io/badge/license-MIT-green)]()

---

## 游戏简介

**Runner's Journey** 是一款 2D 横板无尽跑酷游戏。玩家通过简单的单键操作控制角色跳跃，躲避障碍物和空隙，挑战个人最高分。

### 核心特色

- **程序化地图生成** - 每次游戏体验都是独特的，提供无限重玩价值
- **流畅的操作手感** - 一段跳 + 二段跳系统，响应时间 <100ms
- **动态地形起伏** - 基于 Perlin Noise 算法创造自然流畅的地形变化
- **多生物群系** - 草地、沙漠、雪地，随进度解锁
- **难度递增系统** - 随游戏进行动态调整障碍物密度和地形复杂度

---

## 快速开始

### 系统要求

| 最低配置 | 说明 |
|---------|------|
| **操作系统** | Windows 10 64-bit |
| **处理器** | 双核 2.0GHz+ |
| **内存** | 4GB RAM |
| **显卡** | 支持 DirectX 11，2GB VRAM |
| **存储空间** | 500MB 可用空间 |

### 安装步骤

1. 克隆或下载本仓库
2. 使用 **Unity Hub** 打开项目
3. Unity 版本：**2022.3.62f3c1** (LTS)
4. 打开场景 `Assets/Scenes/login.unity`
5. 点击 Play 按钮开始游戏

### 操作方式

| 平台 | 跳跃控制 |
|------|---------|
| **PC (键盘)** | 空格键 (Space) |
| **移动端 (触屏)** | 点击屏幕任意位置 |
| **主机 (手柄)** | A 按钮 (Xbox) / X 按钮 (PlayStation) |

**二段跳**: 在空中再次按下跳跃键即可触发

---

## 项目结构

```
Runner's Journey/
├── Assets/
│   ├── Scripts/           # 源代码
│   │   ├── Game/          # 游戏流程管理
│   │   ├── Map/           # 地图生成系统
│   │   ├── Player/        # 玩家控制器
│   │   ├── Obstacles/     # 障碍物系统
│   │   ├── UI/            # UI 系统
│   │   └── Editor/        # 编辑器工具
│   ├── Prefabs/           # 预制体
│   ├── Scenes/            # 场景文件
│   ├── ScriptableObjects/ # 配置数据
│   ├── Art/               # 美术资源
│   ├── Audio/             # 音频资源
│   └── Tests/             # 测试代码
│       ├── EditMode/      # 纯逻辑测试
│       └── PlayMode/      # 场景集成测试
├── docs/                  # 项目文档
├── _bmad-output/          # BMad 生成的文档
└── CLAUDE.md              # 项目开发规范
```

---

## 开发进度

| Epic | 名称 | 状态 | 进度 |
|------|------|------|------|
| 1 | 核心玩家控制器 | ✅ 完成 | 7/7 |
| 2 | 基础地图系统 | ✅ 完成 | 8/8 |
| 3 | 障碍物与碰撞 | ✅ 完成 | 7/7 |
| 4 | 游戏流程管理 | ✅ 完成 | 7/7 |
| 5 | UI 系统 | ✅ 完成 | 6/6 |
| 6 | 地图生成扩展 | ✅ 完成 | 6/6 |
| 7 | 音频系统 | 🔄 进行中 | 5/5 |
| 8 | Polish 与优化 | ⏳ 待开始 | 0/6 |

**当前里程碑**: 功能完整 🔄 进行中

---

## 技术架构

### 技术栈

- **引擎**: Unity 2022.3.62f3 LTS
- **渲染管线**: URP (Universal Render Pipeline)
- **后端**: IL2CPP + .NET Standard 2.1
- **架构**: x86_64
- **目标帧率**: 60 FPS

### 核心系统设计

| 系统 | 架构模式 | 说明 |
|------|---------|------|
| **玩家控制** | MonoBehaviour + ECS 混合 | 输入响应 + 状态同步 |
| **地图生成** | ECS/DOTS + Jobs | 并行 Chunk 生成 |
| **障碍物** | ECS + ScriptableObject | 数据驱动生成 |
| **游戏流程** | State Machine | 清晰状态流转 |
| **UI 系统** | UI Toolkit + 数据绑定 | 样式分离 |
| **事件系统** | C# 事件 + ScriptableObject | 解耦通信 |
| **数据持久化** | JSON 序列化 | 支持复杂结构 |

### 命名空间规范

| 路径 | 命名空间 |
|------|----------|
| `Scripts/Map/` | `RunnersJourney.Map` |
| `Scripts/Player/` | `RunnersJourney.Player` |
| `Scripts/Obstacles/` | `RunnersJourney.Obstacles` |
| `Scripts/UI/` | `RunnersJourney.UI` |
| `Scripts/Game/` | `RunnersJourney.Game` |
| `Scripts/Audio/` | `RunnersJourney.Audio` |

---

## 开发指南

### 环境设置

1. 安装 **Unity Hub** 和 **Unity 2022.3.62f3c1**
2. 安装 **Visual Studio 2022** 或 **Rider**
3. 打开项目后，等待 Unity 完成首次导入和编译

### 代码规范

- **类/方法**: `PascalCase`
- **私有字段**: `_camelCase` (下划线前缀)
- **参数**: `camelCase`
- **日志格式**: `Debug.Log($"[{ClassName}] 消息")`

详细规范请参阅 [`CLAUDE.md`](CLAUDE.md)

### Git 提交规范

**格式**: `<type>(<scope>): <subject>`

```bash
# 示例
git commit -m "feat(map): 添加空隙生成功能"
git commit -m "fix(player): 修复跳跃检测"
git commit -m "refactor(ui): 清理 Editor 代码"
```

| type | 说明 | scope |
|------|------|-------|
| `feat` | 新功能 | map, player, ui, obstacles |
| `fix` | 修复 | |
| `docs` | 文档 | |
| `refactor` | 重构 | |
| `test` | 测试 | |

### 运行测试

```bash
# 在 Unity 编辑器中
Window > General > Test Runner

# 或使用命令行
Unity -batchmode -runTests -testPlatform EditMode
Unity -batchmode -runTests -testPlatform PlayMode
```

---

## 文档索引

### 设计文档

- [游戏设计文档 (GDD)](_bmad-output/gdd.md)
- [产品需求文档 (PRD)](_bmad-output/planning-artifacts/prd.md)
- [开发 Epics](_bmad-output/epics.md)
- [游戏架构](_bmad-output/game-architecture.md)

### 技术文档

- [代码规范](CLAUDE.md)
- [UI 开发规范](Assets/Scripts/UI/UI_CONFIG.md)
- [地图生成设计](docs/game-design/MapGenerationPlan.md)
- [地形变化实现](docs/game-design/TerrainVariationPlan-Implementation.md)

### 进度跟踪

- [Sprint 状态](_bmad-output/implementation-artifacts/sprint-status.yaml)
- [Epic 回顾](_bmad-output/implementation-artifacts/)

---

## 项目目标

### 产品目标

| 目标 | 说明 |
|------|------|
| **简单易上手** | 30 秒内理解操作，1 分钟内上手 |
| **流畅操作** | 输入响应 <100ms，稳定 60FPS |
| **无限重玩价值** | 程序化地图生成，每次体验独特 |
| **快速重试** | 死亡后立即重生，无等待 |

### 技术目标

| 指标 | 目标值 |
|------|--------|
| **帧率稳定性** | 稳定 60FPS |
| **输入延迟** | <100ms |
| **加载时间** | 冷启动 <3 秒 |
| **崩溃率** | <1% |
| **测试覆盖率** | 核心系统 100% |

---

## 许可证

本项目采用 [MIT 许可证](LICENSE)

---

## 作者与贡献

- **作者**: liruoyu
- **创建日期**: 2026-03-16
- **最后更新**: 2026-04-08

### 贡献指南

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m "feat: add amazing feature"`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 提交 Pull Request

---

## 致谢

本项目使用以下工具和技术：

- [Unity](https://unity.com/) - 游戏引擎
- [BMad Method](https://www.bmadmethod.com/) - 结构化开发方法
- [Unity Test Framework](https://docs.unity3d.com/Manual/com.unity.test-framework.html) - 测试框架

---

<div align="center">
  <strong>Runner's Journey</strong> - 开始你的旅程，挑战无限可能！
</div>
