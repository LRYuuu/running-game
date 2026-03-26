#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RunnersJourney.Game
{
    /// <summary>
    /// ScoreConfig 创建工具
    /// </summary>
    public class ScoreConfigCreator
    {
        [MenuItem("Square Fireline/Tools/Create ScoreConfig Asset", false, 10)]
        public static void CreateScoreConfigAsset()
        {
            // 检查是否已存在
            var existingPath = "Assets/ScriptableObjects/ScoreConfig.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ScoreConfig>(existingPath);
            if (existing != null)
            {
                Debug.Log($"[ScoreConfigCreator] ScoreConfig already exists at {existingPath}");
                Selection.activeObject = existing;
                return;
            }

            // 创建新的 ScoreConfig
            var config = ScriptableObject.CreateInstance<ScoreConfig>();
            config.scorePerSecond = 1;
            config.scoreInterval = 1f;

            // 确保文件夹存在
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            // 保存资产
            AssetDatabase.CreateAsset(config, existingPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = config;
            Debug.Log($"[ScoreConfigCreator] Created ScoreConfig at {existingPath}");
        }

        [MenuItem("Square Fireline/Tools/Setup ScoreManager in Scene", false, 11)]
        public static void SetupScoreManagerInScene()
        {
            // 查找 ScoreManager
            var scoreManagerObj = GameObject.Find("ScoreManager");
            if (scoreManagerObj == null)
            {
                scoreManagerObj = new GameObject("ScoreManager");
                scoreManagerObj.AddComponent<ScoreManager>();
                Debug.Log("[ScoreManagerSetup] Created ScoreManager GameObject");
            }

            var scoreManager = scoreManagerObj.GetComponent<ScoreManager>();
            if (scoreManager == null)
            {
                scoreManager = scoreManagerObj.AddComponent<ScoreManager>();
                Debug.Log("[ScoreManagerSetup] Added ScoreManager component");
            }

            // 查找或创建 ScoreConfig
            var configPath = "Assets/ScriptableObjects/ScoreConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<ScoreConfig>(configPath);

            if (config == null)
            {
                // 尝试查找任何 ScoreConfig 资产
                var guids = AssetDatabase.FindAssets("t:ScoreConfig");
                if (guids.Length > 0)
                {
                    configPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    config = AssetDatabase.LoadAssetAtPath<ScoreConfig>(configPath);
                }
            }

            // 使用 SerializedObject 分配引用
            if (config != null)
            {
                var so = new SerializedObject(scoreManager);
                var configProp = so.FindProperty("_scoreConfig");
                if (configProp != null)
                {
                    configProp.objectReferenceValue = config;
                    so.ApplyModifiedProperties();
                    Debug.Log($"[ScoreManagerSetup] Assigned ScoreConfig: {configPath}");
                }
            }
            else
            {
                Debug.LogWarning("[ScoreManagerSetup] No ScoreConfig found. Please create one manually.");
            }

            Selection.activeGameObject = scoreManagerObj;
            Debug.Log("[ScoreManagerSetup] Setup complete!");
        }
    }
}
#endif
