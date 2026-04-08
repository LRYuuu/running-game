using UnityEngine;
using System;
using System.Collections;
using RunnersJourney.Player;
using RunnersJourney.Map;
using RunnersJourney.Audio;

namespace RunnersJourney.Game
{
    /// <summary>
    /// 游戏管理器 - 集中管理游戏状态机
    /// 使用单例模式，跨场景持久化
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region 单例
        /// <summary>
        /// GameManager 单例实例
        /// </summary>
        public static GameManager Instance { get; private set; }
        #endregion

        #region 事件
        /// <summary>游戏状态改变时触发（旧状态，新状态）</summary>
        public event Action<GameState, GameState> OnGameStateChanged;
        #endregion

        #region 状态
        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Waiting;
        #endregion

        #region 依赖
        [Header("依赖引用")]
        [Tooltip("玩家死亡控制器")]
        [SerializeField] private PlayerDeathController _playerDeathController;

        [Tooltip("玩家跳跃控制器")]
        [SerializeField] private PlayerJumpController _playerJumpController;

        [Tooltip("地图生成器")]
        [SerializeField] private TilemapEndlessMapGenerator _mapGenerator;

        [Header("游戏配置")]
        [Tooltip("游戏配置 ScriptableObject")]
        [SerializeField] private GameConfig _gameConfig;

        [Header("难度系统")]
        [Tooltip("难度计算器引用")]
        [SerializeField] private DifficultyCalculator _difficultyCalculator;

        [Header("群系系统")]
        [Tooltip("群系管理器引用（混合模式）")]
        [SerializeField] private BiomeManager _biomeManager;

        [Header("音频系统")]
        [Tooltip("游戏背景音乐")]
        [SerializeField] private AudioClip _gameBGM;
        #endregion

        #region 私有字段
        /// <summary>
        /// 检查点位置（用于重生）
        /// </summary>
        private Vector3 _lastSafePosition;

        /// <summary>
        /// 玩家前进距离（用于难度计算和群系切换）- 基于地图滚动距离
        /// </summary>
        private float _playerDistance = 0f;

        /// <summary>
        /// 上一次更新的距离值（避免重复调用）
        /// </summary>
        private float _lastUpdateDistance = 0f;

        /// <summary>
        /// 距离更新阈值（超过此值才更新难度）
        /// </summary>
        private float _distanceUpdateThreshold = 1f;

        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        [SerializeField] private bool _enableDebugLog = false;

        // 世界暂停状态
        private bool _isWorldPaused = false;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            // 单例初始化
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // DontDestroyOnLoad 只能在 play mode 下使用
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            // 自动获取地图生成器引用（如果 Inspector 中未配置）
            if (_mapGenerator == null)
            {
                _mapGenerator = FindObjectOfType<TilemapEndlessMapGenerator>();
                if (_mapGenerator == null && _enableDebugLog)
                {
                    Debug.LogWarning("[GameManager] 未找到 TilemapEndlessMapGenerator 引用！");
                }
            }

            // 初始化检查点为当前玩家位置
            if (_playerDeathController != null)
            {
                _lastSafePosition = _playerDeathController.transform.position;
            }

            // 获取或自动查找 BiomeManager 引用
            if (_biomeManager == null)
            {
                _biomeManager = BiomeManager.Instance;
            }

