using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Reflection;

namespace RunnersJourney.Map.Tests
{
    /// <summary>
    /// 障碍物生成系统单元测试 - EditMode
    /// </summary>
    public class ObstacleGenerationTests
    {
        private TilemapMapConfig _config;
        private GameObject _generatorObject;
        private TilemapEndlessMapGenerator _generator;
        private Tilemap _obstacleTilemap;

        [SetUp]
        public void Setup()
        {
            // 加载或创建配置
            _config = ScriptableObject.CreateInstance<TilemapMapConfig>();
            _config.chunkWidth = 20;
            _config.groundHeight = 5;
            _config.baseHeight = 5;
            _config.flatChunkCount = 3;
            _config.obstacleSpawnChance = 0.5f; // 提高概率以便测试
            _config.minObstacleGap = 3;
            _config.maxObstacleGap = 6;
            _config.obstacleLayerY = 5;

            // 创建空的障碍物 Tile 池
            var obstacleTile1 = ScriptableObject.CreateInstance<Tile>();
            var obstacleTile2 = ScriptableObject.CreateInstance<Tile>();
            _config.obstacleTiles = new TileBase[] { obstacleTile1, obstacleTile2 };

            // 创建测试生成器
            _generatorObject = new GameObject("TestMapGenerator");
            _generator = _generatorObject.AddComponent<TilemapEndlessMapGenerator>();

            // 创建障碍物 Tilemap
            var obstacleTilemapObject = new GameObject("ObstacleTilemap");
            _obstacleTilemap = obstacleTilemapObject.AddComponent<Tilemap>();
            obstacleTilemapObject.transform.SetParent(_generatorObject.transform);

            // 使用反射设置配置和引用
            SetPrivateField("config", _config);
            SetPrivateField("obstacleTilemap", _obstacleTilemap);
            SetPrivateField("playerTransform", _generatorObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_generatorObject != null)
                Object.DestroyImmediate(_generatorObject);
        }

