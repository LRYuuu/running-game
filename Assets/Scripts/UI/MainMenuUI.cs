using UnityEngine;
using UnityEngine.UIElements;
using RunnersJourney.Game;

namespace RunnersJourney.UI
{
    /// <summary>
    /// 主界面 UI 控制器
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        #region 序列化字段
        [Header("UI 组件")]
        [Tooltip("UIDocument 组件")]
        [SerializeField] private UIDocument uiDocument;

        [Tooltip("开始游戏按钮")]
        [SerializeField] private string startButtonName = "start-button";
        #endregion

        #region 私有字段
        private Button _startButton;
        private Label _titleLabel;
        #endregion

        #region Unity 生命周期
        private void OnEnable()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                // 获取 UI 元素
                _startButton = uiDocument.rootVisualElement.Q<Button>(startButtonName);
                _titleLabel = uiDocument.rootVisualElement.Q<Label>("title-label");

                // 注册按钮事件
                if (_startButton != null)
                {
                    _startButton.clicked += OnStartButtonClicked;
                    Debug.Log("[MainMenuUI] Start button registered");
                }
                else
                {
                    Debug.LogWarning("[MainMenuUI] Start button not found");
                }

                // 添加按钮悬停效果
                SetupButtonEffects();
            }
        }

        private void OnDisable()
        {
            // 注销按钮事件
            if (_startButton != null)
            {
                _startButton.clicked -= OnStartButtonClicked;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 设置按钮视觉效果
        /// </summary>
        private void SetupButtonEffects()
        {
            if (_startButton == null)
                return;

            // 添加悬停时的缩放效果
            _startButton.RegisterCallback<PointerEnterEvent>(OnButtonPointerEnter);
            _startButton.RegisterCallback<PointerLeaveEvent>(OnButtonPointerLeave);
        }

        /// <summary>
        /// 按钮指针进入回调
        /// </summary>
        private void OnButtonPointerEnter(PointerEnterEvent evt)
        {
            if (_titleLabel != null)
            {
                // 悬停时标题颜色变亮
                _titleLabel.style.color = new Color(1f, 0.8f, 0.2f);
            }
        }

        /// <summary>
        /// 按钮指针离开回调
        /// </summary>
        private void OnButtonPointerLeave(PointerLeaveEvent evt)
        {
            if (_titleLabel != null)
            {
                // 离开时恢复标题颜色
                _titleLabel.style.color = Color.white;
            }
        }

        /// <summary>
        /// 开始游戏按钮点击回调
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[MainMenuUI] Start button clicked - starting game");
            StartGame();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            // 调用 GameManager 开始游戏
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
                Debug.Log("[MainMenuUI] Game started via GameManager");
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] GameManager instance not found");
            }
        }
        #endregion
    }
}
