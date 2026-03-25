#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquareFireline.UI
{
    /// <summary>
    /// InGameUI 快速设置工具
    /// </summary>
    [InitializeOnLoad]
    public class InGameUIQuickSetup
    {
        static InGameUIQuickSetup()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // 进入编辑模式时自动检查配置
                CheckAndFixConfiguration();
            }
        }

        [MenuItem("Square Fireline/UI/Fix InGameUI Configuration", false, 0)]
        public static void FixInGameUIConfiguration()
        {
            CheckAndFixConfiguration();
        }

        private static void CheckAndFixConfiguration()
        {
            // 查找 InGameUIDocument
            var inGameUIObj = GameObject.Find("InGameUIDocument");
            if (inGameUIObj == null)
            {
                Debug.LogWarning("[InGameUIQuickSetup] InGameUIDocument not found in scene");
                return;
            }

            var uiDoc = inGameUIObj.GetComponent<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogWarning("[InGameUIQuickSetup] UIDocument component not found");
                return;
            }

            // 查找 UXML 资产 - 尝试多个路径
            VisualTreeAsset uxmlAsset = null;
            string[] possiblePaths = new string[]
            {
                "Assets/Resources/UI/InGameUI.uxml",
                "Assets/UI/InGameUI.uxml"
            };

            foreach (var path in possiblePaths)
            {
                uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
                if (uxmlAsset != null)
                {
                    Debug.Log($"[InGameUIQuickSetup] Found UXML at {path}");
                    break;
                }
            }

            if (uxmlAsset == null)
            {
                Debug.LogError("[InGameUIQuickSetup] Could not find InGameUI.uxml in any expected location");
                return;
            }

            // 使用 SerializedObject 设置 sourceAsset
            var uiDocSO = new SerializedObject(uiDoc);
            var sourceAssetProp = uiDocSO.FindProperty("m_SourceAsset");
            if (sourceAssetProp != null)
            {
                sourceAssetProp.objectReferenceValue = uxmlAsset;
                uiDocSO.ApplyModifiedProperties();
                Debug.Log($"[InGameUIQuickSetup] Fixed UIDocument.sourceAsset = {uxmlAsset.name}");
            }

            // 设置 InGameUI 的 uiDocument 引用
            var inGameUI = inGameUIObj.GetComponent<InGameUI>();
            if (inGameUI != null)
            {
                var inGameUISO = new SerializedObject(inGameUI);
                var uiDocProp = inGameUISO.FindProperty("uiDocument");
                if (uiDocProp != null)
                {
                    uiDocProp.objectReferenceValue = uiDoc;
                    inGameUISO.ApplyModifiedProperties();
                    Debug.Log("[InGameUIQuickSetup] Fixed InGameUI.uiDocument reference");
                }
            }

            Debug.Log("[InGameUIQuickSetup] Configuration fix complete!");
        }
    }
}
#endif
