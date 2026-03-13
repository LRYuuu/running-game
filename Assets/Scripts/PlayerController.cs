using UnityEngine;

/// <summary>
/// 玩家控制器
/// 玩家位置固定（X 轴），地图向左滚动模拟奔跑效果
/// 玩家只能跳跃
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("跳跃设置")]
    [Tooltip("跳跃力度")]
    public float jumpForce = 8f;

    [Header("地面检测")]
    [Tooltip("地面检测偏移")]
    public Vector2 groundCheckOffset = new Vector2(0, -0.5f);

    [Tooltip("地面检测大小")]
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);

    private Rigidbody2D _rb;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // 锁定 X 轴位置
        _rb.constraints = RigidbodyConstraints2D.FreezePositionX;
    }

    private void Update()
    {
        // 地面检测 - 使用 OverlapBox 检测 Ground Tilemap
        _isGrounded = Physics2D.OverlapBox(
            (Vector2)transform.position + groundCheckOffset,
            groundCheckSize,
            0f
        ) != null;

        // 跳跃
        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _rb.velocity = new Vector2(0, jumpForce);
        }
    }
}
