using UnityEngine;
using UnityEngine.UIElements;
using RunnersJourney.Audio;

namespace RunnersJourney.UI
{
    /// <summary>
    /// 设置面板控制器 - 处理音量控制 UI
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        #region 序列化字段
        [Header("UI 组件")]
        [Tooltip("UIDocument 组件")]
        [SerializeField] private UIDocument uiDocument;

        [Header("音频")]
        [Tooltip("按钮点击音效剪辑")]
        [SerializeField] private AudioClip _buttonClickSFX;
        #endregion

        #region 私有字段
        private VisualElement _root;
        private Slider _bgmVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Label _bgmVolumeValue;
        private Label _sfxVolumeValue;
        private Button _closeButton;
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
            if (uiDocument != null)
            {
                // 如果 visualTreeAsset 未设置，尝试从 Resources 加载
                if (uiDocument.visualTreeAsset == null)
                {
                    var uxml = Resources.Load<VisualTreeAsset>("UI/SettingsPanel");
                    if (uxml != null)
                    {
                        uiDocument.visualTreeAsset = uxml;
                        Debug.Log("[SettingsPanel] 动态加载 SettingsPanel.uxml 成功");
                    }
                    else
                    {
                        Debug.LogWarning("[SettingsPanel] 无法从 Resources/UI/SettingsPanel 加载 UXML");
                        return;
                    }
                }

                // 等待 UI 构建完成
                StartCoroutine(InitializeAfterUIBuild());
            }
        }

        private System.Collections.IEnumerator InitializeAfterUIBuild()
        {
            // 等待一帧让 UI 构建完成
            yield return null;

            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                _root = uiDocument.rootVisualElement;
                InitializeUI();
                LoadVolumeSettings();
            }
        }

        private void OnDisable()
        {
            // 清理事件
            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.UnregisterValueChangedCallback(OnBGMVolumeChanged);
            }
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.UnregisterValueChangedCallback(OnSFXVolumeChanged);
            }
            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 显示设置面板
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            LoadVolumeSettings();
        }

        /// <summary>
        /// 隐藏设置面板
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
            // 获取滑块
            _bgmVolumeSlider = _root.Q<Slider>("bgm-volume-slider");
            _sfxVolumeSlider = _root.Q<Slider>("sfx-volume-slider");
            _bgmVolumeValue = _root.Q<Label>("bgm-volume-value");
            _sfxVolumeValue = _root.Q<Label>("sfx-volume-value");
            _closeButton = _root.Q<Button>("close-button");

            // 注册事件
            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.RegisterValueChangedCallback(OnBGMVolumeChanged);
            }
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.RegisterValueChangedCallback(OnSFXVolumeChanged);
            }
            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }
        }

        /// <summary>
        /// 加载音量设置
        /// </summary>
        private void LoadVolumeSettings()
        {
            float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.6f);

            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.value = bgmVolume;
                _bgmVolumeValue.text = $"{(int)(bgmVolume * 100)}%";
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = sfxVolume;
                _sfxVolumeValue.text = $"{(int)(sfxVolume * 100)}%";
            }

            // 应用音量到 AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(bgmVolume);
                AudioManager.Instance.SetSFXVolume(sfxVolume);
            }
        }

        /// <summary>
        /// BGM 音量变化回调
        /// </summary>
        private void OnBGMVolumeChanged(ChangeEvent<float> evt)
        {
            float newVolume = Mathf.Clamp01(evt.newValue);
            _bgmVolumeValue.text = $"{(int)(newVolume * 100)}%";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(newVolume);
            }

            PlayerPrefs.SetFloat("BGMVolume", newVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// SFX 音量变化回调
        /// </summary>
        private void OnSFXVolumeChanged(ChangeEvent<float> evt)
        {
            float newVolume = Mathf.Clamp01(evt.newValue);
            _sfxVolumeValue.text = $"{(int)(newVolume * 100)}%";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(newVolume);
            }

            PlayerPrefs.SetFloat("SFXVolume", newVolume);
            PlayerPrefs.Save();
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
        /// 关闭按钮点击回调
        /// </summary>
        private void OnCloseClicked()
        {
            PlayButtonClickSFX();
            Hide();
        }
        #endregion
    }
}
