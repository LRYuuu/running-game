using UnityEngine;
using System;

namespace SquareFireline.Game
{
    /// <summary>
    /// 难度计算器 - 单例模式
    /// 根据玩家距离动态计算当前难度和相关参数
    /// </summary>
    public class DifficultyCalculator : MonoBehaviour
    {
        #region 单例
        /// <summary>
        /// DifficultyCalculator 单例实例
        /// </summary>
        public static DifficultyCalculator Instance { get; private set; }

        /// <summary>
        /// 强制初始化单例实例（仅供测试使用）
        /// </summary>
        /// <param name="instance">要设置为单例的实例</param>
        public static void ForceInitializeInstance(DifficultyCalculator instance)
        {
            if (Instance == null)
            {
                Instance = instance;
            }
        }

        /// <summary>
        /// 重置单例实例（仅供测试使用）
        /// </summary>
        public static void ResetInstance()
        {
            Instance = null;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 难度改变时触发（新的难度值）
        /// </summary>
        public event Action<float> OnDifficultyChanged;

        /// <summary>
        /// 障碍概率改变时触发（新的障碍概率）
        /// </summary>
        public event Action<float> OnObstacleChanceChanged;

        /// <summary>
        /// 空隙概率改变时触发（新的空隙概率）
        /// </summary>
        public event Action<float> OnGapChanceChanged;
        #endregion

        #region 序列化字段
        [Header("难度配置")]
        [Tooltip("难度配置 ScriptableObject")]
        [SerializeField] private DifficultyConfig _config;

        [Header("更新设置")]
        [Tooltip("难度更新间隔（秒），避免每帧计算")]
        [SerializeField] private float updateInterval = 1f;
        #endregion

        #region 公共属性
        /// <summary>
        /// 当前难度值 (0.0 - 1.0)
        /// 0.0 = 最低难度，1.0 = 最高难度
        /// </summary>
        public float CurrentDifficulty => _config != null ? _config.CurrentDifficulty : 0f;

        /// <summary>
        /// 当前障碍概率
        /// </summary>
        public float CurrentObstacleChance => _config != null ? _config.CurrentObstacleChance : 0f;

        /// <summary>
        /// 当前空隙概率
        /// </summary>
        public float CurrentGapChance => _config != null ? _config.CurrentGapChance : 0f;

        /// <summary>
        /// 玩家当前距离
        /// </summary>
        public float CurrentDistance { get; private set; } = 0f;
        #endregion

        #region 私有字段
        /// <summary>
        /// 难度更新计时器
        /// </summary>
        private float _updateTimer = 0f;

        /// <summary>
        /// 上一次的难度值（用于事件触发）
        /// </summary>
        private float _lastDifficulty = 0f;

        /// <summary>
        /// 上一次的障碍概率（用于事件触发）
        /// </summary>
        private float _lastObstacleChance = 0f;

        /// <summary>
        /// 上一次的空隙概率（用于事件触发）
        /// </summary>
        private float _lastGapChance = 0f;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            // 单例初始化
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 验证配置引用
            if (_config == null)
            {
                Debug.LogWarning("[DifficultyCalculator] 缺少 DifficultyConfig 引用！");
            }
        }

        private void Update()
        {
            if (_config == null) return;

            // 更新计时器
            _updateTimer += Time.deltaTime;
            if (_updateTimer < updateInterval) return;

            _updateTimer = 0f;

            // 更新难度
            UpdateDifficulty();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 设置玩家距离并更新难度
        /// </summary>
        /// <param name="distance">玩家前进距离</param>
        public void SetPlayerDistance(float distance)
        {
            CurrentDistance = distance;

            // 立即更新难度
            UpdateDifficulty();
        }

        /// <summary>
        /// 重置难度进度
        /// </summary>
        public void ResetProgress()
        {
            CurrentDistance = 0f;
            _config.ResetDifficulty();

            // 重置跟踪值
            _lastDifficulty = 0f;
            _lastObstacleChance = _config.CurrentObstacleChance;
            _lastGapChance = _config.CurrentGapChance;

            Debug.Log("[DifficultyCalculator] 难度进度已重置");
        }

        /// <summary>
        /// 获取当前难度值（不更新）
        /// </summary>
        /// <returns>难度值 (0.0 - 1.0)</returns>
        public float GetDifficulty()
        {
            return CurrentDifficulty;
        }

        /// <summary>
        /// 获取当前障碍概率（不更新）
        /// </summary>
        /// <returns>障碍概率</returns>
        public float GetObstacleChance()
        {
            return CurrentObstacleChance;
        }

        /// <summary>
        /// 获取当前空隙概率（不更新）
        /// </summary>
        /// <returns>空隙概率</returns>
        public float GetGapChance()
        {
            return CurrentGapChance;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 更新难度并触发事件
        /// </summary>
        private void UpdateDifficulty()
        {
            // 更新配置中的难度值
            _config.UpdateDifficulty(CurrentDistance);

            // 检查难度值是否变化并触发事件
            if (Mathf.Abs(_config.CurrentDifficulty - _lastDifficulty) > 0.01f)
            {
                _lastDifficulty = _config.CurrentDifficulty;
                OnDifficultyChanged?.Invoke(_config.CurrentDifficulty);
            }

            // 检查障碍概率是否变化并触发事件
            if (Mathf.Abs(_config.CurrentObstacleChance - _lastObstacleChance) > 0.01f)
            {
                _lastObstacleChance = _config.CurrentObstacleChance;
                OnObstacleChanceChanged?.Invoke(_config.CurrentObstacleChance);
            }

            // 检查空隙概率是否变化并触发事件
            if (Mathf.Abs(_config.CurrentGapChance - _lastGapChance) > 0.01f)
            {
                _lastGapChance = _config.CurrentGapChance;
                OnGapChanceChanged?.Invoke(_config.CurrentGapChance);
            }
        }
        #endregion

        #region 调试方法
        /// <summary>
        /// 打印当前难度状态到日志
        /// </summary>
        public void LogDifficultyStatus()
        {
            Debug.Log($"[DifficultyCalculator] 难度状态 - 距离={CurrentDistance:F1}m, " +
                $"难度={CurrentDifficulty:F2}, " +
                $"障碍概率={CurrentObstacleChance:F2}, " +
                $"空隙概率={CurrentGapChance:F2}");
        }
        #endregion
    }
}
