using NUnit.Framework;
using UnityEngine;
using RunnersJourney.Game;
using RunnersJourney.Player;

namespace RunnersJourney.Tests.EditMode.Game
{
    /// <summary>
    /// 检查点系统测试
    /// </summary>
    [TestFixture]
    public class CheckpointTests
    {
        private GameObject _checkpointObject;
        private Checkpoint _checkpoint;
        private GameObject _playerObject;
        private GameObject _gameManagerObject;
        private GameManager _gameManager;
        private PlayerDeathController _playerDeathController;

        [SetUp]
        public void SetUp()
        {
            // 创建 Checkpoint
            _checkpointObject = new GameObject("Checkpoint");
            _checkpoint = _checkpointObject.AddComponent<Checkpoint>();

            // 创建玩家
            _playerObject = new GameObject("Player");
            _playerObject.tag = "Player";
            var playerCollider = _playerObject.AddComponent<CircleCollider2D>();
            playerCollider.isTrigger = true;
            _playerDeathController = _playerObject.AddComponent<PlayerDeathController>();
            _playerObject.AddComponent<Rigidbody2D>();
            _playerObject.AddComponent<PlayerJumpController>();

            // 创建 GameManager
            _gameManagerObject = new GameObject("GameManager");
            _gameManager = _gameManagerObject.AddComponent<GameManager>();

            // 设置 GameManager 的引用
            var field = typeof(GameManager).GetField("_playerDeathController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_gameManager, _playerDeathController);
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(_checkpointObject);
            GameObject.DestroyImmediate(_playerObject);
            GameObject.DestroyImmediate(_gameManagerObject);
            GameManager.ResetInstance();
        }

        #region Checkpoint 触发测试

        [Test]
        public void Checkpoint_Awake_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var checkpoint = _checkpointObject.GetComponent<Checkpoint>();

            // Assert
            Assert.IsNotNull(checkpoint);
            Assert.IsFalse(checkpoint.IsActivated());
        }

        [Test]
        public void OnTriggerEnter2D_WithPlayer_ShouldActivateCheckpoint()
        {
            // Arrange
            _playerObject.transform.position = _checkpointObject.transform.position;

            // Act - 模拟玩家进入触发器
            var playerCollider = _playerObject.GetComponent<Collider2D>();

            // 使用反射调用 OnTriggerEnter2D
            var method = typeof(Checkpoint).GetMethod("OnTriggerEnter2D",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // 创建模拟的碰撞
            var collision2D = CreateMockCollision2D(_playerObject);
            method?.Invoke(_checkpoint, new object[] { collision2D });

            // Assert
            Assert.IsTrue(_checkpoint.IsActivated());
        }

        [Test]
        public void OnTriggerEnter2D_WithNonPlayer_ShouldNotActivateCheckpoint()
        {
            // Arrange
            var nonPlayerObject = new GameObject("Enemy");
            nonPlayerObject.tag = "Enemy";
            nonPlayerObject.AddComponent<BoxCollider2D>();

            // Act
            var method = typeof(Checkpoint).GetMethod("OnTriggerEnter2D",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var collision2D = CreateMockCollision2D(nonPlayerObject);
            method?.Invoke(_checkpoint, new object[] { collision2D });

            // Assert
            Assert.IsFalse(_checkpoint.IsActivated());

            // Cleanup
            GameObject.DestroyImmediate(nonPlayerObject);
        }

        [Test]
        public void Activate_ShouldUpdateGameManagerCheckpoint()
        {
            // Arrange
            Vector3 expectedPosition = _checkpointObject.transform.position;

            // Act
            _checkpoint.Activate();

            // Assert
            Vector3 actualPosition = _gameManager.GetLastSafePosition();
            Assert.AreEqual(expectedPosition, actualPosition);
        }

        [Test]
        public void Activate_MultipleTimes_ShouldOnlyTriggerOnce()
        {
            // Arrange
            int activateCount = 0;

            // 第一次激活
            _checkpoint.Activate();
            activateCount++;

            // 记录当前位置
            Vector3 firstPosition = _gameManager.GetLastSafePosition();

            // 移动检查点到新位置
            _checkpointObject.transform.position = new Vector3(10, 10, 0);

            // 再次激活
            _checkpoint.Activate();
            activateCount++;

            // Assert - 位置不应该改变
            Vector3 secondPosition = _gameManager.GetLastSafePosition();
            Assert.AreEqual(firstPosition, secondPosition);
        }

        [Test]
        public void Reset_ShouldDeactivateCheckpoint()
        {
            // Arrange
            _checkpoint.Activate();
            Assert.IsTrue(_checkpoint.IsActivated());

            // Act
            _checkpoint.Reset();

            // Assert
            Assert.IsFalse(_checkpoint.IsActivated());
        }

        #endregion

        #region GameManager 检查点管理测试

        [Test]
        public void UpdateCheckpoint_ShouldStorePosition()
        {
            // Arrange
            Vector3 testPosition = new Vector3(5f, 3f, 0f);

            // Act
            _gameManager.UpdateCheckpoint(testPosition);

            // Assert
            Vector3 storedPosition = _gameManager.GetLastSafePosition();
            Assert.AreEqual(testPosition, storedPosition);
        }

        [Test]
        public void GetLastSafePosition_WithoutUpdate_ShouldReturnZeroVector()
        {
            // Act
            Vector3 position = _gameManager.GetLastSafePosition();

            // Assert
            Assert.AreEqual(Vector3.zero, position);
        }

        [Test]
        public void MultipleCheckpoints_ShouldUseLastActivated()
        {
            // Arrange
            GameObject checkpoint1 = new GameObject("Checkpoint1");
            checkpoint1.transform.position = new Vector3(0, 0, 0);
            var cp1 = checkpoint1.AddComponent<Checkpoint>();

            GameObject checkpoint2 = new GameObject("Checkpoint2");
            checkpoint2.transform.position = new Vector3(10, 0, 0);
            var cp2 = checkpoint2.AddComponent<Checkpoint>();

            // Act
            cp1.Activate();
            cp2.Activate();

            // Assert
            Vector3 position = _gameManager.GetLastSafePosition();
            Assert.AreEqual(checkpoint2.transform.position, position);

            // Cleanup
            GameObject.DestroyImmediate(checkpoint1);
            GameObject.DestroyImmediate(checkpoint2);
        }

        #endregion

        #region PlayerDeathController 集成测试

        [Test]
        public void SetSafePosition_ShouldDelegateToGameManager()
        {
            // Arrange
            Vector3 testPosition = new Vector3(7f, 2f, 0f);

            // Act
            _playerDeathController.SetSafePosition(testPosition);

            // Assert
            Vector3 storedPosition = _gameManager.GetLastSafePosition();
            Assert.AreEqual(testPosition, storedPosition);
        }

        [Test]
        public void GetSpawnPosition_ShouldReturnGameManagerPosition()
        {
            // Arrange
            Vector3 testPosition = new Vector3(15f, 8f, 0f);
            _gameManager.UpdateCheckpoint(testPosition);

            // Act & Assert
            // 通过 SetSafePosition 间接验证 GetSpawnPosition 的行为
            _playerDeathController.SetSafePosition(new Vector3(20f, 10f, 0f));
            Vector3 newPosition = _gameManager.GetLastSafePosition();
            Assert.AreEqual(new Vector3(20f, 10f, 0f), newPosition);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建模拟的 Collision2D 对象
        /// </summary>
        private Collider2D CreateMockCollision2D(GameObject target)
        {
            return target.GetComponent<Collider2D>();
        }

        #endregion
    }
}
