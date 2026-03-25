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
        #endregion

        #region 私有字段
        private VisualElement _root;
        private Label _scoreLabel;
        private Label _highScoreLabel;
        private VisualElement _newRecordContainer;
        private Label _newRecordLabel;
        private int _lastDisplayedScore = -1; // 用于避免重复更新
        private int _lastDisplayedHighScore = -1; // 用于避免重复更新最高分
        private bool _isSubscribed = false;
        private Coroutine _hideNewRecordCoroutine;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            // 如果 uiDocument 未分配，尝试从同一个 GameObject 获取
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

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

            // 订阅分数变化事件（在 UI 初始化完成后再更新显示）
            SubscribeToScoreManager(registerOnly: true);

            // 订阅破纪录事件（在 UI 初始化完成后）
            // 注意：不在这里直接订阅，而是在 InitializeUIElements 完成后订阅
        }

        private void SubscribeToScoreManager(bool registerOnly = false)
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

                // 如果 UI 已初始化，立即更新一次显示
                if (!registerOnly && _scoreLabel != null)
                {
                    UpdateScoreDisplay(ScoreManager.Instance.CurrentScore, ScoreManager.Instance.HighScore);
                }
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

                // UI 初始化完成后，立即更新分数显示
                if (_isSubscribed && ScoreManager.Instance != null)
                {
                    UpdateScoreDisplay(ScoreManager.Instance.CurrentScore, ScoreManager.Instance.HighScore);
                }

                // 订阅破纪录事件（UI 初始化完成后）
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.OnNewRecord += OnNewRecord;
                    Debug.Log("[InGameUI] Subscribed to OnNewRecord event");
                }
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

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnNewRecord -= OnNewRecord;
            }

            // 停止协程
            if (_hideNewRecordCoroutine != null)
            {
                StopCoroutine(_hideNewRecordCoroutine);
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
            // 更新当前分数（避免重复更新）
            if (currentScore != _lastDisplayedScore && _scoreLabel != null)
            {
                _lastDisplayedScore = currentScore;
                _scoreLabel.text = $"分数：{currentScore}";
                Debug.Log($"[InGameUI] Score updated: {_scoreLabel.text}");
            }

            // 更新最高分（避免重复更新）
            if (highScore != _lastDisplayedHighScore && _highScoreLabel != null)
            {
                _lastDisplayedHighScore = highScore;
                _highScoreLabel.text = $"最高分：{highScore}";
                Debug.Log($"[InGameUI] HighScore updated: {_highScoreLabel.text}");
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

            // 默认隐藏游戏内 UI，直到 Show() 被调用
            _root.style.display = DisplayStyle.None;
            Debug.Log("[InGameUI] Root visibility set to None in InitializeUIElements");

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

            // 获取最高分标签引用（无论 showHighScore 设置如何都获取引用）
            _highScoreLabel = _root.Q<Label>("highscore-label");
            if (_highScoreLabel == null)
            {
                Debug.LogWarning("[InGameUI] highscore-label not found in UXML");
            }
            else
            {
                Debug.Log($"[InGameUI] highscore-label found, initial text: {_highScoreLabel.text}");
            }

            // 获取破纪录提示容器引用
            _newRecordContainer = _root.Q<VisualElement>("new-record-container");
            _newRecordLabel = _root.Q<Label>("new-record-label");
            if (_newRecordContainer == null)
            {
                Debug.LogWarning("[InGameUI] new-record-container not found in UXML");
            }
            else
            {
                // 默认隐藏
                _newRecordContainer.style.display = DisplayStyle.None;
                Debug.Log("[InGameUI] new-record-container found, visibility set to None");
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

        /// <summary>
        /// 破纪录事件回调
        /// </summary>
        private void OnNewRecord(int newHighScore)
        {
            Debug.Log($"[InGameUI] New record during game: {newHighScore}");
            ShowNewRecordHint();
        }

        /// <summary>
        /// 显示破纪录提示
        /// </summary>
        public void ShowNewRecordHint()
        {
            if (_newRecordContainer != null)
            {
                _newRecordContainer.style.display = DisplayStyle.Flex;
                Debug.Log("[InGameUI] New record hint shown");

                // 取消之前的隐藏协程
                if (_hideNewRecordCoroutine != null)
                {
                    StopCoroutine(_hideNewRecordCoroutine);
                }

                // 2.5 秒后自动隐藏
                _hideNewRecordCoroutine = StartCoroutine(HideNewRecordAfterDelay(2.5f));
            }
        }

        /// <summary>
        /// 延迟隐藏破纪录提示
        /// </summary>
        private System.Collections.IEnumerator HideNewRecordAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_newRecordContainer != null)
            {
                _newRecordContainer.style.display = DisplayStyle.None;
                Debug.Log("[InGameUI] New record hint hidden");
            }

            _hideNewRecordCoroutine = null;
        }
        #endregion
    }
}
