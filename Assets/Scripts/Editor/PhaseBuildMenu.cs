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
            ("Assets/Scenes/DS02_Fase1_Layout.unity", PhaseTheme.Fase1),
            ("Assets/Scenes/DS03_Fase2_Layout.unity", PhaseTheme.Fase2),
        };

        [MenuItem("Dreamscape/Fases/Gerar todas as fases novas")]
        public static void BuildNewPhases()
        {
            var builders = new List<IPhaseBuilder>();
            foreach (Type t in TypeCache.GetTypesDerivedFrom<IPhaseBuilder>())
                if (!t.IsAbstract && !t.IsInterface)
                    builders.Add((IPhaseBuilder)Activator.CreateInstance(t));
            builders.Sort((a, b) => a.Order.CompareTo(b.Order));

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
        }
    }
}
