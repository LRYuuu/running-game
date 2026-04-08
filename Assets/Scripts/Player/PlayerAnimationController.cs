using UnityEngine;

namespace RunnersJourney.Player
{
    /// <summary>
    /// 玩家动画控制器 - 管理玩家角色的所有动画状态
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        #region 序列化字段
        [Header("动画组件")]
        [Tooltip("Animator 组件")]
        [SerializeField] private Animator _animator;

        [Header("动画参数 Hash（性能优化）")]
        [SerializeField] private string _speedParamHash = "Speed";
        [SerializeField] private string _isGroundedParamHash = "IsGrounded";
        [SerializeField] private string _landTriggerHash = "Land";
        [SerializeField] private string _deathTriggerHash = "Death";
        #endregion

        #region 私有字段
        private int _speedHash;
        private int _isGroundedHash;
        private int _landHash;
        private int _deathHash;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            // 预计算参数 Hash（性能优化）
            _speedHash = Animator.StringToHash(_speedParamHash);
            _isGroundedHash = Animator.StringToHash(_isGroundedParamHash);
            _landHash = Animator.StringToHash(_landTriggerHash);
            _deathHash = Animator.StringToHash(_deathTriggerHash);
        }

        private void Start()
        {
            // 初始化地面状态为 true（玩家一开始在地面上）
            SetGrounded(true);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 更新移动速度（用于 Idle/Run 切换）
        /// </summary>
        /// <param name="speed">水平速度值</param>
        public void SetSpeed(float speed)
        {
            if (_animator != null)
            {
                _animator.SetFloat(_speedHash, Mathf.Abs(speed));
            }
        }

        /// <summary>
        /// 设置是否在地面（控制 Idle/Jump/Fall 状态转换）
        /// </summary>
        /// <param name="isGrounded">是否在地面</param>
        public void SetGrounded(bool isGrounded)
        {
            if (_animator != null)
            {
                _animator.SetBool(_isGroundedHash, isGrounded);
                // 性能优化：移除频繁的 Debug.Log
            }
            else
            {
                Debug.LogWarning("[PlayerAnimationController] SetGrounded - _animator is null!");
            }
        }

        /// <summary>
        /// 触发落地动画
        /// </summary>
        public void TriggerLand()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(_landHash);
            }
        }

        /// <summary>
        /// 触发死亡动画
        /// </summary>
        public void TriggerDeath()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(_deathHash);
            }
        }
        #endregion
    }
}
