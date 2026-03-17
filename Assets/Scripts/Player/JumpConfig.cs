using UnityEngine;

namespace SquareFireline.Player
{
    /// <summary>
    /// 跳跃配置 - ScriptableObject 用于快速调优手感
    /// </summary>
    [CreateAssetMenu(fileName = "JumpConfig", menuName = "Square Fireline/Player/Jump Config")]
    public class JumpConfig : ScriptableObject
    {
        #region 跳跃参数
        [Header("跳跃力度")]
        [Tooltip("跳跃力度（Rigidbody2D.AddForce）")]
        public float jumpForce = 10f;

        [Header("二段跳力度")]
        [Tooltip("二段跳力度（通常略低于一段跳）")]
        public float doubleJumpForce = 8f;
        #endregion

        #region 地面检测
        [Header("地面检测")]
        [Tooltip("地面检测距离（从玩家底部向下）")]
        public float groundCheckDistance = 0.1f;

        [Tooltip("地面层掩码")]
        public LayerMask groundLayer = default;
        #endregion

        #region 高级跳跃机制
        [Header("高级跳跃机制")]
        [Tooltip("跳跃缓冲时间（秒）- 落地前按下跳跃可在缓冲期内生效）")]
        public float jumpBufferTime = 0.2f;

        [Tooltip("土狼时间（秒）- 走出平台边缘后仍可跳跃的时间窗口）")]
        public float coyoteTime = 0.15f;

        [Tooltip("最大下落速度（负值）")]
        public float maxFallSpeed = -15f;
        #endregion
    }
}
