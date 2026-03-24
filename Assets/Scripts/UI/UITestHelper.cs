using UnityEngine;
using UnityEngine.UIElements;

namespace SquareFireline.UI
{
    /// <summary>
    /// UI 测试工具 - 用于验证 UI 系统是否正常工作
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UITestHelper : MonoBehaviour
    {
        [Header("调试")]
        [Tooltip("是否在启动时自动加载 UXML")]
        [SerializeField] private bool autoLoad = true;

        [Tooltip("UXML 路径（Resources 文件夹下）")]
        [SerializeField] private string uxmlPath = "UI/MainMenu";

        private UIDocument _uiDocument;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (autoLoad && _uiDocument != null)
            {
                LoadUXML();
            }
        }

        private void LoadUXML()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(uxmlPath);
            if (uxmlAsset != null)
            {
                _uiDocument.visualTreeAsset = uxmlAsset;
                _uiDocument.enabled = true;
                Debug.Log($"[UITestHelper] UXML 加载成功：{uxmlPath}");
            }
            else
            {
                Debug.LogError($"[UITestHelper] UXML 加载失败：{uxmlPath}");
            }
        }

        [ContextMenu("手动加载 UXML")]
        public void ManualLoadUXML()
        {
            LoadUXML();
        }

        [ContextMenu("打印 UI 状态")]
        public void PrintUIState()
        {
            if (_uiDocument == null)
            {
                Debug.LogWarning("[UITestHelper] UIDocument 组件不存在");
                return;
            }

            Debug.Log($"[UITestHelper] UIDocument.enabled: {_uiDocument.enabled}");
            Debug.Log($"[UITestHelper] UIDocument.visualTreeAsset: {_uiDocument.visualTreeAsset != null}");

            if (_uiDocument.rootVisualElement != null)
            {
                Debug.Log($"[UITestHelper] Root element: {_uiDocument.rootVisualElement.name}");
            }
            else
            {
                Debug.LogWarning("[UITestHelper] Root visual element is null");
            }
        }
    }
}
