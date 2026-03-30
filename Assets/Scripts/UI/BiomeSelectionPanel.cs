using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using RunnersJourney.Map;
using RunnersJourney.Game;
using RunnersJourney.Audio;

namespace RunnersJourney.UI
{
    /// <summary>
    /// 群系选择面板控制器
    /// 支持四种模式：草地、沙漠、雪地、混合群系
    /// </summary>
    public class BiomeSelectionPanel : MonoBehaviour
    {
        #region 序列化字段

        [Header("UI 组件")]
        [Tooltip("UIDocument 组件")]
        [SerializeField] private UIDocument uiDocument;

        [Header("序列化字段")]
        [Tooltip("群系选项按钮的命名前缀")]
        [SerializeField] private string biomeButtonPrefix = "biome-button-";

        [Tooltip("确认按钮名称")]
        [SerializeField] private string confirmButtonName = "confirm-button";

        [Tooltip("取消按钮名称")]
        [SerializeField] private string cancelButtonName = "cancel-button";

        [Tooltip("预览图片名称")]
        [SerializeField] private string previewImageName = "preview-image";

        [Tooltip("描述文本标签名称")]
        [SerializeField] private string descriptionLabelName = "description-label";

        [Header("音频")]
        [Tooltip("按钮点击音效剪辑")]
        [SerializeField] private AudioClip _buttonClickSFX;

        #endregion

        #region 私有字段

        private VisualElement _root;
        private string _selectedModeKey;
        private Dictionary<string, Button> _modeButtons = new Dictionary<string, Button>();
        private Image _previewImage;
        private Label _descriptionLabel;

        // 模式描述
        private static readonly Dictionary<string, string> ModeDescriptions = new Dictionary<string, string>
        {
            { BiomeSelectionData.ModeGrassland, "翠绿的草原，适合新手练习" },
            { BiomeSelectionData.ModeDesert, "金色的沙漠，中等难度挑战" },
            { BiomeSelectionData.ModeSnowland, "冰雪覆盖，高难度考验" },
            { BiomeSelectionData.ModeSequence, "随距离自动切换：草原→沙漠→雪地" }
        };

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            Debug.Log("[BiomeSelectionPanel] Awake called");

            // 测试用：清除之前的选择数据，确保测试新的选择
            BiomeSelectionData.Clear();
            Debug.Log("[BiomeSelectionPanel] Previous selection cleared for testing");

            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
        }

        private void OnEnable()
        {
            Debug.Log("[BiomeSelectionPanel] OnEnable called");

            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            // 只在_root 为空时初始化（避免重复初始化）
            if (uiDocument != null && uiDocument.rootVisualElement != null && _root == null)
            {
                _root = uiDocument.rootVisualElement;
                Debug.Log("[BiomeSelectionPanel] _root initialized in OnEnable");
                Debug.Log("[BiomeSelectionPanel] Calling InitializeUI() from OnEnable");
                InitializeUI();
            }
        }

