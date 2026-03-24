using UnityEngine;

/// <summary>
/// 视差滚动背景组件
/// 用于实现 2D 游戏的背景滚动效果
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("滚动设置")]
    [Tooltip("背景滚动速度（负值向左滚动）")]
    [SerializeField] private float scrollSpeed = -2f;

    [Tooltip("是否循环滚动")]
    [SerializeField] private bool loop = true;

    [Tooltip("视差系数（0=不跟随，1=完全跟随相机）")]
    [SerializeField] [Range(0f, 1f)] private float parallaxFactor = 0.5f;

    [Header("循环设置")]
    [Tooltip("背景纹理的宽度（用于循环计算）")]
    [SerializeField] private float textureWidth = 20f;

    [Tooltip("是否在编辑器中预览滚动效果")]
    [SerializeField] private bool previewInEditor = false;

    private Camera _mainCamera;
    private Vector3 _lastCameraPosition;
    private float _textureUnitSizeX;

    // 滚动暂停状态
    private bool _isScrollingPaused = false;

    private void Start()
    {
        _mainCamera = Camera.main;
        _lastCameraPosition = transform.position;

        // 如果没有设置纹理宽度，尝试从 SpriteRenderer 获取
        if (textureWidth <= 0)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                textureWidth = sr.sprite.bounds.size.x;
            }
        }

        _textureUnitSizeX = textureWidth;
    }

    private void Update()
    {
        // 编辑器预览模式
        if (!Application.isPlaying && previewInEditor)
        {
            transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);
            return;
        }

        // 暂停时不更新滚动
        if (_isScrollingPaused)
            return;

        if (_mainCamera == null)
            return;

        // 计算相机移动带来的视差偏移
        Vector3 deltaMovement = _mainCamera.transform.position - _lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, 0, 0);

        // 应用基础滚动速度
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);

        _lastCameraPosition = _mainCamera.transform.position;

        // 循环滚动
        if (loop && _textureUnitSizeX > 0)
        {
            // 计算相机当前位置
            float cameraX = _mainCamera.transform.position.x;
            float bgX = transform.position.x;

            // 当背景移出视野时，重置到另一侧
            float distanceFromCamera = transform.position.z - _mainCamera.transform.position.z;
            float viewportWidth = _mainCamera.orthographicSize * _mainCamera.aspect * 2;

            // 左边界：背景完全离开左侧视野
            float leftBound = cameraX - viewportWidth / 2 - _textureUnitSizeX / 2;
            // 右边界：背景完全离开右侧视野（虽然正常情况下不会发生）
            float rightBound = cameraX + viewportWidth / 2 + _textureUnitSizeX / 2;

            if (bgX < leftBound)
            {
                transform.position = new Vector3(
                    bgX + _textureUnitSizeX,
                    transform.position.y,
                    transform.position.z
                );
            }
            else if (bgX > rightBound)
            {
                transform.position = new Vector3(
                    bgX - _textureUnitSizeX,
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }

    /// <summary>
    /// 手动设置背景纹理宽度（用于循环计算）
    /// </summary>
    public void SetTextureWidth(float width)
    {
        _textureUnitSizeX = width;
        textureWidth = width;
    }

    /// <summary>
    /// 设置滚动速度
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

    /// <summary>
    /// 设置视差系数
    /// </summary>
    public void SetParallaxFactor(float factor)
    {
        parallaxFactor = Mathf.Clamp01(factor);
    }

    /// <summary>
    /// 设置滚动暂停状态
    /// </summary>
    /// <param name="paused">true=暂停，false=恢复</param>
    public void SetScrollPaused(bool paused)
    {
        _isScrollingPaused = paused;
    }
}
