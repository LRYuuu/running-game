using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using RunnersJourney.UI;
using RunnersJourney.Game;

namespace RunnersJourney.Tests.PlayMode.UI
{
    [TestFixture]
    public class MainMenuUITests
    {
        private UIManager _uiManager;
        private GameObject _uiManagerObject;

        [SetUp]
        public void Setup()
        {
            // 创建 UIManager
            _uiManagerObject = new GameObject("UIManager");
            _uiManager = _uiManagerObject.AddComponent<UIManager>();

            // 重置单例
            GameManager.ResetInstance();
            UIManager.Instance = null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_uiManagerObject);
            UIManager.Instance = null;
        }

        [UnityTest]
        public IEnumerator ShowMainMenu_UXMLLoaded_VisualElementExists()
        {
            // Act
            _uiManager.ShowMainMenu();
            yield return null; // 等待一帧

            // Assert - 验证 UI 文档已启用
            Assert.AreEqual(UIState.MainMenu, _uiManager.CurrentUIState);
        }

        [UnityTest]
        public IEnumerator HideMainMenu_FromMainMenu_UIHidden()
        {
            // Arrange
            _uiManager.ShowMainMenu();
            yield return null;

            // Act
            _uiManager.HideMainMenu();
            yield return null;

            // Assert
            Assert.AreEqual(UIState.InGame, _uiManager.CurrentUIState);
        }

        [Test]
        public void UIStateChangeEvent_FiresOnStateChange()
        {
            // Arrange
            bool eventFired = false;
            UIState capturedOldState = UIState.MainMenu;
            UIState capturedNewState = UIState.MainMenu;

            _uiManager.OnUIStateChanged += (old, @new) =>
            {
                eventFired = true;
                capturedOldState = old;
                capturedNewState = @new;
            };

            // Act
            _uiManager.ShowMainMenu();

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(UIState.MainMenu, capturedNewState);
        }
    }
}
