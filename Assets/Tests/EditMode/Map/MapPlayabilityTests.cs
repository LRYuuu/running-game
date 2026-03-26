using NUnit.Framework;
using UnityEngine;
using RunnersJourney.Map;

namespace RunnersJourney.Tests.EditMode.Map
{
    /// <summary>
    /// 地图可玩性验证器测试
    /// </summary>
    [TestFixture]
    public class MapPlayabilityTests
    {
        private TilemapMapConfig _config;

        [SetUp]
        public void Setup()
        {
            // 创建测试配置
            _config = ScriptableObject.CreateInstance<TilemapMapConfig>();

            // 设置默认可玩性参数
            _config.maxGapWidth = 3;
            _config.maxGapWidthPlayable = 8;
            _config.minObstacleGap = 3;
            _config.minPlayableObstacleGap = 3;
            _config.heightVariation = 2; // 不超过 maxHeightDifference * 2 = 2
            _config.maxHeightDifference = 1;
            _config.flatChunkCount = 3;
            _config.safeStartChunkCount = 3;
            _config.minHeight = 2;
            _config.maxHeight = 8;
            _config.enablePlayabilityValidation = true;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        #region ValidateConfig 测试

        [Test]
        public void ValidateConfig_ValidConfig_ReturnsTrue()
        {
            // Arrange - 使用 Setup 中的默认有效配置

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsTrue(result, "有效配置应该通过验证");
        }

        [Test]
        public void ValidateConfig_GapWidthTooLarge_ReturnsFalse()
        {
            // Arrange
            _config.maxGapWidth = 10; // 超过可玩值 8

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsFalse(result, "最大空隙宽度超过可玩值时应返回 false");
        }

        [Test]
        public void ValidateConfig_ObstacleGapTooSmall_ReturnsFalse()
        {
            // Arrange
            _config.minObstacleGap = 2; // 小于可玩值 3

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsFalse(result, "最小障碍物间隔小于可玩值时应返回 false");
        }

        [Test]
        public void ValidateConfig_HeightVariationTooLarge_ReturnsFalse()
        {
            // Arrange
            _config.heightVariation = 5; // 超过 maxHeightDifference * 2 = 2

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsFalse(result, "高度变化幅度过大时应返回 false");
        }

        [Test]
        public void ValidateConfig_FlatChunkCountInsufficient_ReturnsFalse()
        {
            // Arrange
            _config.flatChunkCount = 2; // 小于 safeStartChunkCount 3

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsFalse(result, "起始平坦区域不足时应返回 false");
        }

        [Test]
        public void ValidateConfig_GapObstacleIntervalInsufficient_ReturnsFalse()
        {
            // Arrange
            // minSafeDistance = minObstacleGap + maxGapWidth + 2 = 3 + 3 + 2 = 8
            // 需要 minGapInterval * chunkWidth < 8
            // 设置 minGapInterval = 0，则 0 * 20 = 0 < 8
            _config.minGapInterval = 0;
            _config.chunkWidth = 20;

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsFalse(result, "空隙 - 障碍物间隔不足时应返回 false");
        }

        [Test]
        public void ValidateConfig_MaxHeightDifferenceTooSmall_ReturnsFalse()
        {
            // Arrange
            _config.maxHeight = 5;
            _config.minHeight = 2;
            _config.maxHeightDifference = 4; // maxHeight - minHeight < maxHeightDifference

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsFalse(result, "最大高度差限制过小时应返回 false");
        }

        [Test]
        public void ValidateConfig_ValidationDisabled_ReturnsTrue()
        {
            // Arrange
            _config.maxGapWidth = 10; // 无效值
            _config.enablePlayabilityValidation = false;

            // Act
            bool result = MapPlayabilityValidator.ValidateConfig(_config);

            // Assert
            Assert.IsTrue(result, "禁用验证时应始终返回 true");
        }

        #endregion

        #region IsGapWidthPlayable 测试

        [Test]
        public void IsGapWidthPlayable_WidthWithinLimit_ReturnsTrue()
        {
            // Arrange
            int gapWidth = 5; // 小于 maxGapWidthPlayable 8

            // Act
            bool result = MapPlayabilityValidator.IsGapWidthPlayable(_config, gapWidth);

            // Assert
            Assert.IsTrue(result, "空隙宽度在可玩范围内时应返回 true");
        }

        [Test]
        public void IsGapWidthPlayable_WidthAtLimit_ReturnsTrue()
        {
            // Arrange
            int gapWidth = 8; // 等于 maxGapWidthPlayable 8

            // Act
            bool result = MapPlayabilityValidator.IsGapWidthPlayable(_config, gapWidth);

            // Assert
            Assert.IsTrue(result, "空隙宽度等于可玩极限时应返回 true");
        }

        [Test]
        public void IsGapWidthPlayable_WidthExceedsLimit_ReturnsFalse()
        {
            // Arrange
            int gapWidth = 9; // 大于 maxGapWidthPlayable 8

            // Act
            bool result = MapPlayabilityValidator.IsGapWidthPlayable(_config, gapWidth);

            // Assert
            Assert.IsFalse(result, "空隙宽度超过可玩极限时应返回 false");
        }

        [Test]
        public void IsGapWidthPlayable_ZeroWidth_ReturnsTrue()
        {
            // Arrange
            int gapWidth = 0;

            // Act
            bool result = MapPlayabilityValidator.IsGapWidthPlayable(_config, gapWidth);

            // Assert
            Assert.IsTrue(result, "零宽度应视为可玩");
        }

        #endregion

        #region IsObstacleGapPlayable 测试

        [Test]
        public void IsObstacleGapPlayable_GapWithinLimit_ReturnsTrue()
        {
            // Arrange
            int obstacleGap = 5; // 大于 minPlayableObstacleGap 3

            // Act
            bool result = MapPlayabilityValidator.IsObstacleGapPlayable(_config, obstacleGap);

            // Assert
            Assert.IsTrue(result, "障碍物间隔在可玩范围内时应返回 true");
        }

        [Test]
        public void IsObstacleGapPlayable_GapAtLimit_ReturnsTrue()
        {
            // Arrange
            int obstacleGap = 3; // 等于 minPlayableObstacleGap 3

            // Act
            bool result = MapPlayabilityValidator.IsObstacleGapPlayable(_config, obstacleGap);

            // Assert
            Assert.IsTrue(result, "障碍物间隔等于可玩极限时应返回 true");
        }

        [Test]
        public void IsObstacleGapPlayable_GapBelowLimit_ReturnsFalse()
        {
            // Arrange
            int obstacleGap = 2; // 小于 minPlayableObstacleGap 3

            // Act
            bool result = MapPlayabilityValidator.IsObstacleGapPlayable(_config, obstacleGap);

            // Assert
            Assert.IsFalse(result, "障碍物间隔小于可玩极限时应返回 false");
        }

        #endregion

        #region IsHeightDifferencePlayable 测试

        [Test]
        public void IsHeightDifferencePlayable_HeightDiffWithinLimit_ReturnsTrue()
        {
            // Arrange
            int heightDiff = 1; // 等于 maxHeightDifference 1

            // Act
            bool result = MapPlayabilityValidator.IsHeightDifferencePlayable(_config, heightDiff);

            // Assert
            Assert.IsTrue(result, "高度差在可玩范围内时应返回 true");
        }

        [Test]
        public void IsHeightDifferencePlayable_HeightDiffAtLimit_ReturnsTrue()
        {
            // Arrange
            int heightDiff = 1; // 等于 maxHeightDifference 1

            // Act
            bool result = MapPlayabilityValidator.IsHeightDifferencePlayable(_config, heightDiff);

            // Assert
            Assert.IsTrue(result, "高度差等于可玩极限时应返回 true");
        }

        [Test]
        public void IsHeightDifferencePlayable_HeightDiffExceedsLimit_ReturnsFalse()
        {
            // Arrange
            int heightDiff = 2; // 大于 maxHeightDifference 1

            // Act
            bool result = MapPlayabilityValidator.IsHeightDifferencePlayable(_config, heightDiff);

            // Assert
            Assert.IsFalse(result, "高度差超过可玩极限时应返回 false");
        }

        [Test]
        public void IsHeightDifferencePlayable_NegativeHeightDiff_ReturnsTrue()
        {
            // Arrange
            int heightDiff = -1; // 负高度差，绝对值在范围内

            // Act
            bool result = MapPlayabilityValidator.IsHeightDifferencePlayable(_config, heightDiff);

            // Assert
            Assert.IsTrue(result, "负高度差绝对值在范围内时应返回 true");
        }

        [Test]
        public void IsHeightDifferencePlayable_LargeNegativeHeightDiff_ReturnsFalse()
        {
            // Arrange
            int heightDiff = -2; // 负高度差，绝对值超过范围

            // Act
            bool result = MapPlayabilityValidator.IsHeightDifferencePlayable(_config, heightDiff);

            // Assert
            Assert.IsFalse(result, "负高度差绝对值超过范围时应返回 false");
        }

        #endregion

        #region 集成测试

        [Test]
        public void Integration_PlayableConfig_AllChecksPass()
        {
            // Arrange - 创建一个完全可玩的配置
            _config.maxGapWidth = 6;
            _config.maxGapWidthPlayable = 8;
            _config.minObstacleGap = 4;
            _config.minPlayableObstacleGap = 3;
            _config.heightVariation = 2;
            _config.maxHeightDifference = 1;
            _config.flatChunkCount = 5;
            _config.safeStartChunkCount = 3;

            // Act & Assert
            Assert.IsTrue(MapPlayabilityValidator.ValidateConfig(_config), "配置验证应通过");
            Assert.IsTrue(MapPlayabilityValidator.IsGapWidthPlayable(_config, 5), "空隙宽度 5 应可玩");
            Assert.IsTrue(MapPlayabilityValidator.IsObstacleGapPlayable(_config, 4), "障碍物间隔 4 应可玩");
            Assert.IsTrue(MapPlayabilityValidator.IsHeightDifferencePlayable(_config, 1), "高度差 1 应可玩");
        }

        [Test]
        public void Integration_UnplayableConfig_MultipleChecksFail()
        {
            // Arrange - 创建一个多处不可玩的配置
            _config.maxGapWidth = 10; // 太大
            _config.minObstacleGap = 1; // 太小
            _config.heightVariation = 5; // 太大

            // Act & Assert
            Assert.IsFalse(MapPlayabilityValidator.ValidateConfig(_config), "配置验证应失败");
            Assert.IsFalse(MapPlayabilityValidator.IsGapWidthPlayable(_config, 10), "空隙宽度 10 不可玩");
            Assert.IsFalse(MapPlayabilityValidator.IsObstacleGapPlayable(_config, 1), "障碍物间隔 1 不可玩");
            Assert.IsFalse(MapPlayabilityValidator.IsHeightDifferencePlayable(_config, 3), "高度差 3 不可玩");
        }

        #endregion
    }
}
