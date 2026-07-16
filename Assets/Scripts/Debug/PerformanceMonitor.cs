using UnityEngine;
using System.Collections.Generic;

namespace FPSTrainingRoom.DebugTools
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Display Settings")]
        public bool showFPS = true;
        public bool showMemory = true;
        public bool showDrawCalls = true;
        public Vector2 displayPosition = new Vector2(Screen.width - 200f, 10f);
        public Color displayColor = Color.green;

        [Header("FPS Settings")]
        public float updateInterval = 0.5f;
        public int targetFPS = 60;
        public Color goodFPSColor = Color.green;
        public Color warningFPSColor = Color.yellow;
        public Color badFPSColor = Color.red;

        protected float fpsAccumulator;
        protected int fpsFrames;
        protected float currentFPS;
        protected float currentFrameTime;
        protected string fpsText;
        protected string memoryText;
        protected string drawCallText;

        protected virtual void Start()
        {
            Application.targetFrameRate = targetFPS;
        }

        protected virtual void Update()
        {
            fpsAccumulator += Time.unscaledDeltaTime;
            fpsFrames++;

            if (fpsAccumulator >= updateInterval)
            {
                currentFPS = fpsFrames / fpsAccumulator;
                currentFrameTime = (fpsAccumulator / fpsFrames) * 1000f;

                fpsText = $"FPS: {currentFPS:F1} ({currentFrameTime:F1}ms)";
                memoryText = $"MEM: {System.GC.GetTotalMemory(false) / 1048576f:F1} MB";
                drawCallText = $"DRAWS: {UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / 1048576f:F1} MB";

                fpsFrames = 0;
                fpsAccumulator = 0f;
            }
        }

        protected virtual void OnGUI()
        {
            float y = displayPosition.y;

            if (showFPS)
            {
                Color fpsColor = goodFPSColor;
                if (currentFPS < targetFPS * 0.7f)
                    fpsColor = warningFPSColor;
                if (currentFPS < targetFPS * 0.4f)
                    fpsColor = badFPSColor;

                GUI.color = fpsColor;
                GUI.Label(new Rect(displayPosition.x, y, 200f, 20f), fpsText);
                y += 18f;
            }

            if (showMemory)
            {
                GUI.color = displayColor;
                GUI.Label(new Rect(displayPosition.x, y, 200f, 20f), memoryText);
                y += 18f;
            }

            if (showDrawCalls)
            {
                GUI.color = displayColor;
                GUI.Label(new Rect(displayPosition.x, y, 200f, 20f), drawCallText);
            }

            GUI.color = Color.white;
        }
    }
}
