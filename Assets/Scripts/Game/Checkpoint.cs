using UnityEngine;

namespace RunnersJourney.Game
{
    /// <summary>
    /// 检查点组件 - 检测玩家通过并保存重生位置
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        #region 序列化字段
        [Header("检查点配置")]
        [Tooltip("检查点触发器大小")]
        [SerializeField] private Vector2 _triggerSize = new Vector2(2f, 3f);

        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        [SerializeField] private bool _enableDebugLog = false;

        [Header("视觉效果")]
        [Tooltip("是否显示检查点图标")]
        [SerializeField] private bool _showVisual = true;

        [Tooltip("检查点激活后的颜色")]
        [SerializeField] private Color _activatedColor = Color.green;

        [Tooltip("检查点未激活的颜色")]
        [SerializeField] private Color _inactiveColor = Color.yellow;
        #endregion

        #region 私有字段
        private bool _isActivated = false;
        private SpriteRenderer _spriteRenderer;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Activate();
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 激活检查点
        /// </summary>
        public void Activate()
        {
            if (_isActivated) return;

            _isActivated = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateCheckpoint(transform.position);
            }

            if (_enableDebugLog)
            {
                Debug.Log($"[Checkpoint] 检查点已激活：{transform.position}");
            }
        }

        /// <summary>
        /// 检查检查点是否已激活
        /// </summary>
        /// <returns>True 如果已激活</returns>
        public bool IsActivated() => _isActivated;

        /// <summary>
        /// 重置检查点状态（用于测试）
        /// </summary>
        public void Reset()
        {
            _isActivated = false;
        }
        #endregion

        #region 编辑器
        /// <summary>
        /// 编辑器中绘制 Gizmos 可视化触发器范围
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = _isActivated ? _activatedColor : _inactiveColor;
            Gizmos.DrawWireCube(transform.position, _triggerSize);
        }

        /// <summary>
        /// 编辑器中绘制 Gizmos 填充
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(_isActivated ? _activatedColor.r : _inactiveColor.r,
                                      _isActivated ? _activatedColor.g : _inactiveColor.g,
                                      _isActivated ? _activatedColor.b : _inactiveColor.b,
                                      0.3f);
            Gizmos.DrawCube(transform.position, _triggerSize);
        }
        #endregion
    }
}
