# 无限滚动背景设置指南

## 方案 A：循环拼接式无限背景

适用于：有固定图案的背景（如连续的山脉、城市天际线、云层）

---

## 设置步骤

### 1. 准备背景模板

使用 Tile Palette 手动铺设一段背景 Tilemap：

```
Hierarchy 结构：
Grid
└── SourceBackground (Tilemap)
```

**要求：**
- 确保背景左右可以无缝连接（左边和右边的图案能衔接）
- 记下铺设的宽度（如 20 个 Tile）
- 确保 Tilemap 的 Sorting Layer 设置为 Background

### 2. 创建无限背景管理器

1. 在 Hierarchy 右键 → **Create Empty**
2. 命名为 `InfiniteBackground`
3. 添加组件 `InfiniteBackgroundTiler`

### 3. 配置组件参数

| 参数 | 说明 | 推荐值 |
|------|------|--------|
| Source Tilemap | 拖入步骤 1 中的 SourceBackground Tilemap | - |
| Segment Width In Tiles | 单个背景段的宽度（Tile 数量） | 20 |
| Segment Count | 预先生成的背景段数量 | 3 |
| Scroll Speed | 滚动速度（负值向左） | -2 |
| Pattern Width | 背景图案重复宽度 | 20 |

### 4. 设置 Sorting Layer（渲染顺序）

1. `Edit → Project Settings → Tags and Layers`
2. 在 **Sorting Layers** 部分点击 `+` 添加新层
3. 命名为 `Background`，拖到列表最底部
4. 选中 `InfiniteBackground` 下的所有 Tilemap Renderer 组件
5. 设置 **Sorting Layer** 为 `Background`

### 5. 调整位置

将 `InfiniteBackground` 的 Position 设置为：
```
X: 0
Y: 0
Z: 10  (确保在相机后方)
```

### 6. 调整相机（可选）

确保 Main Camera 的 Culling Mask 包含所有需要的层。

---

## 参数调优

### 滚动速度
- `-1`：慢速滚动，适合远景
- `-2`：中速滚动，默认值
- `-5` 或更快：快速滚动，适合近景

### 视差效果
修改 `Parallax Factor`（如果有使用 ParallaxBackgroundTilemap）：
- `0.1`：几乎不动，非常远的背景
- `0.3`：标准远景
- `0.5`：中等距离
- `1.0`：和相机完全同步

---

## 常见问题

### Q: 背景不滚动？
A: 检查 Scroll Speed 是否为负数，确保脚本已挂载到 GameObject 上。

### Q: 背景有接缝？
A: 确保模板背景左右两侧图案可以无缝衔接，或者增加 Segment Count。

### Q: 背景在前景上面渲染？
A: 确保设置了正确的 Sorting Layer 为 Background。

### Q: 运行时背景不复制模板？
A: 确保 Source Tilemap 不为空，并且有实际的 Tile 数据。

---

## 与游戏速度同步

如果需要背景速度随游戏加速而变快：

```csharp
// 在游戏速度控制器中调用
InfiniteBackgroundTiler bg = FindObjectOfType<InfiniteBackgroundTiler>();
if (bg != null)
{
    bg.SetSpeedMultiplier(currentGameSpeedMultiplier);
}
```

---

## 脚本文件

- `Assets/Scripts/InfiniteBackgroundTiler.cs` - 主背景循环脚本
- `Assets/Scripts/ParallaxBackgroundTilemap.cs` - 支持 Tilemap 的视差滚动（可选）
- `Assets/Scripts/BackgroundManager.cs` - 多层背景管理器（可选）
