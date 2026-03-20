using NUnit.Framework;
using UnityEngine;
using SquareFireline.Game;

namespace SquareFireline.Tests.EditMode.Game
{
    [TestFixture]
    public class ScoreConfigTests
    {
        private ScoreConfig _scoreConfig;

        [SetUp]
        public void Setup()
        {
            _scoreConfig = ScriptableObject.CreateInstance<ScoreConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_scoreConfig != null)
            {
                Object.DestroyImmediate(_scoreConfig);
            }
        }

        [Test]
        public void ScoreConfig_CreateAsset_HasDefaultScorePerSecond()
        {
            // Assert
            Assert.AreEqual(1, _scoreConfig.scorePerSecond);
        }

        [Test]
        public void ScoreConfig_CreateAsset_HasDefaultScoreInterval()
        {
            // Assert
            Assert.AreEqual(1f, _scoreConfig.scoreInterval);
        }

        [Test]
        public void ScoreConfig_ModifyValues_UpdatesCorrectly()
        {
            // Act
            _scoreConfig.scorePerSecond = 2;
            _scoreConfig.scoreInterval = 0.5f;

            // Assert
            Assert.AreEqual(2, _scoreConfig.scorePerSecond);
            Assert.AreEqual(0.5f, _scoreConfig.scoreInterval);
        }
    }
}
