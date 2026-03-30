using UnityEngine;
using System;
using RunnersJourney.Game;
using RunnersJourney.Audio;

namespace RunnersJourney.Player
{
    /// <summary>
    /// 玩家死亡控制器 - 处理玩家死亡逻辑和事件
    /// </summary>
    public class PlayerDeathController : MonoBehaviour
    {
        #region 序列化字段
        [Header("死亡设置")]
        [Tooltip("死亡后重生延迟（秒）")]
        [SerializeField] private float _respawnDelay = 1f;

        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        [SerializeField] private bool _enableDebugLog = false;

        [Header("碰撞设置")]
        [Tooltip("障碍物所在的 Layer")]
        [SerializeField] private LayerMask _obstacleLayer = 0;

        [Header("音频")]
        [Tooltip("碰撞音效剪辑（玩家死亡时播放）")]
        [SerializeField] private AudioClip _collisionSFX;
        #endregion

        #region 事件
        /// <summary>
        /// 玩家死亡事件
        /// </summary>
        public event Action OnPlayerDied;
        #endregion

        #region 私有字段
        private bool _isDead = false;
        private PlayerJumpController _jumpController;
        private Vector3 _startPosition;
        private bool _isCollisionDetectionEnabled = true;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            _jumpController = GetComponent<PlayerJumpController>();
            // 保存游戏开始时的初始位置
            _startPosition = transform.position;

            // 检查必需的组件
            var rb2d = GetComponent<Rigidbody2D>();
            var collider2d = GetComponent<Collider2D>();

