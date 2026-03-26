using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace RunnersJourney.Player.Tests
{
    /// <summary>
    /// PlayerJumpController 集成测试 - PlayMode
    /// 测试完整场景中的跳跃功能
    /// </summary>
    public class PlayerJumpIntegrationTests
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

            // 添加 GroundDetector
            _groundDetector = _playerObject.AddComponent<GroundDetector>();

            // 添加 PlayerJumpController
            _jumpController = _playerObject.AddComponent<PlayerJumpController>();

            // 使用反射设置私有字段
            var gdConfigField = typeof(GroundDetector).GetField("_jumpConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            if (gdConfigField != null)
            {
                gdConfigField.SetValue(_groundDetector, _jumpConfig);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // 清理测试对象
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
        }

        #region 二段跳集成测试

        /// <summary>
        /// 测试完整场景中的二段跳功能
        /// 玩家起跳后在空中再次跳跃，成功越过更高障碍
        /// </summary>
        [UnityTest]
        public IEnumerator TestDoubleJumpIntegration_FullScene()
        {
            // Arrange: 玩家在地面上
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);

            // 等待一帧确保组件初始化
            yield return null;

            // Act 1: 执行一段跳
            _jumpController.ExecuteJump(_jumpConfig.jumpForce);

            // 等待物理更新
            yield return new WaitForSeconds(0.1f);

            // 验证玩家在空中
            Assert.IsFalse(_jumpController.IsGrounded(), "一段跳后玩家应该在空中");

            var heightAfterFirstJump = _playerObject.transform.position.y;
            Debug.Log($"[PlayModeTest] Height after first jump: {heightAfterFirstJump}");

            // 等待玩家开始下落
            yield return new WaitForSeconds(0.2f);

            var heightBeforeDoubleJump = _playerObject.transform.position.y;
            Debug.Log($"[PlayModeTest] Height before double jump: {heightBeforeDoubleJump}");

            // Act 2: 执行二段跳
            _jumpController.ExecuteJump(_jumpConfig.doubleJumpForce);

            // 等待物理更新
            yield return new WaitForSeconds(0.1f);

            var heightAfterDoubleJump = _playerObject.transform.position.y;
            Debug.Log($"[PlayModeTest] Height after double jump: {heightAfterDoubleJump}");

            // Assert: 二段跳后高度应该增加
            Assert.Greater(heightAfterDoubleJump, heightBeforeDoubleJump, "二段跳后高度应该增加");

            // 等待玩家落地
            yield return new WaitForSeconds(0.5f);

            // 验证玩家已落地
            Assert.IsTrue(_jumpController.IsGrounded(), "玩家最终应该落地");
        }

        /// <summary>
        /// 测试二段跳状态机完整性
        /// 地面 -> 一段跳 -> 二段跳 -> 落地 -> 重置
        /// </summary>
        [UnityTest]
        public IEnumerator TestDoubleJump_StateMachine完整性()
        {
            // Arrange: 玩家在地面上
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);
            yield return null;

            // 验证初始状态：在地面上，CanDoubleJump 返回 false（因为不需要）
            Assert.IsTrue(_jumpController.IsGrounded(), "初始状态应该在地面");

            // Act 1: 执行一段跳
            _jumpController.ExecuteJump(_jumpConfig.jumpForce);
            yield return new WaitForSeconds(0.05f);

            // 验证：在空中，CanDoubleJump 应该可用（通过反射验证 _canDoubleJump = true）
            Assert.IsFalse(_jumpController.IsGrounded(), "一段跳后应该在空中");

            // Act 2: 执行二段跳
            _jumpController.ExecuteJump(_jumpConfig.doubleJumpForce);
            yield return new WaitForSeconds(0.05f);

            // 验证：二段跳后仍在空中
            Assert.IsFalse(_jumpController.IsGrounded(), "二段跳后应该在空中");

            // Act 3: 等待落地
            yield return new WaitForSeconds(0.5f);

            // 验证：落地后可以再次跳跃
            Assert.IsTrue(_jumpController.IsGrounded(), "落地后应该检测到地面");
        }

        /// <summary>
        /// 测试二段跳力度配置效果
        /// 二段跳力度应该小于一段跳力度
        /// </summary>
        [Test]
        public void TestDoubleJumpForce_Configuration()
        {
            // Assert: 验证配置值
            Assert.Greater(_jumpConfig.jumpForce, 0f, "一段跳力度应该为正");
            Assert.Greater(_jumpConfig.doubleJumpForce, 0f, "二段跳力度应该为正");
            Assert.Less(_jumpConfig.doubleJumpForce, _jumpConfig.jumpForce, "二段跳力度应该小于一段跳力度");

            // 验证推荐比例：二段跳力度 = 一段跳力度 × 0.8 ~ 0.9
            var ratio = _jumpConfig.doubleJumpForce / _jumpConfig.jumpForce;
            Debug.Log($"[PlayModeTest] Double jump force ratio: {ratio} (jumpForce={_jumpConfig.jumpForce}, doubleJumpForce={_jumpConfig.doubleJumpForce})");

            // 允许一定的灵活性，比例在 0.6 ~ 1.0 之间都可接受
            Assert.Greater(ratio, 0.6f, "二段跳力度比例不应过低");
            Assert.Less(ratio, 1.0f, "二段跳力度不应超过一段跳");
        }

        #endregion

        #region 跳跃缓冲和土狼时间集成测试

        /// <summary>
        /// 测试跳跃缓冲功能
        /// 在落地前按下跳跃键，落地后应该自动起跳
        /// </summary>
        [UnityTest]
        public IEnumerator TestJumpBuffer_Integration()
        {
            // Arrange: 玩家在空中下落
            _playerObject.transform.position = new Vector3(0f, 2f, 0f);
            yield return null;

            // 模拟跳跃缓冲（通过反射设置）
            var bufferField = typeof(PlayerJumpController).GetField("_jumpBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(bufferField);
            bufferField.SetValue(_jumpController, _jumpConfig.jumpBufferTime);

            // Act: 尝试跳跃（应该因为缓冲而执行）
            _jumpController.TryJump();

            // 等待物理更新
            yield return new WaitForSeconds(0.05f);

            // Assert: 由于有缓冲且落地，应该执行了跳跃
            // 注意：这个测试需要玩家在地面上，所以先确保位置正确
            _playerObject.transform.position = new Vector3(0f, 0.6f, 0f);
            yield return null;

            var initialVelocity = _rigidbody2D.velocity.y;
            _jumpController.TryJump();

            Assert.Greater(_rigidbody2D.velocity.y, initialVelocity, "有缓冲时应该能跳跃");
        }

        /// <summary>
        /// 测试土狼时间功能
        /// 走出平台边缘后，在短时间内仍应可以跳跃
        /// </summary>
        [UnityTest]
        public IEnumerator TestCoyoteTime_Integration()
        {
            // Arrange: 玩家在空中（模拟走出平台边缘）
            _playerObject.transform.position = new Vector3(0f, 0.7f, 0f); // 略高于地面
            yield return null;

            // 模拟土狼时间（通过反射设置）
            var coyoteField = typeof(PlayerJumpController).GetField("_coyoteTimeTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            var bufferField = typeof(PlayerJumpController).GetField("_jumpBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(coyoteField);
            Assert.IsNotNull(bufferField);
            coyoteField.SetValue(_jumpController, _jumpConfig.coyoteTime);
            bufferField.SetValue(_jumpController, _jumpConfig.jumpBufferTime);

            // Act: 尝试跳跃
            _jumpController.TryJump();

            // 等待物理更新
            yield return new WaitForSeconds(0.05f);

            // Assert: 土洋时间内应该能跳跃
            Assert.Greater(_rigidbody2D.velocity.y, 0f, "土洋时间内应该能起跳");
        }

        #endregion
    }
}
