using NUnit.Framework;
using UnityEngine;
using RunnersJourney.Game;

namespace RunnersJourney.Tests.EditMode.Game
{
    /// <summary>
    /// DifficultyConfig 单元测试
    /// </summary>
    [TestFixture]
    public class DifficultyConfigTests
    {
        private DifficultyConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = ScriptableObject.CreateInstance<DifficultyConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }
        }

        #region 默认值测试

        [Test]
        public void DifficultyConfig_CreateAsset_HasDefaultStartDistance()
        {
            // Assert
            Assert.AreEqual(50f, _config.startDistance);
        }

        [Test]
        public void DifficultyConfig_CreateAsset_HasDefaultMaxDistance()
        {
            // Assert
            Assert.AreEqual(500f, _config.maxDistance);
        }

        [Test]
        public void DifficultyConfig_CreateAsset_HasDefaultBaseObstacleChance()
        {
            // Assert
            Assert.AreEqual(0.3f, _config.baseObstacleChance);
        }

        [Test]
        public void DifficultyConfig_CreateAsset_HasDefaultMaxObstacleChance()
        {
            // Assert
            Assert.AreEqual(0.6f, _config.maxObstacleChance);
        }

        [Test]
        public void DifficultyConfig_CreateAsset_HasDefaultBaseGapChance()
        {
            // Assert
            Assert.AreEqual(0.1f, _config.baseGapChance);
        }

        [Test]
        public void DifficultyConfig_CreateAsset_HasDefaultMaxGapChance()
        {
            // Assert
            Assert.AreEqual(0.25f, _config.maxGapChance);
        }

        [Test]
        public void DifficultyConfig_CreateAsset_HasDefaultCurveTypeLinear()
        {
            // Assert
            Assert.AreEqual(CurveType.Linear, _config.curveType);
        }

        #endregion

        #region 难度计算测试 - 安全区域

        [Test]
        public void UpdateDifficulty_DistanceLessThanStart_ReturnsZeroDifficulty()
        {
            // Act
            _config.UpdateDifficulty(0f);

            // Assert
            Assert.AreEqual(0f, _config.CurrentDifficulty);
            Assert.AreEqual(_config.baseObstacleChance, _config.CurrentObstacleChance);
            Assert.AreEqual(_config.baseGapChance, _config.CurrentGapChance);
        }

        [Test]
        public void UpdateDifficulty_DistanceAtStart_ReturnsZeroDifficulty()
        {
            // Arrange
            _config.startDistance = 50f;

            // Act
            _config.UpdateDifficulty(50f);

            // Assert
            Assert.AreEqual(0f, _config.CurrentDifficulty, 0.01f);
        }

        #endregion

        #region 难度计算测试 - 线性曲线

        [Test]
        public void UpdateDifficulty_LinearCurve_MidpointReturnsHalfDifficulty()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;
            _config.curveType = CurveType.Linear;

            // Act
            _config.UpdateDifficulty(50f);

            // Assert
            Assert.AreEqual(0.5f, _config.CurrentDifficulty, 0.01f);
        }

        [Test]
        public void UpdateDifficulty_LinearCurve_MaxDistanceReturnsFullDifficulty()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;
            _config.curveType = CurveType.Linear;

            // Act
            _config.UpdateDifficulty(100f);

            // Assert
            Assert.AreEqual(1f, _config.CurrentDifficulty, 0.01f);
        }

        [Test]
        public void UpdateDifficulty_LinearCurve_BeyondMaxDistance_ReturnsMaxDifficulty()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;
            _config.curveType = CurveType.Linear;

            // Act
            _config.UpdateDifficulty(1000f);

            // Assert
            Assert.AreEqual(1f, _config.CurrentDifficulty, 0.01f);
        }

        #endregion

        #region 难度计算测试 - 指数曲线

        [Test]
        public void UpdateDifficulty_ExponentialCurve_ReturnsSquaredProgress()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;
            _config.curveType = CurveType.Exponential;

            // Act
            _config.UpdateDifficulty(50f); // progress = 0.5

            // Assert: 0.5^2 = 0.25
            Assert.AreEqual(0.25f, _config.CurrentDifficulty, 0.01f);
        }

        [Test]
        public void UpdateDifficulty_ExponentialCurve_LowerEarlyProgress()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;
            _config.curveType = CurveType.Exponential;

            // Act
            _config.UpdateDifficulty(25f); // progress = 0.25

            // Assert: 0.25^2 = 0.0625
            Assert.AreEqual(0.0625f, _config.CurrentDifficulty, 0.01f);
        }

        #endregion

        #region 难度计算测试 - 对数曲线

        [Test]
        public void UpdateDifficulty_LogarithmicCurve_ReturnsFasterEarlyProgress()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;
            _config.curveType = CurveType.Logarithmic;

            // Act
            _config.UpdateDifficulty(50f); // progress = 0.5

            // Assert: log2(0.5 + 1) = log2(1.5) ≈ 0.585
            Assert.AreEqual(0.585f, _config.CurrentDifficulty, 0.02f);
        }

        #endregion

        #region 概率计算测试

        [Test]
        public void UpdateDifficulty_FullDifficulty_ReturnsMaxProbabilities()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;

            // Act
            _config.UpdateDifficulty(100f);

            // Assert
            Assert.AreEqual(_config.maxObstacleChance, _config.CurrentObstacleChance, 0.01f);
            Assert.AreEqual(_config.maxGapChance, _config.CurrentGapChance, 0.01f);
        }

        [Test]
        public void UpdateDifficulty_HalfDifficulty_ReturnsInterpolatedProbabilities()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;
            _config.baseObstacleChance = 0.3f;
            _config.maxObstacleChance = 0.6f;

            // Act
            _config.UpdateDifficulty(50f); // 0.5 difficulty

            // Assert: 0.3 + (0.6 - 0.3) * 0.5 = 0.45
            Assert.AreEqual(0.45f, _config.CurrentObstacleChance, 0.01f);
        }

        #endregion

        #region Reset 测试

        [Test]
        public void ResetDifficulty_ResetsToBaseValues()
        {
            // Arrange
            _config.startDistance = 0f;
            _config.maxDistance = 100f;

            // Act
            _config.UpdateDifficulty(100f); // 先设置为最大难度
            _config.ResetDifficulty();

            // Assert
            Assert.AreEqual(0f, _config.CurrentDifficulty);
            Assert.AreEqual(_config.baseObstacleChance, _config.CurrentObstacleChance);
            Assert.AreEqual(_config.baseGapChance, _config.CurrentGapChance);
        }

        #endregion

        #region 自定义曲线测试

        [Test]
        public void UpdateDifficulty_CustomCurve_UsesAnimationCurve()
        {
            // Arrange
            _config.curveType = CurveType.Custom;
            _config.customCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 0.8f), // 中点时难度为 0.8
                new Keyframe(1f, 1f)
            );
            _config.startDistance = 0f;
            _config.maxDistance = 100f;

            // Act
            _config.UpdateDifficulty(50f); // progress = 0.5

            // Assert
            Assert.AreEqual(0.8f, _config.CurrentDifficulty, 0.01f);
        }

        #endregion
    }
}
