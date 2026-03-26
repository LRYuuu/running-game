using UnityEngine;

namespace RunnersJourney.Game
{
    /// <summary>
    /// 难度配置 - 使用 ScriptableObject 存储难度曲线参数
    /// 可在 Unity Editor 中调整，无需修改代码
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Square Fireline/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        #region 难度起点配置
        /// <summary>
        /// 开始增加难度的距离（米）
        /// </summary>
        [Header("难度起点")]
        [Tooltip("开始增加难度的距离，此距离内保持最低难度")]
        public float startDistance = 50f;
        #endregion

        #region 难度顶点配置
        /// <summary>
        /// 达到最大难度的距离（米）
        /// </summary>
        [Header("难度顶点")]
        [Tooltip("达到最大难度的距离，超过此距离后难度不再增加")]
        public float maxDistance = 500f;
        #endregion

        #region 障碍概率配置
        /// <summary>
        /// 基础障碍概率（最低难度时的概率）
        /// </summary>
        [Header("障碍概率")]
        [Tooltip("基础障碍概率（最低难度时的概率）")]
        [Range(0f, 1f)]
        public float baseObstacleChance = 0.3f;

        /// <summary>
        /// 最大障碍概率（最高难度时的概率）
        /// </summary>
        [Tooltip("最大障碍概率（最高难度时的概率）")]
        [Range(0f, 1f)]
        public float maxObstacleChance = 0.6f;
        #endregion

        #region 空隙概率配置
        /// <summary>
        /// 基础空隙概率（最低难度时的概率）
        /// </summary>
        [Header("空隙概率")]
        [Tooltip("基础空隙概率（最低难度时的概率）")]
        [Range(0f, 1f)]
        public float baseGapChance = 0.1f;

        /// <summary>
        /// 最大空隙概率（最高难度时的概率）
        /// </summary>
        [Tooltip("最大空隙概率（最高难度时的概率）")]
        [Range(0f, 1f)]
        public float maxGapChance = 0.25f;
        #endregion

        #region 曲线类型配置
        /// <summary>
        /// 难度增长曲线类型
        /// </summary>
        [Header("曲线类型")]
        [Tooltip("难度增长曲线类型")]
        public CurveType curveType = CurveType.Linear;

        /// <summary>
        /// 自定义难度曲线（仅当 curveType=Custom 时使用）
        /// </summary>
        [Header("自定义曲线")]
        [Tooltip("自定义难度曲线 - 可在 Inspector 中编辑")]
        public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        #endregion

        #region 公共属性
        /// <summary>
        /// 当前难度值（0.0 - 1.0）
        /// 0.0 = 最低难度，1.0 = 最高难度
        /// </summary>
        public float CurrentDifficulty { get; private set; } = 0f;

        /// <summary>
        /// 当前障碍概率（根据难度计算）
        /// </summary>
        public float CurrentObstacleChance { get; private set; }

        /// <summary>
        /// 当前空隙概率（根据难度计算）
        /// </summary>
        public float CurrentGapChance { get; private set; }
        #endregion

        #region 公共方法
        /// <summary>
        /// 更新难度状态
        /// </summary>
        /// <param name="distance">玩家前进距离</param>
        public void UpdateDifficulty(float distance)
        {
            // 安全区域：距离小于 startDistance 时保持最低难度
            if (distance < startDistance)
            {
                CurrentDifficulty = 0f;
                CurrentObstacleChance = baseObstacleChance;
                CurrentGapChance = baseGapChance;
                return;
            }

            // 计算难度进度 (0.0 - 1.0)
            float progress = Mathf.InverseLerp(startDistance, maxDistance, distance);

            // 应用曲线类型计算难度值
            CurrentDifficulty = CalculateCurveValue(progress);

            // 计算当前障碍概率（在基础值和最大值之间插值）
            CurrentObstacleChance = Mathf.Lerp(baseObstacleChance, maxObstacleChance, CurrentDifficulty);

            // 计算当前空隙概率（在基础值和最大值之间插值）
            CurrentGapChance = Mathf.Lerp(baseGapChance, maxGapChance, CurrentDifficulty);
        }

        /// <summary>
        /// 根据进度计算曲线值
        /// </summary>
        /// <param name="progress">进度 (0.0 - 1.0)</param>
        /// <returns>难度值 (0.0 - 1.0)</returns>
        private float CalculateCurveValue(float progress)
        {
            switch (curveType)
            {
                case CurveType.Linear:
                    return progress;

                case CurveType.Exponential:
                    // 先慢后快，适合 Roguelike 风格
                    return Mathf.Pow(progress, 2);

                case CurveType.Logarithmic:
                    // 先快后慢，适合休闲风格
                    return Mathf.Log(progress + 1, 2);

                case CurveType.Custom:
                    // 使用自定义曲线
                    return customCurve.Evaluate(progress);

                default:
                    return progress;
            }
        }

        /// <summary>
        /// 重置难度状态
        /// </summary>
        public void ResetDifficulty()
        {
            CurrentDifficulty = 0f;
            CurrentObstacleChance = baseObstacleChance;
            CurrentGapChance = baseGapChance;
        }
        #endregion
    }

    /// <summary>
    /// 难度曲线类型枚举
    /// </summary>
    public enum CurveType
    {
        /// <summary>
        /// 线性增长 - 难度随距离均匀增加
        /// </summary>
        Linear,

        /// <summary>
        /// 指数增长 - 先慢后快，适合 Roguelike 风格
        /// </summary>
        Exponential,

        /// <summary>
        /// 对数增长 - 先快后慢，适合休闲风格
        /// </summary>
        Logarithmic,

        /// <summary>
        /// 自定义曲线 - 在 Inspector 中编辑 AnimationCurve
        /// </summary>
        Custom
    }
}
