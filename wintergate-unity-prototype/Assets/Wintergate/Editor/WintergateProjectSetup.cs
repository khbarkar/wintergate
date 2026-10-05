using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wintergate.Editor
{
    [InitializeOnLoad]
    public static class WintergateProjectSetup
    {
        private const string ScenePath = "Assets/Wintergate/WintergateCalendar.unity";

        static WintergateProjectSetup()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        [MenuItem("Wintergate/Rebuild prototype scene")]
        public static void EnsureScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Wintergate Calendar App").AddComponent<WintergateCalendarApp>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.companyName = "Wintergate Studio";
            PlayerSettings.productName = "Wintergate Prototype";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.wintergate.proto");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            AssetDatabase.SaveAssets();
            Debug.Log("Wintergate prototype scene created. Enable OpenXR for Android, then press Play.");
        }
    }
}
