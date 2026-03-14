using UnityEngine;

namespace SquareFireline.Player
{
    /// <summary>
    /// 玩家动画控制器
    /// 管理玩家动画状态机的状态转换
    /// </summary>
    public class PlayerAnimation : MonoBehaviour
    {
        #region 私有字段

        private Animator _animator;
        private PlayerController _playerController;
        private Rigidbody2D _rb;

        // 动画参数哈希
        private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
        private static readonly int VelocityYHash = Animator.StringToHash("velocityY");
        private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
        private static readonly int TriggerLandHash = Animator.StringToHash("triggerLand");

        // 状态追踪
        private bool _wasInAir;
        private float _landTimer;

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerController = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody2D>();

            if (_animator == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimation)}] Animator 组件缺失！");
            }
        }

        private void Update()
        {
            if (_animator == null || _playerController == null) return;

            // 地面状态
            bool isGrounded = _playerController.IsGrounded();
            _animator.SetBool(IsGroundedHash, isGrounded);

            // 垂直速度
            float velocityY = _rb != null ? _rb.velocity.y : 0f;
            _animator.SetFloat(VelocityYHash, velocityY);

            // 跳跃状态
            bool isJumping = !isGrounded && velocityY > 0;
            _animator.SetBool(IsJumpingHash, isJumping);

            // 落地检测
            if (_wasInAir && isGrounded)
            {
                _animator.SetTrigger(TriggerLandHash);
                _landTimer = 0.2f; // 落地缓冲时间
            }
            _wasInAir = !isGrounded;

            // 落地缓冲计时
            if (_landTimer > 0)
            {
                _landTimer -= Time.deltaTime;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 检查是否正在播放落地动画
        /// </summary>
        public bool IsLanding() => _landTimer > 0;

        #endregion
    }
}