        private void Start()
        {
            Debug.Log("[BiomeSelectionPanel] Start called");
            // 确保启动时面板隐藏 - 禁用 GameObject 而不是 UIDocument
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            // 清理按钮事件
            if (_modeButtons != null)
            {
                foreach (var button in _modeButtons.Values)
                {
                    button.clicked -= () => { };
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            Debug.Log($"[BiomeSelectionPanel] Show() called, gameObject.active={gameObject.activeSelf}, _root={(_root != null ? "not null" : "null")}, _modeButtons.Count={_modeButtons.Count}, uiDocument={(_root != null ? "not null" : "null")}");

            gameObject.SetActive(true);

            // 强制 UIDocument 重新加载面板
            if (uiDocument != null)
            {
                uiDocument.enabled = false;
                uiDocument.enabled = true;
                Debug.Log("[BiomeSelectionPanel] uiDocument refreshed");
            }

            // 刷新 UIDocument 后，需要重新获取_root 并重新初始化 UI
            Debug.Log("[BiomeSelectionPanel] Starting re-initialization after UIDocument refresh");
            StartCoroutine(InitializeAfterDelay());
        }

        /// <summary>
        /// 延迟初始化协程
        /// </summary>
        private System.Collections.IEnumerator InitializeAfterDelay()
        {
            yield return null; // 等待一帧

            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                _root = uiDocument.rootVisualElement;
                Debug.Log($"[BiomeSelectionPanel] _root initialized after delay: {(_root != null ? "success" : "failed")}");
                InitializeUI();
                LoadLastSelection();
            }
            else
            {
                Debug.LogError("[BiomeSelectionPanel] Failed to initialize _root after delay");
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化 UI 组件
        /// </summary>
        private void InitializeUI()
        {
            Debug.Log($"[BiomeSelectionPanel] InitializeUI started, _root={(_root != null ? "not null" : "null")}");

            // 获取预览组件
            _previewImage = _root.Q<Image>(previewImageName);
            _descriptionLabel = _root.Q<Label>(descriptionLabelName);
            Debug.Log($"[BiomeSelectionPanel] Preview components initialized: _previewImage={(_previewImage != null ? "not null" : "null")}, _descriptionLabel={(_descriptionLabel != null ? "not null" : "null")}");

            // 注册草地模式按钮
            RegisterBiomeButton(BiomeSelectionData.ModeGrassland, "草原");

            // 注册沙漠模式按钮
            RegisterBiomeButton(BiomeSelectionData.ModeDesert, "沙漠");

            // 注册雪地模式按钮
            RegisterBiomeButton(BiomeSelectionData.ModeSnowland, "雪地");

            // 注册混合模式按钮
            string seqButtonName = $"{biomeButtonPrefix}{BiomeSelectionData.ModeSequence}";
            var seqButton = _root.Q<Button>(seqButtonName);
            if (seqButton != null)
            {
                _modeButtons[BiomeSelectionData.ModeSequence] = seqButton;
                seqButton.clicked += () =>
                {
                    PlayButtonClickSFX();
                    OnModeSelected(BiomeSelectionData.ModeSequence, null);
                };
                Debug.Log($"[BiomeSelectionPanel] 注册混合模式按钮：{seqButtonName}");
            }
            else
            {
                Debug.LogWarning($"[BiomeSelectionPanel] 未找到混合模式按钮：{seqButtonName}");
            }

            Debug.Log("[BiomeSelectionPanel] Starting confirm/cancel button registration");

            // 注册确认/取消按钮
            var confirmButton = _root.Q<Button>(confirmButtonName);
            Debug.Log($"[BiomeSelectionPanel] confirmButton result: {(confirmButton != null ? "found" : "null")}");
            if (confirmButton != null)
            {
                confirmButton.clicked += () =>
                {
                    PlayButtonClickSFX();
                    OnConfirmClicked();
                };
                Debug.Log($"[BiomeSelectionPanel] 确认按钮已注册：{confirmButtonName}");
            }
            else
            {
                Debug.LogWarning($"[BiomeSelectionPanel] 未找到确认按钮：{confirmButtonName}");
            }

            var cancelButton = _root.Q<Button>(cancelButtonName);
            Debug.Log($"[BiomeSelectionPanel] cancelButton result: {(cancelButton != null ? "found" : "null")}");
            if (cancelButton != null)
            {
                cancelButton.clicked += () =>
                {
                    PlayButtonClickSFX();
                    OnCancelClicked();
                };
                Debug.Log($"[BiomeSelectionPanel] 取消按钮已注册：{cancelButtonName}");
            }
            else
            {
                Debug.LogWarning($"[BiomeSelectionPanel] 未找到取消按钮：{cancelButtonName}");
            }

            // 加载上次选择
            LoadLastSelection();

            Debug.Log("[BiomeSelectionPanel] InitializeUI completed");
        }

        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        private void PlayButtonClickSFX()
        {
            if (_buttonClickSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_buttonClickSFX);
            }
        }

        /// <summary>
        /// 注册群系按钮
        /// </summary>
        private void RegisterBiomeButton(string modeKey, string displayName)
        {
            string buttonName = $"{biomeButtonPrefix}{modeKey}";
            var button = _root.Q<Button>(buttonName);
            if (button != null)
            {
                _modeButtons[modeKey] = button;
                button.clicked += () =>
                {
                    PlayButtonClickSFX();
                    OnModeSelected(modeKey, null);
                };
                Debug.Log($"[BiomeSelectionPanel] 注册群系按钮：{buttonName}");
            }
            else
            {
                Debug.LogWarning($"[BiomeSelectionPanel] 未找到群系按钮：{buttonName}");
            }
        }

        /// <summary>
        /// 加载上次选择的模式
        /// </summary>
        private void LoadLastSelection()
        {
            Debug.Log($"[BiomeSelectionPanel] LoadLastSelection called, _selectedModeKey={_selectedModeKey}");

            string lastKey = BiomeSelectionData.LoadSelection();
            if (!string.IsNullOrEmpty(lastKey) && _modeButtons.ContainsKey(lastKey))
            {
                _selectedModeKey = lastKey;
                UpdateButtonVisual();
                UpdatePreview();
            }
            else
            {
                // 默认选择草地
                _selectedModeKey = BiomeSelectionData.ModeGrassland;
                UpdateButtonVisual();
                UpdatePreview();
            }

            Debug.Log($"[BiomeSelectionPanel] LoadLastSelection completed, final _selectedModeKey={_selectedModeKey}");
        }

        /// <summary>
        /// 玩家选择群系模式
        /// </summary>
        private void OnModeSelected(string modeKey, BiomeConfig biome)
        {
            _selectedModeKey = modeKey;
            UpdateButtonVisual();
            UpdatePreview();
        }

        /// <summary>
        /// 更新按钮选中状态
        /// </summary>
        private void UpdateButtonVisual()
        {
            foreach (var kvp in _modeButtons)
            {
                if (kvp.Key == _selectedModeKey)
                {
                    kvp.Value.EnableInClassList("selected", true);
                }
                else
                {
                    kvp.Value.EnableInClassList("selected", false);
                }
            }
        }

        /// <summary>
        /// 更新预览显示
        /// </summary>
        private void UpdatePreview()
        {
            // 根据选择的模式更新预览背景色
            if (_previewImage != null)
            {
                switch (_selectedModeKey)
                {
                    case BiomeSelectionData.ModeGrassland:
                        _previewImage.style.backgroundColor = new Color(0.3f, 0.6f, 0.3f);
                        break;
                    case BiomeSelectionData.ModeDesert:
                        _previewImage.style.backgroundColor = new Color(0.8f, 0.7f, 0.3f);
                        break;
                    case BiomeSelectionData.ModeSnowland:
                        _previewImage.style.backgroundColor = new Color(0.9f, 0.9f, 1f);
                        break;
                    case BiomeSelectionData.ModeSequence:
                        _previewImage.style.backgroundColor = new Color(0.5f, 0.5f, 0.8f);
                        break;
                }
            }

            // 更新描述
            if (_descriptionLabel != null && !string.IsNullOrEmpty(_selectedModeKey))
            {
                if (ModeDescriptions.ContainsKey(_selectedModeKey))
                {
                    _descriptionLabel.text = ModeDescriptions[_selectedModeKey];
                }
            }
        }

        /// <summary>
        /// 确认选择
        /// </summary>
        private void OnConfirmClicked()
        {
            Debug.Log($"[BiomeSelectionPanel] OnConfirmClicked called, _selectedModeKey={_selectedModeKey}");

            if (!string.IsNullOrEmpty(_selectedModeKey))
            {
                // 保存选择
                BiomeSelectionData.SaveSelection(_selectedModeKey);
                Debug.Log($"[BiomeSelectionPanel] 确认选择：{_selectedModeKey}");

                // 关闭面板
                Hide();
            }
            else
            {
                Debug.LogWarning("[BiomeSelectionPanel] OnConfirmClicked called but _selectedModeKey is empty");
            }
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        private void OnCancelClicked()
        {
            // 恢复上次选择
            LoadLastSelection();
            Hide();
        }

        #endregion
    }
}
