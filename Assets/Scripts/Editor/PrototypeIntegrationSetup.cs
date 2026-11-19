using System;
using System.Collections.Generic;
using System.IO;
using Dreamscape.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Integra o protótipo: porta de conclusão nas Fases 1 e 2, cena de encerramento e ordem do build
    /// (menu, Fase 1, Fase 2, demais fases, encerramento). A Fase 2 é a cena da pegadinha (DS-09/DS-15).
    /// </summary>
    public static class PrototypeIntegrationSetup
    {
        public const string FirstPhasePath = "Assets/Scenes/DS07_Fase1_Jogavel.unity";
        public const string SecondPhasePath = PhaseTwoTrapSetup.ScenePath;
        public const string EndingPath = "Assets/Scenes/PrototypeEnding.unity";
        const string MenuPath = "Assets/Scenes/MainMenu.unity";
        const string TemplatePath = "Assets/Scenes/SampleScene.unity";

        [UnityEditor.MenuItem("Dreamscape/Integrar prototipo DS-13")]
        public static void Setup()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Scene first = EditorSceneManager.OpenScene(FirstPhasePath, OpenSceneMode.Single);
            BindGoal("N7 - Porta de conclusao [transicao DS13]");
            EditorSceneManager.SaveScene(first);

            Scene second = EditorSceneManager.OpenScene(SecondPhasePath, OpenSceneMode.Single);
            BindGoal("Portal de conclusao [DS13]");
            EditorSceneManager.SaveScene(second);

            Scene ending = File.Exists(EndingPath)
                ? EditorSceneManager.OpenScene(EndingPath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            if (UnityEngine.Object.FindAnyObjectByType<PrototypeEnding>() == null)
                new GameObject("PrototypeEnding").AddComponent<PrototypeEnding>();
            EditorSceneManager.SaveScene(ending, EndingPath);

            // fases extras já no build mantêm a ordem; o encerramento fecha a sequência e demonstrações vêm depois
            var phases = new List<string> { FirstPhasePath, SecondPhasePath };
            var extras = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                string path = existing.path;
                if (path == MenuPath || path == TemplatePath || path == EndingPath || phases.Contains(path))
                    continue;
                if (Path.GetFileName(path).StartsWith("Fase", StringComparison.Ordinal))
                    phases.Add(path);
                else
                    extras.Add(existing);
            }

            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(MenuPath, true) };
            phases.ForEach(path => scenes.Add(new EditorBuildSettingsScene(path, true)));
            scenes.Add(new EditorBuildSettingsScene(EndingPath, true));
            scenes.AddRange(extras);
            scenes.Add(new EditorBuildSettingsScene(TemplatePath, false));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
            Debug.Log("DS-13: menu, fases e encerramento integrados.");
        }

        static void BindGoal(string name)
        {
            GameObject portal = GameObject.Find(name);
            if (portal == null)
                throw new InvalidOperationException($"Objeto de integração ausente: {name}");
            if (!portal.TryGetComponent<PhaseGoal>(out _))
                portal.AddComponent<PhaseGoal>();
            portal.GetComponent<Collider>().isTrigger = true;
        }
    }
}
