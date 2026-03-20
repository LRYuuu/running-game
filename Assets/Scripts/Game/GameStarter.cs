using UnityEngine;

namespace SquareFireline.Game
{
    /// <summary>
    /// 游戏启动器 - 在场景加载后自动开始游戏
    /// </summary>
    public class GameStarter : MonoBehaviour
    {
        [Header("调试选项")]
        [Tooltip("是否启用详细日志")]
        [SerializeField] private bool _enableDebugLog = true;

        private void Start()
        {
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
