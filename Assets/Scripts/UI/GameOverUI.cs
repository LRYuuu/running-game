using UnityEngine;
using UnityEngine.UIElements;
using SquareFireline.Game;

namespace SquareFireline.UI
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
                var root = uiDocument.rootVisualElement;
                if (root != null)
                {
                    root.style.display = DisplayStyle.None;
                }
            }
        }

        private void OnEnable()
        {
            StartCoroutine(InitializeUIAfterDelay());
        }

        private void OnDisable()
        {
            if (_isSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }

            if (_isSubscribed && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
                ScoreManager.Instance.OnNewRecord -= OnNewRecord;
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 显示游戏结束 UI
        /// </summary>
        public void Show()
        {
            Debug.Log("[GameOverUI] Show() called");
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
                Debug.Log("[GameOverUI] Game over UI shown");
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
            if (_restartButton != null)
            {
                _restartButton.clicked += OnRestartButtonClicked;
                Debug.Log("[GameOverUI] Restart button clicked event registered");
            }
            if (_mainMenuButton != null)
            {
                _mainMenuButton.clicked += OnMainMenuButtonClicked;
                Debug.Log("[GameOverUI] Main menu button clicked event registered");
            }
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
            Debug.Log($"[GameOverUI] OnGameStateChanged: {oldState} -> {newState}");

            switch (newState)
            {
                case GameState.Dying:
                    // 死亡状态：显示界面并更新分数
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
            Debug.Log("[GameOverUI] Restart button clicked");
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
            Debug.Log("[GameOverUI] Main menu button clicked");
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
