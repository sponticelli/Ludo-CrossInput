using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using Ludo.CrossInput;

namespace Ludo.CrossInput.Tests
{
    /// <summary>
    /// Unit tests for the InputManager system to ensure reliability and correctness.
    /// </summary>
    public class InputManagerTests
    {
        private GameObject testGameObject;
        private InputManager inputManager;
        private PlayerInput playerInput;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with required components
            testGameObject = new GameObject("TestInputManager");
            
            // Add PlayerInput component with a basic action map
            playerInput = testGameObject.AddComponent<PlayerInput>();
            
            // Create a simple input action asset for testing
            var actionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = actionAsset.AddActionMap("Player");
            
            // Add basic actions
            var moveAction = actionMap.AddAction("Move", InputActionType.Value);
            moveAction.expectedControlType = "Vector2";
            actionMap.AddAction("Fire", InputActionType.Button);
            actionMap.AddAction("Jump", InputActionType.Button);
            
            playerInput.actions = actionAsset;
            
            // Add InputManager component
            inputManager = testGameObject.AddComponent<InputManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void InputManager_InitializesCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(inputManager);
            Assert.IsNotNull(playerInput);
        }

        [Test]
        public void SetMainCamera_ReturnsInputManagerInstance()
        {
            // Arrange
            var camera = new GameObject("TestCamera").AddComponent<Camera>();
            
            // Act
            var result = inputManager.SetMainCamera(camera);
            
            // Assert
            Assert.AreEqual(inputManager, result);
            
            // Cleanup
            Object.DestroyImmediate(camera.gameObject);
        }

        [Test]
        public void SetJoystick_ReturnsInputManagerInstance()
        {
            // Arrange
            var joystickGO = new GameObject("TestJoystick");
            var joystick = joystickGO.AddComponent<Joystick>();
            
            // Act
            var result = inputManager.SetJoystick(joystick);
            
            // Assert
            Assert.AreEqual(inputManager, result);
            
            // Cleanup
            Object.DestroyImmediate(joystickGO);
        }

        [Test]
        public void GetMove_ReturnsZeroWhenNoInput()
        {
            // Act
            Vector2 movement = inputManager.GetMove();
            
            // Assert
            Assert.AreEqual(Vector2.zero, movement);
        }

        [Test]
        public void GetScreenPosition_ReturnsValidPosition()
        {
            // Act
            Vector2 position = inputManager.GetScreenPosition();
            
            // Assert
            Assert.IsTrue(position.x >= 0 || position.x <= 0); // Just check it's a valid number
            Assert.IsTrue(position.y >= 0 || position.y <= 0);
        }

        [Test]
        public void GetWorldPosition_ReturnsValidPosition()
        {
            // Arrange
            var camera = new GameObject("TestCamera").AddComponent<Camera>();
            inputManager.SetMainCamera(camera);
            
            // Act
            Vector2 worldPos = inputManager.GetWorldPosition();
            
            // Assert
            Assert.IsTrue(worldPos.x >= float.MinValue && worldPos.x <= float.MaxValue);
            Assert.IsTrue(worldPos.y >= float.MinValue && worldPos.y <= float.MaxValue);
            
            // Cleanup
            Object.DestroyImmediate(camera.gameObject);
        }

        [Test]
        public void GetSwipe_ReturnsNoneWhenNoSwipe()
        {
            // Act
            SwipeDirection swipe = inputManager.GetSwipe();
            
            // Assert
            Assert.AreEqual(SwipeDirection.None, swipe);
        }

        [Test]
        public void GetFire_ReturnsFalseWhenNotPressed()
        {
            // Act
            bool firePressed = inputManager.GetFire();
            
            // Assert
            Assert.IsFalse(firePressed);
        }

        [Test]
        public void GetJump_ReturnsFalseWhenNotPressed()
        {
            // Act
            bool jumpPressed = inputManager.GetJump();
            
            // Assert
            Assert.IsFalse(jumpPressed);
        }

        [Test]
        public void GetKey_ReturnsDefaultWhenActionNotFound()
        {
            // Act
            bool result = inputManager.GetKey<bool>("NonExistentAction");
            
            // Assert
            Assert.AreEqual(default(bool), result);
        }

