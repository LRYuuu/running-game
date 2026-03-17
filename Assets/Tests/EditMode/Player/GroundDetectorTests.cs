using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.TestTools;

namespace SquareFireline.Player.Tests
{
    /// <summary>
    /// GroundDetector 单元测试 - EditMode
    /// </summary>
    public class GroundDetectorTests
    {
        private GameObject _playerObject;
        private GameObject _groundObject;
        private GroundDetector _groundDetector;
        private JumpConfig _jumpConfig;

        [SetUp]
        public void Setup()
        {
            // 加载 JumpConfig
            _jumpConfig = Resources.Load<JumpConfig>("Player/JumpConfig");
            Assert.IsNotNull(_jumpConfig, "JumpConfig 应该存在");

            // 创建测试玩家 GameObject
            _playerObject = new GameObject("TestPlayer");
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            // 添加玩家碰撞体（与 GroundDetector 的 groundCheckOffset 配合）
            var playerCollider = _playerObject.AddComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(1f, 1f);

            // 添加地面检测器
            _groundDetector = _playerObject.AddComponent<GroundDetector>();

            // 使用反射设置私有字段（因为 Awake 已经运行）
            var field = typeof(GroundDetector).GetField("_jumpConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "Field _jumpConfig should exist");
            field.SetValue(_groundDetector, _jumpConfig);

            // 创建测试地面
            _groundObject = new GameObject("TestGround");
            _groundObject.transform.position = new Vector3(0f, 0f, 0f);
            var groundCollider = _groundObject.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(100f, 1f); // 确保地面足够大
            _groundObject.layer = LayerMask.NameToLayer("Ground");
        }

        [TearDown]
        public void TearDown()
        {
            // 清理测试对象
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_groundObject != null)
                Object.DestroyImmediate(_groundObject);
        }

        #region 地面检测测试
        [Test]
        public void TestIsGrounded_OnGround_ReturnsTrue()
        {
            // Arrange: 将玩家放置在地面上方很近的位置
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            // Act
            var isGrounded = _groundDetector.IsGrounded();

            // Assert
            Assert.IsTrue(isGrounded, "玩家在地面上方时应该检测到地面");
        }

        [Test]
        public void TestIsGrounded_InAir_ReturnsFalse()
        {
            // Arrange: 将玩家放置在高空
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            // Act
            var isGrounded = _groundDetector.IsGrounded();

            // Assert
            Assert.IsFalse(isGrounded, "玩家在空中时应该检测到没有地面");
        }

        [Test]
        public void TestIsGrounded_EdgeOfGround_ReturnsFalse()
        {
            // Arrange: 将玩家放置在地面边缘外侧
            // 地面大小为 (100f, 1f), 中心在 (0, 0), 所以边缘在 x=±50
            _playerObject.transform.position = new Vector3(60f, 0.6f, 0f);

            // Act
            var isGrounded = _groundDetector.IsGrounded();

            // Assert
            Assert.IsFalse(isGrounded, "玩家在地面边缘外侧时应该检测到没有地面");
        }

        [Test]
        public void TestIsGrounded_MultipleGroundLayers_ReturnsTrue()
        {
            // Arrange: 创建多个地面层
            var groundObject2 = new GameObject("TestGround2");
            groundObject2.transform.position = new Vector3(5f, 0f, 0f);
            groundObject2.AddComponent<BoxCollider2D>();
            groundObject2.layer = LayerMask.NameToLayer("Ground");

            _playerObject.transform.position = new Vector3(5f, 0.6f, 0f);

            // Act
            var isGrounded = _groundDetector.IsGrounded();

            // Assert
            Assert.IsTrue(isGrounded, "玩家在任何地面层上方时都应该检测到地面");

            // Cleanup
            Object.DestroyImmediate(groundObject2);
        }
        #endregion

        #region Raycast 检测测试
        [Test]
        public void TestIsGroundedWithRaycast_OnGround_ReturnsTrue()
        {
            // Arrange: 将玩家放置在地面上方很近的位置（Raycast 距离为 0.1f）
            // 玩家位置 y=0.55f，从中心向下 0.1f 刚好能检测到地面 y=0f
            _playerObject.transform.position = new Vector3(0f, 0.55f, 0f);

            // Act
            var isGrounded = _groundDetector.IsGroundedWithRaycast();

            // Assert
            Assert.IsTrue(isGrounded, "Raycast 检测应该检测到地面");
        }

        [Test]
        public void TestIsGroundedWithRaycast_InAir_ReturnsFalse()
        {
            // Arrange: 将玩家放置在高空
            _playerObject.transform.position = new Vector3(0f, 5f, 0f);

            // Act
            var isGrounded = _groundDetector.IsGroundedWithRaycast();

            // Assert
            Assert.IsFalse(isGrounded, "Raycast 检测应该检测到没有地面");
        }
        #endregion

        #region 配置依赖测试
        [Test]
        public void TestGroundDetector_WithJumpConfig_UsesCorrectLayer()
        {
            // Arrange
            Assert.IsNotNull(_jumpConfig, "JumpConfig 应该存在");

            // Act: 修改 GroundDetector 的配置（通过序列化字段）
            // 注意：实际测试中，JumpConfig 的 groundLayer 应该正确设置

            // Assert: 验证地面检测使用了正确的层
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);
            var isGrounded = _groundDetector.IsGrounded();
            Assert.IsTrue(isGrounded, "应该使用 JumpConfig 中的层设置检测到地面");
        }
        #endregion
    }
}
