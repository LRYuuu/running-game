using UnityEngine;
using UnityEngine.UIElements;

namespace SquareFireline.UI
{
    /// <summary>
    /// 简单的 UI 测试组件 - 直接在场景中显示 UI
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SimpleUITest : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("UXML 路径（Resources 文件夹下）")]
        [SerializeField] private string uxmlPath = "UI/MainMenu";

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc != null)
            {
                var uxml = Resources.Load<VisualTreeAsset>(uxmlPath);
                if (uxml != null)
                {
                    uiDoc.visualTreeAsset = uxml;
                    uiDoc.enabled = true;
                    Debug.Log($"[SimpleUITest] UI 加载成功：{uxmlPath}");
                }
                else
                {
                    Debug.LogError($"[SimpleUITest] 未找到 UXML: {uxmlPath}");
                }
            }
        }
    }
}
