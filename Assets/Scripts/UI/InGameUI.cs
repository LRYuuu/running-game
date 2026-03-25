using UnityEngine;
using UnityEngine.UIElements;
using SquareFireline.Game;

namespace SquareFireline.UI
{
    /// <summary>
    /// 游戏内 UI 控制器 - 显示当前分数
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InGameUI : MonoBehaviour
    {
        #region 序列化字段
        [Header("UI 文档引用")]
        [Tooltip("游戏内 UI UXML 文档")]
        [SerializeField] private UIDocument uiDocument;

        [Header("可选：最高分显示（Story 5-3 扩展）")]
        [Tooltip("是否显示最高分")]
        [SerializeField] private bool showHighScore = false;
        #endregion

        #region 私有字段
        private VisualElement _root;
        private Label _scoreLabel;
        private Label _highScoreLabel;
        private int _lastDisplayedScore = -1; // 用于避免重复更新
        private bool _isSubscribed = false;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            // 如果 uiDocument 未分配，尝试从同一个 GameObject 获取
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            // 获取 UI 根元素（UIDocument 会在 OnEnable 时创建 rootVisualElement）
            if (uiDocument != null)
            {
                Debug.Log($"[InGameUI] Awake called, uiDocument={uiDocument != null}");
            }
            else
            {
                Debug.LogWarning("[InGameUI] UIDocument not assigned");
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[InGameUI] OnEnable called, uiDocument={uiDocument != null}, ScoreManager.Instance={ScoreManager.Instance != null}");

            // 获取 UI 根元素
            if (uiDocument != null)
            {
                // UIDocument 可能需要一帧才能创建 rootVisualElement
                StartCoroutine(InitializeUIAfterDelay());
            }

            // 订阅分数变化事件
            SubscribeToScoreManager();
        }

        private void SubscribeToScoreManager()
        {
            if (_isSubscribed)
            {
                Debug.Log("[InGameUI] Already subscribed to ScoreManager");
                return;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
                _isSubscribed = true;
                Debug.Log($"[InGameUI] Subscribed to ScoreManager, currentScore={ScoreManager.Instance.CurrentScore}");
                // 立即更新一次显示
                UpdateScoreDisplay(ScoreManager.Instance.CurrentScore, ScoreManager.Instance.HighScore);
            }
            else
            {
                Debug.LogWarning("[InGameUI] ScoreManager.Instance is null, will retry in Update");
            }
        }

        private void Update()
        {
            // 如果未订阅且 ScoreManager 已存在，重试订阅
            if (!_isSubscribed && ScoreManager.Instance != null)
            {
                SubscribeToScoreManager();
            }
        }

        private System.Collections.IEnumerator InitializeUIAfterDelay()
        {
            // 等待 UIDocument 创建 rootVisualElement
            yield return null; // 等待一帧

            _root = uiDocument.rootVisualElement;
            if (_root != null)
            {
                InitializeUIElements();
                Debug.Log("[InGameUI] UI initialized after delay");
            }
            else
            {
                Debug.LogWarning("[InGameUI] rootVisualElement is null after waiting");
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (_isSubscribed && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
                _isSubscribed = false;
                Debug.Log("[InGameUI] Unsubscribed from ScoreManager");
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 显示游戏内 UI
        /// </summary>
        public void Show()
        {
            Debug.Log("[InGameUI] Show() called");
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
                Debug.Log("[InGameUI] Show() - root.style.display = Flex");
            }
            else
            {
                Debug.LogWarning("[InGameUI] Show() called but _root is null");
            }
        }

        /// <summary>
        /// 隐藏游戏内 UI
        /// </summary>
        public void Hide()
        {
            if (_root != null)
                _root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 更新分数显示
        /// </summary>
        /// <param name="currentScore">当前分数</param>
        /// <param name="highScore">最高分</param>
        public void UpdateScoreDisplay(int currentScore, int highScore)
        {
            // 避免重复更新（性能优化）
            if (currentScore == _lastDisplayedScore)
                return;

            _lastDisplayedScore = currentScore;

            if (_scoreLabel != null)
            {
                // 可选：使用千位分隔符格式化
                _scoreLabel.text = $"分数：{currentScore}";
                Debug.Log($"[InGameUI] Score updated: {_scoreLabel.text}");
                // 或者：_scoreLabel.text = $"分数：{currentScore:N0}";
            }
            else
            {
                Debug.LogWarning("[InGameUI] _scoreLabel is null, cannot update");
            }

            if (_highScoreLabel != null && showHighScore)
            {
                _highScoreLabel.text = $"最高分：{highScore}";
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 初始化 UI 元素引用
        /// </summary>
        private void InitializeUIElements()
        {
            if (_root == null)
            {
                Debug.LogWarning("[InGameUI] InitializeUIElements called but _root is null");
                return;
            }

            // 获取分数标签引用
            _scoreLabel = _root.Q<Label>("score-label");

            if (_scoreLabel == null)
            {
                Debug.LogWarning("[InGameUI] score-label not found in UXML");
            }
            else
            {
                Debug.Log($"[InGameUI] score-label found, initial text: {_scoreLabel.text}");
            }

            // 获取最高分标签引用（可选）
            if (showHighScore)
            {
                _highScoreLabel = _root.Q<Label>("highscore-label");
            }
        }

        /// <summary>
        /// 分数变化回调
        /// </summary>
        private void OnScoreChanged(int currentScore, int highScore)
        {
            Debug.Log($"[InGameUI] OnScoreChanged: currentScore={currentScore}, highScore={highScore}");
            UpdateScoreDisplay(currentScore, highScore);
        }
        #endregion
    }
}
