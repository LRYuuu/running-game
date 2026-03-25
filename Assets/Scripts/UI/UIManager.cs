using UnityEngine;
using UnityEngine.UIElements;
using System;
using SquareFireline.Game;

namespace SquareFireline.UI
{
    /// <summary>
    /// UI 管理器 - 集中管理 UI 状态和界面显示
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region 单例
        public static UIManager Instance { get; private set; }

        /// <summary>
        /// 重置单例实例（仅用于测试）
        /// </summary>
        public static void ResetInstance()
        {
            Instance = null;
        }
        #endregion

        #region 事件
        /// <summary>UI 状态改变时触发</summary>
        public event Action<UIState, UIState> OnUIStateChanged;
        #endregion

        #region UI 状态
        public UIState CurrentUIState { get; private set; } = UIState.MainMenu;
        #endregion

        #region 序列化字段
        [Header("UI 资源")]
        [Tooltip("主界面 UXML 文件路径（Resources 文件夹下）")]
        [SerializeField] private string mainMenuUxmlPath = "UI/MainMenu";

        [Tooltip("主界面 UXML 文件（可选，优先级高于路径）")]
        [SerializeField] private VisualTreeAsset mainMenuUxml;

        [Header("Canvas 设置")]
        [Tooltip("是否使用世界空间 Canvas（用于 2D 游戏）")]
        [SerializeField] private bool useWorldSpaceCanvas = false;
        #endregion

        #region 私有字段
        private UIDocument _mainMenuDocument;
        private UnityEngine.UIElements.Button _startButton;
        private InGameUI _inGameUI;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            Debug.Log("[UIManager] Awake started");

            // 确保 GameObject 有 RectTransform
            if (GetComponent<RectTransform>() == null)
            {
                gameObject.AddComponent<RectTransform>();
                Debug.Log("[UIManager] RectTransform added");
            }

            // 设置 UIManager 的 RectTransform 为全屏
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            // 单例初始化
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[UIManager] Instance already exists, destroying this one");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 创建 UI 容器
            CreateUIContainer();

            // 初始显示主界面（直接在 Awake 中加载，确保 UI 能显示）
            LoadMainMenuUXML();
            Debug.Log("[UIManager] Awake - Main menu loaded");

