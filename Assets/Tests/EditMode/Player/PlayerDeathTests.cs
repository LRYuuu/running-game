using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SquareFireline.Player.Tests
{
    /// <summary>
    /// 玩家死亡系统单元测试 - EditMode
    /// </summary>
    public class PlayerDeathTests
    {
        private GameObject _playerObject;
        private PlayerDeathController _deathController;
        private PlayerJumpController _jumpController;
        private Rigidbody2D _rigidbody2D;

        [SetUp]
        public void Setup()
        {
            // 创建测试玩家
            _playerObject = new GameObject("TestPlayer");
            _deathController = _playerObject.AddComponent<PlayerDeathController>();
            _jumpController = _playerObject.AddComponent<PlayerJumpController>();
            _rigidbody2D = _playerObject.AddComponent<Rigidbody2D>();

            // 冻结旋转
            _rigidbody2D.freezeRotation = true;
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
        }

        #region 死亡状态测试

        [Test]
        public void IsDead_ReturnsFalse_Initially()
        {
            // 初始状态不应死亡
            Assert.IsFalse(_deathController.IsDead(), "初始状态不应死亡");
        }

        [Test]
        public void Die_SetsDeadState()
        {
            // 调用 Die 后应该标记为死亡
            _deathController.Die();
            Assert.IsTrue(_deathController.IsDead(), "调用 Die() 后应该标记为死亡");
        }

        [Test]
        public void Die_WhenAlreadyDead_DoesNothing()
        {
            // 第一次死亡
            _deathController.Die();
            float firstDeadTime = Time.time;

            // 等待一小段时间
            System.Threading.Thread.Sleep(10);

            // 第二次调用 Die 不应改变状态
            _deathController.Die();
            Assert.IsTrue(_deathController.IsDead(), "已死亡后再次调用 Die() 应保持死亡状态");
        }

        #endregion

        #region 重生测试

        [Test]
        public void Respawn_ResetsDeadState()
        {
            // 先死亡
            _deathController.Die();
            Assert.IsTrue(_deathController.IsDead(), "调用 Die() 后应该标记为死亡");

            // 立即重生（测试用）
            _deathController.RespawnImmediately();
            Assert.IsFalse(_deathController.IsDead(), "重生后应该重置死亡状态");
        }

        [Test]
        public void Respawn_ResetsPlayerPosition()
        {
            // 记录初始位置
            Vector3 initialPosition = _playerObject.transform.position;

            // 移动玩家
            _playerObject.transform.position = new Vector3(100, 100, 0);

            // 死亡并重生
            _deathController.Die();
            _deathController.RespawnImmediately();

            // 验证位置已重置
            Assert.AreEqual(initialPosition, _playerObject.transform.position, "重生后玩家应回到安全位置");
        }

        [Test]
        public void Respawn_ResetsJumpState()
        {
            // 死亡并重生
            _deathController.Die();
            _deathController.RespawnImmediately();

            // 验证跳跃状态已重置（通过检查可以再次跳跃来间接验证）
            Assert.IsTrue(_jumpController.CanDoubleJump() || _jumpController.IsGrounded(),
                "重生后跳跃状态应该重置");
        }

        [Test]
        public void Respawn_ResetsVelocity()
        {
            // 给玩家一个速度
            _rigidbody2D.velocity = new Vector2(10, 5);

            // 死亡并重生
            _deathController.Die();
            _deathController.RespawnImmediately();

            // 验证速度已重置
            Assert.AreEqual(Vector2.zero, _rigidbody2D.velocity, "重生后速度应该重置为零");
        }

        #endregion

        #region 安全位置测试

        [Test]
        public void SetSafePosition_UpdatesRespawnLocation()
        {
            // 设置新的安全位置
            Vector3 newSafePos = new Vector3(50, 10, 0);
            _deathController.SetSafePosition(newSafePos);

            // 死亡并重生
            _deathController.Die();
            _deathController.RespawnImmediately();

            // 验证玩家重生到新位置
            Assert.AreEqual(newSafePos, _playerObject.transform.position, "玩家应该重生到设置的安全位置");
        }

        #endregion

        #region 死亡事件测试

        [Test]
        public void Die_TriggersDeathEvent()
        {
            bool eventTriggered = false;
            _deathController.OnPlayerDied += () => eventTriggered = true;

            _deathController.Die();

            Assert.IsTrue(eventTriggered, "死亡时应该触发 OnPlayerDied 事件");
        }

        #endregion
    }
}
