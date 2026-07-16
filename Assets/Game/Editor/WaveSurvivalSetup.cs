using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WaveSurvival
{
    public class WaveSurvivalSetup : EditorWindow
    {
        [MenuItem("Wave Survival/Setup Scene")]
        public static void SetupScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,
                NewSceneMode.Single);

            var initializer = new GameObject("GameInitializer");
            initializer.AddComponent<GameInitializer>();

            string path = "Assets/Game/Scenes/WaveSurvival.unity";
            if (!AssetDatabase.IsValidFolder("Assets/Game/Scenes"))
                AssetDatabase.CreateFolder("Assets/Game", "Scenes");

            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.SaveAssets();

            Debug.Log("Wave Survival scene created! Press Play to start.");
        }

        [MenuItem("Wave Survival/Setup Scene", true)]
        static bool ValidateSetup() => true;
    }
}
