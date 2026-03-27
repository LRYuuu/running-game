using UnityEngine;
using UnityEngine.Tilemaps;

namespace RunnersJourney.Map
{
    /// <summary>
    /// 生物群系配置（ScriptableObject）
    /// 用于配置不同群系的视觉元素
    /// </summary>
    [CreateAssetMenu(fileName = "NewBiome", menuName = "Runner's Journey/Biome Config")]
    public class BiomeConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("群系名称")]
        public string biomeName = "Grassland";

        [Header("地表 Tile（顶层）")]
        [Tooltip("左边界草坪 Tile（左侧有黑边）")]
        public TileBase grassLeft;

        [Tooltip("中间草坪 Tile（无缝衔接）")]
        public TileBase grassMiddle;

        [Tooltip("右边界草坪 Tile（右侧有黑边）")]
        public TileBase grassRight;

        [Tooltip("孤立草坪 Tile（两侧都有黑边，用于单格凸起地块）")]
        public TileBase grassIsolated;

        [Header("土壤 Tile（地下层）")]
        [Tooltip("普通土壤 Tile")]
        public TileBase dirtTile;

        [Tooltip("左边界土壤 Tile（左侧有黑边）")]
        public TileBase dirtLeft;

        [Tooltip("右边界土壤 Tile（右侧有黑边）")]
        public TileBase dirtRight;

        [Header("视觉配置")]
        [Tooltip("天空颜色")]
        public Color skyColor = Color.cyan;

        [Tooltip("背景颜色")]
        public Color backgroundColor = Color.green;

        [Header("障碍物")]
        [Tooltip("该群系特有的障碍物 Tile（群系专属障碍物池）")]
        public TileBase[] obstacleTiles;

        #region 辅助方法

        /// <summary>
        /// 根据边界状态获取地表 Tile
        /// </summary>
        public TileBase GetGrassTile(bool leftExposed, bool rightExposed)
        {
            if (leftExposed && rightExposed)
            {
                // 两侧都暴露（孤立/凸起地块）
                return grassIsolated != null ? grassIsolated : grassMiddle;
            }
            else if (leftExposed)
            {
                // 左侧暴露
                return grassLeft != null ? grassLeft : grassMiddle;
            }
            else if (rightExposed)
            {
                // 右侧暴露
                return grassRight != null ? grassRight : grassMiddle;
            }
            else
            {
                // 中间（无缝）
                return grassMiddle;
            }
        }

        /// <summary>
        /// 根据边界状态获取土壤 Tile
        /// </summary>
        public TileBase GetDirtTile(bool leftExposed, bool rightExposed)
        {
            if (leftExposed && !rightExposed)
            {
                // 左侧暴露
                return dirtLeft != null ? dirtLeft : dirtTile;
            }
            else if (!leftExposed && rightExposed)
            {
                // 右侧暴露
                return dirtRight != null ? dirtRight : dirtTile;
            }
            else
            {
                // 中间或两侧都暴露（使用普通 Tile 加翻转）
                return dirtTile;
            }
        }

        #endregion
    }
}
