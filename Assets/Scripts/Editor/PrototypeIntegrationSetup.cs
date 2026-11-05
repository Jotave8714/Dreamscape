using System;
using System.Collections.Generic;
using System.IO;
using Dreamscape.Characters;
using Dreamscape.Core;
using Dreamscape.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.EditorTools
{
    public static class PrototypeIntegrationSetup
    {
        public const string FirstPhasePath = "Assets/Scenes/DS07_Fase1_Jogavel.unity";
        public const string SecondPhasePath = "Assets/Scenes/DS13_Fase2_Jogavel.unity";
        public const string EndingPath = "Assets/Scenes/PrototypeEnding.unity";
        const string MenuPath = "Assets/Scenes/MainMenu.unity";

        [UnityEditor.MenuItem("Dreamscape/Integrar prototipo DS-13")]
        public static void Setup()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Scene first = EditorSceneManager.OpenScene(FirstPhasePath, OpenSceneMode.Single);
            BindExit("N7 - Porta de conclusao [transicao DS13]");
            PowerUpSetupMenu.Setup();
            DisableReviewCameras();
            EditorSceneManager.SaveScene(first);

            Scene second = EditorSceneManager.OpenScene(File.Exists(SecondPhasePath)
                ? SecondPhasePath : "Assets/Scenes/DS03_Fase2_Layout.unity", OpenSceneMode.Single);
            if (!EditorApplication.ExecuteMenuItem("Dreamscape/Montar personagens na cena"))
                throw new InvalidOperationException("Não foi possível montar os personagens na Fase 2.");
            GameObject player = GameObject.Find("Player");
            Vector3 spawn = Required("Spawn - inicio Fase 2").transform.position;
            player.transform.position = spawn;
            GameObject.Find("Moema").transform.position = spawn + new Vector3(1.5f, 0f, -1.5f);

            DisableReviewCameras();

            GameObject fallZone = Required("Zona de queda [DS07]");
            if (!fallZone.TryGetComponent<PhaseFallZone>(out _))
                fallZone.AddComponent<PhaseFallZone>();
            fallZone.GetComponent<BoxCollider>().isTrigger = true;
            BindExit("Portal de conclusao [DS13]");
            PowerUpSetupMenu.Setup();
            EditorSceneManager.SaveScene(second, SecondPhasePath);

            Scene ending = File.Exists(EndingPath)
                ? EditorSceneManager.OpenScene(EndingPath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            if (UnityEngine.Object.FindAnyObjectByType<PrototypeEnding>() == null)
                new GameObject("PrototypeEnding").AddComponent<PrototypeEnding>();
            EditorSceneManager.SaveScene(ending, EndingPath);

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in new[] { MenuPath, FirstPhasePath, SecondPhasePath, EndingPath })
                scenes.Add(new EditorBuildSettingsScene(path, true));
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (scenes.Exists(scene => scene.path == existing.path))
                    continue;
                scenes.Add(new EditorBuildSettingsScene(existing.path,
                    existing.enabled && existing.path != "Assets/Scenes/SampleScene.unity"));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
            Debug.Log("DS-13: menu, Fase 1, Fase 2 e encerramento integrados.");
        }

        static void BindExit(string name)
        {
            GameObject portal = Required(name);
            if (!portal.TryGetComponent<PhaseExit>(out _))
                portal.AddComponent<PhaseExit>();
            portal.GetComponent<BoxCollider>().isTrigger = true;
        }

        static void DisableReviewCameras()
        {
            Camera main = Camera.main;
            foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>())
            {
                if (camera == main)
                    continue;
                camera.enabled = false;
                if (camera.TryGetComponent(out AudioListener listener))
                    listener.enabled = false;
            }
        }

        static GameObject Required(string name)
        {
            GameObject found = GameObject.Find(name);
            if (found == null)
                throw new InvalidOperationException($"Objeto de integração ausente: {name}");
            return found;
        }
    }
}