            if (rb2d == null)
            {
                Debug.LogError($"[PlayerDeathController] 玩家缺少 Rigidbody2D 组件！gameObject={gameObject.name}");
            }
            if (collider2d == null)
            {
                Debug.LogError($"[PlayerDeathController] 玩家缺少 Collider2D 组件！gameObject={gameObject.name}");
            }

            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] 组件检查 - Rigidbody2D: {(rb2d != null ? "OK" : "MISSING")}, Collider2D: {(collider2d != null ? "OK" : "MISSING")}");
            }
        }

        private void OnEnable()
        {
            // 订阅游戏状态变化事件
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                // 初始化碰撞检测状态
                _isCollisionDetectionEnabled = GameManager.Instance.CurrentState != GameState.Waiting;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        // 添加碰撞检测的调试日志
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Waiting 状态下禁用碰撞检测
            if (!_isCollisionDetectionEnabled)
            {
                return;
            }

            Debug.Log($"[PlayerDeathController] 发生碰撞：{collision.gameObject.name}, 接触点数量：{collision.contactCount}");

            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] OnCollisionEnter2D: {collision.gameObject.name}, layer: {collision.gameObject.layer}, tag: {collision.gameObject.tag}");
            }

            if (IsObstacle(collision.gameObject))
            {
                HandleObstacleCollision();
            }
        }

        /// <summary>
        /// 检测与障碍物的触发器碰撞
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Waiting 状态下禁用碰撞检测
            if (!_isCollisionDetectionEnabled)
            {
                return;
            }

            Debug.Log($"[PlayerDeathController] 触发器碰撞：{other.gameObject.name}");

            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] OnTriggerEnter2D: {other.gameObject.name}, layer: {other.gameObject.layer}, tag: {other.gameObject.tag}");
            }

            if (IsObstacle(other.gameObject))
            {
                HandleObstacleCollision();
            }
        }

        /// <summary>
        /// 检查对象是否是障碍物
        /// </summary>
        private bool IsObstacle(GameObject obj)
        {
            // 方法 1: 通过 Layer 检测
            if (_obstacleLayer.value > 0)
            {
                bool isObstacle = ((_obstacleLayer.value & (1 << obj.layer)) != 0);
                if (_enableDebugLog)
                {
                    Debug.Log($"[PlayerDeathController] Layer 检测：{obj.name} layer={obj.layer}, isObstacle={isObstacle}");
                }
                return isObstacle;
            }

            // 方法 2: 通过 Tag 检测（后备方案）
            bool isTagObstacle = obj.CompareTag("Obstacle");
            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] Tag 检测：{obj.name} tag={obj.tag}, isTagObstacle={isTagObstacle}");
            }
            return isTagObstacle;
        }

        /// <summary>
        /// 游戏状态变化回调
        /// </summary>
        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            // Waiting 状态下禁用碰撞检测，其他状态启用
            _isCollisionDetectionEnabled = (newState != GameState.Waiting);
            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] 碰撞检测已{(_isCollisionDetectionEnabled ? "启用" : "禁用")}");
            }

            // Waiting 状态下重置死亡标志，允许重新开始游戏
            if (newState == GameState.Waiting)
            {
                _isDead = false;
                if (_enableDebugLog)
                {
                    Debug.Log("[PlayerDeathController] 死亡标志已重置");
                }
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取死亡重生延迟（只读）
        /// </summary>
        public float RespawnDelay => _respawnDelay;

        /// <summary>
        /// 处理与障碍物的碰撞
        /// </summary>
        private void HandleObstacleCollision()
        {
            if (_isDead)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[PlayerDeathController] 已经死亡，忽略碰撞");
                }
                return;
            }

            if (_enableDebugLog)
            {
                Debug.Log("[PlayerDeathController] 检测到障碍物碰撞，触发死亡...");
            }

            Die();
        }

        /// <summary>
        /// 触发玩家死亡
        /// </summary>
        public void Die()
        {
            if (_isDead) return;

            _isDead = true;

            // 播放碰撞音效（Story 7-3）- 音量设为 40%
            if (_collisionSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_collisionSFX, 0.4f);
                if (_enableDebugLog)
                {
                    Debug.Log($"[PlayerDeathController] 播放碰撞音效：{_collisionSFX.name}");
                }
            }
            else if (_collisionSFX == null)
            {
                Debug.LogWarning("[PlayerDeathController] 碰撞音效未配置，请在 Inspector 中分配 Collision SFX 字段");
            }

            Debug.Log($"[PlayerDeathController] 玩家死亡，将在 {_respawnDelay} 秒后重生...");

            // 触发死亡事件
            if (OnPlayerDied != null)
            {
                Debug.Log($"[PlayerDeathController] 触发 OnPlayerDied 事件，订阅者数量：{OnPlayerDied.GetInvocationList().Length}");
                OnPlayerDied?.Invoke();
            }
            else
            {
                Debug.LogWarning("[PlayerDeathController] OnPlayerDied 事件没有订阅者！");
            }
        }

        /// <summary>
        /// 重生到安全位置（由 GameManager 调用）
        /// </summary>
        public void Respawn()
        {
            _isDead = false;

            // 从 GameManager 获取检查点位置（优先）或起始位置
            Vector3 spawnPosition = GetSpawnPosition();
            transform.position = spawnPosition;

            // 重置跳跃状态
            if (_jumpController != null)
            {
                _jumpController.ResetJumpState();
            }

            // 重置速度
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            Debug.Log($"[PlayerDeathController] 玩家重生到位置：{spawnPosition}");
        }

        /// <summary>
        /// 重置起始位置（游戏开始时调用）
        /// </summary>
        public void ResetStartPosition()
        {
            _startPosition = transform.position;
            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] 起始位置已重置：{_startPosition}");
            }
        }

        /// <summary>
        /// 获取起始位置（用于 GameManager）
        /// </summary>
        public Vector3 GetStartPosition()
        {
            return _startPosition;
        }

        /// <summary>
        /// 更新安全位置（用于重生）- 直接修改初始位置
        /// </summary>
        /// <param name="position">安全位置</param>
        public void SetSafePosition(Vector3 position)
        {
            _startPosition = position;
        }

        /// <summary>
        /// 获取重生位置
        /// </summary>
        /// <returns>重生位置（优先使用 GameManager 的检查点位置）</returns>
        private Vector3 GetSpawnPosition()
        {
            // 优先从 GameManager 获取检查点位置
            if (GameManager.Instance != null)
            {
                Vector3 checkpointPosition = GameManager.Instance.GetLastSafePosition();
                Debug.Log($"[PlayerDeathController] 从 GameManager 获取检查点位置：{checkpointPosition}");
                return checkpointPosition;
            }

            // 回退到游戏开始时的初始位置
            Debug.Log($"[PlayerDeathController] GameManager 为空，使用起始位置：{_startPosition}");
            return _startPosition;
        }

        /// <summary>
        /// 获取玩家是否死亡状态
        /// </summary>
        /// <returns>True 如果玩家已死亡</returns>
        public bool IsDead()
        {
            return _isDead;
        }

        /// <summary>
        /// 立即重生（用于测试）
        /// </summary>
        public void RespawnImmediately()
        {
            Respawn();
        }
        #endregion
    }
}
