using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 程序化背景生成器
/// 运行时动态生成背景 Tile，适合随机分布的背景装饰
/// </summary>
public class ProceduralBackgroundGenerator : MonoBehaviour
{
    [Header("生成配置")]
    [Tooltip("每次生成的 Tile 数量")]
    [SerializeField] private int tilesPerGeneration = 10;

    [Tooltip("生成触发距离（相机距离生成线多远时触发新生成）")]
    [SerializeField] private float generationTriggerDistance = 10f;

    [Tooltip("背景 Tile 数组（随机选择）")]
    [SerializeField] private TileBase[] backgroundTiles;

    [Header("生成区域")]
    [Tooltip("生成起始 X 位置")]
    [SerializeField] private float startX = 0f;

    [Tooltip("生成高度范围（Y 轴）")]
    [SerializeField] private Vector2Int heightRange = new Vector2Int(-5, 5);

    [Header("噪声设置（用于随机分布）")]
    [Tooltip("噪声缩放值")]
    [SerializeField] private float noiseScale = 0.1f;

    [Tooltip("噪声强度")]
    [SerializeField] private float noiseStrength = 2f;

    private Tilemap _tilemap;
    private float _nextGenerationX;
    private float _leftmostTileX;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;

        // 获取或创建 Tilemap
        _tilemap = GetComponent<Tilemap>();
        if (_tilemap == null)
        {
            _tilemap = gameObject.AddComponent<Tilemap>();
        }

        _nextGenerationX = startX;
        _leftmostTileX = startX;

        // 预先生成一片区域
        for (int i = 0; i < 5; i++)
        {
            GenerateSegment();
        }
    }

    private void Update()
    {
        if (_mainCamera == null)
            return;

        float cameraX = _mainCamera.transform.position.x;

        // 当相机接近生成线时，生成新的背景
        if (cameraX + generationTriggerDistance > _nextGenerationX)
        {
            GenerateSegment();
        }

        // 清除移出视野太远的背景（可选，节省内存）
        CleanupOldTiles(cameraX);
    }

    /// <summary>
    /// 生成一段背景
    /// </summary>
    private void GenerateSegment()
    {
        for (int i = 0; i < tilesPerGeneration; i++)
        {
            float x = _nextGenerationX + i;

            // 使用噪声计算 Y 位置（产生自然的起伏）
            float noiseValue = Mathf.PerlinNoise(x * noiseScale, 0);
            int y = Mathf.RoundToInt(Mathf.Lerp(heightRange.x, heightRange.y, noiseValue));

            // 随机选择 Tile
            if (backgroundTiles.Length > 0)
            {
                int tileIndex = Random.Range(0, backgroundTiles.Length);
                Vector3Int pos = new Vector3Int(Mathf.RoundToInt(x), y, 0);
                _tilemap.SetTile(pos, backgroundTiles[tileIndex]);
            }

            _leftmostTileX = Mathf.Min(_leftmostTileX, x);
        }

        _nextGenerationX += tilesPerGeneration;
    }

    /// <summary>
    /// 清除太老的 Tile（节省内存）
    /// </summary>
    private void CleanupOldTiles(float cameraX)
    {
        float cleanupThreshold = cameraX - 20f; // 相机后方 20 单位

        if (_leftmostTileX < cleanupThreshold)
        {
            // 清除左侧的 Tile
            for (int x = Mathf.RoundToInt(_leftmostTileX); x < cleanupThreshold; x++)
            {
                for (int y = heightRange.x; y <= heightRange.y; y++)
                {
                    _tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
            _leftmostTileX = cleanupThreshold;
        }
    }

    /// <summary>
    /// 添加背景 Tile 到数组
    /// </summary>
    public void AddBackgroundTile(TileBase tile)
    {
        if (backgroundTiles == null)
            backgroundTiles = new TileBase[0];

        System.Array.Resize(ref backgroundTiles, backgroundTiles.Length + 1);
        backgroundTiles[backgroundTiles.Length - 1] = tile;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下预览生成
    /// </summary>
    [ContextMenu("Generate Preview")]
    private void GeneratePreview()
    {
        if (_tilemap == null)
            _tilemap = gameObject.AddComponent<Tilemap>();

        for (int i = 0; i < 20; i++)
        {
            int y = Random.Range(heightRange.x, heightRange.y + 1);
            int tileIndex = Random.Range(0, backgroundTiles.Length);
            _tilemap.SetTile(new Vector3Int(i, y, 0), backgroundTiles[tileIndex]);
        }
    }

    /// <summary>
    /// 清除所有 Tile
    /// </summary>
    [ContextMenu("Clear All Tiles")]
    private void ClearAllTiles()
    {
        if (_tilemap != null)
        {
            _tilemap.ClearAllTiles();
        }
    }
#endif
}
