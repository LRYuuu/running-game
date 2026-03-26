using NUnit.Framework;
using UnityEngine;
using RunnersJourney.Game;
using System.Reflection;

namespace RunnersJourney.Tests.EditMode.Game
{
    [TestFixture]
    public class ScoreManagerTests
    {
        private ScoreManager _scoreManager;
        private GameObject _testObject;

        [SetUp]
        public void Setup()
        {
            // 重置单例
            ScoreManager.ResetInstance();
            GameManager.ResetInstance();

            // 创建测试对象
            _testObject = new GameObject("ScoreManagerTest");
            _scoreManager = _testObject.AddComponent<ScoreManager>();

            // 手动调用 Awake 来初始化单例（EditMode 测试需要）
            var awakeMethod = typeof(ScoreManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awakeMethod?.Invoke(_scoreManager, null);

            // 确保 PlayerPrefs 干净
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testObject != null)
            {
                Object.DestroyImmediate(_testObject);
            }
            // 确保单例被清理（OnDestroy 已经会处理，但为了安全起见）
            ScoreManager.ResetInstance();
            // 清理 PlayerPrefs
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Instance_SingleInstance_ReturnsCorrectInstance()
        {
            // Act
            var instance = ScoreManager.Instance;

            // Assert
            Assert.AreEqual(_scoreManager, instance);
        }

        [Test]
        public void AddScore_ZeroInitialValue_IncreasesScore()
        {
            // Arrange
            int recordedScore = 0;
            _scoreManager.OnScoreChanged += (score, highScore) => recordedScore = score;

            // Act
            _scoreManager.AddScore(10);

            // Assert
            Assert.AreEqual(10, _scoreManager.CurrentScore);
            Assert.AreEqual(10, recordedScore);
        }

        [Test]
        public void ResetScore_NonZeroValue_ResetsToZero()
        {
            // Arrange
            _scoreManager.AddScore(50);
            int recordedScore = -1;
            _scoreManager.OnScoreChanged += (score, highScore) => recordedScore = score;

            // Act
            _scoreManager.ResetScore();

            // Assert
            Assert.AreEqual(0, _scoreManager.CurrentScore);
            Assert.AreEqual(0, recordedScore);
        }

        [Test]
        public void CheckHighScore_NewHighScore_SavesAndReturnsTrue()
        {
            // Arrange
            _scoreManager.AddScore(100);

            // Act
            bool isNewRecord = _scoreManager.CheckHighScore();

            // Assert
            Assert.IsTrue(isNewRecord);
            Assert.AreEqual(100, _scoreManager.HighScore);
        }

        [Test]
        public void CheckHighScore_BelowHighScore_ReturnsFalse()
        {
            // Arrange
            _scoreManager.AddScore(50);
            _scoreManager.CheckHighScore(); // Set high score to 50
            _scoreManager.ResetScore();
            _scoreManager.AddScore(30);

            // Act
            bool isNewRecord = _scoreManager.CheckHighScore();

            // Assert
            Assert.IsFalse(isNewRecord);
            Assert.AreEqual(50, _scoreManager.HighScore);
        }

        [Test]
        public void HighScorePersistence_SaveAndLoad_CorrectValue()
        {
            // Arrange
            const string key = "RunnersJourney_HighScore";
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            _scoreManager.AddScore(200);
            _scoreManager.CheckHighScore();

            // 强制保存
            PlayerPrefs.Save();

            // Act - simulate reload
            Object.DestroyImmediate(_testObject);
            _testObject = null; // Clear reference
            ScoreManager.ResetInstance();

            // Create new instance
            var newObject = new GameObject("ScoreManagerTest2");
            var newScoreManager = newObject.AddComponent<ScoreManager>();

            // 手动调用 Awake 来加载 PlayerPrefs
            var awakeMethod = typeof(ScoreManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awakeMethod?.Invoke(newScoreManager, null);

            // Assert
            Assert.AreEqual(200, newScoreManager.HighScore);

            // Cleanup
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Object.DestroyImmediate(newObject);
        }

        #region Story 4-5 Tests: 死亡时分数重置测试

        [Test]
        public void ResetScore_WhenDyingState_ResetsCurrentScoreToZero()
        {
            // Arrange
            _scoreManager.AddScore(100);
            int recordedCurrentScore = -1;
            _scoreManager.OnScoreChanged += (current, high) => recordedCurrentScore = current;

            // Act
            _scoreManager.ResetScore();

            // Assert
            Assert.AreEqual(0, _scoreManager.CurrentScore);
            Assert.AreEqual(0, recordedCurrentScore);
        }

        [Test]
        public void HighScore_UnaffectedByDeath_RemainsIntact()
        {
            // Arrange
            _scoreManager.AddScore(100);
            _scoreManager.CheckHighScore(); // 保存为最高分
            int expectedHighScore = _scoreManager.HighScore;

            // Act
            _scoreManager.ResetScore();

            // Assert
            Assert.AreEqual(0, _scoreManager.CurrentScore);
            Assert.AreEqual(expectedHighScore, _scoreManager.HighScore);
        }

        [Test]
        public void OnGameStateChanged_ToDying_CallsResetScore()
        {
            // Arrange
            _scoreManager.AddScore(100);
            int recordedCurrentScore = -1;
            _scoreManager.OnScoreChanged += (current, high) => recordedCurrentScore = current;

            // Act - 模拟切换到 Dying 状态
            var onGameStateChangedMethod = typeof(ScoreManager).GetMethod("OnGameStateChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onGameStateChangedMethod?.Invoke(_scoreManager, new object[] { GameState.Playing, GameState.Dying });

            // Assert
            Assert.AreEqual(0, _scoreManager.CurrentScore);
            Assert.AreEqual(0, recordedCurrentScore);
        }

        [Test]
        public void OnGameStateChanged_ToDying_HighScoreUnchanged()
        {
            // Arrange
            _scoreManager.AddScore(100);
            _scoreManager.CheckHighScore(); // 保存为最高分
            int expectedHighScore = _scoreManager.HighScore;

            // Act - 模拟切换到 Dying 状态
            var onGameStateChangedMethod = typeof(ScoreManager).GetMethod("OnGameStateChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onGameStateChangedMethod?.Invoke(_scoreManager, new object[] { GameState.Playing, GameState.Dying });

            // Assert
            Assert.AreEqual(expectedHighScore, _scoreManager.HighScore);
        }

        [Test]
        public void ScoreAfterRespawn_StartsAccumulatingFromZero()
        {
            // Arrange
            _scoreManager.AddScore(50);
            _scoreManager.ResetScore();

            // Act - 模拟重新开始
            _scoreManager.AddScore(10);

            // Assert
            Assert.AreEqual(10, _scoreManager.CurrentScore);
        }

        [Test]
        public void OnGameStateChanged_ToWaiting_ResetsScore()
        {
            // Arrange
            _scoreManager.AddScore(75);
            int recordedCurrentScore = -1;
            _scoreManager.OnScoreChanged += (current, high) => recordedCurrentScore = current;

            // Act - 模拟切换到 Waiting 状态
            var onGameStateChangedMethod = typeof(ScoreManager).GetMethod("OnGameStateChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onGameStateChangedMethod?.Invoke(_scoreManager, new object[] { GameState.Playing, GameState.Waiting });

            // Assert
            Assert.AreEqual(0, _scoreManager.CurrentScore);
            Assert.AreEqual(0, recordedCurrentScore);
        }

        #endregion
    }
}
