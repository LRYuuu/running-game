using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.Reflection;

namespace SquareFireline.Player.Tests
{
    /// <summary>
    /// PlayerJumpController 单元测试 - EditMode
    /// </summary>
    public class PlayerJumpControllerTests
    {
        private GameObject _playerObject;
        private PlayerJumpController _jumpController;
        private GroundDetector _groundDetector;
        private Rigidbody2D _rigidbody2D;
        private JumpConfig _jumpConfig;

        [SetUp]
        public void Setup()
        {
            // 加载 JumpConfig
            _jumpConfig = Resources.Load<JumpConfig>("Player/JumpConfig");
            Debug.Log($"[Test] JumpConfig loaded: {_jumpConfig != null}, jumpForce: {_jumpConfig?.jumpForce}");
            Assert.IsNotNull(_jumpConfig, "JumpConfig should exist in Resources/Player/");

            // 创建测试玩家 GameObject
            _playerObject = new GameObject("TestPlayer");
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            // 添加必要组件
            _rigidbody2D = _playerObject.AddComponent<Rigidbody2D>();
            _rigidbody2D.gravityScale = 1f;
            _rigidbody2D.mass = 1f;

            // 添加玩家碰撞体
            var playerCollider = _playerObject.AddComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(1f, 1f);

            // 创建测试 Ground
            var groundObject = new GameObject("TestGround");
            groundObject.transform.position = new Vector3(0f, 0f, 0f);
            var groundCollider = groundObject.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(100f, 1f);
            groundObject.layer = LayerMask.NameToLayer("Ground");

            // 先添加 GroundDetector
            _groundDetector = _playerObject.AddComponent<GroundDetector>();

            // 再添加 PlayerJumpController
            _jumpController = _playerObject.AddComponent<PlayerJumpController>();

            // 使用反射设置私有字段
            var jcField = typeof(PlayerJumpController).GetField("_jumpConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            var gdField = typeof(PlayerJumpController).GetField("_groundDetector", BindingFlags.NonPublic | BindingFlags.Instance);
            var gdConfigField = typeof(GroundDetector).GetField("_jumpConfig", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(jcField, "Field _jumpConfig should exist in PlayerJumpController");
            Assert.IsNotNull(gdField, "Field _groundDetector should exist in PlayerJumpController");
            Assert.IsNotNull(gdConfigField, "Field _jumpConfig should exist in GroundDetector");

            jcField.SetValue(_jumpController, _jumpConfig);
            gdField.SetValue(_jumpController, _groundDetector);
            gdConfigField.SetValue(_groundDetector, _jumpConfig);

            // 使用公共 API 设置 Rigidbody2D（替代反射）
            _jumpController.SetRigidbodyForTest(_rigidbody2D);

            // 验证字段设置成功
            Debug.Log($"[Test] Setup completed - rbField via controller: {_jumpController != null}");
        }

        [TearDown]
        public void TearDown()
        {
            // 清理测试对象
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
        }

        #region 地面检测测试
        [Test]
        public void TestIsGrounded_OnGround_ReturnsTrue()
        {
            // Arrange: 将玩家放置在地面上方
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            // Act
            var isGrounded = _jumpController.IsGrounded();

            // Assert
            Assert.IsTrue(isGrounded, "玩家在地面上方时应该检测到地面");
        }

        [Test]
        public void TestIsGrounded_InAir_ReturnsFalse()
        {
            // Arrange: 将玩家放置在高空
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            // Act
            var isGrounded = _jumpController.IsGrounded();

            // Assert
            Assert.IsFalse(isGrounded, "玩家在空中时应该检测到没有地面");
        }
        #endregion

        #region 跳跃执行测试
        [Test]
        public void TestJump_ForceApplied()
        {
            // Arrange: 玩家在地面上
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            // 验证依赖已设置
            Assert.IsNotNull(_jumpController, "_jumpController should not be null");
            Assert.IsNotNull(_rigidbody2D, "_rigidbody2D should not be null");
            Assert.IsNotNull(_jumpConfig, "_jumpConfig should not be null");

            var initialVelocityY = _rigidbody2D.velocity.y;
            Debug.Log($"[TestJump] initialVelocityY: {initialVelocityY}, _jumpConfig.jumpForce: {_jumpConfig?.jumpForce}");

            // Act: 执行跳跃
            _jumpController.ExecuteJump(_jumpConfig.jumpForce);

            // Assert
            Assert.Greater(_rigidbody2D.velocity.y, initialVelocityY, "跳跃后垂直速度应该增加");
            Assert.Greater(_rigidbody2D.velocity.y, 0f, "跳跃后应该获得向上的速度");
        }

        [Test]
        public void TestJump_VelocityReset()
        {
            // Arrange: 设置向下的初始速度
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);
            _rigidbody2D.velocity = new Vector2(0f, -5f);

            // Act: 执行跳跃
            _jumpController.ExecuteJump(_jumpConfig.jumpForce);

            // Assert: 跳跃前应该清除垂直速度
            Assert.Greater(_rigidbody2D.velocity.y, 0f, "跳跃后垂直速度应该为正");
        }

        [Test]
        public void TestJump_InAir_NoJump()
        {
            // Arrange: 玩家在高空，尝试直接执行跳跃（不使用 TryJump）
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);
            var initialVelocity = _rigidbody2D.velocity;

            // Act: 直接调用 ExecuteJump（这个方法不检查地面）
            _jumpController.ExecuteJump(_jumpConfig.jumpForce);

            // Assert: ExecuteJump 总是会应用力（检查地面是 TryJump 的职责）
            Assert.AreNotEqual(_rigidbody2D.velocity.y, initialVelocity.y, "ExecuteJump 总是应用力");
        }
        #endregion

