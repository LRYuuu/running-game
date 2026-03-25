#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquareFireline.UI
{
    /// <summary>
    /// InGameUI 配置修复工具
    /// </summary>
    public class InGameUIConfigFixer
    {
        [MenuItem("Square Fireline/UI/Fix InGameUI Setup", false, 0)]
        public static void FixInGameUISetup()
        {
            // 查找或创建 InGameUIDocument
            var inGameUIObj = GameObject.Find("InGameUIDocument");
            if (inGameUIObj == null)
            {
                inGameUIObj = new GameObject("InGameUIDocument");
                inGameUIObj.AddComponent<UIDocument>();
                inGameUIObj.AddComponent<InGameUI>();
                Debug.Log("[InGameUIConfigFixer] Created InGameUIDocument");
            }

            var uiDoc = inGameUIObj.GetComponent<UIDocument>();
            var inGameUI = inGameUIObj.GetComponent<InGameUI>();

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
                    Debug.Log($"[InGameUIConfigFixer] Found UXML at {path}");
                    break;
                }
            }

            if (uxmlAsset == null)
            {
                Debug.LogError("[InGameUIConfigFixer] Could not find InGameUI.uxml in any expected location");
                return;
            }

            // 使用 SerializedObject 设置 UIDocument 的 sourceAsset
            var uiDocSO = new SerializedObject(uiDoc);
            var sourceAssetProp = uiDocSO.FindProperty("m_SourceAsset");
            if (sourceAssetProp != null)
            {
                sourceAssetProp.objectReferenceValue = uxmlAsset;
                uiDocSO.ApplyModifiedProperties();
                Debug.Log($"[InGameUIConfigFixer] Set UIDocument.sourceAsset = {uxmlAsset.name}");
            }

            // 设置 InGameUI 的 uiDocument 引用
            var inGameUISO = new SerializedObject(inGameUI);
            var uiDocProp = inGameUISO.FindProperty("uiDocument");
            if (uiDocProp != null)
            {
                uiDocProp.objectReferenceValue = uiDoc;
                inGameUISO.ApplyModifiedProperties();
                Debug.Log("[InGameUIConfigFixer] Set InGameUI.uiDocument reference");
            }

            Selection.activeGameObject = inGameUIObj;
            Debug.Log("[InGameUIConfigFixer] Setup complete! Select InGameUIDocument to verify.");
        }
    }
}
#endif
