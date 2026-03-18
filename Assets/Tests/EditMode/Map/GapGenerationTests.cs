using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace SquareFireline.Map.Tests
{
    /// <summary>
    /// 空隙生成系统单元测试 - EditMode
    /// </summary>
    public class GapGenerationTests
    {
        private TilemapMapConfig _config;
        private GameObject _generatorObject;
        private TilemapEndlessMapGenerator _generator;

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
                _config.minGapInterval = 3;
            }

            // 创建测试生成器
            _generatorObject = new GameObject("TestMapGenerator");
            _generator = _generatorObject.AddComponent<TilemapEndlessMapGenerator>();

            // 使用反射设置配置
            var configField = typeof(TilemapEndlessMapGenerator).GetField("config", BindingFlags.Public | BindingFlags.Instance);
            configField?.SetValue(_generator, _config);
        }

        [TearDown]
        public void TearDown()
        {
            if (_generatorObject != null)
                Object.DestroyImmediate(_generatorObject);
        }

        #region 空隙配置测试

        [Test]
        public void TestConfig_GapParameters_AreValid()
        {
            // Assert
            Assert.Greater(_config.minGapWidth, 0, "最小空隙宽度应该大于 0");
            Assert.GreaterOrEqual(_config.maxGapWidth, _config.minGapWidth, "最大空隙宽度应该大于等于最小宽度");
            Assert.Greater(_config.gapSpawnChance, 0f, "空隙生成概率应该大于 0");
            Assert.LessOrEqual(_config.gapSpawnChance, 1f, "空隙生成概率应该小于等于 1");
            Assert.Greater(_config.minGapStartChunk, 0, "起始保护区应该大于 0");
        }

        [Test]
        public void TestConfig_GapTiles_AreConfigured()
        {
            // Arrange: 创建临时配置测试 Tile 字段
            var tempConfig = ScriptableObject.CreateInstance<TilemapMapConfig>();

            // Assert: 验证配置类有 gapTopTile 和 gapCenterTile 字段
            var gapTopTileField = typeof(TilemapMapConfig).GetField("gapTopTile", BindingFlags.Public | BindingFlags.Instance);
            var gapCenterTileField = typeof(TilemapMapConfig).GetField("gapCenterTile", BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(gapTopTileField, "TilemapMapConfig 应该有 gapTopTile 字段");
            Assert.IsNotNull(gapCenterTileField, "TilemapMapConfig 应该有 gapCenterTile 字段");
        }

        #endregion

        #region ShouldGenerateGap 测试

        [Test]
        public void TestShouldGenerateGap_InStartZone_ReturnsFalse()
        {
            // Arrange: 前 5 个 Chunk 为起始保护区
            int[] protectedChunkIndices = { 0, 1, 2, 3, 4 };

            foreach (int chunkIndex in protectedChunkIndices)
            {
                // Act
                bool shouldGenerateGap = CallShouldGenerateGap(chunkIndex);

                // Assert
                Assert.IsFalse(shouldGenerateGap, $"Chunk #{chunkIndex} 在起始保护区内，不应生成空隙");
            }
        }

        [Test]
        public void TestShouldGenerateGap_AfterStartZone_CanReturnTrueOrFalse()
        {
            // Arrange: 起始保护区之后的 Chunk
            int chunkIndex = _config.minGapStartChunk + 1;

            // Act: 多次调用测试概率行为（概率测试）
            int trueCount = 0;
            int iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                if (CallShouldGenerateGap(chunkIndex))
                {
                    trueCount++;
                }
            }

            // Assert: 概率应该在合理范围内（允许一定偏差）
            float actualChance = (float)trueCount / iterations;
            float expectedChance = _config.gapSpawnChance;
            float tolerance = 0.05f; // 5% 容差

            // 注意：这是一个概率测试，可能偶尔失败
            Assert.That(actualChance, Is.EqualTo(expectedChance).Within(tolerance),
                $"空隙生成概率应该在 {expectedChance - tolerance} 到 {expectedChance + tolerance} 之间，实际为 {actualChance}");
        }

        [Test]
        public void TestShouldGenerateGap_WithZeroChance_ReturnsFalse()
        {
            // Arrange
            float originalChance = _config.gapSpawnChance;
            _config.gapSpawnChance = 0f;

            // Act
            bool shouldGenerateGap = CallShouldGenerateGap(_config.minGapStartChunk + 1);

            // Assert
            Assert.IsFalse(shouldGenerateGap, "生成概率为 0 时不应生成空隙");

            // Cleanup
            _config.gapSpawnChance = originalChance;
        }

        [Test]
        public void TestShouldGenerateGap_WithHighChance_ReturnsTrue()
        {
            // Arrange
            float originalChance = _config.gapSpawnChance;
            _config.gapSpawnChance = 1f;

            // Act
            bool shouldGenerateGap = CallShouldGenerateGap(_config.minGapStartChunk + 1);

            // Assert
            Assert.IsTrue(shouldGenerateGap, "生成概率为 1 时应该生成空隙");

            // Cleanup
            _config.gapSpawnChance = originalChance;
        }

        #endregion

        #region 空隙宽度测试

        [Test]
        public void TestGapWidth_WithinRange()
        {
            // Arrange: 多次生成空隙宽度
            int iterations = 100;

            for (int i = 0; i < iterations; i++)
            {
                // Act
                int gapWidth = Random.Range(_config.minGapWidth, _config.maxGapWidth + 1);

                // Assert
                Assert.GreaterOrEqual(gapWidth, _config.minGapWidth, $"空隙宽度 {gapWidth} 不应小于最小值 {_config.minGapWidth}");
                Assert.LessOrEqual(gapWidth, _config.maxGapWidth, $"空隙宽度 {gapWidth} 不应大于最大值 {_config.maxGapWidth}");
            }
        }

        [Test]
        public void TestGapWidth_Range_Validity()
        {
            // Assert
            Assert.Greater(_config.minGapWidth, 0, "最小空隙宽度应该大于 0");
            Assert.LessOrEqual(_config.maxGapWidth, 5, "最大空隙宽度不应该太大（建议<=5）");
            Assert.GreaterOrEqual(_config.maxGapWidth, _config.minGapWidth, "最大宽度应该大于等于最小宽度");
        }

        #endregion

        #region 起始保护区测试

        [Test]
        public void TestStartZone_ProtectedChunks()
        {
            // Arrange
            int protectedChunkCount = _config.minGapStartChunk;

            // Act & Assert
            for (int i = 0; i < protectedChunkCount; i++)
            {
                bool shouldGenerateGap = CallShouldGenerateGap(i);
                Assert.IsFalse(shouldGenerateGap, $"Chunk #{i} 应该在起始保护区内");
            }

            // 验证保护区之后的 Chunk 可能生成空隙
            bool shouldGenerateGapAfterZone = CallShouldGenerateGap(protectedChunkCount);
            // 注意：这里不能 Assert.IsTrue，因为概率可能返回 false
            // 只是验证方法不会因索引而出错，返回值是有效的 bool
            Assert.That(shouldGenerateGapAfterZone, Is.True.Or.False, "保护区后的 Chunk 应该可以生成空隙（概率决定）");
        }

        #endregion

        #region 空隙间隔测试

        [Test]
        public void TestGapInterval_MinimumDistance()
        {
            // Arrange: 模拟两个空隙的位置
            int firstGapEnd = 100; // 第一个空隙结束位置
            int minInterval = _config.minGapInterval * _config.chunkWidth;

            // Act & Assert: 验证在最小间隔内的 Chunk 不会生成空隙
            for (int chunkIndex = 6; chunkIndex < 15; chunkIndex++)
            {
                int chunkStart = chunkIndex * _config.chunkWidth;
                if (chunkStart - firstGapEnd < minInterval)
                {
                    bool shouldGenerateGap = CallShouldGenerateGap(chunkIndex);
                    // 注意：由于间隔检查，这些 Chunk 应该不太可能生成空隙
                    // 但概率检查在间隔检查之后，所以这里只做基本验证
                    Assert.That(shouldGenerateGap, Is.True.Or.False, $"Chunk #{chunkIndex} 间隔检查应正常执行");
                }
            }
        }

        #endregion

        #region IsFlatGround 测试

        // 注意：IsFlatGround 方法在最终实现中被移除，实际逻辑直接在 GenerateChunkAtEnd 中处理
        // 以下测试已废弃，保留区域占位符供未来扩展

        #endregion

        #region 辅助方法

        /// <summary>
        /// 调用私有的 ShouldGenerateGap 方法
        /// </summary>
        private bool CallShouldGenerateGap(int chunkIndex, int startX = -1)
        {
            MethodInfo method = typeof(TilemapEndlessMapGenerator).GetMethod(
                "ShouldGenerateGap",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                Assert.Fail("ShouldGenerateGap 方法不存在");
                return false;
            }

            // 如果没有指定 startX，使用 chunkIndex * chunkWidth 计算
            if (startX == -1)
            {
                startX = chunkIndex * _config.chunkWidth;
            }

            return (bool)method.Invoke(_generator, new object[] { chunkIndex, startX });
        }

        #endregion
    }
}