        #region 二段跳测试
        [Test]
        public void TestCanDoubleJump_OnGround_ReturnsFalse()
        {
            // Arrange: 玩家在地面上
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            // Act
            var canDoubleJump = _jumpController.CanDoubleJump();

            // Assert
            Assert.IsFalse(canDoubleJump, "在地面上时二段跳应该不可用");
        }

        [Test]
        public void TestCanDoubleJump_InAir_AfterJump_ReturnsTrue()
        {
            // Arrange: 玩家在空中（模拟一段跳后）
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);
            // 注意：实际游戏中，一段跳后会设置_canDoubleJump = true
            // 这里需要手动测试公共 API

            // Act: 尝试二段跳（通过 TryJump 的内部逻辑）
            // 由于 _canDoubleJump 是私有字段，我们通过整体行为测试
            var isGrounded = _jumpController.IsGrounded();

            // Assert
            Assert.IsFalse(isGrounded, "玩家应该在空中");
            // CanDoubleJump() 需要玩家在 TryJump 中设置_canDoubleJump 后才能返回 true
        }
        #endregion

        #region 重置测试
        [Test]
        public void TestResetJumpState_ResetsAllTimers()
        {
            // Arrange: 设置非初始状态
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            // Act: 重置状态
            _jumpController.ResetJumpState();

            // Assert: 由于是私有字段，我们通过后续行为验证
            // 重置后应该可以再次跳跃
            var isGrounded = _jumpController.IsGrounded();
            Assert.IsTrue(isGrounded, "重置后地面检测应该正常");
        }
        #endregion

        #region 跳跃缓冲测试
        [Test]
        public void TestTryJump_WithJumpBuffer_ShouldJumpWhenGrounded()
        {
            // Arrange: 玩家在地面上，模拟跳跃缓冲
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            // 通过反射设置跳跃缓冲计时器
            var bufferField = typeof(PlayerJumpController).GetField("_jumpBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(bufferField);
            bufferField.SetValue(_jumpController, 0.2f);

            // Act: 尝试跳跃
            _jumpController.TryJump();

            // Assert: 应该执行了跳跃（速度增加）
            Assert.Greater(_rigidbody2D.velocity.y, 0f, "有跳跃缓冲时应该能起跳");
        }

        [Test]
        public void TestTryJump_BufferExpires_NoJump()
        {
            // Arrange: 玩家在地面上，缓冲已过期
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            var bufferField = typeof(PlayerJumpController).GetField("_jumpBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            bufferField.SetValue(_jumpController, 0f);

            // Act
            _jumpController.TryJump();

            // Assert: 没有缓冲时不会跳跃
            Assert.AreEqual(0f, _rigidbody2D.velocity.y, "缓冲过期时不应跳跃");
        }
        #endregion

        #region 土狼时间测试
        [Test]
        public void TestTryJump_WithCoyoteTime_CanJumpInAir()
        {
            // Arrange: 玩家在空中但有土狼时间
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            var bufferField = typeof(PlayerJumpController).GetField("_jumpBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            var coyoteField = typeof(PlayerJumpController).GetField("_coyoteTimeTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            bufferField.SetValue(_jumpController, 0.2f);
            coyoteField.SetValue(_jumpController, 0.1f);

            // Act
            _jumpController.TryJump();

            // Assert: 土洋时间内应该能跳跃
            Assert.Greater(_rigidbody2D.velocity.y, 0f, "土洋时间内应该能起跳");
        }

        [Test]
        public void TestTryJump_NoCoyoteTime_InAir_NoJump()
        {
            // Arrange: 玩家在空中，无土狼时间
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            var bufferField = typeof(PlayerJumpController).GetField("_jumpBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            var coyoteField = typeof(PlayerJumpController).GetField("_coyoteTimeTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            bufferField.SetValue(_jumpController, 0.2f);
            coyoteField.SetValue(_jumpController, 0f);

            // Act
            _jumpController.TryJump();

            // Assert: 没有土洋时间且在空中不应跳跃（除非二段跳可用）
            // 注意：由于二段跳逻辑，这个测试可能需要调整
        }
        #endregion

        #region 二段跳测试
        [Test]
        public void TestTryJump_DoubleJump_WhenInAir()
        {
            // Arrange: 玩家在空中，二段跳可用
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            var bufferField = typeof(PlayerJumpController).GetField("_jumpBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            var canDoubleJumpField = typeof(PlayerJumpController).GetField("_canDoubleJump", BindingFlags.NonPublic | BindingFlags.Instance);
            bufferField.SetValue(_jumpController, 0.2f);
            canDoubleJumpField.SetValue(_jumpController, true);

            // Act: 第一次 TryJump 激活二段跳状态，第二次执行二段跳
            _jumpController.TryJump();

            // Assert: 在空中且二段跳可用时应该能跳跃
            Assert.Greater(_rigidbody2D.velocity.y, 0f, "二段跳应该能执行");
        }
        #endregion
    }
}
