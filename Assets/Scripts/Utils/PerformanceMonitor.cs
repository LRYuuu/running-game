using UnityEngine;
using System;

namespace RunnersJourney.Utils
{
    /// <summary>
    /// 性能监控工具 - 开发调试用
    /// 在运行时显示 FPS 和内存使用情况
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        #region 序列化字段
        [Header("监控设置")]
        [Tooltip("更新间隔（秒）")]
        [SerializeField] private float _updateInterval = 0.5f;

        [Header("显示设置")]
        [Tooltip("是否在控制台输出性能数据")]
        [SerializeField] private bool _logToConsole = true;

        [Tooltip("是否在屏幕上显示")]
        [SerializeField] private bool _showOnScreen = true;

        [Header("性能阈值")]
        [Tooltip("警告 FPS 阈值")]
        [SerializeField] private int _warningFPSThreshold = 45;

        [Tooltip("错误 FPS 阈值")]
        [SerializeField] private int _errorFPSThreshold = 30;

        [Header("屏幕显示位置")]
        [Tooltip("屏幕显示位置")]
        [SerializeField] private TextAnchor _screenPosition = TextAnchor.UpperLeft;

        [Tooltip("显示字体大小")]
        [SerializeField] private int _fontSize = 20;
        #endregion

        #region 私有字段
        private float _fps;
        private float _minFps;
        private float _maxFps;
        private float _memoryMB;
        private float _lastUpdate;
        private int _frameCount;
        private float _fpsAccumulator;
        private GUIStyle _style;
        private Rect _screenRect;
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            UpdateScreenRect();
        }

        private void Update()
        {
            _frameCount++;
            _fpsAccumulator += Time.deltaTime;

            if (Time.time - _lastUpdate >= _updateInterval)
            {
                _fps = _frameCount / _fpsAccumulator;

                if (_lastUpdate > 0)
                {
                    if (_minFps == 0 || _fps < _minFps) _minFps = _fps;
                    if (_fps > _maxFps) _maxFps = _fps;
                }

                _memoryMB = GC.GetTotalMemory(false) / 1024f / 1024f;

                if (_logToConsole)
                {
                    string status = GetFPSStatus();
                    if (_fps < _errorFPSThreshold)
                    {
                        Debug.LogWarning($"[Performance] {status}, Memory: {_memoryMB:F1}MB");
                    }
                    else
                    {
                        Debug.Log($"[Performance] {status}, Memory: {_memoryMB:F1}MB");
                    }
                }

                _frameCount = 0;
                _fpsAccumulator = 0f;
                _lastUpdate = Time.time;
            }
        }

        private void OnGUI()
        {
            if (!_showOnScreen) return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize,
                    fontStyle = FontStyle.Bold
                };
            }

            if (_fps < _errorFPSThreshold)
            {
                _style.normal.textColor = Color.red;
            }
            else if (_fps < _warningFPSThreshold)
            {
                _style.normal.textColor = Color.yellow;
            }
            else
            {
                _style.normal.textColor = Color.green;
            }

            string displayText = $"FPS: {_fps:F1}\nMin: {_minFps:F1} | Max: {_maxFps:F1}\nMemory: {_memoryMB:F1}MB";
            GUI.Label(_screenRect, displayText, _style);
        }
        #endregion

        #region 公共方法
        public float GetFPS() => _fps;

        public float GetMemoryMB() => _memoryMB;

        public void ResetStats()
        {
            _minFps = 0;
            _maxFps = 0;
            _lastUpdate = 0;
        }

        public string GetFPSStatus()
        {
            if (_fps >= 55) return $"FPS: {_fps:F1} (Excellent)";
            if (_fps >= _warningFPSThreshold) return $"FPS: {_fps:F1} (Good)";
            if (_fps >= _errorFPSThreshold) return $"FPS: {_fps:F1} (Warning)";
            return $"FPS: {_fps:F1} (Critical)";
        }
        #endregion

        #region 私有方法
        private void UpdateScreenRect()
        {
            float width = 200f;
            float height = 80f;
            float margin = 10f;

            switch (_screenPosition)
            {
                case TextAnchor.UpperLeft:
                    _screenRect = new Rect(margin, margin, width, height);
                    break;
                case TextAnchor.UpperRight:
                    _screenRect = new Rect(Screen.width - width - margin, margin, width, height);
                    break;
                case TextAnchor.LowerLeft:
                    _screenRect = new Rect(margin, Screen.height - height - margin, width, height);
                    break;
                case TextAnchor.LowerRight:
                    _screenRect = new Rect(Screen.width - width - margin, Screen.height - height - margin, width, height);
                    break;
                default:
                    _screenRect = new Rect(margin, margin, width, height);
                    break;
            }
        }
        #endregion
    }
}