# 主界面 (Main Menu) 使用说明

## 快速开始

### 方法一：手动配置（推荐）

1. **在 Unity 编辑器中打开 login 场景**
   ```
   Assets/Scenes/login.unity
   ```

2. **创建 UIManager GameObject**
   - 在 Hierarchy 窗口右键 → `Create Empty`
   - 命名为 `UIManager`

3. **添加 UIManager 组件**
   - 选中 `UIManager` GameObject
   - 在 Inspector 窗口点击 `Add Component`
   - 搜索并添加 `UIManager` (命名空间：SquareFireline.UI)

4. **配置 UXML 引用**
   - 在 Inspector 中找到 `Main Menu Uxml` 字段
   - 从 Project 窗口拖动 `Assets/Resources/UI/MainMenu.uxml` 到该字段

5. **运行游戏测试**
   - 点击 Unity 的 Play 按钮
   - 应该看到主界面显示（黑色背景、标题、开始游戏按钮）
   - 点击"开始游戏"按钮，游戏应该开始

---

### 方法二：使用编辑器工具

1. **打开 UI 设置工具**
   ```
   菜单栏 → Square Fireline → Setup UI Scene
   ```

2. **点击配置按钮**
   - 点击 `1. 创建 UIManager`
   - 点击 `2. 配置 UXML 引用`

3. **运行游戏测试**

---

## 文件结构

```
Assets/
├── Resources/UI/
│   ├── MainMenu.uxml    # 主界面结构定义
│   └── MainMenu.uss     # 主界面样式表
├── Scripts/UI/
│   ├── UIManager.cs     # UI 管理器（单例）
│   ├── MainMenuUI.cs    # 主界面控制器
│   └── UIState.cs       # UI 状态枚举
└── Scenes/
    └── login.unity      # 主场景
```

---

## 组件说明

### UIManager (核心管理器)

**作用**: 管理所有 UI 状态和显示

**属性**:
- `mainMenuUxml` (VisualTreeAsset): 主界面 UXML 文件引用

**方法**:
- `ShowMainMenu()`: 显示主界面
- `HideMainMenu()`: 隐藏主界面
- `ShowInGameUI()`: 显示游戏内 UI（预留）

**事件**:
- 订阅 `GameManager.OnGameStateChanged` 事件
- 自动响应游戏状态变化

---

### MainMenuUI (主界面控制器)

**作用**: 处理主界面的用户交互

**属性**:
- `uiDocument` (UIDocument): UIDocument 组件引用
- `startButtonName` (string): 开始按钮名称（默认："start-button"）

**方法**:
- `StartGame()`: 调用 GameManager 开始游戏

---

## 工作流程

```
游戏启动
    ↓
UIManager.Awake() - 单例初始化
    ↓
UIManager.Start() - ShowMainMenu()
    ↓
加载 UXML 并显示主界面
    ↓
玩家点击"开始游戏"
    ↓
MainMenuUI.StartGame() → GameManager.StartGame()
    ↓
GameState: Waiting → Playing
    ↓
UIManager.OnGameStateChanged()
    ↓
UIState: MainMenu → InGame (隐藏主界面)
    ↓
游戏开始运行
```

---

## 常见问题

### Q1: 主界面不显示
**检查**:
1. UIManager 是否存在于场景中
2. mainMenuUxml 字段是否已配置
3. 检查控制台是否有错误日志

### Q2: 点击按钮无反应
**检查**:
1. GameManager 是否存在
2. MainMenuUI 组件是否添加到 UIDocument 上
3. 检查按钮名称是否为 "start-button"

### Q3: UI 样式不正确
**检查**:
1. MainMenu.uss 是否与 UXML 在同一目录
2. UXML 中是否包含 `<ui:Style src="MainMenu.uss" />`

---

## 自定义样式

编辑 `Assets/Resources/UI/MainMenu.uss` 文件：

```css
/* 修改标题颜色 */
.title {
    color: rgb(255, 215, 0); /* 金色 */
    font-size: 72px;
}

/* 修改按钮颜色 */
.start-button {
    background-color: rgb(220, 60, 60); /* 红色 */
}
```

---

## 扩展功能

### 添加新的 UI 元素

1. **编辑 UXML** (`MainMenu.uxml`):
```xml
<ui:Label name="score-label" text="分数：0" class="score" />
```

2. **添加样式** (`MainMenu.uss`):
```css
.score {
    font-size: 24px;
    color: white;
}
```

3. **在代码中访问**:
```csharp
var scoreLabel = uiDocument.rootVisualElement.Q<Label>("score-label");
scoreLabel.text = "分数：" + score;
```

---

## 下一步

完成主界面配置后，继续实施:
- **Story 5-2**: 游戏中显示当前分数
- **Story 5-3**: 显示历史最高分
- **Story 5-4**: 破纪录提示

---

**文档更新时间**: 2026-03-20
