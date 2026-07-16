using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WaveSurvival
{
    public class MainMenuSetup : EditorWindow
    {
        [MenuItem("Wave Survival/Create Main Menu")]
        public static void CreateMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,
                NewSceneMode.Single);

            MainMenu.CreateMainMenuScene();

            if (!AssetDatabase.IsValidFolder("Assets/Game/Scenes"))
                AssetDatabase.CreateFolder("Assets/Game", "Scenes");

            EditorSceneManager.SaveScene(scene, "Assets/Game/Scenes/MainMenu.unity");
            AssetDatabase.SaveAssets();

            // Add both scenes to build settings
            var buildSettings = new EditorBuildSettingsScene[2];
            buildSettings[0] = new EditorBuildSettingsScene("Assets/Game/Scenes/MainMenu.unity", true);
            buildSettings[1] = new EditorBuildSettingsScene("Assets/Game/Scenes/WaveSurvival.unity", true);
            EditorBuildSettings.scenes = buildSettings;

            Debug.Log("Main Menu scene created! Set as scene 0 in Build Settings.");
        }
    }
}
