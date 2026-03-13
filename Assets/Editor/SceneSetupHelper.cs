using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using SquareFireline.Map;

namespace SquareFireline.Editor
{
    /// <summary>
    /// 编辑器工具：设置 EndlessMapTest 场景
    /// </summary>
    public static class SceneSetupHelper
    {
        [MenuItem("Tools/Square Fireline/Setup EndlessMapTest Scene")]
        public static void SetupEndlessMapTestScene()
        {
            // 查找或创建 MapGenerator
            GameObject mapGenerator = GameObject.Find("MapGenerator");
            if (mapGenerator == null)
            {
                mapGenerator = new GameObject("MapGenerator");
            }

            // 获取或添加 TilemapEndlessMapGenerator 组件
            var generator = mapGenerator.GetComponent<TilemapEndlessMapGenerator>();
            if (generator == null)
            {
                generator = mapGenerator.AddComponent<TilemapEndlessMapGenerator>();
            }

            // 查找 Grid 和 Tilemap
            GameObject grid = GameObject.Find("Grid");
            if (grid == null)
            {
                grid = new GameObject("Grid");
                grid.AddComponent<Grid>();
            }

            // 查找或创建 Ground Tilemap
            GameObject groundObj = GameObject.Find("Ground");
            if (groundObj == null)
            {
                groundObj = new GameObject("Ground");
                groundObj.transform.SetParent(grid.transform);
            }

            var groundTilemap = groundObj.GetComponent<Tilemap>();
            if (groundTilemap == null)
            {
                groundTilemap = groundObj.AddComponent<Tilemap>();
            }
            if (groundObj.GetComponent<TilemapRenderer>() == null)
            {
                groundObj.AddComponent<TilemapRenderer>();
            }
            if (groundObj.GetComponent<TilemapCollider2D>() == null)
            {
                groundObj.AddComponent<TilemapCollider2D>();
            }

            // 查找或创建 Obstacles Tilemap
            GameObject obstaclesObj = GameObject.Find("Obstacles");
            if (obstaclesObj == null)
            {
                obstaclesObj = new GameObject("Obstacles");
                obstaclesObj.transform.SetParent(grid.transform);
            }

            var obstacleTilemap = obstaclesObj.GetComponent<Tilemap>();
            if (obstacleTilemap == null)
            {
                obstacleTilemap = obstaclesObj.AddComponent<Tilemap>();
            }
            if (obstaclesObj.GetComponent<TilemapRenderer>() == null)
            {
                obstaclesObj.AddComponent<TilemapRenderer>();
            }
            if (obstaclesObj.GetComponent<TilemapCollider2D>() == null)
            {
                obstaclesObj.AddComponent<TilemapCollider2D>();
            }

            // 加载 MapConfig
            var config = AssetDatabase.LoadAssetAtPath<TilemapMapConfig>("Assets/MapConfig.asset");
            if (config == null)
            {
                Debug.LogError("[SceneSetupHelper] 未找到 MapConfig 资产！");
                return;
            }

            // 设置引用
            generator.groundTilemap = groundTilemap;
            generator.obstacleTilemap = obstacleTilemap;
            generator.config = config;

            // 查找玩家
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                generator.playerTransform = player.transform;
            }

            EditorUtility.SetDirty(generator);
            AssetDatabase.SaveAssets();

            Debug.Log("[SceneSetupHelper] 场景设置完成！");
            Debug.Log($"[SceneSetupHelper] - MapGenerator: {mapGenerator.name}");
            Debug.Log($"[SceneSetupHelper] - Ground Tilemap: {groundTilemap.name}");
            Debug.Log($"[SceneSetupHelper] - Obstacles Tilemap: {obstacleTilemap.name}");
            Debug.Log($"[SceneSetupHelper] - MapConfig: {config.name}");
        }
    }
}
