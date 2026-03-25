#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquareFireline.UI
{
    /// <summary>
    /// InGameUI 编辑器脚本 - 自动配置 UIDocument 引用
    /// </summary>
    [CustomEditor(typeof(InGameUI))]
    public class InGameUIEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            var inGameUI = (InGameUI)target;

            // 自动查找并分配 UIDocument
            if (inGameUI.GetComponent<UIDocument>() != null)
            {
                var so = new SerializedObject(inGameUI);
                var uiDocProp = so.FindProperty("uiDocument");
                if (uiDocProp != null && uiDocProp.objectReferenceValue == null)
                {
                    uiDocProp.objectReferenceValue = inGameUI.GetComponent<UIDocument>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[InGameUIEditor] Auto-assigned UIDocument reference");
                }
            }
        }
    }

    /// <summary>
    /// 菜单项 - 在场景中设置 InGameUI
    /// </summary>
    public class InGameUISetupUtility
    {
        [MenuItem("Square Fireline/UI/Setup InGameUI in Scene", false, 10)]
        public static void SetupInGameUIInScene()
        {
            // 查找 InGameUIDocument
            var inGameUIObj = GameObject.Find("InGameUIDocument");
            if (inGameUIObj == null)
            {
                // 创建 GameObject
                inGameUIObj = new GameObject("InGameUIDocument");
                Debug.Log("[InGameUISetup] Created InGameUIDocument GameObject");
            }

            // 添加 UIDocument 组件
            var uiDoc = inGameUIObj.GetComponent<UIDocument>();
            if (uiDoc == null)
            {
                uiDoc = inGameUIObj.AddComponent<UIDocument>();
                Debug.Log("[InGameUISetup] Added UIDocument component");
            }

            // 添加 InGameUI 组件
            var inGameUI = inGameUIObj.GetComponent<InGameUI>();
            if (inGameUI == null)
            {
                inGameUI = inGameUIObj.AddComponent<InGameUI>();
                Debug.Log("[InGameUISetup] Added InGameUI component");
            }

            // 加载 UXML 资源
            var uxmlPath = "Assets/Resources/UI/InGameUI.uxml";
            var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (uxmlAsset != null)
            {
                // 使用 SerializedObject 设置 UIDocument 的 sourceAsset
                var uiDocSO = new SerializedObject(uiDoc);
                var sourceAssetProp = uiDocSO.FindProperty("m_SourceAsset");
                if (sourceAssetProp != null)
                {
                    sourceAssetProp.objectReferenceValue = uxmlAsset;
                    uiDocSO.ApplyModifiedProperties();
                    Debug.Log($"[InGameUISetup] Assigned UXML: {uxmlPath}");
                }
            }
            else
            {
                Debug.LogError($"[InGameUISetup] Failed to load UXML at {uxmlPath}");
            }

            // 设置 InGameUI 的 uiDocument 引用
            var so = new SerializedObject(inGameUI);
            var uiDocProp = so.FindProperty("uiDocument");
            if (uiDocProp != null)
            {
                uiDocProp.objectReferenceValue = uiDoc;
                so.ApplyModifiedProperties();
                Debug.Log("[InGameUISetup] Auto-assigned UIDocument reference in InGameUI");
            }

            Selection.activeGameObject = inGameUIObj;
            Debug.Log("[InGameUISetup] Setup complete! InGameUIDocument is ready.");
        }
    }
}
#endif
