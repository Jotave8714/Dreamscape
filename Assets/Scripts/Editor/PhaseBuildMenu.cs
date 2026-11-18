using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Gera as fases novas por código (cada <see cref="IPhaseBuilder"/>) e melhora a iluminação das existentes.
    /// Em batchmode: Unity -batchmode -quit -projectPath . -executeMethod Dreamscape.EditorTools.PhaseBuildMenu.BuildAllBatch
    /// </summary>
    public static class PhaseBuildMenu
    {
        static readonly (string scene, PhaseTheme theme)[] ExistingPhases =
        {
            ("Assets/Scenes/DS07_Fase1_Jogavel.unity", PhaseTheme.Fase1),
            ("Assets/Scenes/DS09_Fase2_Jogavel.unity", PhaseTheme.Fase2),
            ("Assets/Scenes/DS02_Fase1_Layout.unity", PhaseTheme.Fase1),
            ("Assets/Scenes/DS03_Fase2_Layout.unity", PhaseTheme.Fase2),
        };

        static List<IPhaseBuilder> Builders()
        {
            var builders = new List<IPhaseBuilder>();
            foreach (Type t in TypeCache.GetTypesDerivedFrom<IPhaseBuilder>())
                if (!t.IsAbstract && !t.IsInterface)
                    builders.Add((IPhaseBuilder)Activator.CreateInstance(t));
            builders.Sort((a, b) => a.Order.CompareTo(b.Order));
            return builders;
        }

        /// <summary>Gera só a fase com essa cena (nome sem extensão), por exemplo "Fase8_SaguaoDosSussurros".</summary>
        public static void BuildPhase(string sceneName) =>
            Build(Builders().FindAll(b => b.SceneName == sceneName));

        [MenuItem("Dreamscape/Fases/Gerar todas as fases novas")]
        public static void BuildNewPhases() => Build(Builders());

        /// <summary>
        /// Reordena as cenas "FaseN_..." do build pelo número N, mantendo-as no mesmo bloco (entre as Fases 1-2 e o
        /// encerramento). Gerar uma fase isolada a insere antes do encerramento; isto a coloca no lugar certo.
        /// </summary>
        [MenuItem("Dreamscape/Fases/Ordenar fases no build")]
        public static void SortBuildOrder()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var slots = new List<int>();
            var phases = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < scenes.Count; i++)
                if (PhaseNumber(scenes[i].path) > 0)
                {
                    slots.Add(i);
                    phases.Add(scenes[i]);
                }
            phases.Sort((a, b) => PhaseNumber(a.path).CompareTo(PhaseNumber(b.path)));
            for (int i = 0; i < slots.Count; i++)
                scenes[slots[i]] = phases[i];
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static int PhaseNumber(string path)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!name.StartsWith("Fase"))
                return 0;
            int end = 4;
            while (end < name.Length && char.IsDigit(name[end]))
                end++;
            return end > 4 ? int.Parse(name.Substring(4, end - 4)) : 0;
        }

        static void Build(List<IPhaseBuilder> builders)
        {
            if (builders.Count == 0)
                Debug.LogWarning("Dreamscape: nenhuma fase encontrada para gerar.");
            foreach (IPhaseBuilder builder in builders)
            {
                try
                {
                    PhaseKit kit = PhaseKit.Begin(builder.SceneName, builder.Theme);
                    builder.Build(kit);
                    kit.Finish();
                    Debug.Log($"Dreamscape: fase {builder.Order} gerada ({builder.SceneName}).");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Dreamscape: falha ao gerar {builder.SceneName}: {e}");
                    if (Application.isBatchMode)
                        EditorApplication.Exit(1);
                }
            }
            SortBuildOrder();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Dreamscape/Fases/Melhorar iluminação das fases existentes")]
        public static void LightExistingPhases()
        {
            foreach ((string path, PhaseTheme theme) in ExistingPhases)
            {
                if (!System.IO.File.Exists(path))
                    continue;
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                PhaseLighting.Apply(scene, theme);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Dreamscape: iluminação aplicada em {path}.");
            }
            AssetDatabase.SaveAssets();
        }

        public static void BuildAllBatch()
        {
            AssetDatabase.Refresh();
            BuildNewPhases();
            LightExistingPhases();
            ExistingPhaseDressing.DressAll();
        }
    }
}
