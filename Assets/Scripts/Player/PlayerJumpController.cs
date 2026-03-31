using UnityEngine;
using RunnersJourney.Game;
using RunnersJourney.Audio;

namespace RunnersJourney.Player
{
    /// <summary>
    /// 玩家跳跃控制器 - 处理一段跳和地面检测集成
    /// 使用 New Input System 进行输入处理
    /// </summary>
    public class PlayerJumpController : MonoBehaviour
    {
        #region 序列化字段
        [Header("依赖引用")]
        [Tooltip("跳跃配置 ScriptableObject")]
        [SerializeField] private JumpConfig _jumpConfig;

        [Tooltip("地面检测器组件")]
        [SerializeField] private GroundDetector _groundDetector;

        [Header("音频")]
        [Tooltip("跳跃音效剪辑")]
        [SerializeField] private AudioClip _jumpSFX;

        [Header("动画")]
        [Tooltip("动画控制器组件")]
        [SerializeField] private PlayerAnimationController _animationController;

        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        [SerializeField] private bool _enableDebugLog = false;
        #endregion

        #region 私有字段
        private Rigidbody2D _rigidbody2D;
        private float _jumpBufferTimer;
        private float _coyoteTimeTimer;
        private int _jumpCount;
        private bool _isGroundedLastFrame;
        private bool _isInputEnabled = true;
        private bool _wasAirborneLastFrame = false;
        private bool _isJumping; // 标记跳跃状态
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            // 获取组件引用 - 延迟一帧获取以确保组件已注册
            // 在 EditMode 测试中，AddComponent 后立即调用 Awake 可能导致 GetComponent 返回 null
            _rigidbody2D = GetComponent<Rigidbody2D>();

            // 如果 Awake 时获取失败，尝试立即再次获取
            if (_rigidbody2D == null)
            {
                if (_enableDebugLog)
                {
                    Debug.LogWarning($"[PlayerJumpController] Awake: GetComponent<Rigidbody2D>() returned null, trying again...");
                }
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }

            // Debug: 检查组件获取
            if (_rigidbody2D == null)
            {
                Debug.LogError($"[PlayerJumpController] Awake: Failed to get Rigidbody2D! gameObject={gameObject.name}");
            }
            else if (_enableDebugLog)
            {
                Debug.Log($"[PlayerJumpController] Awake: Successfully got Rigidbody2D for {gameObject.name}");
            }

            // 冻结旋转，防止玩家在空中旋转
            if (_rigidbody2D != null)
            {
                _rigidbody2D.freezeRotation = true;
            }

            // 如果未显式分配，尝试获取子组件
            if (_groundDetector == null)
            {
                _groundDetector = GetComponent<GroundDetector>();
            }

            // 加载配置（如果未分配）
            if (_jumpConfig == null)
            {
                _jumpConfig = Resources.Load<JumpConfig>("Player/JumpConfig");
            }

            // 初始化状态
            _jumpBufferTimer = 0f;
            _coyoteTimeTimer = 0f;
            _jumpCount = 0;
            _isGroundedLastFrame = false;
        }

