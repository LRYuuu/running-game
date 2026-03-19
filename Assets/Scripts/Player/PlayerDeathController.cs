using UnityEngine;
using System;

namespace SquareFireline.Player
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
        #endregion

        #region 事件
        /// <summary>
        /// 玩家死亡事件
        /// </summary>
        public event Action OnPlayerDied;
        #endregion

        #region 私有字段
        private bool _isDead = false;
        private float _respawnTimer = 0f;
        private Vector3 _lastSafePosition;
        private PlayerJumpController _jumpController;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            _jumpController = GetComponent<PlayerJumpController>();
            _lastSafePosition = transform.position;
        }

        private void Update()
        {
            if (_isDead)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
        }

        /// <summary>
        /// 检测与障碍物的碰撞
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
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
                return ((_obstacleLayer.value & (1 << obj.layer)) != 0);
            }
            // 方法 2: 通过 Tag 检测（后备方案）
            return obj.CompareTag("Obstacle");
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
            if (_isDead) return;

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
            _respawnTimer = _respawnDelay;

            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] 玩家死亡，将在 {_respawnDelay} 秒后重生...");
            }

            // 触发死亡事件
            OnPlayerDied?.Invoke();
        }

        /// <summary>
        /// 重生到安全位置
        /// </summary>
        private void Respawn()
        {
            _isDead = false;
            transform.position = _lastSafePosition;

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

            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerDeathController] 玩家重生到位置：{_lastSafePosition}");
            }
        }

        /// <summary>
        /// 更新安全位置（用于重生）
        /// </summary>
        /// <param name="position">安全位置</param>
        public void SetSafePosition(Vector3 position)
        {
            _lastSafePosition = position;
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