            // 初始化游戏内 UI 引用
            InitializeInGameUI();
        }

        private void OnEnable()
        {
            // 订阅游戏状态变化事件
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 显示主界面
        /// </summary>
        public void ShowMainMenu()
        {
            if (_mainMenuDocument != null)
            {
                _mainMenuDocument.enabled = true;
                Debug.Log("[UIManager] Main menu enabled");

                // 重新注册按钮事件（UIDocument 禁用后重新启用时需要重新注册）
                RegisterStartButton();
            }
            else
            {
                // 如果文档未加载，重新加载
                LoadMainMenuUXML();
            }

            if (CurrentUIState == UIState.MainMenu)
                return;

            var oldState = CurrentUIState;
            CurrentUIState = UIState.MainMenu;
            OnUIStateChanged?.Invoke(oldState, CurrentUIState);
        }

        /// <summary>
        /// 隐藏主界面
        /// </summary>
        public void HideMainMenu()
        {
            if (CurrentUIState != UIState.MainMenu)
                return;

            var oldState = CurrentUIState;
            CurrentUIState = UIState.InGame;

            // 隐藏主界面
            if (_mainMenuDocument != null)
            {
                _mainMenuDocument.enabled = false;
                Debug.Log("[UIManager] Main menu hidden");
            }

            Debug.Log("[UIManager] HideMainMenu completed");
            OnUIStateChanged?.Invoke(oldState, CurrentUIState);
        }

        /// <summary>
        /// 显示游戏内 UI（预留，后续 Story 实现）
        /// </summary>
        public void ShowInGameUI()
        {
            Debug.Log($"[UIManager] ShowInGameUI called, CurrentUIState={CurrentUIState}");
            Debug.Log($"[UIManager] _inGameUI is null: {_inGameUI == null}");

            // 允许从 MainMenu 或 InGame 状态调用（HideMainMenu 可能已经设置了 InGame 状态）
            if (CurrentUIState != UIState.InGame && CurrentUIState != UIState.MainMenu)
            {
                Debug.LogWarning($"[UIManager] ShowInGameUI called with unexpected state: {CurrentUIState}");
                return;
            }

            var oldState = CurrentUIState;
            CurrentUIState = UIState.InGame;

            // 显示游戏内 UI
            if (_inGameUI != null)
            {
                _inGameUI.Show();
                Debug.Log("[UIManager] Called _inGameUI.Show()");
            }
            else
            {
                Debug.LogWarning("[UIManager] _inGameUI is null, cannot show InGameUI");
            }

            Debug.Log("[UIManager] In-game UI shown");
            OnUIStateChanged?.Invoke(oldState, CurrentUIState);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 初始化游戏内 UI 引用
        /// </summary>
        private void InitializeInGameUI()
        {
            // 查找游戏内 UI 控制器
            var inGameUIObj = GameObject.Find("InGameUIDocument");
            if (inGameUIObj != null)
            {
                _inGameUI = inGameUIObj.GetComponent<InGameUI>();
                if (_inGameUI == null)
                {
                    _inGameUI = inGameUIObj.AddComponent<InGameUI>();
                }
                Debug.Log("[UIManager] InGameUI initialized");
            }
            else
            {
                Debug.LogWarning("[UIManager] InGameUIDocument not found in scene");
            }
        }

        /// <summary>
        /// 创建 UI 容器 GameObject
        /// </summary>
        private void CreateUIContainer()
        {
            // 不需要动态创建 UI 容器，场景中的 MainMenuUIDocument 已处理 UI 显示
            Debug.Log("[UIManager] Using scene-based MainMenuUIDocument for UI display");
        }

        /// <summary>
        /// 加载主界面 UXML
        /// </summary>
        private void LoadMainMenuUXML()
        {
            // 使用场景中静态创建的 MainMenuUIDocument
            var uiDocGameObject = GameObject.Find("MainMenuUIDocument");
            if (uiDocGameObject != null)
            {
                _mainMenuDocument = uiDocGameObject.GetComponent<UIDocument>();
                if (_mainMenuDocument != null)
                {
                    Debug.Log("[UIManager] Found scene-based MainMenuUIDocument");

                    // 延迟检查 rootVisualElement 并注册按钮事件
                    StartCoroutine(CheckRootVisualElementAndRegisterButtons());
                    return;
                }
            }

            Debug.LogError("[UIManager] Failed to find MainMenuUIDocument in scene");
        }

        /// <summary>
        /// 检查 rootVisualElement 是否创建成功并注册按钮事件
        /// </summary>
        private System.Collections.IEnumerator CheckRootVisualElementAndRegisterButtons()
        {
            Debug.Log("[UIManager] CheckRootVisualElementAndRegisterButtons started");

            // 等待 UIDocument 创建 rootVisualElement
            yield return new WaitForSeconds(0.5f);

            if (_mainMenuDocument != null)
            {
                Debug.Log("[UIManager] _mainMenuDocument is not null, checking rootVisualElement");
                var root = _mainMenuDocument.rootVisualElement;
                if (root != null)
                {
                    Debug.Log($"[UIManager] rootVisualElement created: {root.name}");
                    Debug.Log($"[UIManager] Root child count: {root.childCount}");
                    Debug.Log($"[UIManager] Root world bound: {root.worldBound}");

                    // 显式设置根元素的全屏样式，确保 UI 正确拉伸
                    root.style.width = new StyleLength(Length.Percent(100f));
                    root.style.height = new StyleLength(Length.Percent(100f));

                    // 注册开始游戏按钮事件
                    RegisterStartButton();
                }
                else
                {
                    Debug.LogWarning("[UIManager] rootVisualElement is null after waiting one frame");
                }
            }

            // 等待布局计算完成
            yield return new WaitForSeconds(0.1f);

            if (_mainMenuDocument != null && _mainMenuDocument.rootVisualElement != null)
            {
                var root = _mainMenuDocument.rootVisualElement;
                Debug.Log($"[UIManager] Delayed check - Root layout: {root.worldBound}");
                Debug.Log($"[UIManager] Root resolved style width: {root.resolvedStyle.width}, height: {root.resolvedStyle.height}");

                // 打印子元素的布局信息
                if (root.childCount > 0)
                {
                    var child = root[0];
                    Debug.Log($"[UIManager] Child[0] name: {child.name}, layout: {child.worldBound}");
                    Debug.Log($"[UIManager] Child[0] resolved style height: {child.resolvedStyle.height}");
                }
            }
        }

        /// <summary>
        /// 注册开始游戏按钮点击事件
        /// </summary>
        private void RegisterStartButton()
        {
            if (_mainMenuDocument == null || _mainMenuDocument.rootVisualElement == null)
            {
                Debug.LogWarning("[UIManager] Cannot register start button: rootVisualElement is null");
                return;
            }

            _startButton = _mainMenuDocument.rootVisualElement.Q<UnityEngine.UIElements.Button>("start-button");
            if (_startButton != null)
            {
                _startButton.clicked += OnStartButtonClicked;
                Debug.Log("[UIManager] Start button clicked event registered");
            }
            else
            {
                Debug.LogWarning("[UIManager] Start button not found in UXML");
            }
        }

        /// <summary>
        /// 开始游戏按钮点击回调
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[UIManager] Start button clicked - starting game");

            // 调用 GameManager 开始游戏
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
                Debug.Log("[UIManager] Game started via GameManager");
            }
            else
            {
                Debug.LogWarning("[UIManager] GameManager instance not found");
            }

            // 关闭主界面 UI
            HideMainMenu();
        }

        /// <summary>
        /// 游戏状态变化回调
        /// </summary>
        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"[UIManager] Game state changed: {oldState} → {newState}");

            switch (newState)
            {
                case GameState.Waiting:
                    // 等待开始状态 → 显示主界面，隐藏游戏内 UI
                    ShowMainMenu();
                    if (_inGameUI != null) _inGameUI.Hide();
                    break;

                case GameState.Playing:
                    // 游戏中状态 → 隐藏主界面，显示游戏内 UI
                    HideMainMenu();
                    ShowInGameUI();
                    break;

                case GameState.Dying:
                case GameState.Respawning:
                    // 死亡/重生状态 → 不处理 UI（保持游戏内 UI 或隐藏）
                    break;
            }
        }
        #endregion
    }
}
