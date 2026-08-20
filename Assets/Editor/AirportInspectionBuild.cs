using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AirportInspection.Editor
{
    public static class AirportInspectionBuild
    {
        private const string ScenePath = "Assets/Scenes/Inspection.unity";

        [MenuItem("Airport Inspection/Prepare Scene")]
        public static void PrepareScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Inspection";
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        public static void BuildMac()
        {
            PrepareScene();
            Directory.CreateDirectory("Builds/Mac");
            var report = BuildPipeline.BuildPlayer(
                new[] { ScenePath },
                "Builds/Mac/Airport Inspection.app",
                BuildTarget.StandaloneOSX,
                BuildOptions.Development);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new UnityEditor.Build.BuildFailedException($"Build failed: {report.summary.result}");
            Debug.Log($"BUILD_OK size={report.summary.totalSize}");
        }
    }
}
