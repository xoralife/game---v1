using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using FPSTrainingRoom.Training;

namespace FPSTrainingRoom.EditorTools
{
    public class SceneSetupEditor : EditorWindow
    {
        [MenuItem("FPS Training/Setup Training Scene")]
        public static void SetupTrainingScene()
        {
            // Create scene if needed
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,
                NewSceneMode.Single);

            // Remove default objects if any
            var defaultObjects = scene.GetRootGameObjects();
            foreach (var obj in defaultObjects)
            {
                if (obj.name != "Directional Light" && obj.name != "Main Camera")
                    DestroyImmediate(obj);
            }

            // Add the initializer
            var initializerObj = new GameObject("TrainingRoomInitializer");
            var initializer = initializerObj.AddComponent<TrainingRoomInitializer>();
            initializer.autoSetupOnStart = true;

            // Save scene
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            string scenePath = "Assets/Scenes/ShootingRange.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Training scene created at {scenePath}");
            Debug.Log("Run the scene and the TrainingRoomInitializer will auto-setup everything.");
        }

        [MenuItem("FPS Training/Generate Default Assets", true)]
        public static bool ValidateGenerateAssets()
        {
            return true;
        }
    }
}
