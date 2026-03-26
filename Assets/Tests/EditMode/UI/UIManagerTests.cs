using NUnit.Framework;
using UnityEngine;
using RunnersJourney.UI;
using RunnersJourney.Game;

namespace RunnersJourney.Tests.EditMode.UI
{
    [TestFixture]
    public class UIManagerTests
    {
        private UIManager _uiManager;
        private GameObject _testObject;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // 确保测试开始前重置单例
            GameManager.ResetInstance();
            UIManager.ResetInstance();
        }

        [SetUp]
        public void Setup()
        {
            // 确保每次测试前重置单例
            GameManager.ResetInstance();
            UIManager.ResetInstance();

            // 创建测试对象
            _testObject = new GameObject("UIManagerTest");
            _uiManager = _testObject.AddComponent<UIManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testObject);
            UIManager.ResetInstance();
        }

        [Test]
        public void Instance_SingleInstance_ReturnsCorrectInstance()
        {
            // Act - 在 Awake 执行后，Instance 应该已经设置
            var instance = UIManager.Instance;

            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(_uiManager, instance);
        }

        [Test]
        public void ShowMainMenu_MainMenuState_TriggerEvent()
        {
            // Arrange
            UIState recordedOldState = UIState.MainMenu; // 修改初始状态
            UIState recordedNewState = UIState.MainMenu;
            bool eventTriggered = false;
            _uiManager.OnUIStateChanged += (old, @new) =>
            {
                recordedOldState = old;
                recordedNewState = @new;
                eventTriggered = true;
            };

            // Act - 强制调用 ShowMainMenu
            _uiManager.ShowMainMenu();

            // Assert
            Assert.AreEqual(UIState.MainMenu, _uiManager.CurrentUIState);
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(UIState.MainMenu, recordedOldState);
            Assert.AreEqual(UIState.MainMenu, recordedNewState);
        }

        [Test]
        public void HideMainMenu_FromMainMenuState_ChangesToInGame()
        {
            // Arrange - 先显示主界面
            _uiManager.ShowMainMenu();

            // Act
            _uiManager.HideMainMenu();

            // Assert
            Assert.AreEqual(UIState.InGame, _uiManager.CurrentUIState);
        }

        [Test]
        public void ShowInGameUI_InGameState_TriggerEvent()
        {
            // Arrange
            UIState recordedState = UIState.MainMenu;
            _uiManager.OnUIStateChanged += (old, @new) => recordedState = @new;
            _uiManager.ShowMainMenu(); // 先切换到 MainMenu

            // Act
            _uiManager.ShowInGameUI();

            // Assert
            Assert.AreEqual(UIState.InGame, _uiManager.CurrentUIState);
            Assert.AreEqual(UIState.InGame, recordedState);
        }

        [Test]
        public void ShowMainMenu_AlreadyInMainMenuState_NoOp()
        {
            // Arrange
            int eventCount = 0;
            _uiManager.OnUIStateChanged += (old, @new) => eventCount++;
            _uiManager.ShowMainMenu();
            eventCount = 0; // 重置计数器

            // Act
            _uiManager.ShowMainMenu();

            // Assert
            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void HideMainMenu_NotInMainMenuState_NoOp()
        {
            // Arrange
            int eventCount = 0;
            _uiManager.OnUIStateChanged += (old, @new) => eventCount++;
            _uiManager.ShowMainMenu();
            _uiManager.HideMainMenu();
            eventCount = 0; // 重置计数器

            // Act
            _uiManager.HideMainMenu();

            // Assert
            Assert.AreEqual(0, eventCount);
        }
    }
}
