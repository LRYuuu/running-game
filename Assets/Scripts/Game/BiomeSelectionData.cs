using UnityEngine;

namespace RunnersJourney.Game
{
    /// <summary>
    /// 群系选择数据持久化
    /// 使用 PlayerPrefs 保存玩家选择的群系模式
    /// 支持四种模式：草地、沙漠、雪地、混合群系
    /// </summary>
    public static class BiomeSelectionData
    {
        #region 常量

        private const string SelectedBiomeKey = "SelectedBiome";
        private const string HasSelectedKey = "HasSelectedBiome";

        /// <summary>
        /// 草地模式键值（固定模式）
        /// </summary>
        public const string ModeGrassland = "grassland";

        /// <summary>
        /// 沙漠模式键值（固定模式）
        /// </summary>
        public const string ModeDesert = "desert";

        /// <summary>
        /// 雪地模式键值（固定模式）
        /// </summary>
        public const string ModeSnowland = "snowland";

        /// <summary>
        /// 混合群系模式键值（混合模式）
        /// </summary>
        public const string ModeSequence = "sequence";

        #endregion

        #region 公共方法

        /// <summary>
        /// 保存玩家选择的群系模式
        /// </summary>
        /// <param name="modeKey">群系模式键值</param>
        public static void SaveSelection(string modeKey)
        {
            if (string.IsNullOrEmpty(modeKey))
            {
                Debug.LogWarning("[BiomeSelectionData] 尝试保存空的群系模式");
                return;
            }

            PlayerPrefs.SetString(SelectedBiomeKey, modeKey);
            PlayerPrefs.SetInt(HasSelectedKey, 1);
            PlayerPrefs.Save();
            Debug.Log($"[BiomeSelectionData] 保存选择：{modeKey}");
        }

        /// <summary>
        /// 加载玩家上次选择的群系模式
        /// </summary>
        /// <returns>群系模式键值，如果没有选择过则返回 null</returns>
        public static string LoadSelection()
        {
            if (PlayerPrefs.HasKey(SelectedBiomeKey))
            {
                string value = PlayerPrefs.GetString(SelectedBiomeKey);
                Debug.Log($"[BiomeSelectionData] LoadSelection: found={value}");
                return value;
            }
            Debug.Log("[BiomeSelectionData] LoadSelection: no saved selection");
            return null;
        }

        /// <summary>
        /// 是否已经选择过群系
        /// </summary>
        /// <returns>true=已选择，false=未选择</returns>
        public static bool HasSelectedBefore()
        {
            int value = PlayerPrefs.GetInt(HasSelectedKey, 0);
            bool result = (value == 1);
            Debug.Log($"[BiomeSelectionData] HasSelectedBefore: {result} (PlayerPrefs value={value})");
            return result;
        }

        /// <summary>
        /// 重置群系选择
        /// </summary>
        public static void ResetSelection()
        {
            PlayerPrefs.DeleteKey(SelectedBiomeKey);
            PlayerPrefs.DeleteKey(HasSelectedKey);
            Debug.Log("[BiomeSelectionData] 重置群系选择");
        }

        /// <summary>
        /// 清除群系选择数据（用于测试）
        /// </summary>
        public static void Clear()
        {
            PlayerPrefs.DeleteKey(SelectedBiomeKey);
            PlayerPrefs.DeleteKey(HasSelectedKey);
        }

        /// <summary>
        /// 是否为混合模式
        /// </summary>
        /// <param name="modeKey">群系模式键值</param>
        /// <returns>true=混合模式，false=固定模式</returns>
        public static bool IsSequenceMode(string modeKey)
        {
            return modeKey == ModeSequence;
        }

        /// <summary>
        /// 是否为有效的群系模式
        /// </summary>
        /// <param name="modeKey">群系模式键值</param>
        /// <returns>true=有效，false=无效</returns>
        public static bool IsValidMode(string modeKey)
        {
            return modeKey == ModeGrassland ||
                   modeKey == ModeDesert ||
                   modeKey == ModeSnowland ||
                   modeKey == ModeSequence;
        }

        #endregion
    }
}
