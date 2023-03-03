using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MultiTanks
{
    public class Helper : EditorWindow
    {
        [MenuItem("File/Build Last &b", false, 220)]
        public static void LaunchLastBuild()
        {
            Debug.Log("Launching last build...");
            Process.Start($"{Application.dataPath}/../Builds/MultiTanks.exe");
        }
    }
}