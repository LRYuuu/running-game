using UnityEngine;
using System;

namespace SquareFireline.Game
{
    /// <summary>
    /// 分数管理器 - 管理当前分数和最高分
    /// 使用单例模式，跨场景持久化
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        #region 单例
        /// <summary>
        /// ScoreManager 单例实例
        /// </summary>
        public static ScoreManager Instance { get; private set; }
        #endregion

        #region 事件
        /// <summary>分数改变时触发（当前分数，最高分）</summary>
        public event Action<int, int> OnScoreChanged;
        #endregion

        #region 分数
        /// <summary>
        /// 当前分数
        /// </summary>
        public int CurrentScore { get; private set; }

        /// <summary>
        /// 历史最高分
        /// </summary>
        public int HighScore { get; private set; }
        #endregion

        #region 配置
        [Header("游戏配置")]
        [Tooltip("分数配置 ScriptableObject")]
        [SerializeField] private ScoreConfig _scoreConfig;
        #endregion

        #region 私有字段
        private float _accumulationTime;
        private bool _isAccumulating;
        private const string HighScoreKey = "SquareFireline_HighScore";

        // 缓存配置值（从 ScoreConfig 读取）
        private int _scorePerSecond;
        private float _scoreInterval;
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

            // DontDestroyOnLoad 只能在 play mode 下使用
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            // 从 ScoreConfig 读取配置
            if (_scoreConfig != null)
            {
                _scorePerSecond = _scoreConfig.scorePerSecond;
                _scoreInterval = _scoreConfig.scoreInterval;
            }
            else
            {
                // 默认值
                _scorePerSecond = 1;
                _scoreInterval = 1f;
                Debug.LogWarning("[ScoreManager] ScoreConfig 未设置，使用默认值");
            }

            // 加载最高分
            LoadHighScore();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnEnable()
        {
            // 订阅游戏状态变化
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void Update()
        {
            if (!_isAccumulating)
                return;

            _accumulationTime += Time.deltaTime;
            if (_accumulationTime >= _scoreInterval)
            {
                AddScore(_scorePerSecond);
                _accumulationTime -= _scoreInterval;
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 增加分数
        /// </summary>
        /// <param name="amount">增加的分数</param>
        public void AddScore(int amount)
        {
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore, HighScore);
        }

        /// <summary>
        /// 重置分数（死亡时调用）
        /// </summary>
        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore, HighScore);
        }

        /// <summary>
        /// 检查是否打破纪录
        /// </summary>
        /// <returns>是否打破纪录</returns>
        public bool CheckHighScore()
        {
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                SaveHighScore();
                OnScoreChanged?.Invoke(CurrentScore, HighScore);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重置单例实例（仅用于测试）
        /// </summary>
        public static void ResetInstance()
        {
            Instance = null;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 游戏状态改变回调
        /// </summary>
        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    _isAccumulating = true;
                    _accumulationTime = 0f;
                    break;
                case GameState.Dying:
                    _isAccumulating = false;
                    CheckHighScore();
                    ResetScore();  // Story 4-5: 死亡时重置分数
                    break;
                case GameState.Waiting:
                    _isAccumulating = false;
                    ResetScore();
                    break;
            }
        }

        /// <summary>
        /// 保存最高分
        /// </summary>
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载最高分
        /// </summary>
        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }
        #endregion
    }
}
