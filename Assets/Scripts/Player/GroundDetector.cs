using UnityEngine;

namespace SquareFireline.Player
{
    /// <summary>
    /// 地面检测器 - 使用 Physics2D.OverlapBox 检测玩家是否在地面
    /// </summary>
    public class GroundDetector : MonoBehaviour
    {
        #region 序列化字段
        [Header("地面检测")]
        [Tooltip("地面检测偏移（从玩家位置向下）")]
        [SerializeField] private Vector2 _groundCheckOffset = new Vector2(0f, -0.5f);

        [Header("地面检测")]
        [Tooltip("地面检测盒子尺寸（宽度要小于玩家宽度，避免碰到侧面）")]
        [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.3f, 0.05f);
        #endregion

        #region 私有字段
        private JumpConfig _jumpConfig;
        private bool _lastFrameGrounded = false;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            _jumpConfig = Resources.Load<JumpConfig>("Player/JumpConfig");
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 检测玩家是否在地面上
        /// </summary>
        /// <returns>True 如果玩家在地面上，否则 False</returns>
        public bool IsGrounded()
        {
            var checkPos = (Vector2)transform.position + _groundCheckOffset;

            // 获取地面层，但排除玩家自身的 Layer
            LayerMask groundLayer = _jumpConfig != null ? _jumpConfig.groundLayer : LayerMask.GetMask("Ground");

            var hit = Physics2D.OverlapBox(
                checkPos,
                _groundCheckSize,
                0f,
                groundLayer
            );

            // 排除玩家自身的 Collider
            if (hit != null && hit.transform == transform)
            {
                return false;
            }

            return hit != null;
        }

        /// <summary>
        /// 使用 Raycast 方式进行地面检测（备选方案）
        /// </summary>
        /// <returns>True 如果玩家在地面上，否则 False</returns>
        public bool IsGroundedWithRaycast()
        {
            if (_jumpConfig == null)
                return false;

            var hit = Physics2D.Raycast(
                origin: transform.position,
                direction: Vector2.down,
                distance: _jumpConfig.groundCheckDistance,
                layerMask: _jumpConfig.groundLayer
            );

            DebugGroundCheck(hit.collider != null);

            return hit.collider != null;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// Debug 可视化地面检测
        /// </summary>
        /// <param name="isGrounded">是否在地面上</param>
        private void OnDrawGizmos()
        {
            // 绘制检测盒子
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            var gizmoPos = (Vector2)transform.position + _groundCheckOffset;
            Gizmos.DrawCube(gizmoPos, _groundCheckSize);

            // 如果在地面上，绘制绿色
            if (IsGrounded())
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawCube(gizmoPos, _groundCheckSize);
            }
        }

        /// <summary>
        /// Debug 地面检测结果
        /// </summary>
        /// <param name="isGrounded">是否在地面上</param>
        private void DebugGroundCheck(bool isGrounded)
        {
            // 可在需要时启用详细日志
            // Debug.Log($"[GroundDetector] IsGrounded: {isGrounded}");
        }
        #endregion
    }
}
