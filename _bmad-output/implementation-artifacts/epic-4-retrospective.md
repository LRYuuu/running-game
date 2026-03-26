# Epic 4 回顾会议 - 游戏流程管理

**项目**: Runner's Journey
**Epic**: Epic 4 - 游戏流程管理
**回顾日期**: 2026-03-20
**主持人**: Bob (Scrum Master)
**参与者**: Alice (Product Owner), Charlie (Senior Dev), Dana (QA Engineer), Elena (Junior Dev), liruoyu (Project Lead)

---

## 📊 Epic 4 完成度

### 故事完成状态

| 故事 | 优先级 | 状态 | 说明 |
|------|--------|------|------|
| 4-1: 游戏状态管理 | P0 | ✅ done | GameState 枚举、状态机、ChangeState 方法 |
| 4-2: 分数系统 | P0 | ✅ done | 分数累加、最高分、PlayerPrefs 持久化 |
| 4-3: 立即重生 | P0 | ✅ done | StartRespawn 协程、快速重生流程 |
| 4-4: 检查点系统 | P0 | ✅ done | Checkpoint 类、动态生成、距离激活 |
| 4-5: 死亡时重置分数 | P0 | ✅ done | OnGameStateChanged 中调用 ResetScore() |
| 4-6: 最高分保存 | P0 | ✅ done | 验证 4-2 已实现的最高分功能 |
| 4-7: 游戏配置 ScriptableObject | P1 | ✅ done | GameConfig、ScoreConfig 配置类 |

**完成率**: 7/7 故事 (100%) - Epic 4 已完成 ✅

---

## 🎉 第一部分：庆祝成功

### 达成的里程碑

1. **完整的游戏流程管理系统** - 实现了 GameState 枚举 (Waiting/Playing/Dying/Respawning) 和状态机
2. **分数系统** - 实现了实时累加分数、历史最高分、PlayerPrefs 持久化
3. **检查点机制** - 实现了基于距离的自动激活、死亡后重生到检查点位置
4. **配置驱动架构** - 创建了 GameConfig 和 ScoreConfig ScriptableObject，所有参数可调整
5. **事件驱动设计** - 使用 OnGameStateChanged、OnScoreChanged 事件解耦系统

### 技术亮点

- ✅ GameManager 单例模式，跨场景持久化
- ✅ ScoreManager 单例模式，分数不丢失
- ✅ Checkpoint 动态生成和激活逻辑
- ✅ 空值保护，配置未设置时使用默认值
- ✅ 12+ 个 EditMode 测试用例覆盖

---

## 👍 第二部分：保持的做法（进展顺利的方面）

### 技术实践

| 做法 | 说明 | 影响 |
|------|------|------|
| **ScriptableObject 配置** | GameConfig、ScoreConfig 将参数外置 | 设计师可调参数，无需修改代码 |
| **单例模式** | GameManager、ScoreManager 使用单例 | 全局访问方便，跨场景持久化 |
| **事件订阅模式** | ScoreManager 订阅 OnGameStateChanged | 系统解耦，自动响应状态变化 |
| **空值保护** | 配置未设置时使用默认值 | 避免 NullReferenceException |
| **测试先行** | 每个 Story 都有对应测试 | 快速验证、减少回归 bug |

### 流程实践

| 做法 | 说明 | 影响 |
|------|------|------|
| **Story 文档模板化** | Implementation Notes 给出代码示例 | 实现起来顺畅，减少理解成本 |
| **配置与代码分离** | 所有魔法数字移到 ScriptableObject | 参数调整不需要重新编译 |
| **事件驱动架构** | 系统间通过事件通信 | 降低耦合，易于扩展 |

---

## 🔄 第三部分：改进机会

### 遇到的挑战

| 挑战 | 根本原因 | 解决方案 |
|------|----------|----------|
| Story 4-6 是验证型 Story | 功能在 4-2 已实现，4-6 只是确认 | 未来类似验证型 Story 合并到实现 Story |
| 检查点激活半径未实测 | checkpointActivateRadius 参数靠理论设定 | 创建实测流程或配置验证工具 |
| UI 系统未实现 | Epic 4 聚焦核心流程 | Epic 5 优先实现 UI 显示 |

### 技术债务

| 债务项 | 严重性 | 影响范围 | 建议解决时间 |
|--------|--------|----------|--------------|
| UI 系统未实现 | 中 | 玩家看不到分数 | Epic 5 优先实现 |
| 检查点可视化缺失 | 低 | 编辑器调试不便 | 添加 Gizmos 调试 |
| 配置验证工具缺失 | 低 | 参数平衡靠猜测 | 创建配置验证器 |
| PlayerPrefs 未加密 | 低 | 单机游戏无影响 | 如做联机功能需加密 |

---

## 💡 第四部分：经验教训

### 技术经验

| 教训 | 场景 | 应用建议 |
|------|------|----------|
| **事件驱动优于直接调用** | ScoreManager 订阅 GameState 变化 | 后续系统间通信优先使用事件 |
| **配置与代码分离** | GameConfig/ScoreConfig | 所有可调参数都使用 ScriptableObject |
| **单例适合全局管理器** | GameManager、ScoreManager | 跨场景 persistent 的对象用单例 |
| **PlayerPrefs 适合简单持久化** | 最高分存储 | 轻量数据直接用 PlayerPrefs |
| **协程适合延迟操作** | StartRespawn 使用 IEnumerator | 延迟重生、动画等待等场景 |

