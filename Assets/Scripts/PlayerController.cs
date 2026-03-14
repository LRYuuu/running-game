using UnityEngine;

namespace SquareFireline.Player
{
    /// <summary>
    /// 玩家控制器
    /// 玩家位置固定（X 轴），地图向左滚动模拟奔跑效果
    /// 支持跳跃、二段跳、跳跃缓冲和土狼时间
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region 序列化字段

        [Header("跳跃设置")]
        [Tooltip("一段跳的初始速度，决定跳跃高度")]
        [SerializeField] private float jumpHeight = 16f;

        [Tooltip("二段跳的初始速度")]
        [SerializeField] private float doubleJumpHeight = 14f;

        [Header("地面检测")]
        [Tooltip("地面检测偏移")]
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0, -0.5f);

        [Tooltip("地面检测大小")]
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);

        [Header("辅助机制")]
        [Tooltip("跳跃缓冲时间（秒）- 落地前输入跳跃，落地后自动起跳的缓冲时间")]
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Tooltip("土狼时间（秒）- 离开平台边缘后仍可跳跃的时长")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("最大下落速度 - 限制最大下落速度，防止过快")]
        [SerializeField] private float maxFallSpeed = -15f;

        #endregion

        #region 私有字段

        private Rigidbody2D _rb;
        private bool _isGrounded;
        private bool _hasJumped; // 是否已经跳过（用于二段跳）
        private float _jumpBufferTimer;
        private float _coyoteTimeTimer;
        private float _lastGroundedTime;

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
            }

            // 锁定 X 轴位置
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX;

            // 设置重力系数
            _rb.gravityScale = 2.5f;
        }

        private void Update()
        {
            // 地面检测
            _isGrounded = Physics2D.OverlapBox(
                (Vector2)transform.position + groundCheckOffset,
                groundCheckSize,
                0f,
                LayerMask.GetMask("Ground")
            ) != null;

            // 更新地面计时器
            if (_isGrounded)
            {
                _lastGroundedTime = Time.time;
                _hasJumped = false; // 落地后重置跳跃状态
            }

            // 土狼时间计时
            _coyoteTimeTimer = Time.time - _lastGroundedTime;

            // 跳跃缓冲计时
            if (_jumpBufferTimer > 0)
            {
                _jumpBufferTimer -= Time.deltaTime;
            }

            // 检测跳跃输入
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.K))
            {
                _jumpBufferTimer = jumpBufferTime;
                Debug.Log($"[Player] 检测到跳跃输入！缓冲时间设置为：{jumpBufferTime}");
            }

            // 执行跳跃
            if (_jumpBufferTimer > 0 && CanJump())
            {
                PerformJump();
                _jumpBufferTimer = 0;
            }
        }

        private void FixedUpdate()
        {
            // 限制最大下落速度
            if (_rb.velocity.y < maxFallSpeed)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, maxFallSpeed);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查是否可以跳跃
        /// </summary>
        private bool CanJump()
        {
            // 在地面或土狼时间内：可以一段跳
            bool canGroundJump = _isGrounded || _coyoteTimeTimer <= coyoteTime;
            if (canGroundJump)
            {
                return true;
            }

            // 在空中：可以二段跳（只要还没跳过）
            return !_hasJumped;
        }

        /// <summary>
        /// 执行跳跃
        /// </summary>
        private void PerformJump()
        {
            if (_isGrounded || _coyoteTimeTimer <= coyoteTime)
            {
                // 一段跳（或土狼跳）
                _rb.velocity = new Vector2(_rb.velocity.x, jumpHeight);
                _hasJumped = true;
                Debug.Log($"[Player] 一段跳！力度：{jumpHeight}");
            }
            else
            {
                // 二段跳
                _rb.velocity = new Vector2(_rb.velocity.x, doubleJumpHeight);
                _hasJumped = true; // 修复：二段跳后也要设置为 true
                Debug.Log($"[Player] 二段跳！力度：{doubleJumpHeight}");
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 检查是否在地面
        /// </summary>
        public bool IsGrounded() => _isGrounded;

        /// <summary>
        /// 检查是否可以二段跳
        /// </summary>
        public bool CanDoubleJump() => !_hasJumped && !_isGrounded;

        #endregion

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 绘制地面检测区域
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                (Vector2)transform.position + groundCheckOffset,
                groundCheckSize
            );
        }
        #endif
    }
}
