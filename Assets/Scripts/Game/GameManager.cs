using UnityEngine;
using System;
using System.Collections;
using SquareFireline.Player;

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
        #endregion

        #region 私有字段
        /// <summary>
        /// 检查点位置（用于重生）
        /// </summary>
        private Vector3 _lastSafePosition;

        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        [SerializeField] private bool _enableDebugLog = false;
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
                _playerDeathController.OnPlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            if (_playerDeathController != null)
                _playerDeathController.OnPlayerDied -= OnPlayerDied;
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
        /// 重启游戏（从任何状态回到 Waiting）
        /// </summary>
        public void RestartGame()
        {
            ChangeState(GameState.Waiting);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 玩家死亡回调
        /// </summary>
        private void OnPlayerDied()
        {
            if (CurrentState != GameState.Playing)
            {
                if (_enableDebugLog)
                {
                    Debug.LogWarning($"[GameManager] OnPlayerDied called but state is {CurrentState}, expected Playing");
                }
                return;
            }

            ChangeState(GameState.Dying);
            StartCoroutine(StartRespawn());
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

            OnGameStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// 重生流程协程
        /// </summary>
        private IEnumerator StartRespawn()
        {
            // 等待死亡延迟
            float respawnDelay = _playerDeathController != null
                ? _playerDeathController.RespawnDelay
                : 1f;

            yield return new WaitForSeconds(respawnDelay);

            ChangeState(GameState.Respawning);

            // 重置玩家状态
            if (_playerJumpController != null)
            {
                _playerJumpController.ResetJumpState();
            }

            // 重置位置
            if (_playerDeathController != null)
            {
                _playerDeathController.SetSafePosition(_lastSafePosition);
                _playerDeathController.RespawnImmediately();
            }

            // 恢复游戏状态
            ChangeState(GameState.Playing);
        }
        #endregion
    }
}
