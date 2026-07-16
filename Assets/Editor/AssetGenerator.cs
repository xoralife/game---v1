using UnityEngine;
using UnityEditor;
using FPSTrainingRoom.Weapons;
using FPSTrainingRoom.Training;
using System.IO;

namespace FPSTrainingRoom.EditorTools
{
    public class AssetGenerator : EditorWindow
    {
        [MenuItem("FPS Training/Generate Default Assets")]
        public static void GenerateDefaultAssets()
        {
            GenerateRecoilPattern("AK-47", 600f, 30,
                new Vector2[] { Vector2.up * 0.5f, Vector2.up * 0.8f, Vector2.up * 1.2f,
                    Vector2.up * 1.5f + Vector2.right * 0.3f,
                    Vector2.up * 1.8f + Vector2.right * 0.5f,
                    Vector2.up * 2f + Vector2.right * 0.2f,
                    Vector2.up * 2.2f - Vector2.right * 0.3f,
                    Vector2.up * 2.5f - Vector2.right * 0.5f });

            GenerateRecoilPattern("M4A1", 700f, 25,
                new Vector2[] { Vector2.up * 0.4f, Vector2.up * 0.6f, Vector2.up * 0.9f,
                    Vector2.up * 1.1f + Vector2.right * 0.2f,
                    Vector2.up * 1.3f + Vector2.right * 0.4f,
                    Vector2.up * 1.5f,
                    Vector2.up * 1.7f - Vector2.right * 0.3f,
                    Vector2.up * 1.9f - Vector2.right * 0.4f });

            CreateTrainingSettings();

            Debug.Log("Default assets generated successfully!");
        }

        static void GenerateRecoilPattern(string name, float rpm, int length, Vector2[] keyPoints)
        {
            string path = $"Assets/ScriptableObjects/RecoilPatterns/{name}_Recoil.asset";
            if (File.Exists(path)) return;

            var pattern = ScriptableObject.CreateInstance<SprayPattern>();
            pattern.name = $"{name} Recoil Pattern";
            pattern.fireRateRPM = rpm;
            pattern.patternLength = length;

            Keyframe[] xKeys = new Keyframe[keyPoints.Length];
            Keyframe[] yKeys = new Keyframe[keyPoints.Length];

            for (int i = 0; i < keyPoints.Length; i++)
            {
                float t = (float)i / (keyPoints.Length - 1);
                xKeys[i] = new Keyframe(t, keyPoints[i].x);
                yKeys[i] = new Keyframe(t, keyPoints[i].y);
            }

            pattern.recoilCurveX = new AnimationCurve(xKeys);
            pattern.recoilCurveY = new AnimationCurve(yKeys);

            AssetDatabase.CreateAsset(pattern, path);
            AssetDatabase.SaveAssets();
        }

        static void CreateTrainingSettings()
        {
            string path = "Assets/Resources/DefaultTrainingSettings.asset";
            if (File.Exists(path)) return;

            var settings = ScriptableObject.CreateInstance<TrainingSettings>();
            settings.name = "Default Training Settings";

            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
        }
    }
}
