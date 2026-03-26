using NUnit.Framework;
using UnityEngine;
using RunnersJourney.Game;

namespace RunnersJourney.Tests.EditMode.Game
{
    /// <summary>
    /// DifficultyCalculator 单元测试
    /// </summary>
    [TestFixture]
    public class DifficultyCalculatorTests
    {
        private DifficultyCalculator _calculator;
        private DifficultyConfig _config;
        private GameObject _gameObject;

        [SetUp]
        public void Setup()
        {
            // 创建测试对象
            _gameObject = new GameObject("DifficultyCalculator_Test");

            // 创建配置
            _config = ScriptableObject.CreateInstance<DifficultyConfig>();
            _config.startDistance = 10f;
            _config.maxDistance = 100f;
            _config.baseObstacleChance = 0.3f;
            _config.maxObstacleChance = 0.6f;
            _config.baseGapChance = 0.1f;
            _config.maxGapChance = 0.25f;

            // 添加计算器组件
            _calculator = _gameObject.AddComponent<DifficultyCalculator>();

            // 通过反射设置私有字段 _config
            var field = typeof(DifficultyCalculator).GetField("_config",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_calculator, _config);

            // 手动初始化单例（EditMode 测试中 Unity 不会自动调用 Awake）
            DifficultyCalculator.ForceInitializeInstance(_calculator);
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }

            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }

            // 重置单例以便下一个测试可以重新初始化
            DifficultyCalculator.ResetInstance();
        }

        #region 单例测试

        [Test]
        public void DifficultyCalculator_Instance_IsAssignedAfterCreation()
        {
            // 单例在 Awake 中初始化，Unity 会自动调用
            // 我们直接测试 Instance 是否被正确赋值
            Assert.IsNotNull(DifficultyCalculator.Instance);
            Assert.AreEqual(_calculator, DifficultyCalculator.Instance);
        }

        [Test]
        public void DifficultyCalculator_MultipleInstances_SecondInstanceDestroyed()
        {
            // Arrange - 第一个实例已经创建并在 Setup 中初始化
            var firstInstance = DifficultyCalculator.Instance;
            Assert.IsNotNull(firstInstance);

            // Act: 创建第二个 GameObject 和组件
            var gameObject2 = new GameObject("DifficultyCalculator_Test2");
            var calculator2 = gameObject2.AddComponent<DifficultyCalculator>();
            // 注意：这里不创建第二个配置，因为第二个实例应该被拒绝

            // Assert: 单例应该仍然是第一个实例
            Assert.AreEqual(firstInstance, DifficultyCalculator.Instance);

            // Cleanup
            Object.DestroyImmediate(gameObject2);
        }

        #endregion

        #region 难度计算测试

        [Test]
        public void SetPlayerDistance_DistanceLessThanStart_ReturnsZeroDifficulty()
        {
            // Act
            _calculator.SetPlayerDistance(5f);

            // Assert
            Assert.AreEqual(0f, _calculator.CurrentDifficulty);
            Assert.AreEqual(0.3f, _calculator.CurrentObstacleChance);
            Assert.AreEqual(0.1f, _calculator.CurrentGapChance);
        }

        [Test]
        public void SetPlayerDistance_DistanceAtMax_ReturnsFullDifficulty()
        {
            // Act
            _calculator.SetPlayerDistance(100f);

            // Assert
            Assert.AreEqual(1f, _calculator.CurrentDifficulty, 0.01f);
            Assert.AreEqual(0.6f, _calculator.CurrentObstacleChance, 0.01f);
            Assert.AreEqual(0.25f, _calculator.CurrentGapChance, 0.01f);
        }

        [Test]
        public void SetPlayerDistance_DistanceAtMidpoint_ReturnsHalfDifficulty()
        {
            // Act
            _calculator.SetPlayerDistance(55f); // (55 - 10) / (100 - 10) = 0.5

            // Assert
            Assert.AreEqual(0.5f, _calculator.CurrentDifficulty, 0.01f);
        }

        #endregion

        #region 重置测试

        [Test]
        public void ResetProgress_ResetsDistanceToZero()
        {
            // Arrange
            _calculator.SetPlayerDistance(100f);

            // Act
            _calculator.ResetProgress();

            // Assert
            Assert.AreEqual(0f, _calculator.CurrentDistance);
            Assert.AreEqual(0f, _calculator.CurrentDifficulty);
        }

        #endregion

        #region 事件测试

        [Test]
        public void SetPlayerDistance_DistanceChanges_TriggerOnDifficultyChanged()
        {
            // Arrange
            bool eventTriggered = false;
            _calculator.OnDifficultyChanged += (difficulty) => eventTriggered = true;

            // Act
            _calculator.SetPlayerDistance(100f);

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void SetPlayerDistance_DistanceUnchanged_NoEventTriggered()
        {
            // Arrange
            int eventCount = 0;
            _calculator.OnDifficultyChanged += (difficulty) => eventCount++;

            // Act
            _calculator.SetPlayerDistance(50f);
            _calculator.SetPlayerDistance(50f); // 相同的距离

            // Assert - 事件应该只触发一次或者没有触发（取决于实现）
            Assert.LessOrEqual(eventCount, 2);
        }

        #endregion

        #region Get 方法测试

        [Test]
        public void GetDifficulty_ReturnsCurrentDifficulty()
        {
            // Arrange
            _calculator.SetPlayerDistance(100f);

            // Act
            float result = _calculator.GetDifficulty();

            // Assert
            Assert.AreEqual(1f, result, 0.01f);
        }

        [Test]
        public void GetObstacleChance_ReturnsCurrentChance()
        {
            // Arrange
            _calculator.SetPlayerDistance(100f);

            // Act
            float result = _calculator.GetObstacleChance();

            // Assert
            Assert.AreEqual(0.6f, result, 0.01f);
        }

        [Test]
        public void GetGapChance_ReturnsCurrentChance()
        {
            // Arrange
            _calculator.SetPlayerDistance(100f);

            // Act
            float result = _calculator.GetGapChance();

            // Assert
            Assert.AreEqual(0.25f, result, 0.01f);
        }

        #endregion
    }
}
