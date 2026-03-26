using UnityEngine;

namespace RunnersJourney.Game
{
    /// <summary>
    /// 游戏启动器 - 在场景加载后自动开始游戏
    /// </summary>
    public class GameStarter : MonoBehaviour
    {
        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        [SerializeField] private bool _enableDebugLog = true;

        [Header("启动选项")]
        [Tooltip("是否在游戏启动时自动开始（true=直接开始游戏，false=显示主界面等待点击开始按钮）")]
        [SerializeField] private bool _autoStartGame = false;

        private void Start()
        {
            Debug.Log($"[GameStarter] Start() called, _autoStartGame={_autoStartGame}, instance={GetInstanceID()}");

            // 如果禁用自动启动，显示主界面等待玩家点击开始按钮
            if (!_autoStartGame)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[GameStarter] 自动启动已禁用，等待主界面开始游戏");
                }
                return;
            }

            // 防止旧实例（DontDestroyOnLoad）在场景重新加载时启动游戏
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Waiting)
            {
                if (_enableDebugLog)
                {
                    Debug.Log($"[GameStarter] GameManager 已经在状态 {GameManager.Instance.CurrentState}，跳过自动启动");
                }
                return;
            }

            if (GameManager.Instance != null)
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[GameStarter] 调用 GameManager.StartGame()");
                }
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("[GameStarter] GameManager.Instance 为空！");
            }
        }
    }
}
