using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 用于 Tilemap 背景的滚动组件
/// 挂载在 Grid 对象上
/// </summary>
public class ParallaxBackgroundTilemap : MonoBehaviour
{
    [Header("滚动设置")]
    [Tooltip("背景滚动速度（负值向左滚动）")]
    [SerializeField] private float scrollSpeed = -2f;

    [Tooltip("视差系数（0=不跟随，1=完全跟随相机）")]
    [SerializeField] [Range(0f, 1f)] private float parallaxFactor = 0.3f;

    [Header("循环设置")]
    [Tooltip("背景图案重复宽度（用于循环计算）")]
    [SerializeField] private float patternWidth = 20f;

    [Tooltip("是否循环滚动")]
    [SerializeField] private bool loop = true;

    [Tooltip("是否在编辑器中预览")]
    [SerializeField] private bool previewInEditor = false;

    private Camera _mainCamera;
    private Vector3 _lastCameraPosition;
    private Tilemap _tilemap;

    // 滚动暂停状态
    private bool _isScrollingPaused = false;

    private void Start()
    {
        _mainCamera = Camera.main;
        _lastCameraPosition = transform.position;
        _tilemap = GetComponentInChildren<Tilemap>();

        // 如果没有设置图案宽度，尝试从 Tilemap 计算
        if (patternWidth <= 0 && _tilemap != null)
        {
            Bounds bounds = _tilemap.localBounds;
            patternWidth = bounds.size.x;
        }
    }

    private void Update()
    {
        // 编辑器预览
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

        // 视差滚动
        Vector3 deltaMovement = _mainCamera.transform.position - _lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, 0, 0);

        // 基础滚动
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);

        _lastCameraPosition = _mainCamera.transform.position;

        // 循环逻辑
        if (loop && patternWidth > 0)
        {
            float cameraX = _mainCamera.transform.position.x;
            float bgX = transform.position.x;
            float viewportWidth = _mainCamera.orthographicSize * _mainCamera.aspect * 2;

            float leftBound = cameraX - viewportWidth / 2 - patternWidth;
            float rightBound = cameraX + viewportWidth / 2;

            if (bgX < leftBound)
            {
                transform.position = new Vector3(bgX + patternWidth, transform.position.y, transform.position.z);
            }
            else if (bgX > rightBound)
            {
                transform.position = new Vector3(bgX - patternWidth, transform.position.y, transform.position.z);
            }
        }
    }

    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

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
