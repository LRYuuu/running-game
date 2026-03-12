using UnityEngine;

/// <summary>
/// 背景管理器 - 管理多层视差背景
/// 挂载在 Camera 或场景中的 Backgrounds 空对象上
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [Header("背景层级")]
    [Tooltip("远距离背景（移动最慢）")]
    [SerializeField] private ParallaxBackground farBackground;

    [Tooltip("中距离背景（移动速度中等）")]
    [SerializeField] private ParallaxBackground midBackground;

    [Tooltip("近距离背景（移动最快）")]
    [SerializeField] private ParallaxBackground nearBackground;

    [Header("速度设置")]
    [Tooltip("基础滚动速度")]
    [SerializeField] private float baseScrollSpeed = -2f;

    [Tooltip("远距离背景速度系数")]
    [SerializeField] [Range(0f, 1f)] private float farSpeedMultiplier = 0.2f;

    [Tooltip("中距离背景速度系数")]
    [SerializeField] [Range(0f, 1f)] private float midSpeedMultiplier = 0.5f;

    [Tooltip("近距离背景速度系数")]
    [SerializeField] [Range(0f, 1f)] private float nearSpeedMultiplier = 1f;

    [Header("游戏速度关联")]
    [Tooltip("是否根据游戏速度调整背景滚动速度")]
    [SerializeField] private bool syncWithGameSpeed = true;

    private static BackgroundManager _instance;
    public static BackgroundManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeBackgrounds();
    }

    /// <summary>
    /// 初始化背景层级
    /// </summary>
    private void InitializeBackgrounds()
    {
        // 如果没有自动赋值，尝试从子对象中查找
        if (farBackground == null)
            farBackground = GetComponentInChildren<ParallaxBackground>();

        // 设置各层速度
        UpdateBackgroundSpeeds();
    }

    /// <summary>
    /// 更新所有背景的速度
    /// </summary>
    private void UpdateBackgroundSpeeds()
    {
        if (farBackground != null)
        {
            farBackground.SetScrollSpeed(baseScrollSpeed * farSpeedMultiplier);
            farBackground.SetParallaxFactor(farSpeedMultiplier);
        }

        if (midBackground != null)
        {
            midBackground.SetScrollSpeed(baseScrollSpeed * midSpeedMultiplier);
            midBackground.SetParallaxFactor(midSpeedMultiplier);
        }

        if (nearBackground != null)
        {
            nearBackground.SetScrollSpeed(baseScrollSpeed * nearSpeedMultiplier);
            nearBackground.SetParallaxFactor(nearSpeedMultiplier);
        }
    }

    /// <summary>
    /// 设置基础滚动速度
    /// </summary>
    public void SetBaseScrollSpeed(float speed)
    {
        baseScrollSpeed = speed;
        UpdateBackgroundSpeeds();
    }

    /// <summary>
    /// 根据游戏速度调整背景速度（用于加速效果）
    /// </summary>
    public void SetGameSpeedMultiplier(float multiplier)
    {
        if (syncWithGameSpeed)
        {
            float adjustedSpeed = baseScrollSpeed * multiplier;

            if (farBackground != null)
                farBackground.SetScrollSpeed(adjustedSpeed * farSpeedMultiplier);

            if (midBackground != null)
                midBackground.SetScrollSpeed(adjustedSpeed * midSpeedMultiplier);

            if (nearBackground != null)
                nearBackground.SetScrollSpeed(adjustedSpeed * nearSpeedMultiplier);
        }
    }

    /// <summary>
    /// 动态设置背景 Sprite
    /// </summary>
    public void SetBackgroundSprite(Sprite sprite, BackgroundLayer layer)
    {
        ParallaxBackground targetBg = GetBackgroundByLayer(layer);
        if (targetBg != null)
        {
            SpriteRenderer sr = targetBg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
                // 重新计算纹理宽度
                if (sprite != null)
                {
                    targetBg.SetTextureWidth(sprite.bounds.size.x);
                }
            }
        }
    }

    /// <summary>
    /// 根据层级获取背景
    /// </summary>
    private ParallaxBackground GetBackgroundByLayer(BackgroundLayer layer)
    {
        return layer switch
        {
            BackgroundLayer.Far => farBackground,
            BackgroundLayer.Mid => midBackground,
            BackgroundLayer.Near => nearBackground,
            _ => null
        };
    }

    /// <summary>
    /// 背景层级枚举
    /// </summary>
    public enum BackgroundLayer
    {
        Far,    // 远距离
        Mid,    // 中距离
        Near    // 近距离
    }
}