### 流程经验

| 教训 | 场景 | 应用建议 |
|------|------|----------|
| **验证型 Story 可合并** | 4-6 是 4-2 的验证 | 未来类似情况合并到实现 Story |
| **Story 依赖关系要明确** | 4-5 依赖 4-2 的 ResetScore() | 规划 Story 时明确标注依赖 |
| **配置参数需要实际测试验证** | checkpointActivateRadius 需要实测 | 创建配置验证工具或实测流程 |
| **Implementation Notes 很宝贵** | 故事文档中的代码示例 | 继续使用模板，保持代码示例 |

---

## 📋 第五部分：行动计划（应用于 Epic 5）

### 过程改进

| 行动项 | 负责人 | 优先级 | 验证方式 |
|--------|--------|--------|----------|
| UI 事件订阅 GameManager 和 ScoreManager | Developer | 高 | 代码审查检查事件订阅 |
| UI 配置使用 ScriptableObject | Developer | 高 | 检查 UI 参数是否可配置 |
| 分数显示格式可配置 | Developer | 中 | UIConfig 添加格式字符串 |
| 创建 UI 测试场景 | Developer | 中 | 独立场景测试 UI 功能 |

### 技术改进

| 行动项 | 负责人 | 优先级 | 验证方式 |
|--------|--------|--------|----------|
| 为 Checkpoint 添加 Gizmos 调试可视化 | Developer | 中 | 编辑器下显示激活半径 |
| 创建 UI 动画配置 | Developer | 低 | 动画时长、缓动函数可配 |
| 分数格式化 (如 1,000 分隔符) | Developer | 低 | 大分数显示更清晰 |
| 创建 MapConfig 验证工具 | Developer | 低 | 运行时验证可玩性参数 |

---

## 🎯 第六部分：Epic 5 准备

### Epic 5 故事列表和依赖关系

| 故事 | 说明 | 依赖 | 状态 |
|------|------|------|------|
| 5-1: 主界面 | 开始游戏 UI | 无 | backlog |
| 5-2: 当前分数显示 | 游戏中显示分数 | 依赖 ScoreManager.OnScoreChanged | backlog |
| 5-3: 历史最高分显示 | 显示最高分 | 依赖 ScoreManager.HighScore | backlog |
| 5-4: 破纪录提示 | 打破纪录时通知 | 依赖 5-2、5-3 | backlog |
| 5-5: 持久化最高分 | 关闭游戏不丢失 | 已在 4-6 实现 | backlog |
| 5-6: 生物群系选择 | 选择已解锁群系 | 依赖 Epic 6 的群系系统 | backlog |

### 从 Epic 4 学到的经验应用到 Epic 5

| Epic 4 经验 | Epic 5 应用 |
|-------------|-------------|
| ScriptableObject 配置模式 | 创建 UIConfig 配置 UI 参数 |
| 事件订阅模式 | UI 订阅 OnScoreChanged 事件更新分数 |
| 单例模式 | UIManager 使用单例管理 UI 状态 |
| 空值保护 | UI 配置未设置时使用默认值 |
| 测试先行 | 为 UI 逻辑编写 EditMode 测试 |

### Epic 5 风险提示

| 风险 | 可能性 | 影响 | 缓解措施 |
|------|--------|------|----------|
| UI Toolkit 学习曲线 | 中 | 中 | 先学习 UI Toolkit 基础再实现 |
| 分数显示性能 | 低 | 低 | 避免每帧更新 UI 文本 |
| 多分辨率适配 | 中 | 中 | 使用 Canvas Scaler 和参考分辨率 |
| 5-6 依赖 Epic 6 | 高 | 中 | 调整优先级或移到 Epic 6 后 |

---

## 📝 回顾会议总结

### Epic 4 亮点
- ✅ 7/7 故事 100% 完成
- ✅ 完整的游戏流程管理系统 (Waiting→Playing→Dying→Respawning→Playing)
- ✅ 配置驱动架构 (GameConfig、ScoreConfig)
- ✅ 事件驱动设计 (OnGameStateChanged、OnScoreChanged)
- ✅ 12+ 个测试用例覆盖

### 关键收获
1. ScriptableObject 配置模式适合所有可调参数
2. 事件订阅模式减少系统耦合
3. 单例模式适合全局管理器
4. 验证型 Story 可合并到实现 Story
5. Implementation Notes 中的代码示例大幅提升实现效率

### Epic 5 改进重点
1. UI 系统使用事件订阅 ScoreManager
2. UI 参数使用 ScriptableObject 配置 (UIConfig)
3. 为 Checkpoint 添加 Gizmos 调试可视化
4. 调整 5-5 和 5-6 的优先级 (5-5 已在 4-6 实现，5-6 依赖 Epic 6)

### 下一冲刺重点
- 开始 **Epic 5（UI 系统）** 规划
- 实现主界面、分数显示、最高分显示、破纪录提示
- 应用 Epic 4 经验：配置驱动、事件订阅、测试先行

---

**行动计划执行检查点**: 在 Epic 5 完成后，回顾本行动计划执行情况，评估改进效果。

---

*文档生成时间*: 2026-03-20
*文档版本*: 1.0
*状态*: Done ✅
