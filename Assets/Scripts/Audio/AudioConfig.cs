using UnityEngine;

namespace RunnersJourney.Audio
{
    /// <summary>
    /// 音频配置 ScriptableObject
    /// 用于配置背景音乐、音效和总音量参数
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Runner's Journey/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        #region 音量设置
        [Header("音量设置")]
        [Tooltip("背景音乐默认音量 (0-1)")]
        [Range(0f, 1f)]
        public float bgmVolume = 0.8f;

        [Tooltip("音效默认音量 (0-1)")]
        [Range(0f, 1f)]
        public float sfxVolume = 0.6f;

        [Tooltip("总音量控制 (0-1)")]
        [Range(0f, 1f)]
        public float masterVolume = 1.0f;
        #endregion
    }
}
