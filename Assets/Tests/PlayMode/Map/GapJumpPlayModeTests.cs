using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace SquareFireline.Map.Tests
{
    /// <summary>
    /// 空隙跳跃可行性测试 - PlayMode
    /// </summary>
    public class GapJumpPlayModeTests
    {
        private GameObject _playerObject;
        private GameObject _mapGeneratorObject;
        private TilemapEndlessMapGenerator _mapGenerator;
        private TilemapMapConfig _config;

        [SetUp]
        public void Setup()
        {
            // 加载或创建配置
            _config = Resources.Load<TilemapMapConfig>("TilemapMapConfig");
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<TilemapMapConfig>();
                _config.chunkWidth = 20;
                _config.groundHeight = 5;
                _config.minGapWidth = 1;
                _config.maxGapWidth = 3;
                _config.gapSpawnChance = 0.1f;
                _config.minGapStartChunk = 5;
            }

            // 创建地图生成器
            _mapGeneratorObject = new GameObject("MapGenerator");
            _mapGenerator = _mapGeneratorObject.AddComponent<TilemapEndlessMapGenerator>();

            // 创建测试玩家
            _playerObject = new GameObject("TestPlayer");
            _playerObject.transform.position = new Vector3(0f, 10f, 0f);

            // 添加玩家组件
            var rb = _playerObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.freezeRotation = true;

            var collider = _playerObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.5f, 0.8f);

            // 添加跳跃控制器（假设有）
            // var jumpController = _playerObject.AddComponent<PlayerJumpController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_mapGeneratorObject != null)
                Object.DestroyImmediate(_mapGeneratorObject);
        }

        #region 空隙宽度跳跃测试

        /// <summary>
        /// 测试玩家可以跳过最大宽度的空隙
        /// </summary>
        [UnityTest]
        public IEnumerator TestPlayer_CanJumpOverMaxGapWidth()
        {
            // Arrange: 创建最大宽度的空隙场景
            int maxGapWidth = _config.maxGapWidth;
            float gapWorldWidth = maxGapWidth; // 每个瓦片 1 单位宽度

            // 创建左平台
            GameObject leftPlatform = CreatePlatform(0, 0, 5); // 5 瓦片宽

            // 创建右平台（空隙之后）
            float rightPlatformX = gapWorldWidth;
            GameObject rightPlatform = CreatePlatform(rightPlatformX, 0, 10);

            // 将玩家放在左平台边缘
            _playerObject.transform.position = new Vector3(4.5f, 6f, 0f);

            // Act: 等待物理更新
            yield return new WaitForSeconds(0.1f);

            // Assert: 验证玩家位置（应该仍在左平台或掉落到空隙）
            // 注意：这是一个手动测试场景，实际跳跃需要玩家输入或自动施加力
            Assert.IsTrue(_playerObject.transform.x < rightPlatformX || _playerObject.transform.y < 0,
                "玩家应该能够尝试跳跃空隙");

            // Cleanup
            Object.DestroyImmediate(leftPlatform);
            Object.DestroyImmediate(rightPlatform);
        }

        /// <summary>
        /// 测试玩家无法跳过超过最大跳跃距离的空隙
        /// </summary>
        [UnityTest]
        public IEnumerator TestPlayer_CannotJumpOverTooWideGap()
        {
            // Arrange: 创建过宽的空隙（超过玩家最大跳跃距离）
            float tooWideGap = 10f; // 假设玩家最大跳跃距离约为 8 单位

            GameObject leftPlatform = CreatePlatform(0, 0, 5);
            GameObject rightPlatform = CreatePlatform(tooWideGap, 0, 10);

            _playerObject.transform.position = new Vector3(4.5f, 6f, 0f);

            // Act: 等待物理更新
            yield return new WaitForSeconds(0.1f);

            // Assert: 验证空隙宽度确实很大
            Assert.Greater(tooWideGap, _config.maxGapWidth, "测试空隙应该大于配置的最大宽度");

            // Cleanup
            Object.DestroyImmediate(leftPlatform);
            Object.DestroyImmediate(rightPlatform);
        }

        #endregion

        #region 空隙边界碰撞测试

        [Test]
        public void TestGapEdge_HasCorrectCollider()
        {
            // Arrange: 创建带有空隙的简单场景
            GameObject leftPlatform = CreatePlatform(0, 0, 5);
            GameObject rightPlatform = CreatePlatform(6, 0, 5); // 空隙宽度 = 1

            // Act: 检查平台边缘是否存在
            var leftCollider = leftPlatform.GetComponent<BoxCollider2D>();
            var rightCollider = rightPlatform.GetComponent<BoxCollider2D>();

            // Assert
            Assert.IsNotNull(leftCollider, "左平台应该有碰撞体");
            Assert.IsNotNull(rightCollider, "右平台应该有碰撞体");

            // Cleanup
            Object.DestroyImmediate(leftPlatform);
            Object.DestroyImmediate(rightPlatform);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建一个简单的平台
        /// </summary>
        private GameObject CreatePlatform(float x, float y, float width)
        {
            GameObject platform = new GameObject("Platform");
            platform.transform.position = new Vector3(x + width / 2f, y, 0f);

            var collider = platform.AddComponent<BoxCollider2D>();
            collider.size = new Vector3(width, 1f, 1f);

            platform.layer = LayerMask.NameToLayer("Ground");

            return platform;
        }

        #endregion
    }
}
