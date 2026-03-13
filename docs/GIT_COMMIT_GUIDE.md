# Git 提交规范

本项目采用语义化提交规范，确保提交历史清晰、可追溯。

---

## 提交格式

```
<type>: <subject>

<body>
```

### 结构说明

- **type**（必填）：提交类型，见下表
- **subject**（必填）：简短描述，不超过 50 字符
- **body**（可选）：详细说明，每行不超过 72 字符

---

## 提交类型（type）

| 类型 | 说明 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat(map): 实现无尽地图滚动系统` |
| `fix` | 修复 Bug | `fix(scene): 修复玩家碰撞体大小` |
| `docs` | 文档变更 | `docs: 添加项目设计文档` |
| `style` | 代码格式（不影响功能） | `style: 格式化代码缩进` |
| `refactor` | 重构（非新功能、非 Bug 修复） | `refactor(player): 重构玩家状态机` |
| `chore` | 构建过程、辅助工具、配置等变动 | `chore: 更新项目配置文件` |
| `perf` | 性能优化 | `perf: 优化 Tilemap 渲染性能` |
| `test` | 添加或修改测试 | `test(map): 添加地图生成单元测试` |

---

## 主题行（subject）规范

1. **使用祈使句**：用"添加"而不是"添加了"
2. **首字母小写**（中文无此要求）
3. **末尾不加句号**
4. **保持简洁**：不超过 50 字符

---

## 正文（body）规范

1. **使用项目符号列表**描述详细变更
2. **每行不超过 72 字符**
3. **说明"为什么"而不是"是什么"**（代码本身已说明是什么）

---

## 完整示例

### 示例 1：新功能
```
feat(map): 实现无尽地图滚动系统

- 重写 TilemapEndlessMapGenerator：地图向左滚动，玩家位置固定
- 修改 PlayerController：移除自动移动，添加 X 轴位置锁定
- 新增 TilemapMapConfig 配置类
```

### 示例 2：Bug 修复
```
fix(scene): 修复 login.unity 场景配置

- 添加 Grid 和 MapGenerator 对象
- 修复 PlayerController 脚本引用丢失
- 调整玩家初始位置到地面 (y=0)
- 修复 BoxCollider2D 大小 (0.0001 -> 1)
```

### 示例 3：资源配置
```
feat(assets): 添加游戏资源文件

- 添加 GroundTile 纹理和瓦片资源
- 添加 GroundPalette 预设
- 添加 MapConfig 配置资产
- 添加 Plugins 和 Prefabs 目录
```

### 示例 4：文档更新
```
docs: 添加项目和设计文档

- 添加 CLAUDE.md 项目开发规范
- 添加 MapGenerationPlan.md 地图生成计划
- 添加 TileRequirements.md 瓦片需求文档
```

### 示例 5：清理工作
```
chore: 清理过时的 README 文件

- 删除 Assets/Scripts/README_BackgroundSetup.md
```

---

## 提交策略

### 原子提交

每个提交应该是**原子的**，即：
- 完成一个独立的功能/修复
- 可以独立编译运行
- 回滚后不会影响其他功能

### 推荐拆分方式

| 变更内容 | 建议提交 |
|----------|----------|
| 核心代码逻辑 | `feat/fix/refactor: <模块>: 描述` |
| 场景配置 | `fix(scene): 描述` |
| 资源文件 | `feat(assets): 描述` |
| 项目配置 | `chore: 更新项目配置文件` |
| 文档 | `docs: 描述` |
| 清理文件 | `chore: 清理/删除 描述` |

### 不推荐的提交方式

```bash
# 错误：一次性提交所有变更
git add .
git commit -m "update"
```

```bash
# 正确：分类提交
git add Assets/Scripts/Map/
git commit -m "feat(map): 实现地图生成器"

git add Assets/Scenes/login.unity
git commit -m "fix(scene): 修复场景配置"

git add Assets/Art/
git commit -m "feat(assets): 添加纹理资源"
```

---

## 实际操作流程

### 1. 查看变更
```bash
git status
git diff
```

### 2. 分类暂存
```bash
# 按模块/类型分别暂存
git add Assets/Scripts/Map/
git add Assets/Scenes/login.unity
git add Assets/Art/Textures/
```

### 3. 逐个提交
```bash
git commit -m "feat(map): 实现地图生成器"
git commit -m "fix(scene): 修复场景配置"
git commit -m "feat(assets): 添加纹理资源"
```

### 4. 推送远程
```bash
git push origin main
```

---

## 常用命令速查

```bash
# 查看提交历史
git log --oneline -10

# 查看文件变更
git diff <file>

# 撤销暂存
git restore --staged <file>

# 修改最后一次提交
git commit --amend -m "新消息"

# 查看状态
git status
```

---

## 协作规范

### 分支命名

| 分支类型 | 命名格式 | 示例 |
|----------|----------|------|
| 主分支 | `main` | - |
| 功能分支 | `feat/<功能名>` | `feat/map-system` |
| 修复分支 | `fix/<问题名>` | `fix/player-collision` |
| 实验分支 | `exp/<实验名>` | `exp/new-renderer` |

### 合并规范

- 功能开发完成后合并到 `main`
- 合并前确保编译通过
- 使用 `git merge --no-ff` 保留分支历史

---

## 附录：Commit Message 模板

```bash
# 功能开发
feat(<scope>): <描述>

- 新增/修改/删除 ...
- ...

# Bug 修复
fix(<scope>): <描述>

- 问题原因：...
- 修复方案：...

# 资源配置
feat(assets): <描述>

- 添加 ... 资源
- 配置 ... 资产

# 文档更新
docs: <描述>

- 添加/更新 ... 文档

# 清理工作
chore: <描述>

- 删除/清理 ... 文件
```
