#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquareFireline.UI
{
    /// <summary>
    /// InGameUI 运行时配置器
    /// </summary>
    [InitializeOnLoad]
    public class InGameUIRuntimeConfigurator
    {
        static InGameUIRuntimeConfigurator()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // 退出编辑模式前配置 InGameUIDocument
                ConfigureInGameUIDocument();
            }
        }

        [MenuItem("Square Fireline/UI/Configure InGameUIDocument", false, 1)]
        public static void ConfigureInGameUIDocumentNow()
        {
            ConfigureInGameUIDocument();
        }

        private static void ConfigureInGameUIDocument()
        {
            // 查找 InGameUIDocument
            var inGameUIObj = GameObject.Find("InGameUIDocument");
            if (inGameUIObj == null)
            {
                Debug.LogWarning("[InGameUIRuntimeConfigurator] InGameUIDocument not found");
                return;
            }

            var uiDoc = inGameUIObj.GetComponent<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogWarning("[InGameUIRuntimeConfigurator] UIDocument component not found");
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
                    Debug.Log($"[InGameUIRuntimeConfigurator] Found UXML at {path}");
                    break;
                }
            }

            if (uxmlAsset == null)
            {
                Debug.LogError("[InGameUIRuntimeConfigurator] Could not find InGameUI.uxml");
                return;
            }

            // 直接设置 UIDocument.sourceAsset（使用 serialized object）
            SerializedObject uiDocSO = new SerializedObject(uiDoc);
            SerializedProperty sourceAssetProp = uiDocSO.FindProperty("sourceAsset");

            if (sourceAssetProp != null)
            {
                sourceAssetProp.objectReferenceValue = uxmlAsset;
                uiDocSO.ApplyModifiedProperties();
                Debug.Log($"[InGameUIRuntimeConfigurator] Configured UIDocument.sourceAsset = {uxmlAsset.name}");
            }
            else
            {
                Debug.LogError("[InGameUIRuntimeConfigurator] Could not find 'sourceAsset' property on UIDocument");
            }

            Debug.Log("[InGameUIRuntimeConfigurator] Configuration complete!");
        }
    }
}
#endif