        #region 辅助方法

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(TilemapEndlessMapGenerator).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_generator, value);
        }

        private object GetPrivateField(string fieldName)
        {
            var field = typeof(TilemapEndlessMapGenerator).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(_generator);
        }

        private object InvokePrivateMethod(string methodName, params object[] parameters)
        {
            var method = typeof(TilemapEndlessMapGenerator).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            return method?.Invoke(_generator, parameters);
        }

        #endregion

        #region 障碍物生成概率测试

        [Test]
        public void ShouldSpawnObstacle_RespectsSpawnChance()
        {
            // 设置 100% 生成概率
            _config.obstacleSpawnChance = 1.0f;
            SetPrivateField("config", _config);

            // 设置足够的间隔
            SetPrivateField("lastObstacleWorldX", -999);
            SetPrivateField("currentObstacleGap", 0);

            // 初始化空隙范围为负值，避免影响测试
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            // 预填充高度缓存，避免 GetColumnHeight 访问 config
            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[10] = 5;
            heightCache[9] = 5; // 也需要前一列的高度

            // 调用私有方法
            bool result = (bool)InvokePrivateMethod("ShouldSpawnObstacle", 10);

            Assert.IsTrue(result, "100% 概率时应该生成障碍物");
        }

        [Test]
        public void ShouldSpawnObstacle_ZeroChance_NeverSpawns()
        {
            // 设置 0% 生成概率
            _config.obstacleSpawnChance = 0.0f;
            SetPrivateField("config", _config);
            SetPrivateField("lastObstacleWorldX", -999);
            SetPrivateField("currentObstacleGap", 0);
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            // 预填充高度缓存
            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            for (int i = 0; i <= 100; i++)
            {
                heightCache[i] = 5;
            }

            // 测试多次
            for (int i = 0; i < 10; i++)
            {
                bool result = (bool)InvokePrivateMethod("ShouldSpawnObstacle", i * 10);
                Assert.IsFalse(result, "0% 概率时不应生成障碍物");
            }
        }

        #endregion

        #region 障碍物间隔测试

        [Test]
        public void ShouldSpawnObstacle_RespectsMinGap()
        {
            _config.minObstacleGap = 5;
            SetPrivateField("config", _config);
            SetPrivateField("lastObstacleWorldX", 10);
            SetPrivateField("currentObstacleGap", 5);
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            // 预填充高度缓存
            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[13] = 5;
            heightCache[12] = 5;
            heightCache[16] = 5;
            heightCache[15] = 5;

            // 距离太近，不应生成
            bool result = (bool)InvokePrivateMethod("ShouldSpawnObstacle", 13); // 距离=3 < 5
            Assert.IsFalse(result, "距离小于最小间隔时不应生成障碍物");

            // 距离足够，应该生成（假设概率通过）
            _config.obstacleSpawnChance = 1.0f;
            SetPrivateField("config", _config);
            result = (bool)InvokePrivateMethod("ShouldSpawnObstacle", 16); // 距离=6 >= 5
            Assert.IsTrue(result, "距离大于最小间隔时应该生成障碍物");
        }

        [Test]
        public void ShouldSpawnObstacle_UpdatesGapAfterSpawn()
        {
            _config.obstacleSpawnChance = 1.0f;
            _config.minObstacleGap = 3;
            _config.maxObstacleGap = 6;
            SetPrivateField("config", _config);
            SetPrivateField("lastObstacleWorldX", 10);
            SetPrivateField("currentObstacleGap", 4);
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            // 预填充高度缓存
            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[15] = 5;
            heightCache[14] = 5;

            // 生成障碍物
            InvokePrivateMethod("ShouldSpawnObstacle", 15);

            // 验证间隔已更新
            int newGap = (int)GetPrivateField("currentObstacleGap");
            Assert.GreaterOrEqual(newGap, 3, "新间隔应在 minObstacleGap 范围内");
            Assert.LessOrEqual(newGap, 6, "新间隔应在 maxObstacleGap 范围内");
        }

        #endregion

        #region 高度差测试

        [Test]
        public void ShouldSpawnObstacle_HeightDifference_NoSpawn()
        {
            _config.obstacleSpawnChance = 1.0f;
            SetPrivateField("config", _config);
            SetPrivateField("lastObstacleWorldX", -999);
            SetPrivateField("currentObstacleGap", 0);
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            // 模拟有高度差的情况（通过反射设置高度缓存）
            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[10] = 5;
            heightCache[11] = 6; // 高度差=1
            heightCache[9] = 5;  // 前一列高度

            bool result = (bool)InvokePrivateMethod("ShouldSpawnObstacle", 11);
            Assert.IsFalse(result, "高度差>0 时不应生成障碍物");
        }

        [Test]
        public void ShouldSpawnObstacle_NoHeightDifference_CanSpawn()
        {
            _config.obstacleSpawnChance = 1.0f;
            SetPrivateField("config", _config);
            SetPrivateField("lastObstacleWorldX", -999);
            SetPrivateField("currentObstacleGap", 0);
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            // 模拟没有高度差的情况
            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[10] = 5;
            heightCache[11] = 5; // 高度差=0
            heightCache[9] = 5;  // 前一列高度

            bool result = (bool)InvokePrivateMethod("ShouldSpawnObstacle", 11);
            Assert.IsTrue(result, "高度差=0 时可以生成障碍物");
        }

        #endregion

        #region 障碍物生成位置测试

        [Test]
        public void SpawnObstacle_PlacesOnGroundLayer()
        {
            // 设置测试环境
            _config.obstacleSpawnChance = 1.0f;
            SetPrivateField("config", _config);
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            // 设置高度缓存
            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[10] = 5;

            // 调用生成方法
            InvokePrivateMethod("SpawnObstacle", 10);

            // 验证障碍物生成在正确位置（y = columnHeight = 5）
            var tile = _obstacleTilemap.GetTile(new Vector3Int(10, 5, 0));
            Assert.IsNotNull(tile, "障碍物应该生成在草坪层上方 (y=5)");
        }

        [Test]
        public void SpawnObstacle_UpdatesLastObstaclePosition()
        {
            _config.obstacleSpawnChance = 1.0f;
            SetPrivateField("config", _config);
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[10] = 5;

            InvokePrivateMethod("SpawnObstacle", 10);

            int lastPos = (int)GetPrivateField("lastObstacleWorldX");
            Assert.AreEqual(10, lastPos, "lastObstacleWorldX 应该更新为生成的位置");
        }

        #endregion

        #region 空隙与障碍物互斥测试

        [Test]
        public void GapArea_NoObstacleSpawn()
        {
            // 设置空隙范围
            SetPrivateField("gapStartWorldX", 10);
            SetPrivateField("gapEndWorldX", 13);

            // 空隙上方不应该生成障碍物
            // 这通过 GenerateChunkAtEnd 中的逻辑控制
            // 这里验证空隙检测逻辑
            bool isInGap = (bool)InvokePrivateMethod("IsInGap", 11);
            Assert.IsTrue(isInGap, "x=11 应该在空隙范围内");

            isInGap = (bool)InvokePrivateMethod("IsInGap", 13);
            Assert.IsFalse(isInGap, "x=13 不应该在空隙范围内 (exclusive)");
        }

        #endregion

        #region 辅助方法测试

        [Test]
        public void GetColumnHeight_ReturnsCorrectHeight()
        {
            SetPrivateField("gapStartWorldX", -100);
            SetPrivateField("gapEndWorldX", -99);

            var heightCache = (System.Collections.Generic.Dictionary<int, int>)GetPrivateField("_heightCache");
            heightCache[5] = 7;

            int height = (int)InvokePrivateMethod("GetColumnHeight", 5);
            Assert.AreEqual(7, height, "应该返回缓存的高度值");
        }

        #endregion
    }
}
