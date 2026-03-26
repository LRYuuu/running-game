# Runner's Journey - 项目开发规范

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

## 📝 代码规范（必须遵守）

### 命名空间
| 路径 | 命名空间 |
|------|----------|
| Scripts/Map/ | `RunnersJourney.Map` |
| Scripts/Player/ | `RunnersJourney.Player` |
| Scripts/Obstacles/ | `RunnersJourney.Obstacles` |
| Scripts/UI/ | `RunnersJourney.UI` |
| Scripts/Audio/ | `RunnersJourney.Audio` |
| Scripts/Utils/ | `RunnersJourney.Utils` |

### 命名规则
- 类/方法：`PascalCase`
- 私有字段：`_camelCase`（下划线前缀）
- 参数：`camelCase`

### 代码结构
- 公共方法：必须添加 `/// <summary>` 文档注释
- 序列化字段：必须添加 `[Header()]` 和 `[Tooltip()]`
- 日志格式：`Debug.Log($"[ClassName] 消息")`

---

## 🎨 UI Toolkit 规范（重要）

**所有 UI 必须使用 1920x1080 作为参考分辨率**

### PanelSettings
- 路径：`Assets/Resources/UI/StandardPanelSettings.panelsettings`
- 参考分辨率：**1920 x 1080**
- 缩放模式：**ScaleWithScreenSize**
- 宽高匹配：**0.5**

### UXML 模板
```xml
<?xml version="1.0" encoding="utf-8"?>
<UXML xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="UnityEngine.UIElements">
    <Style src="YourFileName.uss" />
    <VisualElement name="root-container" class="root-container">
        <!-- UI 元素 -->
    </VisualElement>
</UXML>
```

### USS 根容器样式
```css
.root-container {
    width: 100%;
    height: 100%;
}
```

### 详细文档
- [`Assets/Scripts/UI/UI_CONFIG.md`](Assets/Scripts/UI/UI_CONFIG.md) - UI 开发规范全文

---

## 📋 代码模板

### MonoBehaviour
```csharp
using UnityEngine;

namespace RunnersJourney.Map
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

namespace RunnersJourney.Map
{
    [CreateAssetMenu(fileName = "MapConfig", menuName = "Runner's Journey/Map Config")]
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
├── Managers (DontDestroyOnLoad) - GameManager, UIManager
├── Camera - Main Camera
├── Map - Grid + MapGenerator
├── Player
└── UI Canvas
```

### 场景列表
| 场景 | 用途 | 索引 |
|------|------|------|
| login.unity | 主菜单 | 0 |

---

## 📦 构建设置

- **平台**: Windows (StandaloneWindows64)
- **架构**: x86_64
- **后端**: IL2CPP + .NET Standard 2.1
- **帧率**: 60 FPS

---

## 📝 Git 提交规范

**格式**: `<type>(<scope>): <subject>`

| type | 说明 | scope |
|------|------|-------|
| feat | 新功能 | map, player, ui, background, obstacles |
| fix | 修复 | |
| docs | 文档 | |
| refactor | 重构 | |
| test | 测试 | |

**示例**:
```bash
git commit -m "feat(map): 添加空隙生成功能"
git commit -m "fix(player): 修复跳跃检测"
```

---

## ⚙️ 当前进度

查看 `_bmad-output/implementation-artifacts/sprint-status.yaml`

**Epic 5 (UI 系统)**: ✅ 6/6 完成
- 主界面、分数显示、最高分、破纪录提示、持久化、死亡重开 UI
