using UnityEngine;

namespace RunnersJourney.Audio
{
    /// <summary>
    /// 音频管理器单例
    /// 负责管理背景音乐和音效的播放
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region 单例
        private static AudioManager _instance;

        /// <summary>
        /// 音频管理器单例实例
        /// </summary>
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region 序列化字段
        [Header("音频配置")]
        [Tooltip("音频配置文件")]
        [SerializeField] private AudioConfig audioConfig;

        [Header("音频源")]
        [Tooltip("背景音乐音频源")]
        [SerializeField] private AudioSource bgmSource;

        [Tooltip("音效音频源")]
        [SerializeField] private AudioSource sfxSource;

        [Header("音频剪辑")]
        [Tooltip("默认背景音乐剪辑")]
        [SerializeField] private AudioClip defaultBGM;
        #endregion

        #region 私有字段
        private AudioClip _currentBGM;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void Start()
        {
            // 加载保存的音量设置（PlayerPrefs 优先于 AudioConfig）
            float savedBGMVolume = PlayerPrefs.GetFloat("BGMVolume", audioConfig != null ? audioConfig.bgmVolume : 0.8f);
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", audioConfig != null ? audioConfig.sfxVolume : 0.6f);

            SetBGMVolume(savedBGMVolume);
            SetSFXVolume(savedSFXVolume);

            Debug.Log($"[AudioManager] 加载音量设置 - BGM: {savedBGMVolume}, SFX: {savedSFXVolume}");

            // 如果有默认 BGM，自动播放
            if (defaultBGM != null)
            {
                PlayBGM(defaultBGM);
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 初始化音频源
        /// </summary>
        private void InitializeAudioSources()
        {
            // 创建或获取 AudioSource 组件
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            // 配置 BGM 音频源
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f; // 2D 音频

            // 配置 SFX 音频源
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D 音频
        }
        #endregion

        #region 公共方法 - BGM 控制
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clip">要播放的音乐剪辑</param>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] 尝试播放 null 背景音乐");
                return;
            }

            _currentBGM = clip;
            bgmSource.clip = clip;
            bgmSource.Play();
            Debug.Log($"[AudioManager] 开始播放 BGM: {clip.name}");
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            if (bgmSource.isPlaying)
            {
                bgmSource.Stop();
                _currentBGM = null;
                Debug.Log("[AudioManager] 停止播放 BGM");
            }
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        /// <param name="volume">音量值 (0-1)</param>
        public void SetBGMVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            bgmSource.volume = volume;
        }

        /// <summary>
        /// 获取当前 BGM 音量
        /// </summary>
        /// <returns>当前音量值</returns>
        public float GetCurrentBGMVolume() => bgmSource.volume;
        #endregion

        #region 公共方法 - SFX 控制
        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="volume">音量值 (0-1)</param>
        public void SetSFXVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            sfxSource.volume = volume;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">要播放的音效剪辑</param>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] 尝试播放 null 音效");
                return;
            }
            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// 播放音效（带音量控制）
        /// </summary>
        /// <param name="clip">要播放的音效剪辑</param>
        /// <param name="volume">音量值 (0-1)</param>
        public void PlaySFX(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] 尝试播放 null 音效");
                return;
            }
            sfxSource.PlayOneShot(clip, volume);
        }
        #endregion

        #region 公共方法 - 音频配置
        /// <summary>
        /// 设置音频配置
        /// </summary>
        /// <param name="config">新的音频配置</param>
        public void SetAudioConfig(AudioConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("[AudioManager] 尝试设置 null 音频配置");
                return;
            }

            audioConfig = config;
            SetBGMVolume(config.bgmVolume);
            SetSFXVolume(config.sfxVolume);
        }
        #endregion

        #region 内部方法 - 测试辅助
        /// <summary>
        /// 重置单例实例（仅用于测试）
        /// </summary>
        internal static void ResetInstanceForTests()
        {
            _instance = null;
        }
        #endregion
    }
}
