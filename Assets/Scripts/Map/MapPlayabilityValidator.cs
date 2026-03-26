using UnityEngine;

namespace RunnersJourney.Map
{
    /// <summary>
    /// 地图可玩性验证器 - 验证地图配置参数的可玩性
    /// </summary>
    public static class MapPlayabilityValidator
    {
        /// <summary>
        /// 验证地图配置参数的可玩性
        /// </summary>
        /// <param name="config">地图配置</param>
        /// <returns>验证是否通过</returns>
        public static bool ValidateConfig(TilemapMapConfig config)
        {
            if (!config.enablePlayabilityValidation)
            {
                Debug.Log("[MapPlayabilityValidator] 可玩性验证已禁用，跳过验证");
                return true;
            }

            bool hasWarning = false;

            // 1. 验证空隙宽度
            if (config.maxGapWidth > config.maxGapWidthPlayable)
            {
                Debug.LogWarning($"[MapPlayabilityValidator] 警告：最大空隙宽度 ({config.maxGapWidth}) 超过可玩值 ({config.maxGapWidthPlayable})，玩家可能无法跳过！");
                hasWarning = true;
            }

            // 2. 验证障碍物间隔
            if (config.minObstacleGap < config.minPlayableObstacleGap)
            {
                Debug.LogWarning($"[MapPlayabilityValidator] 警告：最小障碍物间隔 ({config.minObstacleGap}) 小于可玩值 ({config.minPlayableObstacleGap})，玩家可能无法通过！");
                hasWarning = true;
            }

            // 3. 验证高度差
            if (config.heightVariation > config.maxHeightDifference * 2)
            {
                Debug.LogWarning($"[MapPlayabilityValidator] 警告：高度变化幅度过大 ({config.heightVariation})，可能导致地形不可玩！");
                hasWarning = true;
            }

            // 4. 验证起始区域
            if (config.flatChunkCount < config.safeStartChunkCount)
            {
                Debug.LogWarning($"[MapPlayabilityValidator] 警告：起始平坦区域不足 ({config.flatChunkCount} < {config.safeStartChunkCount})，玩家可能没有足够时间熟悉操作！");
                hasWarning = true;
            }

            // 5. 验证空隙 - 障碍物间隔
            int minGapIntervalInTiles = config.minGapInterval * config.chunkWidth;
            int minSafeDistance = config.minObstacleGap + config.maxGapWidth + 2; // +2 为空隙两侧安全边界
            if (minGapIntervalInTiles < minSafeDistance)
            {
                Debug.LogWarning($"[MapPlayabilityValidator] 警告：空隙间隔 ({minGapIntervalInTiles}) 可能不足以形成安全的障碍物布局 (需要至少 {minSafeDistance})！");
                hasWarning = true;
            }

            // 6. 验证最大高度差限制
            if (config.maxHeight < config.minHeight + config.maxHeightDifference)
            {
                Debug.LogWarning($"[MapPlayabilityValidator] 警告：最大高度 ({config.maxHeight}) 与最小高度 ({config.minHeight}) 差距过小，限制了地形变化！");
                hasWarning = true;
            }

            if (!hasWarning)
            {
                Debug.Log("[MapPlayabilityValidator] 配置验证通过，所有参数符合可玩性要求！");
            }

            return !hasWarning;
        }

        /// <summary>
        /// 验证空隙宽度是否可玩
        /// </summary>
        /// <param name="config">地图配置</param>
        /// <param name="gapWidth">空隙宽度</param>
        /// <returns>是否可玩</returns>
        public static bool IsGapWidthPlayable(TilemapMapConfig config, int gapWidth)
        {
            return gapWidth <= config.maxGapWidthPlayable;
        }

        /// <summary>
        /// 验证障碍物间隔是否可玩
        /// </summary>
        /// <param name="config">地图配置</param>
        /// <param name="obstacleGap">障碍物间隔</param>
        /// <returns>是否可玩</returns>
        public static bool IsObstacleGapPlayable(TilemapMapConfig config, int obstacleGap)
        {
            return obstacleGap >= config.minPlayableObstacleGap;
        }

        /// <summary>
        /// 验证高度差是否可玩
        /// </summary>
        /// <param name="config">地图配置</param>
        /// <param name="heightDifference">高度差</param>
        /// <returns>是否可玩</returns>
        public static bool IsHeightDifferencePlayable(TilemapMapConfig config, int heightDifference)
        {
            return Mathf.Abs(heightDifference) <= config.maxHeightDifference;
        }
    }
}
