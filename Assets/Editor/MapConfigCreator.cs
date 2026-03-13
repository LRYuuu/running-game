using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using SquareFireline.Map;
using System.Linq;

namespace SquareFireline.Editor
{
    /// <summary>
    /// 编辑器工具：创建并配置 TilemapMapConfig 资产
    /// </summary>
    public static class MapConfigCreator
    {
        private const string CREATE_MENU_PATH = "Tools/Square Fireline/Create MapConfig";
        private const string DEFAULT_CONFIG_PATH = "Assets/MapConfig.asset";

        /// <summary>
        /// 菜单项：创建 MapConfig 资产
        /// </summary>
        [MenuItem(CREATE_MENU_PATH)]
        public static void CreateMapConfig()
        {
            // 检查是否已存在
            if (AssetDatabase.LoadAssetAtPath<TilemapMapConfig>(DEFAULT_CONFIG_PATH) != null)
            {
                Debug.LogWarning("[MapConfigCreator] MapConfig 已存在，跳过创建");
                PingExistingConfig();
                return;
            }

            // 创建新的 MapConfig
            var config = ScriptableObject.CreateInstance<TilemapMapConfig>();

            // 设置默认值
            config.chunkWidth = 20;
            config.groundHeight = 5;
            config.aheadChunkCount = 3;
            config.behindChunkCount = 2;
            config.obstacleSpawnChance = 0.3f;
            config.minObstacleGap = 3;
            config.maxObstacleGap = 8;
            config.obstacleLayerY = 5;

            // 自动查找并分配 Tile
            AutoAssignTiles(config);

            // 保存资产
            AssetDatabase.CreateAsset(config, DEFAULT_CONFIG_PATH);
            AssetDatabase.SaveAssets();

            // 选中并 ping 新创建的资产
            var asset = AssetDatabase.LoadAssetAtPath<TilemapMapConfig>(DEFAULT_CONFIG_PATH);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            Debug.Log($"[MapConfigCreator] MapConfig 创建成功：{DEFAULT_CONFIG_PATH}");
            Debug.Log($"[MapConfigCreator] 请在 Inspector 中检查并完善 Tile 引用");
        }

        /// <summary>
        /// 自动查找并分配 Tile
        /// </summary>
        private static void AutoAssignTiles(TilemapMapConfig config)
        {
            // 查找所有 Tile 资产
            var allTiles = FindAllTilesInProject();

            // 分配草坪 Tile（根据名称或索引推测）
            AssignGroundTiles(config, allTiles);

            // 分配土壤 Tile
            AssignDirtTiles(config, allTiles);

            // 分配障碍物 Tile
            AssignObstacleTiles(config, allTiles);
        }

        /// <summary>
        /// 查找项目中所有 Tile 资产
        /// </summary>
        private static TileBase[] FindAllTilesInProject()
        {
            var guids = AssetDatabase.FindAssets("t:TileBase");
            var tiles = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(tile => tile != null)
                .ToArray();

            Debug.Log($"[MapConfigCreator] 找到 {tiles.Length} 个 Tile 资产");
            return tiles;
        }

        /// <summary>
        /// 分配草坪 Tile（grassLeft, grassMiddle, grassRight）
        /// </summary>
        private static void AssignGroundTiles(TilemapMapConfig config, TileBase[] allTiles)
        {
            // 优先从 GroundTile 文件夹查找
            var groundTiles = allTiles
                .Where(t => t.name.Contains("tilemap_packed"))
                .OrderBy(t => t.name)
                .ToArray();

            if (groundTiles.Length >= 3)
            {
                // 尝试根据名称或索引分配
                // 通常 tilemap_packed_0, 1, 2 是左、中、右草坪
                var tile0 = GetTileByName(groundTiles, "tilemap_packed_0");
                var tile1 = GetTileByName(groundTiles, "tilemap_packed_1");
                var tile2 = GetTileByName(groundTiles, "tilemap_packed_2");

                if (tile0 != null) config.grassLeft = tile0;
                if (tile1 != null) config.grassMiddle = tile1;
                if (tile2 != null) config.grassRight = tile2;

                Debug.Log("[MapConfigCreator] 已分配草坪 Tile: grassLeft, grassMiddle, grassRight");
            }
            else
            {
                Debug.LogWarning("[MapConfigCreator] 未找到足够的草坪 Tile，请手动分配");
            }
        }

