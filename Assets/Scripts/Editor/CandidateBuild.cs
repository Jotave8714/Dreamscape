using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    public static class CandidateBuild
    {
        const string Menu = "Assets/Scenes/MainMenu.unity";
        const string First = "Assets/Scenes/DS07_Fase1_Jogavel.unity";
        const string Second = "Assets/Scenes/DS09_Fase2_Jogavel.unity";
        const string Ending = "Assets/Scenes/PrototypeEnding.unity";

        public static string[] SelectScenes(string[] configured, string scope)
        {
            if (scope != "full" && scope != "prototype")
                throw new ArgumentException("DS-22: escopo inválido: " + scope);
            if (configured.Length < 4 || configured[0] != Menu || configured[1] != First ||
                configured[2] != Second || configured[configured.Length - 1] != Ending ||
                configured.Distinct().Count() != configured.Length || configured.Any(path => !File.Exists(path)))
                throw new InvalidOperationException("DS-22: configure menu, fases em ordem e encerramento, sem cenas ausentes ou duplicadas.");
            for (int i = 3; i < configured.Length - 1; i++)
            {
                string name = Path.GetFileNameWithoutExtension(configured[i]);
                if (!name.StartsWith($"Fase{i}_", StringComparison.Ordinal))
                    throw new InvalidOperationException($"DS-22: fase {i} ausente ou fora de ordem: {configured[i]}.");
            }
            return scope == "prototype" ? new[] { Menu, First, Second, Ending } : configured;
        }

        [Serializable]
        class Report
        {
            public string version;
            public string unity;
            public string result;
            public string platform;
            public string scope;
            public string architecture;
            public string[] scenes;
            public int errors;
            public int warnings;
        }

        public static void Build()
        {
            string output = Argument("-candidateOutput");
            string version = Argument("-candidateVersion");
            string reportPath = Argument("-candidateReport");
            string platform = Argument("-candidateTarget");
            string scope = Argument("-candidateScope");
            BuildTarget target = Target(platform);
            string[] configured = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                .Select(scene => scene.path).ToArray();
            string[] scenes = SelectScenes(configured, scope);

            string requiredVersion = File.ReadLines("ProjectSettings/ProjectVersion.txt")
                .First(line => line.StartsWith("m_EditorVersion: ")).Substring("m_EditorVersion: ".Length).Trim();
            if (Application.unityVersion != requiredVersion)
                throw new InvalidOperationException($"DS-22: Unity incorreta. Projeto: {requiredVersion}; editor: {Application.unityVersion}.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
                throw new InvalidOperationException($"DS-22: suporte de build não instalado: {platform}.");

            string previousVersion = PlayerSettings.bundleVersion;
            var standalone = UnityEditor.Build.NamedBuildTarget.Standalone;
            ScriptingImplementation previousBackend = PlayerSettings.GetScriptingBackend(standalone);
            string platformName = BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX);
            string previousArchitecture = EditorUserBuildSettings.GetPlatformSettings(platformName, "Architecture");
            try
            {
                PlayerSettings.bundleVersion = version;
                PlayerSettings.SetScriptingBackend(standalone, ScriptingImplementation.Mono2x);
                if (target == BuildTarget.StandaloneOSX)
                    EditorUserBuildSettings.SetPlatformSettings(platformName, "Architecture", "arm64");
                BuildReport build = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = output,
                    target = target,
                    options = BuildOptions.None
                });
                var report = new Report
                {
                    version = version,
                    unity = Application.unityVersion,
                    result = build.summary.result.ToString(),
                    platform = platform,
                    scope = scope,
                    architecture = platform == "macos" ? "ARM64" : "x86_64",
                    scenes = scenes,
                    errors = build.summary.totalErrors,
                    warnings = build.summary.totalWarnings
                };
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                if (build.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException($"DS-22: build falhou: {build.summary.result} ({build.summary.totalErrors} erros).");
                Debug.Log($"DS22_BUILD_OK: {version}; {build.summary.totalErrors} erros.");
            }
            finally
            {
                PlayerSettings.bundleVersion = previousVersion;
                PlayerSettings.SetScriptingBackend(standalone, previousBackend);
                if (target == BuildTarget.StandaloneOSX)
                    EditorUserBuildSettings.SetPlatformSettings(platformName, "Architecture", previousArchitecture);
                AssetDatabase.SaveAssets();
            }
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            if (index < 0 || index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                throw new ArgumentException($"DS-22: argumento obrigatório: {name}");
            return PathArgument(name) ? Path.GetFullPath(args[index + 1]) : args[index + 1];
        }

        static bool PathArgument(string name) => name == "-candidateOutput" || name == "-candidateReport";

        static BuildTarget Target(string platform)
        {
            switch (platform)
            {
                case "macos": return BuildTarget.StandaloneOSX;
                case "windows": return BuildTarget.StandaloneWindows64;
                case "linux": return BuildTarget.StandaloneLinux64;
                default: throw new ArgumentException($"DS-22: plataforma inválida: {platform}");
            }
        }
    }
}
