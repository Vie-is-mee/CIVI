using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Civi.EditorTools
{
    public static class CiviBuild
    {
        [MenuItem("CIVI/Build Windows")]
        public static void Build()
        {
            CiviProjectSetup.Run();
            CiviSceneBuilder.Build();
            string output = Path.GetFullPath("../release/CIVI/CIVI.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { CiviSceneBuilder.ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            var s = report.summary;
            Debug.Log("CIVI build " + s.result + " " + (s.totalSize / 1048576) + " MB, " + s.totalErrors + " errors, " + (int)s.totalTime.TotalSeconds + " s");
            if (s.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
