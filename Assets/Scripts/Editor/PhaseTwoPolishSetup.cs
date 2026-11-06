using System;
using Dreamscape.Phases;
using Dreamscape.PowerUps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    public static class PhaseTwoPolishSetup
    {
        const string MaterialFolder = "Assets/Design/DS15";

        [MenuItem("Dreamscape/DS-15/Ajustar cena de demonstracao")]
        public static void Setup()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var scene = EditorSceneManager.OpenScene(PhaseTwoTrapSetup.ScenePath);
            SetupActiveScene();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Dreamscape/DS-15/Ajustar Fase 2 aberta")]
        public static void SetupActiveScene()
        {
            GameObject pickup = Required("Coletavel de velocidade [DS11]");
            GameObject fallZone = Required("Zona de queda [DS07]");
            GameObject introduction = Required("Inicio");
            var tiles = new GameObject[4];
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = Required($"Piso temporario {i + 1} [DS09]");
            AudioClip collection = Sound("Coleta");
            AudioClip failure = Sound("Erro");
            AudioClip success = Sound("Acerto");

            PhaseTwoTrapSetup.SetupActiveScene();
            PowerUpSetupMenu.Setup();
            Undo.SetCurrentGroupName("Ajustar Fase 2 DS-15");
            int group = Undo.GetCurrentGroup();
            var bridge = UnityEngine.Object.FindAnyObjectByType<TemporaryBridge>();
            var bridgeSettings = new SerializedObject(bridge);
            bridgeSettings.FindProperty("collapseDelay").floatValue = 1.6f;
            bridgeSettings.ApplyModifiedProperties();
            PhaseTwoTrapSetup.Sign(introduction, "PEGUE A VELOCIDADE\nSHIFT PARA CORRER", new Vector3(-2.5f, 1.5f, 4.5f), 0.035f);

            Material safe = Palette("Seguro", new Color(0.18f, 0.40f, 0.44f));
            foreach (string name in new[] { "Entrada e checkpoint", "Bifurcacao", "Desvio lateral seguro", "Patamar de leitura e acionamento" })
                Paint(Required(name), safe);
            Paint(Required("Rota direta enganosa - termina em queda [DS09]"), Palette("Enganoso", new Color(0.64f, 0.38f, 0.35f)));
            Material temporary = Palette("Temporario", new Color(0.55f, 0.48f, 0.70f));
            foreach (GameObject tile in tiles)
                Paint(tile, temporary);
            Material arrival = Palette("Chegada", new Color(0.46f, 0.68f, 0.56f));
            Paint(Required("Patamar final"), arrival);
            Paint(Required("Portal de conclusao [DS13]"), arrival);
            Material speed = Palette("Velocidade", new Color(0.90f, 0.76f, 0.35f));
            Paint(pickup, speed);
            Paint(bridge.transform.Find("Faixa visivel").gameObject, speed);

            GameObject root = GameObject.Find("DS15 - Retorno audiovisual");
            if (root == null)
            {
                root = new GameObject("DS15 - Retorno audiovisual");
                Undo.RegisterCreatedObjectUndo(root, root.name);
            }
            var feedback = GetOrAdd<PhaseTwoFeedback>(root);
            var serialized = new SerializedObject(feedback);
            serialized.FindProperty("pickup").objectReferenceValue = pickup.GetComponent<SpeedPickup>();
            serialized.FindProperty("bridge").objectReferenceValue = bridge;
            serialized.FindProperty("collectionSound").objectReferenceValue = collection;
            serialized.FindProperty("failureSound").objectReferenceValue = failure;
            serialized.FindProperty("successSound").objectReferenceValue = success;
            serialized.FindProperty("pickupLabel").objectReferenceValue = introduction.GetComponent<TextMesh>();
            var renderers = serialized.FindProperty("floorRenderers");
            renderers.arraySize = tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
                renderers.GetArrayElementAtIndex(i).objectReferenceValue = tiles[i].GetComponent<Renderer>();
            serialized.ApplyModifiedProperties();

            BindTrigger(fallZone, feedback, false);
            Transform finish = root.transform.Find("Chegada da ponte");
            if (finish == null)
            {
                var go = new GameObject("Chegada da ponte", typeof(BoxCollider));
                Undo.RegisterCreatedObjectUndo(go, go.name);
                go.transform.SetParent(root.transform, false);
                finish = go.transform;
            }
            Undo.RecordObject(finish, "Chegada da ponte");
            finish.position = new Vector3(-6f, 1f, 27.8f);
            var box = finish.GetComponent<BoxCollider>();
            Undo.RecordObject(box, "Chegada da ponte");
            box.size = new Vector3(4f, 2f, 0.8f);
            BindTrigger(finish.gameObject, feedback, true);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Undo.CollapseUndoOperations(group);
        }

        static void BindTrigger(GameObject go, PhaseTwoFeedback feedback, bool success)
        {
            var trigger = GetOrAdd<PhaseTwoFeedbackTrigger>(go);
            var serialized = new SerializedObject(trigger);
            serialized.FindProperty("feedback").objectReferenceValue = feedback;
            serialized.FindProperty("success").boolValue = success;
            serialized.ApplyModifiedProperties();
            Undo.RecordObject(go.GetComponent<BoxCollider>(), "Contato da Fase 2");
            go.GetComponent<BoxCollider>().isTrigger = true;
        }

        static Material Palette(string name, Color color)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/Design", "DS15");
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>($"Assets/Design/DS03/{name}.mat"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Paint(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            Undo.RecordObject(renderer, "Paleta da Fase 2");
            renderer.sharedMaterial = material;
        }

        static AudioClip Sound(string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/DS15/{name}.wav");
            if (clip == null)
                throw new InvalidOperationException($"Som DS-15 ausente: {name}");
            return clip;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component =>
            go.TryGetComponent(out T component) ? component : Undo.AddComponent<T>(go);

        static GameObject Required(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
                throw new InvalidOperationException($"Objeto da Fase 2 ausente: {name}");
            return go;
        }
    }
}