            if (_biomeManager != null)
            {
                Debug.Log($"[GameManager] BiomeManager 引用已设置：{_biomeManager.name}");
            }
            else
            {
                Debug.LogWarning("[GameManager] BiomeManager 引用为空，请确保场景中有 BiomeManager GameObject");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnEnable()
        {
            // 订阅玩家死亡事件
            if (_playerDeathController != null)
            {
                Debug.Log("[GameManager] 订阅 OnPlayerDied 事件");
                _playerDeathController.OnPlayerDied += OnPlayerDied;
            }
            else
            {
                Debug.LogWarning("[GameManager] _playerDeathController 为空，无法订阅事件");
            }

            // 初始化世界暂停状态（Waiting 状态下暂停世界）
            if (CurrentState == GameState.Waiting)
            {
                SetWorldPaused(true);
            }
        }

        private void OnDisable()
        {
            if (_playerDeathController != null)
            {
                _playerDeathController.OnPlayerDied -= OnPlayerDied;
            }
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                // 使用地图滚动距离作为玩家前进距离
                float newDistance = 0f;
                if (_mapGenerator != null)
                {
                    newDistance = _mapGenerator.GetScrollDistance();
                }
                else if (_playerDeathController != null)
                {
                    // 回退：使用玩家位置（如果地图生成器不可用）
                    newDistance = Mathf.Max(0f, _playerDeathController.transform.position.x);
                }

                if (newDistance > _playerDistance)
                {
                    _playerDistance = newDistance;

                    // 性能优化：只有当距离变化超过阈值时才更新难度
                    // 避免每帧都调用 DifficultyCalculator
                    if (_playerDistance - _lastUpdateDistance >= _distanceUpdateThreshold)
                    {
                        _lastUpdateDistance = _playerDistance;

                        if (_difficultyCalculator != null)
                        {
                            _difficultyCalculator.SetPlayerDistance(_playerDistance);
                        }
                    }
                }
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 重置单例实例（仅用于测试）
        /// </summary>
        public static void ResetInstance()
        {
            Instance = null;
        }

        /// <summary>
        /// 开始游戏（从 Waiting → Playing）
        /// </summary>
        public void StartGame()
        {
            if (CurrentState != GameState.Waiting)
            {
                if (_enableDebugLog)
                {
                    Debug.LogWarning($"[GameManager] StartGame called but state is {CurrentState}, expected Waiting");
                }
            }

            // 验证地图生成器引用
            if (_mapGenerator == null)
            {
                _mapGenerator = FindObjectOfType<TilemapEndlessMapGenerator>();
                if (_mapGenerator == null)
                {
                    Debug.LogError("[GameManager] 未找到 TilemapEndlessMapGenerator！请确保场景中有地图生成器组件。");
                }
                else if (_enableDebugLog)
                {
                    Debug.Log($"[GameManager] 已自动获取地图生成器引用：{_mapGenerator.name}");
                }
            }

            // 重置玩家起始位置（游戏开始时保存当前位置）
            if (_playerDeathController != null)
            {
                _playerDeathController.ResetStartPosition();
            }

            // 应用群系选择（故事 6-2）
            ApplyBiomeSelection();

            // 初始化地图（确保地图已生成）
            if (_mapGenerator != null)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[GameManager] 初始化地图...");
                }
                _mapGenerator.Initialize();
            }

            // 播放背景音乐（Story 7-1）
            if (AudioManager.Instance != null && _gameBGM != null)
            {
                AudioManager.Instance.PlayBGM(_gameBGM);
            }
            else if (_gameBGM == null)
            {
                Debug.LogWarning("[GameManager] _gameBGM 未设置，请在 Inspector 中指定背景音乐剪辑");
            }

            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// 更新检查点位置
        /// </summary>
        /// <param name="position">安全位置</param>
        public void UpdateCheckpoint(Vector3 position)
        {
            _lastSafePosition = position;
            if (_enableDebugLog)
            {
                Debug.Log($"[GameManager] Checkpoint updated: {_lastSafePosition}");
            }
        }

        /// <summary>
        /// 获取最后一个安全位置
        /// </summary>
        /// <returns>检查点位置</returns>
        public Vector3 GetLastSafePosition()
        {
            return _lastSafePosition;
        }

        /// <summary>
        /// 重启游戏（从任何状态回到 Waiting）
        /// </summary>
        public void RestartGame()
        {
            // 重置难度进度
            if (_difficultyCalculator != null)
            {
                _difficultyCalculator.ResetProgress();
            }

            // 重置群系进度（混合模式）
            if (_biomeManager != null)
            {
                _biomeManager.ResetProgress();
            }

            // 重置玩家距离
            _playerDistance = 0f;

            ChangeState(GameState.Waiting);
        }

        /// <summary>
        /// 从死亡状态重新开始游戏（玩家点击重新开始按钮后调用）
        /// </summary>
        public void ResumeGameFromDeath()
        {
            Debug.Log("[GameManager] ResumeGameFromDeath called - starting respawn sequence");

            // 重置难度进度
            if (_difficultyCalculator != null)
            {
                _difficultyCalculator.ResetProgress();
            }

            // 重置群系进度（混合模式）
            if (_biomeManager != null)
            {
                _biomeManager.ResetProgress();
            }

            // 重置玩家距离
            _playerDistance = 0f;

            // 1. 清理地图和障碍物
            if (_mapGenerator != null)
            {
                _mapGenerator.Cleanup();
            }

            // 2. 重置玩家跳跃状态
            if (_playerJumpController != null)
            {
                _playerJumpController.ResetJumpState();
            }

            // 3. 重置玩家起始位置
            if (_playerDeathController != null)
            {
                _playerDeathController.ResetStartPosition();
            }

            // 4. 重置玩家位置
            if (_playerDeathController != null)
            {
                _playerDeathController.Respawn();
            }

            // 5. 重新生成地图
            if (_mapGenerator != null)
            {
                _mapGenerator.Initialize();
            }

            // 6. 恢复游戏状态
            ChangeState(GameState.Playing);
            Debug.Log("[GameManager] Game resumed from death - respawn complete");
        }

        /// <summary>
        /// 设置世界暂停状态（暂停/恢复地图和背景滚动）
        /// </summary>
        /// <param name="paused">true=暂停，false=恢复</param>
        public void SetWorldPaused(bool paused)
        {
            if (_isWorldPaused == paused)
                return;

            _isWorldPaused = paused;

            if (_enableDebugLog)
            {
                Debug.Log($"[GameManager] 世界已{(paused ? "暂停" : "恢复")}");
            }

            // 暂停/恢复地图滚动
            if (_mapGenerator != null)
            {
                _mapGenerator.SetScrollPaused(paused);
            }

            // 暂停/恢复背景滚动
            var backgrounds = FindObjectsOfType<MonoBehaviour>();
            foreach (var bg in backgrounds)
            {
                if (bg is ParallaxBackground parallaxBg)
                {
                    parallaxBg.SetScrollPaused(paused);
                }
                else if (bg is ParallaxBackgroundTilemap tilemapBg)
                {
                    tilemapBg.SetScrollPaused(paused);
                }
            }
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 应用群系选择（故事 6-2）
        /// 根据玩家选择的群系模式设置 BiomeManager
        /// </summary>
        private void ApplyBiomeSelection()
        {
            Debug.Log("[GameManager] ApplyBiomeSelection() started");

            // 检查玩家是否选择了群系模式
            if (BiomeSelectionData.HasSelectedBefore())
            {
                string selectedMode = BiomeSelectionData.LoadSelection();
                Debug.Log($"[GameManager] ApplyBiomeSelection: HasSelectedBefore=true, selectedMode={selectedMode}");

                if (!string.IsNullOrEmpty(selectedMode))
                {
                    // 判断是否为混合模式
                    if (BiomeSelectionData.IsSequenceMode(selectedMode))
                    {
                        // 混合模式：使用群系序列
                        var sequence = GetBiomeSequence();
                        if (sequence != null && _biomeManager != null)
                        {
                            _biomeManager.SetBiomeSequence(sequence);
                            Debug.Log("[GameManager] 使用混合群系模式");
                        }
                        else if (_biomeManager == null)
                        {
                            Debug.LogWarning("[GameManager] BiomeManager 为空，无法设置混合模式");
                        }
                        else
                        {
                            Debug.LogWarning("[GameManager] 群系序列配置为 null");
                        }
                    }
                    else
                    {
                        // 固定模式：使用选定群系
                        var biomeConfig = FindBiomeConfigByMode(selectedMode);
                        Debug.Log($"[GameManager] FindBiomeConfigByMode({selectedMode}) result: {(biomeConfig != null ? "found" : "null")}");

                        if (biomeConfig != null && _biomeManager != null)
                        {
                            _biomeManager.SetBiome(biomeConfig);
                            Debug.Log($"[GameManager] 使用固定群系：{selectedMode}");
                        }
                        else if (_biomeManager == null)
                        {
                            Debug.LogWarning("[GameManager] BiomeManager 为空，无法设置群系");
                        }
                        else if (biomeConfig == null)
                        {
                            Debug.LogWarning($"[GameManager] 未找到群系配置：{selectedMode}");
                        }
                    }
                    return;
                }
            }
            else
            {
                Debug.Log("[GameManager] ApplyBiomeSelection: HasSelectedBefore=false");
            }

            // 如果玩家未选择，使用 BiomeManager 的默认配置
            Debug.Log("[GameManager] 使用默认群系配置");
            if (_biomeManager != null)
            {
                // 优先使用 Inspector 中配置的 defaultBiome
                BiomeConfig defaultBiome = _biomeManager.defaultBiome;

                // 如果 Inspector 中未配置，尝试从 Resources 加载草地群系
                if (defaultBiome == null)
                {
                    var allBiomes = Resources.LoadAll<BiomeConfig>("Biomes");
                    foreach (var biome in allBiomes)
                    {
                        if (biome.biomeName.ToLower().Contains("grassland") ||
                            biome.biomeName.ToLower().Contains("草地") ||
                            biome.biomeName.ToLower().Contains("grass"))
                        {
                            defaultBiome = biome;
                            Debug.Log($"[GameManager] 从 Resources 加载默认群系：{biome.biomeName}");
                            break;
                        }
                    }
                }

                if (defaultBiome != null)
                {
                    _biomeManager.SetBiome(defaultBiome);
                    Debug.Log($"[GameManager] 默认群系已设置：{defaultBiome.biomeName}");
                }
                else
                {
                    Debug.LogError("[GameManager] 无法找到默认群系配置，请确保 Resources/Biomes 目录下有 GrasslandBiome 资产");
                }
            }
            else
            {
                Debug.LogWarning("[GameManager] BiomeManager 为空，无法设置默认群系");
            }
        }

        /// <summary>
        /// 获取群系序列配置
        /// </summary>
        private BiomeSequence GetBiomeSequence()
        {
            var sequences = Resources.LoadAll<BiomeSequence>("Biomes/Sequences");
            if (sequences.Length > 0)
            {
                return sequences[0];
            }
            return null;
        }

        /// <summary>
        /// 根据模式键值查找群系配置
        /// </summary>
        private BiomeConfig FindBiomeConfigByMode(string modeKey)
        {
            var allBiomes = Resources.LoadAll<BiomeConfig>("Biomes");
            Debug.Log($"[GameManager] FindBiomeConfigByMode: found {allBiomes.Length} biomes");

            foreach (var biome in allBiomes)
            {
                Debug.Log($"[GameManager] Checking biome: {biome.biomeName}");

                // 检查 biomeName 是否包含对应的英文单词
                string biomeNameLower = biome.biomeName.ToLower();
                bool isMatch = false;

                if (modeKey == BiomeSelectionData.ModeGrassland && biomeNameLower.Contains("grassland"))
                {
                    isMatch = true;
                }
                else if (modeKey == BiomeSelectionData.ModeDesert && biomeNameLower.Contains("desert"))
                {
                    isMatch = true;
                }
                else if (modeKey == BiomeSelectionData.ModeSnowland && biomeNameLower.Contains("snowland"))
                {
                    isMatch = true;
                }
                else if (modeKey == BiomeSelectionData.ModeSequence && biomeNameLower.Contains("sequence"))
                {
                    isMatch = true;
                }

                if (isMatch)
                {
                    Debug.Log($"[GameManager] Found matching biome: {biome.biomeName} for mode: {modeKey}");
                    return biome;
                }
            }

            Debug.LogWarning($"[GameManager] No matching biome found for mode: {modeKey}");
            return null;
        }

        /// <summary>
        /// 根据群系名称获取模式键值
        /// </summary>
        private string GetModeKeyForBiome(string biomeName)
        {
            switch (biomeName.ToLower())
            {
                case "grassland": return BiomeSelectionData.ModeGrassland;
                case "desert": return BiomeSelectionData.ModeDesert;
                case "snowland": return BiomeSelectionData.ModeSnowland;
                default: return biomeName.ToLower();
            }
        }

        /// <summary>
        /// 玩家死亡回调
        /// </summary>
        private void OnPlayerDied()
        {
            Debug.Log($"[GameManager] OnPlayerDied 被调用，当前状态：{CurrentState}");

            if (CurrentState != GameState.Playing)
            {
                Debug.LogWarning($"[GameManager] OnPlayerDied called but state is {CurrentState}, expected Playing");
                return;
            }

            Debug.Log("[GameManager] 玩家死亡，进入 Dying 状态（等待玩家操作）");
            ChangeState(GameState.Dying);
            // 不再自动重生，等待玩家点击重新开始按钮
        }

        /// <summary>
        /// 改变游戏状态
        /// </summary>
        /// <param name="newState">新状态</param>
        private void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
                return;

            GameState oldState = CurrentState;
            CurrentState = newState;

            // 根据状态自动暂停/恢复世界
            if (newState == GameState.Waiting || newState == GameState.Dying)
            {
                // Waiting/Dying 状态：暂停世界滚动
                SetWorldPaused(true);
            }
            else if (newState == GameState.Playing)
            {
                // Playing 状态：恢复世界滚动
                SetWorldPaused(false);
            }

            OnGameStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// 重生流程协程
        /// </summary>
        private IEnumerator StartRespawn()
        {
            if (_enableDebugLog)
            {
                Debug.Log("[GameManager] 开始重生流程...");
            }

            // 等待死亡延迟 - 从 GameConfig 读取
            float respawnDelay = _gameConfig != null
                ? _gameConfig.respawnDelay
                : 1f;

            if (_enableDebugLog)
            {
                Debug.Log($"[GameManager] 等待重生延迟：{respawnDelay}秒");
            }

            yield return new WaitForSeconds(respawnDelay);

            ChangeState(GameState.Respawning);

            // 1. 清理地图和障碍物
            if (_mapGenerator != null)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[GameManager] 清理地图和障碍物...");
                }
                _mapGenerator.Cleanup();
            }
            else
            {
                if (_enableDebugLog)
                {
                    Debug.LogWarning("[GameManager] _mapGenerator 为空，跳过地图清理");
                }
            }

            // 2. 重置玩家跳跃状态
            if (_playerJumpController != null)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[GameManager] 重置玩家跳跃状态");
                }
                _playerJumpController.ResetJumpState();
            }

            // 3. 重置玩家位置到起始点
            if (_playerDeathController != null)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[GameManager] 重置玩家位置到起始点");
                }
                _playerDeathController.Respawn();
            }
            else
            {
                if (_enableDebugLog)
                {
                    Debug.LogWarning("[GameManager] _playerDeathController 为空，跳过玩家位置重置");
                }
            }

            // 4. 重新生成地图
            if (_mapGenerator != null)
            {
                Debug.Log("[GameManager] 重新生成地图...");
                _mapGenerator.Initialize();
                Debug.Log("[GameManager] 地图重新生成完成");
            }
            else
            {
                Debug.LogWarning("[GameManager] _mapGenerator 为空，跳过地图重新生成");
            }

            // 5. 恢复游戏状态
            ChangeState(GameState.Playing);

            if (_enableDebugLog)
            {
                Debug.Log("[GameManager] 重生流程完成 - 玩家已回到起始位置，地图已重新生成");
            }
        }
        #endregion
    }
}
