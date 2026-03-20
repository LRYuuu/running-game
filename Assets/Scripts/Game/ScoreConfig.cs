using UnityEngine;

namespace SquareFireline.Game
{
    /// <summary>
    /// 分数配置 - 使用 ScriptableObject 存储分数系统参数
    /// 可在 Unity Editor 中调整，无需修改代码
    /// </summary>
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "Square Fireline/Score Config")]
    public class ScoreConfig : ScriptableObject
    {
        #region 分数配置
        /// <summary>
        /// 每秒增加的分数
        /// </summary>
        [Header("分数配置")]
        [Tooltip("每秒增加的分数")]
        public int scorePerSecond = 1;

        /// <summary>
        /// 分数累加间隔（秒）
        /// </summary>
        [Tooltip("分数累加的时间间隔")]
        public float scoreInterval = 1f;
        #endregion
    }
}
