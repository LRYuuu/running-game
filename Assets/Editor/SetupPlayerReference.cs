using UnityEngine;
using UnityEditor;
using SquareFireline.Map;

namespace SquareFireline.Editor
{
    /// <summary>
    /// 快速设置玩家引用
    /// </summary>
    public static class SetupPlayerReference
    {
        [MenuItem("Tools/Square Fireline/Setup Player Reference")]
        public static void Setup()
        {
            // 查找玩家
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                // 创建玩家
                player = new GameObject("Player");
                player.AddComponent<SpriteRenderer>();
                player.AddComponent<Rigidbody2D>();
                player.AddComponent<BoxCollider2D>();
                player.transform.position = new Vector3(2, 6, 0);
            }

            // 查找 MapGenerator
            GameObject mapGenerator = GameObject.Find("MapGenerator");
            if (mapGenerator == null)
            {
                Debug.LogError("[SetupPlayerReference] 未找到 MapGenerator！");
                return;
            }

            var generator = mapGenerator.GetComponent<TilemapEndlessMapGenerator>();
            if (generator == null)
            {
                Debug.LogError("[SetupPlayerReference] MapGenerator 缺少 TilemapEndlessMapGenerator 组件！");
                return;
            }

            // 设置引用
            generator.playerTransform = player.transform;
            EditorUtility.SetDirty(generator);

            // 设置玩家位置（起点 x=2, y=groundHeight=6）
            player.transform.position = new Vector3(2, 6, 0);

            Debug.Log("[SetupPlayerReference] 玩家引用设置完成！");
            Debug.Log($"[SetupPlayerReference] - Player: {player.name} @ {player.transform.position}");
        }
    }
}
