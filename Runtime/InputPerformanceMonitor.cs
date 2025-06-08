using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Monitors input system performance and provides metrics for optimization.
    /// </summary>
    public class InputPerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [Tooltip("Enable performance monitoring")]
        [SerializeField] private bool enableMonitoring = true;
        
        [Tooltip("Update interval for performance metrics in seconds")]
        [SerializeField] private float updateInterval = 1f;
        
        [Tooltip("Maximum number of performance samples to keep")]
        [SerializeField] private int maxSamples = 100;
        
        [Tooltip("Log performance warnings when thresholds are exceeded")]
        [SerializeField] private bool logPerformanceWarnings = true;

        [Header("Performance Thresholds")]
        [Tooltip("Warning threshold for input processing time in milliseconds")]
        [SerializeField] private float inputProcessingTimeThreshold = 5f;
        
        [Tooltip("Warning threshold for memory allocation in KB per frame")]
        [SerializeField] private float memoryAllocationThreshold = 10f;

        // Performance metrics
        private readonly Queue<PerformanceSample> performanceSamples = new Queue<PerformanceSample>();
        private float lastUpdateTime;
        private long lastGCMemory;
        private int frameCount;
        private float totalInputProcessingTime;

        // Current frame metrics
        private float currentFrameInputTime;
        private long currentFrameMemoryStart;

        public struct PerformanceSample
        {
            public float timestamp;
            public float inputProcessingTime;
            public float memoryAllocation;
            public int inputEventsProcessed;
            public float frameTime;

            public PerformanceSample(float timestamp, float inputTime, float memory, int events, float frameTime)
            {
                this.timestamp = timestamp;
                this.inputProcessingTime = inputTime;
                this.memoryAllocation = memory;
                this.inputEventsProcessed = events;
                this.frameTime = frameTime;
            }
        }

        // Properties for external access
        public bool IsMonitoring => enableMonitoring;
        public float AverageInputProcessingTime { get; private set; }
        public float AverageMemoryAllocation { get; private set; }
        public float AverageFrameTime { get; private set; }
        public int TotalInputEventsProcessed { get; private set; }

        // Events
        public event Action<PerformanceSample> OnPerformanceSampleRecorded;
        public event Action<string> OnPerformanceWarning;

        private void Start()
        {
            if (enableMonitoring)
            {
                lastUpdateTime = Time.time;
                lastGCMemory = GC.GetTotalMemory(false);
                ResetFrameMetrics();
            }
        }

        private void Update()
        {
            if (!enableMonitoring) return;

            frameCount++;
            
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                RecordPerformanceSample();
                ResetFrameMetrics();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Starts timing input processing for the current frame.
        /// Call this at the beginning of input processing.
        /// </summary>
        public void StartInputTiming()
        {
            if (!enableMonitoring) return;
            
            currentFrameMemoryStart = GC.GetTotalMemory(false);
            currentFrameInputTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Ends timing input processing for the current frame.
        /// Call this at the end of input processing.
        /// </summary>
        public void EndInputTiming(int eventsProcessed = 1)
        {
            if (!enableMonitoring) return;

            float processingTime = (Time.realtimeSinceStartup - currentFrameInputTime) * 1000f; // Convert to milliseconds
            totalInputProcessingTime += processingTime;
            TotalInputEventsProcessed += eventsProcessed;

            // Check for performance warnings
            if (logPerformanceWarnings && processingTime > inputProcessingTimeThreshold)
            {
                string warning = $"Input processing time exceeded threshold: {processingTime:F2}ms (threshold: {inputProcessingTimeThreshold}ms)";
                OnPerformanceWarning?.Invoke(warning);
                Debug.LogWarning($"[InputPerformanceMonitor] {warning}");
            }
        }

        /// <summary>
        /// Records a performance sample and updates running averages.
        /// </summary>
        private void RecordPerformanceSample()
        {
            long currentMemory = GC.GetTotalMemory(false);
            float memoryDelta = (currentMemory - lastGCMemory) / 1024f; // Convert to KB
            float avgInputTime = frameCount > 0 ? totalInputProcessingTime / frameCount : 0f;
            float avgFrameTime = frameCount > 0 ? updateInterval * 1000f / frameCount : 0f; // Convert to milliseconds

            var sample = new PerformanceSample(
                Time.time,
                avgInputTime,
                memoryDelta,
                TotalInputEventsProcessed,
                avgFrameTime
            );

            // Add sample to queue
            performanceSamples.Enqueue(sample);
            
            // Remove old samples if we exceed the maximum
            while (performanceSamples.Count > maxSamples)
            {
                performanceSamples.Dequeue();
            }

            // Update running averages
            UpdateAverages();

            // Check for memory allocation warnings
            if (logPerformanceWarnings && memoryDelta > memoryAllocationThreshold)
            {
                string warning = $"Memory allocation exceeded threshold: {memoryDelta:F2}KB (threshold: {memoryAllocationThreshold}KB)";
                OnPerformanceWarning?.Invoke(warning);
                Debug.LogWarning($"[InputPerformanceMonitor] {warning}");
            }

            OnPerformanceSampleRecorded?.Invoke(sample);
            lastGCMemory = currentMemory;
        }

        /// <summary>
        /// Updates running averages based on current samples.
        /// </summary>
        private void UpdateAverages()
        {
            if (performanceSamples.Count == 0) return;

            float totalInputTime = 0f;
            float totalMemory = 0f;
            float totalFrameTime = 0f;

            foreach (var sample in performanceSamples)
            {
                totalInputTime += sample.inputProcessingTime;
                totalMemory += sample.memoryAllocation;
                totalFrameTime += sample.frameTime;
            }

            int sampleCount = performanceSamples.Count;
            AverageInputProcessingTime = totalInputTime / sampleCount;
            AverageMemoryAllocation = totalMemory / sampleCount;
            AverageFrameTime = totalFrameTime / sampleCount;
        }

        /// <summary>
        /// Resets frame-specific metrics.
        /// </summary>
        private void ResetFrameMetrics()
        {
            frameCount = 0;
            totalInputProcessingTime = 0f;
            TotalInputEventsProcessed = 0;
        }

        /// <summary>
        /// Gets all recorded performance samples.
        /// </summary>
        public PerformanceSample[] GetPerformanceSamples()
        {
            return performanceSamples.ToArray();
        }

        /// <summary>
        /// Clears all recorded performance samples.
        /// </summary>
        public void ClearPerformanceSamples()
        {
            performanceSamples.Clear();
            ResetFrameMetrics();
            AverageInputProcessingTime = 0f;
            AverageMemoryAllocation = 0f;
            AverageFrameTime = 0f;
        }

        /// <summary>
        /// Gets a performance report as a formatted string.
        /// </summary>
        public string GetPerformanceReport()
        {
            if (!enableMonitoring)
                return "Performance monitoring is disabled.";

            return $"Input Performance Report:\n" +
                   $"- Average Input Processing Time: {AverageInputProcessingTime:F2}ms\n" +
                   $"- Average Memory Allocation: {AverageMemoryAllocation:F2}KB/interval\n" +
                   $"- Average Frame Time: {AverageFrameTime:F2}ms\n" +
                   $"- Total Input Events Processed: {TotalInputEventsProcessed}\n" +
                   $"- Sample Count: {performanceSamples.Count}/{maxSamples}";
        }

        /// <summary>
        /// Enables or disables performance monitoring at runtime.
        /// </summary>
        public void SetMonitoringEnabled(bool enabled)
        {
            if (enableMonitoring == enabled) return;

            enableMonitoring = enabled;
            
            if (enabled)
            {
                lastUpdateTime = Time.time;
                lastGCMemory = GC.GetTotalMemory(false);
                ResetFrameMetrics();
                Debug.Log("[InputPerformanceMonitor] Performance monitoring enabled");
            }
            else
            {
                Debug.Log("[InputPerformanceMonitor] Performance monitoring disabled");
            }
        }

        /// <summary>
        /// Sets the update interval for performance sampling.
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.1f, interval);
        }

        /// <summary>
        /// Sets the maximum number of samples to keep.
        /// </summary>
        public void SetMaxSamples(int maxCount)
        {
            maxSamples = Mathf.Max(10, maxCount);
            
            // Remove excess samples if necessary
            while (performanceSamples.Count > maxSamples)
            {
                performanceSamples.Dequeue();
            }
        }
    }
}
