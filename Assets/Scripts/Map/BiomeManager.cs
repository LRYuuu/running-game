using UnityEngine;
using UnityEngine.Events;

namespace RunnersJourney.Map
{
    /// <summary>
    /// 生物群系管理器（单例）
    /// 支持两种模式：
    /// 1. 固定模式：使用单一群系配置
    /// 2. 混合模式：随距离自动切换群系序列
    /// </summary>
    public class BiomeManager : MonoBehaviour
    {
        #region 单例

        private static BiomeManager _instance;
        public static BiomeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BiomeManager>();
                }
                return _instance;
            }
        }

        #endregion

        #region 序列化字段

        [Header("默认群系")]
        [Tooltip("游戏启动时使用的默认群系（固定模式）")]
        public BiomeConfig defaultBiome;

        [Header("群系序列（混合模式）")]
        [Tooltip("群系序列配置（可选，用于混合模式）")]
        public BiomeSequence biomeSequence;

        #endregion

        #region 公共属性

        /// <summary>
        /// 当前激活的群系
        /// </summary>
        public BiomeConfig CurrentBiome { get; private set; }

        /// <summary>
        /// 当前使用的序列（混合模式）
        /// </summary>
        public BiomeSequence CurrentSequence { get; private set; }

        #endregion

        #region 私有字段

        /// <summary>
        /// 当前生成的 Chunk 数量（混合模式）
        /// </summary>
        private int _currentChunkCount = 0;

        /// <summary>
        /// 当前群系阶段索引（混合模式）
        /// </summary>
        private int _currentStageIndex = 0;

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
        }

        private void Start()
        {
            // 优先使用序列配置（混合模式）
            if (biomeSequence != null && biomeSequence.useSequence)
            {
                SetBiomeSequence(biomeSequence, 0);
            }
            // 否则使用固定模式
            else if (defaultBiome != null)
            {
                SetBiome(defaultBiome);
            }
        }

        #endregion

        #region 群系切换事件

        /// <summary>
        /// 群系切换事件
        /// </summary>
        public UnityEvent<BiomeConfig> OnBiomeChanged = new();

        #endregion

        #region 公共方法

        /// <summary>
        /// 固定模式：设置单一群系
        /// </summary>
        public void SetBiome(BiomeConfig newBiome)
        {
            if (newBiome == null)
            {
                Debug.LogWarning("[BiomeManager] 尝试设置为 null 的群系，使用默认群系");
                newBiome = defaultBiome;
            }

            CurrentBiome = newBiome;
            CurrentSequence = null;
            Debug.Log($"[BiomeManager] 切换到群系：{newBiome.biomeName}（固定模式）");
            OnBiomeChanged?.Invoke(newBiome);
        }

        /// <summary>
        /// 混合模式：设置群系序列
        /// </summary>
        /// <param name="sequence">群系序列配置</param>
        /// <param name="chunkCount">当前生成的 Chunk 数量</param>
        public void SetBiomeSequence(BiomeSequence sequence, int chunkCount = 0)
        {
            if (sequence == null)
            {
                Debug.LogWarning("[BiomeManager] 序列配置为 null");
                return;
            }

            CurrentSequence = sequence;
            _currentChunkCount = chunkCount;
            _currentStageIndex = 0;

            // 直接设置初始群系为第一个阶段
            if (sequence.biomeStages.Count > 0 && sequence.biomeStages[0].biome != null)
            {
                CurrentBiome = sequence.biomeStages[0].biome;
                Debug.Log($"[BiomeManager] 初始群系设置为：{CurrentBiome.biomeName}");
            }

            // 根据当前 Chunk 数量更新群系
            UpdateBiomeByChunkCount(chunkCount);
        }

        /// <summary>
        /// 混合模式：根据生成的 Chunk 数量更新群系
        /// </summary>
        /// <param name="chunkCount">已生成的 Chunk 数量</param>
        public void UpdateBiomeByChunkCount(int chunkCount)
        {
            if (CurrentSequence == null || !CurrentSequence.useSequence)
            {
                return; // 固定模式，不需要更新
            }

            // 计算当前应该处于哪个阶段
            int targetStageIndex = 0;
            int accumulatedChunks = 0;

            for (int i = 0; i < CurrentSequence.biomeStages.Count; i++)
            {
                var stage = CurrentSequence.biomeStages[i];

                if (stage.biome == null)
                {
                    continue;
                }

                if (chunkCount >= accumulatedChunks + stage.transitionChunks)
                {
                    accumulatedChunks += stage.transitionChunks;
                    targetStageIndex = i + 1;
                }
                else
                {
                    // 找到当前阶段
                    targetStageIndex = i;
                    break;
                }
            }

            // 限制到最大索引
            if (targetStageIndex >= CurrentSequence.biomeStages.Count)
            {
                targetStageIndex = CurrentSequence.biomeStages.Count - 1;
            }

            Debug.Log($"[BiomeManager] UpdateBiomeByChunkCount: chunkCount={chunkCount}, targetStageIndex={targetStageIndex}, currentStageIndex={_currentStageIndex}");

            // 如果阶段发生变化，切换群系
            if (targetStageIndex != _currentStageIndex)
            {
                var newBiome = CurrentSequence.biomeStages[targetStageIndex].biome;
                Debug.Log($"[BiomeManager] 切换到群系：{newBiome.biomeName}（Chunk 数量：{chunkCount}，阶段：{targetStageIndex + 1}/{CurrentSequence.biomeStages.Count}）");
                CurrentBiome = newBiome;
                _currentStageIndex = targetStageIndex;
                OnBiomeChanged?.Invoke(CurrentBiome);
            }
            else
            {
                Debug.Log($"[BiomeManager] 阶段未变化，当前群系：{CurrentBiome?.biomeName}");
            }

            _currentChunkCount = chunkCount;
        }

        /// <summary>
        /// 获取当前群系（供地图生成器使用）
        /// </summary>
        public BiomeConfig GetCurrentBiome()
        {
            return CurrentBiome ?? defaultBiome;
        }

        /// <summary>
        /// 重置进度（死亡重生时调用）
        /// </summary>
        public void ResetProgress()
        {
            _currentChunkCount = 0;
            _currentStageIndex = 0;

            if (CurrentSequence != null && CurrentSequence.useSequence)
            {
                // 混合模式：重置到起始群系
                // 显式设置 CurrentBiome 为第一个阶段
                if (CurrentSequence.biomeStages.Count > 0 && CurrentSequence.biomeStages[0].biome != null)
                {
                    CurrentBiome = CurrentSequence.biomeStages[0].biome;
                    Debug.Log($"[BiomeManager] 重置进度 - 回到起始群系：{CurrentBiome.biomeName}");
                }
                // 更新事件通知
                OnBiomeChanged?.Invoke(CurrentBiome);
            }
            else if (defaultBiome != null)
            {
                // 固定模式：重置到默认群系
                SetBiome(defaultBiome);
            }
        }

        #endregion
    }
}