        private void OnEnable()
        {
            // 订阅游戏状态变化事件
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                // 初始化输入状态
                _isInputEnabled = GameManager.Instance.CurrentState != GameState.Waiting;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void Update()
        {
            // Waiting 状态下禁用输入处理
            if (!_isInputEnabled)
                return;

            // 更新地面状态
            bool wasGrounded = _isGroundedLastFrame;
            _isGroundedLastFrame = IsGrounded();

            // 更新计时器
            if (_jumpBufferTimer > 0f)
            {
                _jumpBufferTimer -= Time.deltaTime;
            }

            if (_coyoteTimeTimer > 0f)
            {
                _coyoteTimeTimer -= Time.deltaTime;
            }

            // 重置跳跃计数（当玩家刚落地时）
            if (_isGroundedLastFrame && !wasGrounded)
            {
                _jumpCount = 0;
            }

            // 启动土狼时间（当玩家刚离开地面时）
            if (!_isGroundedLastFrame && wasGrounded)
            {
                _coyoteTimeTimer = _jumpConfig.coyoteTime;
            }

            // 检测落地瞬间
            if (_isGroundedLastFrame && _wasAirborneLastFrame)
            {
                _isJumping = false;
                if (_animationController != null)
                {
                    _animationController.TriggerLand();
                }
            }
            _wasAirborneLastFrame = !_isGroundedLastFrame;

            // 更新动画速度参数（用于 Idle/Run 切换）
            if (_animationController != null)
            {
                float speed = Mathf.Abs(_rigidbody2D != null ? _rigidbody2D.velocity.x : 0f);
                _animationController.SetSpeed(speed);

                // 跳跃状态下强制设置为 false，直到落地
                bool groundedValue = _isJumping ? false : _isGroundedLastFrame;
                _animationController.SetGrounded(groundedValue);
                Debug.Log($"[PlayerJumpController] Update - _isGroundedLastFrame={_isGroundedLastFrame}, _isJumping={_isJumping}, setting IsGrounded={groundedValue}");
            }

            // 处理跳跃输入（Legacy Input - 待迁移到 New Input System）
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.K))
            {
                _jumpBufferTimer = _jumpConfig.jumpBufferTime;
                TryJump();
            }
        }

        private void FixedUpdate()
        {
            // 限制最大下落速度
            if (_rigidbody2D.velocity.y < _jumpConfig.maxFallSpeed)
            {
                _rigidbody2D.velocity = new Vector2(
                    _rigidbody2D.velocity.x,
                    _jumpConfig.maxFallSpeed
                );
            }
        }

        /// <summary>
        /// 游戏状态变化回调
        /// </summary>
        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            // Waiting 状态下禁用输入，其他状态启用
            _isInputEnabled = (newState != GameState.Waiting);
            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerJumpController] 输入控制已{(_isInputEnabled ? "启用" : "禁用")}");
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 测试专用：手动设置 Rigidbody2D 引用（用于 EditMode 测试）
        /// </summary>
        public void SetRigidbodyForTest(Rigidbody2D rb)
        {
            _rigidbody2D = rb;
            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerJumpController] SetRigidbodyForTest called, rb==null: {rb == null}");
            }
        }

        /// <summary>
        /// 尝试执行跳跃
        /// </summary>
        public void TryJump()
        {
            if (_jumpBufferTimer <= 0f)
            {
                return;
            }

            bool grounded = IsGrounded();
            Debug.Log($"[PlayerJumpController] TryJump: grounded={grounded}, _jumpCount={_jumpCount}, coyoteTime={_coyoteTimeTimer}");

            // 一段跳条件：在地面上
            if (grounded)
            {
                ExecuteJump(_jumpConfig.jumpForce);
                _jumpBufferTimer = 0f;
                _jumpCount = 1;
                Debug.Log($"[PlayerJumpController] 执行一段跳，_jumpCount={_jumpCount}");
                return;
            }

            // 土狼时间跳跃：在空中但土狼时间计时器仍有效
            if (_coyoteTimeTimer > 0f && !grounded)
            {
                ExecuteJump(_jumpConfig.jumpForce);
                _jumpBufferTimer = 0f;
                _coyoteTimeTimer = 0f;
                _jumpCount = 1;
                Debug.Log($"[PlayerJumpController] 执行土狼跳，_jumpCount={_jumpCount}");
                return;
            }

            // 二段跳条件：在空中且跳跃次数等于 1
            if (_jumpCount == 1 && !grounded)
            {
                ExecuteJump(_jumpConfig.doubleJumpForce);
                _jumpBufferTimer = 0f;
                _jumpCount = 2;
                Debug.Log($"[PlayerJumpController] 执行二段跳，_jumpCount={_jumpCount}");
                return;
            }

            Debug.Log($"[PlayerJumpController] 跳跃条件不满足：grounded={grounded}, _jumpCount={_jumpCount}, coyoteTime={_coyoteTimeTimer}");
        }

        /// <summary>
        /// 执行跳跃（使用 JumpConfig 的力度）
        /// </summary>
        /// <param name="force">跳跃力度</param>
        public void ExecuteJump(float force)
        {
            // Debug: 检查依赖
            if (_rigidbody2D == null)
            {
                Debug.LogError($"[PlayerJumpController] ExecuteJump: _rigidbody2D is null! gameObject={gameObject.name}");
                return;
            }

            if (_jumpConfig == null)
            {
                Debug.LogError($"[PlayerJumpController] ExecuteJump: _jumpConfig is null! gameObject={gameObject.name}");
                // 使用默认值继续
            }

            // 清除垂直速度，确保跳跃高度一致
            var velocity = _rigidbody2D.velocity;
            velocity.y = 0f;
            _rigidbody2D.velocity = velocity;

            // 应用向上力
            _rigidbody2D.AddForce(Vector2.up * force, ForceMode2D.Impulse);

            // 播放跳跃音效
            if (_jumpSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_jumpSFX);
                if (_enableDebugLog)
                {
                    Debug.Log($"[PlayerJumpController] 播放跳跃音效：{_jumpSFX.name}");
                }
            }
            else if (_jumpSFX == null)
            {
                Debug.LogWarning("[PlayerJumpController] 跳跃音效未配置，请在 Inspector 中分配 Jump SFX 字段");
            }

            // 触发跳跃动画（通过 SetGrounded(false) 触发状态机转换到 Jump 状态）
            if (_animationController != null)
            {
                _isJumping = true;
                _animationController.SetGrounded(false);
                Debug.Log($"[PlayerJumpController] Jump executed - _isJumping={_isJumping}, IsGrounded=false");
            }

            // 日志输出
            if (_enableDebugLog)
            {
                Debug.Log($"[PlayerJumpController] Jump executed, force: {force}");
            }
        }

        /// <summary>
        /// 检测玩家是否在地面上
        /// </summary>
        /// <returns>True 如果玩家在地面上，否则 False</returns>
        public bool IsGrounded()
        {
            if (_groundDetector != null)
            {
                return _groundDetector.IsGrounded();
            }

            // 备选方案：直接使用 Physics2D.OverlapBox
            var groundCheckOffset = new Vector2(0f, -0.5f);
            var groundCheckSize = new Vector2(0.8f, 0.1f);
            return Physics2D.OverlapBox(
                (Vector2)transform.position + groundCheckOffset,
                groundCheckSize,
                0f,
                _jumpConfig != null ? _jumpConfig.groundLayer : LayerMask.GetMask("Ground")
            ) != null;
        }

        /// <summary>
        /// 重置跳跃状态（用于死亡/重生）
        /// </summary>
        public void ResetJumpState()
        {
            _jumpBufferTimer = 0f;
            _coyoteTimeTimer = 0f;
            _jumpCount = 0;
            _isGroundedLastFrame = false;
        }

        /// <summary>
        /// 获取跳跃缓冲剩余时间
        /// </summary>
        /// <returns>跳跃缓冲时间（秒）</returns>
        public float GetJumpBufferTime()
        {
            return _jumpBufferTimer;
        }

        /// <summary>
        /// 获取土洋时间剩余时间
        /// </summary>
        /// <returns>土洋时间（秒）</returns>
        public float GetCoyoteTime()
        {
            return _coyoteTimeTimer;
        }

        /// <summary>
        /// 检查二段跳是否可用
        /// </summary>
        /// <returns>True 如果二段跳可用，否则 False</returns>
        public bool CanDoubleJump()
        {
            return _jumpCount < 2 && !IsGrounded();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// Debug 可视化地面检测范围
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_groundDetector != null)
            {
                // GroundDetector 会负责绘制
                return;
            }

            // 备选方案的 Gizmos
            var groundCheckOffset = new Vector2(0f, -0.5f);
            var groundCheckSize = new Vector2(0.8f, 0.1f);
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube((Vector2)transform.position + groundCheckOffset, groundCheckSize);
        }
        #endregion
    }
}
