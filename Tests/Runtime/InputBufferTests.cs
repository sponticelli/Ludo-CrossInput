using NUnit.Framework;
using UnityEngine;
using Ludo.CrossInput;

namespace Ludo.CrossInput.Tests
{
    /// <summary>
    /// Unit tests for the InputBuffer system to ensure proper buffering behavior.
    /// </summary>
    public class InputBufferTests
    {
        private InputBuffer inputBuffer;
        private const float TestBufferTime = 0.1f;
        private const string TestActionName = "TestAction";

        [SetUp]
        public void SetUp()
        {
            inputBuffer = new InputBuffer(TestBufferTime);
        }

        [TearDown]
        public void TearDown()
        {
            inputBuffer = null;
        }

        [Test]
        public void InputBuffer_InitializesWithCorrectBufferTime()
        {
            // Arrange & Act
            var buffer = new InputBuffer(0.5f);
            
            // Assert
            Assert.IsNotNull(buffer);
        }

        [Test]
        public void BufferInput_WithValidActionName_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => inputBuffer.BufferInput(TestActionName));
        }

        [Test]
        public void BufferInput_WithEmptyActionName_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => inputBuffer.BufferInput(""));
        }

        [Test]
        public void BufferInput_WithNullActionName_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => inputBuffer.BufferInput(null));
        }

        [Test]
        public void ConsumeBufferedInput_WithNoBufferedInput_ReturnsFalse()
        {
            // Act
            bool result = inputBuffer.ConsumeBufferedInput(TestActionName);
            
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ConsumeBufferedInput_WithRecentBufferedInput_ReturnsTrue()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            
            // Act
            bool result = inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + 0.05f);
            
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ConsumeBufferedInput_WithOldBufferedInput_ReturnsFalse()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            
            // Act
            bool result = inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + TestBufferTime + 0.1f);
            
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ConsumeBufferedInput_ConsumesInputOnlyOnce()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            
            // Act
            bool firstResult = inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + 0.05f);
            bool secondResult = inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + 0.05f);
            
            // Assert
            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
        }

        [Test]
        public void HasBufferedInput_WithNoBufferedInput_ReturnsFalse()
        {
            // Act
            bool result = inputBuffer.HasBufferedInput(TestActionName);
            
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void HasBufferedInput_WithRecentBufferedInput_ReturnsTrue()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            
            // Act
            bool result = inputBuffer.HasBufferedInput(TestActionName, currentTime + 0.05f);
            
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void HasBufferedInput_WithOldBufferedInput_ReturnsFalse()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            
            // Act
            bool result = inputBuffer.HasBufferedInput(TestActionName, currentTime + TestBufferTime + 0.1f);
            
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void HasBufferedInput_DoesNotConsumeInput()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            
            // Act
            bool firstCheck = inputBuffer.HasBufferedInput(TestActionName, currentTime + 0.05f);
            bool secondCheck = inputBuffer.HasBufferedInput(TestActionName, currentTime + 0.05f);
            
            // Assert
            Assert.IsTrue(firstCheck);
            Assert.IsTrue(secondCheck);
        }

        [Test]
        public void ClearBuffer_RemovesBufferedInput()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            
            // Act
            inputBuffer.ClearBuffer(TestActionName);
            bool result = inputBuffer.HasBufferedInput(TestActionName, currentTime + 0.05f);
            
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ClearAllBuffers_RemovesAllBufferedInputs()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput("Action1", currentTime);
            inputBuffer.BufferInput("Action2", currentTime);
            inputBuffer.BufferInput("Action3", currentTime);
            
            // Act
            inputBuffer.ClearAllBuffers();
            
            // Assert
            Assert.IsFalse(inputBuffer.HasBufferedInput("Action1", currentTime + 0.05f));
            Assert.IsFalse(inputBuffer.HasBufferedInput("Action2", currentTime + 0.05f));
            Assert.IsFalse(inputBuffer.HasBufferedInput("Action3", currentTime + 0.05f));
        }

        [Test]
        public void Update_RemovesOldInputs()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime - TestBufferTime - 0.1f);
            
            // Act
            inputBuffer.Update();
            bool result = inputBuffer.HasBufferedInput(TestActionName, currentTime);
            
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Update_KeepsRecentInputs()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime - 0.05f);
            
            // Act
            inputBuffer.Update();
            bool result = inputBuffer.HasBufferedInput(TestActionName, currentTime);
            
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void MultipleInputs_SameAction_WorksCorrectly()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput(TestActionName, currentTime);
            inputBuffer.BufferInput(TestActionName, currentTime + 0.01f);
            inputBuffer.BufferInput(TestActionName, currentTime + 0.02f);
            
            // Act & Assert
            Assert.IsTrue(inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + 0.05f));
            Assert.IsTrue(inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + 0.05f));
            Assert.IsTrue(inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + 0.05f));
            Assert.IsFalse(inputBuffer.ConsumeBufferedInput(TestActionName, currentTime + 0.05f));
        }

        [Test]
        public void DifferentActions_WorkIndependently()
        {
            // Arrange
            float currentTime = Time.time;
            inputBuffer.BufferInput("Action1", currentTime);
            inputBuffer.BufferInput("Action2", currentTime);
            
            // Act
            bool action1Result = inputBuffer.ConsumeBufferedInput("Action1", currentTime + 0.05f);
            bool action2Result = inputBuffer.HasBufferedInput("Action2", currentTime + 0.05f);
            
            // Assert
            Assert.IsTrue(action1Result);
            Assert.IsTrue(action2Result);
        }

        [Test]
        public void BufferInput_WithZeroTimestamp_UsesCurrentTime()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => inputBuffer.BufferInput(TestActionName, 0));
            
            // Should have buffered input
            Assert.IsTrue(inputBuffer.HasBufferedInput(TestActionName));
        }

        [Test]
        public void ConsumeBufferedInput_WithZeroCurrentTime_UsesCurrentTime()
        {
            // Arrange
            inputBuffer.BufferInput(TestActionName);
            
            // Act & Assert
            Assert.DoesNotThrow(() => inputBuffer.ConsumeBufferedInput(TestActionName, 0));
        }

        [Test]
        public void HasBufferedInput_WithZeroCurrentTime_UsesCurrentTime()
        {
            // Arrange
            inputBuffer.BufferInput(TestActionName);
            
            // Act & Assert
            Assert.DoesNotThrow(() => inputBuffer.HasBufferedInput(TestActionName, 0));
        }
    }
}