        /// <summary>
        /// 分配土壤 Tile（单个）
        /// </summary>
        private static void AssignDirtTiles(TilemapMapConfig config, TileBase[] allTiles)
        {
            var groundTiles = allTiles
                .Where(t => t.name.Contains("tilemap_packed"))
                .OrderBy(t => t.name)
                .ToArray();

            // 分配单个土壤 Tile（通常是 tilemap_packed_3）
            var dirtTile = GetTileByName(groundTiles, "tilemap_packed_3");
            if (dirtTile != null)
            {
                config.dirtTile = dirtTile;
                Debug.Log("[MapConfigCreator] 已分配土壤 Tile: dirtTile（通过翻转创造不同效果）");
            }
            else
            {
                Debug.LogWarning("[MapConfigCreator] 未找到土壤 Tile，请手动分配 dirtTile");
            }
        }

        /// <summary>
        /// 分配障碍物 Tile
        /// </summary>
        private static void AssignObstacleTiles(TilemapMapConfig config, TileBase[] allTiles)
        {
            // 查找可能的障碍物 Tile（名称包含 obstacle 或者在特定索引范围）
            var obstacleTiles = allTiles
                .Where(t => t.name.Contains("obstacle") ||
                            t.name.Contains("stone") ||
                            t.name.Contains("box") ||
                            t.name.Contains("spike"))
                .ToArray();

            if (obstacleTiles.Length > 0)
            {
                config.obstacleTiles = obstacleTiles;
                Debug.Log($"[MapConfigCreator] 已分配 {obstacleTiles.Length} 个障碍物 Tile");
            }
            else
            {
                // 如果没有找到明确的障碍物 Tile，尝试从 GroundTile 中找一些可能的
                var groundTiles = allTiles
                    .Where(t => t.name.Contains("tilemap_packed"))
                    .Where(t =>
                    {
                        int index = GetTileIndex(t.name);
                        // 假设索引 20-30 之间可能是障碍物
                        return index >= 20 && index <= 50;
                    })
                    .Take(3)
                    .ToArray();

                if (groundTiles.Length > 0)
                {
                    config.obstacleTiles = groundTiles;
                    Debug.Log($"[MapConfigCreator] 已分配 {groundTiles.Length} 个占位障碍物 Tile，请手动确认");
                }
                else
                {
                    Debug.LogWarning("[MapConfigCreator] 未找到障碍物 Tile，请手动分配 obstacleTiles");
                }
            }
        }

        /// <summary>
        /// 根据名称获取 Tile
        /// </summary>
        private static TileBase GetTileByName(TileBase[] tiles, string name)
        {
            return tiles.FirstOrDefault(t => t.name == name);
        }

        /// <summary>
        /// 从 Tile 名称中提取索引
        /// </summary>
        private static int GetTileIndex(string tileName)
        {
            // 解析 "tilemap_packed_X" 中的 X
            var parts = tileName.Split('_');
            if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int index))
            {
                return index;
            }
            return -1;
        }

        /// <summary>
        /// Ping 已存在的 MapConfig
        /// </summary>
        private static void PingExistingConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TilemapMapConfig>(DEFAULT_CONFIG_PATH);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
            }
        }

        /// <summary>
        /// 验证菜单项（当选中 TilemapMapConfig 时禁用）
        /// </summary>
        [MenuItem(CREATE_MENU_PATH, true)]
        public static bool CreateMapConfigValidation()
        {
            return true;
        }
    }
}
