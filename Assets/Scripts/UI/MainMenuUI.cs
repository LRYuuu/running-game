using UnityEngine;
using UnityEngine.UIElements;
using RunnersJourney.Game;
using RunnersJourney.Audio;

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

        [Tooltip("选择群系按钮")]
        [SerializeField] private string biomeSelectionButtonName = "biome-selection-button";

        [Tooltip("设置按钮")]
        [SerializeField] private string settingsButtonName = "settings-button";

        [Header("音频")]
        [Tooltip("按钮点击音效剪辑")]
        [SerializeField] private AudioClip _buttonClickSFX;

        [Header("引用")]
        [Tooltip("群系选择面板组件")]
        [SerializeField] private BiomeSelectionPanel biomeSelectionPanel;

        [Tooltip("设置面板组件")]
        [SerializeField] private SettingsPanel settingsPanel;
        #endregion

        #region 私有字段
        private Button _startButton;
        private Button _biomeSelectionButton;
        private Button _settingsButton;
        private Label _titleLabel;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
        }

        private void OnEnable()
        {
            // 延迟注册按钮事件（等待 UI 重建完成）
            if (uiDocument != null)
            {
                StartCoroutine(RegisterButtonsAfterDelay());
            }

            // 查找群系选择面板（每次 OnEnable 都重新查找，确保引用有效）
            var allPanels = FindObjectsByType<BiomeSelectionPanel>(FindObjectsSortMode.None);
            if (allPanels.Length > 0)
            {
                biomeSelectionPanel = allPanels[0];
                Debug.Log($"[MainMenuUI] BiomeSelectionPanel found: {biomeSelectionPanel.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] BiomeSelectionPanel not found in scene");
            }
        }

        private void OnDisable()
        {
            // 注销按钮事件
            UnregisterButtons();
        }
        #endregion

        #region 公共方法

        /// <summary>
        /// 重新注册按钮事件（在 UIDocument 重新启用后调用）
        /// </summary>
        public void RegisterButtons()
        {
            StartCoroutine(RegisterButtonsAfterDelay());
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 延迟注册按钮事件（等待 UI 重建完成）
        /// </summary>
        private System.Collections.IEnumerator RegisterButtonsAfterDelay()
        {
            yield return new WaitForSeconds(0.1f);

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("[MainMenuUI] Cannot register buttons: rootVisualElement is null");
                yield break;
            }

            // 先注销旧的事件（如果有）
            UnregisterButtons();

            // 获取 UI 元素
            _startButton = uiDocument.rootVisualElement.Q<Button>(startButtonName);
            _titleLabel = uiDocument.rootVisualElement.Q<Label>("title-label");
            _biomeSelectionButton = uiDocument.rootVisualElement.Q<Button>(biomeSelectionButtonName);
            _settingsButton = uiDocument.rootVisualElement.Q<Button>(settingsButtonName);

            // 注册开始游戏按钮事件
            if (_startButton != null)
            {
                _startButton.clicked += OnStartButtonClicked;
                Debug.Log("[MainMenuUI] Start button registered");
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Start button not found");
            }

            // 注册选择群系按钮事件
            if (_biomeSelectionButton != null)
            {
                _biomeSelectionButton.clicked += OnBiomeSelectionButtonClicked;
                Debug.Log("[MainMenuUI] Biome selection button registered");
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Biome selection button not found");
            }

            // 注册设置按钮事件
            if (_settingsButton != null)
            {
                _settingsButton.clicked += OnSettingsButtonClicked;
                Debug.Log("[MainMenuUI] Settings button registered");
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Settings button not found");
            }

            // 添加按钮悬停效果
            SetupButtonEffects();
        }

        /// <summary>
        /// 注销按钮事件
        /// </summary>
        private void UnregisterButtons()
        {
            if (_startButton != null)
            {
                _startButton.clicked -= OnStartButtonClicked;
            }

            if (_biomeSelectionButton != null)
            {
                _biomeSelectionButton.clicked -= OnBiomeSelectionButtonClicked;
            }

            if (_settingsButton != null)
            {
                _settingsButton.clicked -= OnSettingsButtonClicked;
            }
        }

        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        private void PlayButtonClickSFX()
        {
            if (_buttonClickSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_buttonClickSFX);
                Debug.Log($"[MainMenuUI] 播放按钮点击音效：{_buttonClickSFX.name}");
            }
            else if (_buttonClickSFX == null)
            {
                Debug.LogWarning("[MainMenuUI] 按钮点击音效未配置，请在 Inspector 中分配 Button Click SFX 字段");
            }
        }

        /// <summary>
        /// 选择群系按钮点击回调
        /// </summary>
        private void OnBiomeSelectionButtonClicked()
        {
            PlayButtonClickSFX();
            // 每次点击时动态查找 BiomeSelectionPanel，确保引用有效
            // 使用 FindObjectOfType 查找场景中的组件（包括禁用的 GameObject）
            BiomeSelectionPanel panel = FindObjectOfType<BiomeSelectionPanel>(includeInactive: true);

            Debug.Log($"[MainMenuUI] Biome selection button clicked - panel={(panel != null ? "not null" : "null")}");
            if (panel != null)
            {
                Debug.Log("[MainMenuUI] Calling BiomeSelectionPanel.Show()");
                panel.Show();
            }
            else
            {
                Debug.LogError("[MainMenuUI] BiomeSelectionPanel is null!");
            }
        }

        /// <summary>
        /// 设置按钮点击回调
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            PlayButtonClickSFX();
            // 优先使用 Inspector 分配的引用，如果为空则动态查找
            SettingsPanel panel = settingsPanel;
            if (panel == null)
            {
                panel = FindObjectOfType<SettingsPanel>(includeInactive: true);
            }

            Debug.Log($"[MainMenuUI] Settings button clicked - panel={(panel != null ? "not null" : "null")}");
            if (panel != null)
            {
                Debug.Log("[MainMenuUI] Calling SettingsPanel.Show()");
                panel.Show();
            }
            else
            {
                Debug.LogError("[MainMenuUI] SettingsPanel is null!");
            }
        }

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
            PlayButtonClickSFX();
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
