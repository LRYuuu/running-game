using NUnit.Framework;
using UnityEngine;
using RunnersJourney.Game;

namespace RunnersJourney.Tests.EditMode.Game
{
    [TestFixture]
    public class GameConfigTests
    {
        private GameConfig _gameConfig;

        [SetUp]
        public void Setup()
        {
            _gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameConfig != null)
            {
                Object.DestroyImmediate(_gameConfig);
            }
        }

        [Test]
        public void GameConfig_CreateAsset_HasDefaultRespawnDelay()
        {
            // Assert
            Assert.AreEqual(1.0f, _gameConfig.respawnDelay);
        }

        [Test]
        public void GameConfig_CreateAsset_HasDefaultInitialState()
        {
            // Assert
            Assert.AreEqual(GameState.Waiting, _gameConfig.initialState);
        }

        [Test]
        public void GameConfig_CreateAsset_HasDefaultCheckpointActivateRadius()
        {
            // Assert
            Assert.AreEqual(0.5f, _gameConfig.checkpointActivateRadius);
        }

        [Test]
        public void GameConfig_ModifyValues_UpdatesCorrectly()
        {
            // Act
            _gameConfig.respawnDelay = 2.0f;
            _gameConfig.initialState = GameState.Playing;
            _gameConfig.checkpointActivateRadius = 1.0f;

            // Assert
            Assert.AreEqual(2.0f, _gameConfig.respawnDelay);
            Assert.AreEqual(GameState.Playing, _gameConfig.initialState);
            Assert.AreEqual(1.0f, _gameConfig.checkpointActivateRadius);
        }
    }
}
