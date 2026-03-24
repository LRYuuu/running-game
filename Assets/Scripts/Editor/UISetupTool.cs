using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SquareFireline.UI;

namespace SquareFireline.Editor
{
    /// <summary>
    /// UI 设置工具 - 帮助配置 UI 场景
    /// </summary>
    public class UISetupTool : EditorWindow
    {
        [MenuItem("Square Fireline/Setup UI Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<UISetupTool>("UI 设置工具");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("UI 设置工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("1. 创建 UIManager"))
            {
                CreateUIManager();
            }

            if (GUILayout.Button("2. 配置 UXML 引用"))
            {
                ConfigureUXML();
            }

            if (GUILayout.Button("3. 运行测试"))
            {
                RunTests();
            }

            GUILayout.Space(20);
            GUILayout.Label("说明:", EditorStyles.boldLabel);
            GUILayout.Label("1. 确保场景中有 GameManager");
            GUILayout.Label("2. 点击 '创建 UIManager' 按钮");
            GUILayout.Label("3. 点击 '配置 UXML 引用' 按钮");
            GUILayout.Label("4. 运行游戏测试主界面");
        }

        private static void CreateUIManager()
        {
            var uiManagerObject = GameObject.Find("UIManager");
            if (uiManagerObject == null)
            {
                uiManagerObject = new GameObject("UIManager");
                uiManagerObject.AddComponent<UIManager>();
                Undo.RegisterCreatedObjectUndo(uiManagerObject, "Create UIManager");
                Debug.Log("[UISetupTool] 创建了 UIManager");
            }
            else
            {
                if (uiManagerObject.GetComponent<UIManager>() == null)
                {
                    Undo.AddComponent<UIManager>(uiManagerObject);
                    Debug.Log("[UISetupTool] 添加了 UIManager 组件");
                }
            }

            // 设置为 DontDestroyOnLoad (需要在运行时)
            Selection.activeGameObject = uiManagerObject;
        }

        private static void ConfigureUXML()
        {
            var uxmlPath = "Assets/Resources/UI/MainMenu.uxml";
            var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (uxmlAsset == null)
            {
                Debug.LogError($"[UISetupTool] 未找到 UXML 文件：{uxmlPath}");
                return;
            }

            var uiManagerObject = GameObject.Find("UIManager");
            if (uiManagerObject == null)
            {
                Debug.LogError("[UISetupTool] 未找到 UIManager，请先创建");
                return;
            }

            var uiManager = uiManagerObject.GetComponent<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[UISetupTool] UIManager 未找到 UIManager 组件");
                return;
            }

            SerializedObject serializedObject = new SerializedObject(uiManager);
            var prop = serializedObject.FindProperty("mainMenuUxml");
            if (prop != null)
            {
                prop.objectReferenceValue = uxmlAsset;
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"[UISetupTool] 已配置 UXML: {uxmlAsset.name}");
            }
            else
            {
                Debug.LogError("[UISetupTool] 未找到 mainMenuUxml 属性");
            }
        }

        private static void RunTests()
        {
            Debug.Log("[UISetupTool] 请打开 Test Runner 窗口运行测试");
        }
    }
}
