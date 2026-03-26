using UnityEngine;

namespace RunnersJourney.Game
{
    /// <summary>
    /// 游戏配置 - 使用 ScriptableObject 存储游戏参数
    /// 可在 Unity Editor 中调整，无需修改代码
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Square Fireline/Game Config")]
    public class GameConfig : ScriptableObject
    {
        #region 重生配置
        /// <summary>
        /// 重生延迟时间（秒）
        /// </summary>
        [Header("重生配置")]
        [Tooltip("玩家死亡后到重生的延迟时间")]
        public float respawnDelay = 1.0f;
        #endregion

        #region 游戏状态
        /// <summary>
        /// 初始游戏状态
        /// </summary>
        [Header("游戏状态")]
        [Tooltip("游戏启动时的初始状态")]
        public GameState initialState = GameState.Waiting;
        #endregion

        #region 检查点配置
        /// <summary>
        /// 检查点激活半径
        /// </summary>
        [Header("检查点配置")]
        [Tooltip("玩家进入此半径范围内激活检查点")]
        public float checkpointActivateRadius = 0.5f;
        #endregion
    }
}