        [Test]
        public void GetKey_ReturnsDefaultWhenActionNameIsEmpty()
        {
            // Act
            bool result = inputManager.GetKey<bool>("");
            
            // Assert
            Assert.AreEqual(default(bool), result);
        }

        [Test]
        public void GetKey_ReturnsDefaultWhenActionNameIsNull()
        {
            // Act
            bool result = inputManager.GetKey<bool>(null);
            
            // Assert
            Assert.AreEqual(default(bool), result);
        }

        [UnityTest]
        public IEnumerator InputManager_HandlesMultipleFramesCorrectly()
        {
            // Arrange
            int frameCount = 0;
            
            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                frameCount++;
                
                // Test that input methods don't throw exceptions over multiple frames
                Assert.DoesNotThrow(() => inputManager.GetMove());
                Assert.DoesNotThrow(() => inputManager.GetFire());
                Assert.DoesNotThrow(() => inputManager.GetJump());
                Assert.DoesNotThrow(() => inputManager.GetScreenPosition());
                
                yield return null; // Wait one frame
            }
            
            Assert.AreEqual(5, frameCount);
        }

        [Test]
        public void InputManager_DisposesCorrectly()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => inputManager.Dispose());
            
            // Test that methods still work after disposal (should handle gracefully)
            Assert.DoesNotThrow(() => inputManager.GetMove());
            Assert.DoesNotThrow(() => inputManager.GetFire());
        }

        [Test]
        public void FluentInterface_ChainsCorrectly()
        {
            // Arrange
            var camera = new GameObject("TestCamera").AddComponent<Camera>();
            var joystickGO = new GameObject("TestJoystick");
            var joystick = joystickGO.AddComponent<Joystick>();
            
            // Act & Assert
            var result = inputManager
                .SetMainCamera(camera)
                .SetJoystick(joystick);
            
            Assert.AreEqual(inputManager, result);
            
            // Cleanup
            Object.DestroyImmediate(camera.gameObject);
            Object.DestroyImmediate(joystickGO);
        }

        [Test]
        public void InputActionNames_AreNotEmpty()
        {
            // Assert
            Assert.IsNotEmpty(InputActionNames.MOVE);
            Assert.IsNotEmpty(InputActionNames.FIRE);
            Assert.IsNotEmpty(InputActionNames.JUMP);
            Assert.IsNotEmpty(InputActionNames.CROUCH);
            Assert.IsNotEmpty(InputActionNames.SPRINT);
            Assert.IsNotEmpty(InputActionNames.RELOAD);
            Assert.IsNotEmpty(InputActionNames.INVENTORY);
            Assert.IsNotEmpty(InputActionNames.INTERACT);
            Assert.IsNotEmpty(InputActionNames.MAP);
            Assert.IsNotEmpty(InputActionNames.PREVIOUS);
            Assert.IsNotEmpty(InputActionNames.NEXT);
            Assert.IsNotEmpty(InputActionNames.PAUSE);
            Assert.IsNotEmpty(InputActionNames.BACK);
        }

        [Test]
        public void InputActionNames_AreUnique()
        {
            // Arrange
            var actionNames = new[]
            {
                InputActionNames.MOVE,
                InputActionNames.FIRE,
                InputActionNames.JUMP,
                InputActionNames.CROUCH,
                InputActionNames.SPRINT,
                InputActionNames.RELOAD,
                InputActionNames.INVENTORY,
                InputActionNames.INTERACT,
                InputActionNames.MAP,
                InputActionNames.PREVIOUS,
                InputActionNames.NEXT,
                InputActionNames.PAUSE,
                InputActionNames.BACK
            };
            
            // Act & Assert
            for (int i = 0; i < actionNames.Length; i++)
            {
                for (int j = i + 1; j < actionNames.Length; j++)
                {
                    Assert.AreNotEqual(actionNames[i], actionNames[j], 
                        $"Action names '{actionNames[i]}' and '{actionNames[j]}' are not unique");
                }
            }
        }
    }
}
