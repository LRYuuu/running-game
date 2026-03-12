using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 可循环的背景系统
/// 预先生成多段背景，运行时循环拼接实现无限滚动效果
/// </summary>
public class InfiniteBackgroundTiler : MonoBehaviour
{
    [Header("背景配置")]
    [Tooltip("用于生成背景的 Tilemap（模板）")]
    [SerializeField] private Tilemap sourceTilemap;

    [Tooltip("单个背景段的宽度（瓦片数量）")]
    [SerializeField] private int segmentWidthInTiles = 20;

    [Tooltip("预先生成的背景段数量（建议 3 段）")]
    [SerializeField] private int segmentCount = 3;

    [Tooltip("背景滚动速度")]
    [SerializeField] private float scrollSpeed = -2f;

    private Tilemap[] _backgroundSegments;
    private float _segmentWorldWidth;
    private Camera _mainCamera;
    private BoundsInt _sourceBounds;

    private void Start()
    {
        _mainCamera = Camera.main;

        // 计算单个段的实际世界宽度
        _segmentWorldWidth = segmentWidthInTiles * 1f;

        // 获取源 Tilemap 的边界
        if (sourceTilemap != null)
        {
            _sourceBounds = sourceTilemap.cellBounds;
        }

        // 初始化背景段
        InitializeBackgroundSegments();
    }

    /// <summary>
    /// 初始化背景段数组
    /// </summary>
    private void InitializeBackgroundSegments()
    {
        _backgroundSegments = new Tilemap[segmentCount];

        // 创建所有背景段
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segmentObj = new GameObject($"BackgroundSegment_{i}");
            segmentObj.transform.SetParent(transform);

            Tilemap segmentTilemap = segmentObj.AddComponent<Tilemap>();
            TilemapRenderer segmentRenderer = segmentObj.AddComponent<TilemapRenderer>();

            // 设置位置（并排排列，无缝衔接）
            // 关键：位置 = i * 段宽度，这样每个段紧挨着
            float xPos = i * _segmentWorldWidth;
            segmentObj.transform.position = new Vector3(xPos, 0, 0);

            // 复制 Tile 数据（原样复制，不做坐标偏移）
            if (sourceTilemap != null)
            {
                CopyTileDataSimple(sourceTilemap, segmentTilemap);
            }

            // 设置渲染层级
            if (segmentRenderer != null)
            {
                segmentRenderer.sortingLayerName = "Background";
                segmentRenderer.sortingOrder = 0;
            }

            _backgroundSegments[i] = segmentTilemap;
        }
    }

    /// <summary>
    /// 简单复制 Tile 数据（原样复制）
    /// </summary>
    private void CopyTileDataSimple(Tilemap source, Tilemap target)
    {
        BoundsInt bounds = source.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                TileBase tile = source.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    target.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    private void Update()
    {
        if (_mainCamera == null)
            return;

        // 整体向左滚动
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);

        // 检查是否需要循环
        CheckAndRecycleSegments();
    }

    /// <summary>
    /// 检查并回收移出视野的背景段
    /// </summary>
    private void CheckAndRecycleSegments()
    {
        float cameraX = _mainCamera.transform.position.x;
        // 视野左边界（再往左一个段宽度的距离作为触发点）
        float triggerX = cameraX - _mainCamera.orthographicSize * _mainCamera.aspect - _segmentWorldWidth;

        // 找到最左边的背景段
        float leftMostX = float.MaxValue;
        int leftMostIndex = -1;

        for (int i = 0; i < segmentCount; i++)
        {
            if (_backgroundSegments[i] == null)
                continue;

            float segmentX = _backgroundSegments[i].transform.position.x;
            if (segmentX < leftMostX)
            {
                leftMostX = segmentX;
                leftMostIndex = i;
            }
        }

        if (leftMostIndex >= 0 && leftMostX < triggerX)
        {
            // 找到最右边的背景段
            float rightMostX = float.MinValue;

            for (int i = 0; i < segmentCount; i++)
            {
                if (_backgroundSegments[i] == null)
                    continue;

                float segmentX = _backgroundSegments[i].transform.position.x;
                if (segmentX > rightMostX)
                {
                    rightMostX = segmentX;
                }
            }

            // 将最左边的背景段移到最右边（紧挨着当前最右边的段）
            float newX = rightMostX + _segmentWorldWidth;
            _backgroundSegments[leftMostIndex].transform.position = new Vector3(newX, 0, 0);
        }
    }

    /// <summary>
    /// 设置滚动速度
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下手动生成背景预览
    /// </summary>
    [ContextMenu("Generate Background Segments")]
    private void GeneratePreview()
    {
        InitializeBackgroundSegments();
    }

    /// <summary>
    /// 清除所有生成的背景段
    /// </summary>
    [ContextMenu("Clear Background Segments")]
    private void ClearSegments()
    {
        if (_backgroundSegments != null)
        {
            foreach (var segment in _backgroundSegments)
            {
                if (segment != null)
                {
                    DestroyImmediate(segment.gameObject);
                }
            }
        }
    }
#endif
}
