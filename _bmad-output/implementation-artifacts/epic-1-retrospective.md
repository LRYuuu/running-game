# Epic 1 回顾 - 核心玩家控制器

**完成日期**: 2026-03-17
**状态**: 完成 (7/7 Stories)

---

## 故事完成情况

| Story | 描述 | 状态 |
|-------|------|------|
| 1-1 | 作为玩家可以按跳跃键起跳 | ✅ done |
| 1-2 | 作为玩家可以在空中再次跳跃（二段跳） | ✅ done |
| 1-3 | 作为玩家希望跳跃响应迅速 | ✅ done |
| 1-4 | 作为玩家希望落地后可以立即再次跳跃 | ✅ done |
| 1-5 | 作为玩家希望走出平台边缘后有短暂跳跃窗口（土狼时间） | ✅ done |
| 1-6 | 作为开发者希望有地面检测系统 | ✅ done |
| 1-7 | 作为开发者希望有跳跃参数配置 | ✅ done |

---

## 技术实现总结

### 核心组件

1. **PlayerJumpController.cs** - 主跳跃控制器
   - 处理输入检测和跳跃执行
   - 管理跳跃缓冲和土狼时间计时器
   - 跳跃计数器控制一段跳/二段跳状态

2. **GroundDetector.cs** - 地面检测器
   - 使用 `Physics2D.OverlapBox` 进行地面检测
   - 排除玩家自身 Collider 避免误检测
   - 可配置检测偏移和盒子尺寸

3. **JumpConfig.asset** - 跳跃配置 ScriptableObject
   - 跳跃力度：`jumpForce = 7`, `doubleJumpForce = 4`
   - 高级机制：`jumpBufferTime = 0.2s`, `coyoteTime = 0.15s`
   - 地面检测：`groundLayer = Default`, `groundCheckDistance = 0.1`

### 关键修复

1. **二段跳逻辑修复**
   - 问题：`_jumpCount` 每帧在地面时被重置
   - 修复：改为只在落地瞬间重置 `if (_isGroundedLastFrame && !wasGrounded)`

2. **地面检测误检测修复**
   - 问题：检测盒子过大 (0.8 x 0.1)，碰到障碍物侧面
   - 修复：缩小到 0.3 x 0.05，并排除玩家自身 Collider

3. **Layer 配置修复**
   - 问题：`groundLayer.m_Bits = 256`（第 8 层），但场景地面在 Default 层（第 0 层）
   - 修复：改为 `m_Bits = 1`

4. **空中旋转修复**
   - 问题：角色在空中自由旋转
   - 修复：`_rigidbody2D.freezeRotation = true`

---

## 经验教训

### 做得好的
1. **测试驱动开发** - 为跳跃逻辑编写了 EditMode 测试，确保功能正确
2. **ScriptableObject 配置** - 便于快速调优跳跃手感参数
3. **日志诊断** - 使用详细日志快速定位地面检测问题

### 需要改进的
1. **Layer 配置管理** - 应该在项目初期统一配置 Layer，避免后期不匹配
2. **地面检测尺寸** - 应该在设计阶段就确定合适的检测范围，而不是迭代试错

### 踩过的坑
1. **竞态条件** - Update() 中地面检测和输入处理的顺序影响跳跃逻辑
2. **物理更新时机** - 速度限制等物理操作应放在 FixedUpdate() 中
3. **OverlapBox 检测** - 需要注意检测盒子尺寸和位置，避免误检测

---

## 下一步计划

Epic 1 完成后，玩家跳跃系统已经具备完整的核心功能：
- ✅ 一段跳、二段跳
- ✅ 跳跃缓冲
- ✅ 土狼时间
- ✅ 快速响应
- ✅ 落地立即起跳

接下来进入 **Epic 2: 基础地图系统**，实现无尽地图生成。

---

## 参考资源
- [Celeste 跳跃机制分析](https://www.youtube.com/watch?v=ogmaGJWqoC4)
- [Unity 2D 平台跳跃最佳实践](https://learn.unity.com/tutorial/2d-game-kit-jump-and-coyote-time)
