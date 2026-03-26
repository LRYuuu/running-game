using UnityEngine;
using UnityEngine.UIElements;
using RunnersJourney.Game;

namespace RunnersJourney.UI
{
    /// <summary>
    /// 游戏结束 UI 控制器 - 显示死亡界面和重新开始选项
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameOverUI : MonoBehaviour
    {
        #region 序列化字段
        [Header("UI 文档引用")]
        [Tooltip("游戏结束 UI UXML 文件")]
        [SerializeField] private UIDocument uiDocument;
        #endregion

        #region 私有字段
        private VisualElement _root;
        private Label _scoreLabel;
        private Label _highScoreLabel;
        private VisualElement _newRecordBadge;
        private UnityEngine.UIElements.Button _restartButton;
        private UnityEngine.UIElements.Button _mainMenuButton;
        private bool _isSubscribed = false;
        private bool _areButtonEventsRegistered = false;
        private int _lastDisplayedScore = -1;
        private int _lastDisplayedHighScore = -1;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null)
            {
                Debug.Log("[GameOverUI] Awake called, uiDocument assigned");
            }
            else
            {
                Debug.LogWarning("[GameOverUI] UIDocument not found on GameObject");
            }
        }

        private void OnEnable()
        {
            Debug.Log("[GameOverUI] OnEnable called, starting UI initialization");
            StartCoroutine(InitializeUIAfterDelay());
        }

        private void OnDisable()
        {
            // 取消订阅事件
            if (_isSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }

            if (_isSubscribed && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
                ScoreManager.Instance.OnNewRecord -= OnNewRecord;
            }

            // 取消注册按钮事件
            if (_areButtonEventsRegistered && _restartButton != null)
            {
                _restartButton.clicked -= OnRestartButtonClicked;
            }
            if (_areButtonEventsRegistered && _mainMenuButton != null)
            {
                _mainMenuButton.clicked -= OnMainMenuButtonClicked;
            }

            // 重置订阅标志，允许下次 OnEnable 时重新订阅
            _isSubscribed = false;
            _areButtonEventsRegistered = false;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 显示游戏结束 UI
        /// </summary>
        public void Show()
        {
            Debug.Log($"[GameOverUI] Show() called, _root is null: {_root == null}");
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;

                // 设置 SortingOrder 确保 UI 在最前面
                if (uiDocument != null)
                {
                    uiDocument.panelSettings.sortingOrder = 100;
                    Debug.Log("[GameOverUI] SortingOrder set to 100");
                }

                Debug.Log("[GameOverUI] Game over UI shown");
            }
            else
            {
                Debug.LogWarning("[GameOverUI] Show() called but _root is null, restarting UI initialization");
                // 如果 _root 为 null，重新初始化
                _isSubscribed = false; // 重置订阅标志，允许重新订阅
                StartCoroutine(InitializeUIAfterDelay());
            }
        }

        /// <summary>
        /// 隐藏游戏结束 UI
        /// </summary>
        public void Hide()
        {
            Debug.Log("[GameOverUI] Hide() called");
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
                Debug.Log("[GameOverUI] Game over UI hidden");
            }
        }

        /// <summary>
        /// 更新分数显示
        /// </summary>
        /// <param name="currentScore">当前分数</param>
        /// <param name="highScore">最高分</param>
        public void UpdateScoreDisplay(int currentScore, int highScore)
        {
            if (_scoreLabel != null && currentScore != _lastDisplayedScore)
            {
                _lastDisplayedScore = currentScore;
                _scoreLabel.text = $"分数：{currentScore}";
                Debug.Log($"[GameOverUI] Score updated: {_scoreLabel.text}");
            }

            if (_highScoreLabel != null && highScore != _lastDisplayedHighScore)
            {
                _lastDisplayedHighScore = highScore;
                _highScoreLabel.text = $"最高分：{highScore}";
                Debug.Log($"[GameOverUI] HighScore updated: {_highScoreLabel.text}");
            }
        }

        /// <summary>
        /// 显示新纪录标志
        /// </summary>
        public void ShowNewRecordBadge()
        {
            Debug.Log("[GameOverUI] ShowNewRecordBadge() called");
            if (_newRecordBadge != null)
            {
                _newRecordBadge.style.display = DisplayStyle.Flex;
                Debug.Log("[GameOverUI] New record badge shown");
            }
        }

        /// <summary>
        /// 隐藏新纪录标志
        /// </summary>
        public void HideNewRecordBadge()
        {
            if (_newRecordBadge != null)
            {
                _newRecordBadge.style.display = DisplayStyle.None;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 延迟初始化 UI 元素
        /// </summary>
        private System.Collections.IEnumerator InitializeUIAfterDelay()
        {
            yield return null;

            _root = uiDocument.rootVisualElement;
            if (_root != null)
            {
                // 初始隐藏 UI
                _root.style.display = DisplayStyle.None;

                InitializeUIElements();
                Debug.Log("[GameOverUI] UI initialized");

                // 订阅事件
                SubscribeToEvents();
            }
            else
            {
                Debug.LogWarning("[GameOverUI] rootVisualElement is null after waiting");
            }
        }

        /// <summary>
        /// 初始化 UI 元素引用
        /// </summary>
        private void InitializeUIElements()
        {
            if (_root == null)
            {
                Debug.LogWarning("[GameOverUI] InitializeUIElements called but _root is null");
                return;
            }

            _scoreLabel = _root.Q<Label>("final-score-label");
            _highScoreLabel = _root.Q<Label>("highscore-display-label");
            _newRecordBadge = _root.Q<VisualElement>("new-record-badge");
            _restartButton = _root.Q<UnityEngine.UIElements.Button>("restart-button");
            _mainMenuButton = _root.Q<UnityEngine.UIElements.Button>("main-menu-button");

            // 默认隐藏新纪录标志
            if (_newRecordBadge != null)
            {
                _newRecordBadge.style.display = DisplayStyle.None;
            }

            // 验证元素
            Debug.Log($"[GameOverUI] _root: {_root.name}, childCount: {_root.childCount}");
            Debug.Log($"[GameOverUI] _scoreLabel found: {_scoreLabel != null}");
            Debug.Log($"[GameOverUI] _highScoreLabel found: {_highScoreLabel != null}");
            Debug.Log($"[GameOverUI] _restartButton found: {_restartButton != null}");
            Debug.Log($"[GameOverUI] _mainMenuButton found: {_mainMenuButton != null}");

            if (_scoreLabel == null)
            {
                Debug.LogWarning("[GameOverUI] final-score-label not found");
            }
            if (_highScoreLabel == null)
            {
                Debug.LogWarning("[GameOverUI] highscore-display-label not found");
            }
            if (_restartButton == null)
            {
                Debug.LogWarning("[GameOverUI] restart-button not found");
            }
            if (_mainMenuButton == null)
            {
                Debug.LogWarning("[GameOverUI] main-menu-button not found");
            }

            // 注册按钮事件
            if (_restartButton != null && !_areButtonEventsRegistered)
            {
                _restartButton.clicked += OnRestartButtonClicked;
                Debug.Log("[GameOverUI] Restart button clicked event registered");
            }
            if (_mainMenuButton != null && !_areButtonEventsRegistered)
            {
                _mainMenuButton.clicked += OnMainMenuButtonClicked;
                Debug.Log("[GameOverUI] Main menu button clicked event registered");
            }
            _areButtonEventsRegistered = true;
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_isSubscribed)
            {
                Debug.Log("[GameOverUI] Already subscribed to events");
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                Debug.Log("[GameOverUI] Subscribed to GameManager.OnGameStateChanged");
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
                ScoreManager.Instance.OnNewRecord += OnNewRecord;
                Debug.Log("[GameOverUI] Subscribed to ScoreManager events");
            }

            _isSubscribed = true;
        }

        /// <summary>
        /// 游戏状态变化回调
        /// </summary>
        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"[GameOverUI] OnGameStateChanged: {oldState} -> {newState}, _isSubscribed={_isSubscribed}");

            switch (newState)
            {
                case GameState.Dying:
                    // 死亡状态：显示界面并更新分数
                    Debug.Log($"[GameOverUI] Dying state: calling Show(), _root={_root != null}");
                    Show();
                    if (ScoreManager.Instance != null)
                    {
                        UpdateScoreDisplay(ScoreManager.Instance.CurrentScore, ScoreManager.Instance.HighScore);
                    }
                    break;

                case GameState.Playing:
                case GameState.Waiting:
                    // 游戏开始或回到主菜单：隐藏界面
                    Hide();
                    HideNewRecordBadge();
                    break;
            }
        }

        /// <summary>
        /// 分数变化回调
        /// </summary>
        private void OnScoreChanged(int currentScore, int highScore)
        {
            Debug.Log($"[GameOverUI] OnScoreChanged: {currentScore}, {highScore}");
            UpdateScoreDisplay(currentScore, highScore);
        }

        /// <summary>
        /// 破纪录事件回调
        /// </summary>
        private void OnNewRecord(int score)
        {
            Debug.Log($"[GameOverUI] OnNewRecord: {score}");
            ShowNewRecordBadge();
        }

        /// <summary>
        /// 重新开始按钮点击回调
        /// </summary>
        private void OnRestartButtonClicked()
        {
            Debug.Log("[GameOverUI] OnRestartButtonClicked called - button was clicked!");
            Hide();
            HideNewRecordBadge();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGameFromDeath();
                Debug.Log("[GameOverUI] Called GameManager.ResumeGameFromDeath()");
            }
        }

        /// <summary>
        /// 返回主菜单按钮点击回调
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            Debug.Log("[GameOverUI] OnMainMenuButtonClicked called - button was clicked!");
            Hide();
            HideNewRecordBadge();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
                Debug.Log("[GameOverUI] Called GameManager.RestartGame() to return to main menu");
            }
        }
        #endregion
    }
}
