using UnityEngine;
using System;
using System.Collections;
using SquareFireline.Player;
using SquareFireline.Map;

namespace SquareFireline.Game
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
        #endregion

        #region 私有字段
        /// <summary>
        /// 检查点位置（用于重生）
        /// </summary>
        private Vector3 _lastSafePosition;

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

            // 初始化地图（确保地图已生成）
            if (_mapGenerator != null)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[GameManager] 初始化地图...");
                }
                _mapGenerator.Initialize();
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
            ChangeState(GameState.Waiting);
        }

        /// <summary>
        /// 从死亡状态重新开始游戏（玩家点击重新开始按钮后调用）
        /// </summary>
        public void ResumeGameFromDeath()
        {
            Debug.Log("[GameManager] ResumeGameFromDeath called - starting respawn sequence");

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
