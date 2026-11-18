using System;
using System.Collections.Generic;
using Dreamscape.Characters;
using Dreamscape.Phases;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    public static class PhaseTwoTrapSetup
    {
        public const string ScenePath = "Assets/Scenes/DS09_Fase2_Jogavel.unity";
        const string BridgeName = "DS09 - Ponte temporaria";

        [MenuItem("Dreamscape/DS-09/Criar cena jogavel")]
        public static void CreateTestScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/DS03_Fase2_Layout.unity");
            if (!EditorApplication.ExecuteMenuItem("Dreamscape/Montar personagens na cena"))
                throw new InvalidOperationException("Não foi possível montar os personagens na Fase 2.");
            Vector3 spawn = Required("Spawn - inicio Fase 2").transform.position;
            Required("Player").transform.position = spawn;
            Required("Moema").transform.position = spawn + new Vector3(1.5f, 0f, -1.5f);
            foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>())
            {
                if (camera == Camera.main)
                    continue;
                camera.enabled = false;
                if (camera.TryGetComponent(out AudioListener listener))
                    listener.enabled = false;
            }
            PowerUpSetupMenu.Setup();
            SetupActiveScene();
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = scenes.FindIndex(item => item.path == ScenePath);
            if (index < 0)
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            else
                scenes[index].enabled = true;
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("DS-09: cena jogável criada. Abra-a e use WASD, Shift e Esc para testar.");
        }

        [MenuItem("Dreamscape/DS-09/Configurar pegadinha na cena aberta")]
        public static void SetupActiveScene()
        {
            // Validar o layout antes de editar permite usar a mesma montagem na cena integrada do DS-13.
            var tiles = new GameObject[4];
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = Required($"Piso temporario {i + 1} [DS09]");
            GameObject fallZone = Required("Zona de queda [DS07]");
            GameObject status = Required("Piso");
            GameObject bait = Required("Isca");
            GameObject hint = Required("Pista");
            GameObject detour = Required("Desvio");
            GameObject activation = Required("Acionamento");
            GameObject deadEnd = Required("Fim da isca");

            Undo.SetCurrentGroupName("Configurar pegadinha DS-09");
            int group = Undo.GetCurrentGroup();
            GameObject root = GameObject.Find(BridgeName);
            if (root == null)
            {
                root = new GameObject(BridgeName);
                Undo.RegisterCreatedObjectUndo(root, "Ponte temporária");
            }
            Undo.RecordObject(root.transform, "Faixa de ativação");
            root.transform.position = new Vector3(-6f, 0f, 18.65f);
            BoxCollider trigger = GetOrAdd<BoxCollider>(root);
            Undo.RecordObject(trigger, "Faixa de ativação");
            trigger.isTrigger = true;
            trigger.center = Vector3.up;
            trigger.size = new Vector3(4f, 2f, 0.35f);

            if (root.transform.Find("Faixa visivel") == null)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "Faixa visivel";
                Undo.RegisterCreatedObjectUndo(stripe, "Faixa visível");
                stripe.transform.SetParent(root.transform, false);
                stripe.transform.localPosition = new Vector3(0f, 0.025f, 0f);
                stripe.transform.localScale = new Vector3(4f, 0.05f, 0.35f);
                Undo.DestroyObjectImmediate(stripe.GetComponent<BoxCollider>());
                stripe.GetComponent<Renderer>().sharedMaterial =
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Design/DS03/Velocidade.mat");
            }

            TemporaryBridge bridge = GetOrAdd<TemporaryBridge>(root);
            var serialized = new SerializedObject(bridge);
            SerializedProperty floors = serialized.FindProperty("floorTiles");
            floors.arraySize = tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
                floors.GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
            serialized.FindProperty("statusLabel").objectReferenceValue = status.GetComponent<TextMesh>();
            serialized.ApplyModifiedProperties();

            GetOrAdd<PhaseFallZone>(fallZone);
            Undo.RecordObject(fallZone.GetComponent<BoxCollider>(), "Zona de queda");
            fallZone.GetComponent<BoxCollider>().isTrigger = true;
            Sign(bait, "CORRA RETO ^", new Vector3(0f, 2.2f, 9.5f));
            Sign(hint, "OLHE PARA O LADO\nSIGA AS MARCAS", new Vector3(-6f, 2.4f, 9.2f));
            Sign(detour, "DESVIO", new Vector3(-6f, 0.1f, 13f));
            detour.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Sign(activation, "CRUZE A FAIXA", new Vector3(-6f, 0.1f, 17f));
            activation.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Sign(status, $"PONTE TEMPORARIA\nA FAIXA INICIA {bridge.CollapseDelay:0.0} s",
                new Vector3(-8f, 2.4f, 28f), 0.07f);
            Sign(deadEnd, "SAIDA ^", new Vector3(0f, 0.1f, 16f));
            deadEnd.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Undo.CollapseUndoOperations(group);
        }

        internal static void Sign(GameObject go, string text, Vector3 position, float characterSize = 0.045f)
        {
            var label = go.GetComponent<TextMesh>();
            Undo.RecordObject(go.transform, "Pista DS-09");
            Undo.RecordObject(label, "Pista DS-09");
            go.transform.SetPositionAndRotation(position, Quaternion.identity);
            go.transform.localScale = Vector3.one;
            label.text = text;
            label.characterSize = characterSize;
            label.fontSize = 64;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;

            Transform background = go.transform.Find("Fundo DS09");
            if (background == null)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "Fundo DS09";
                Undo.RegisterCreatedObjectUndo(panel, "Fundo da placa");
                Undo.DestroyObjectImmediate(panel.GetComponent<BoxCollider>());
                panel.transform.SetParent(go.transform, false);
                background = panel.transform;
            }
            Undo.RecordObject(background, "Fundo da placa");
            Bounds bounds = go.GetComponent<Renderer>().localBounds;
            background.localPosition = bounds.center + Vector3.forward * 0.04f;
            background.localRotation = Quaternion.identity;
            background.localScale = new Vector3(bounds.size.x + 0.3f, bounds.size.y + 0.2f, 0.03f);
            var renderer = background.GetComponent<Renderer>();
            Undo.RecordObject(renderer, "Fundo da placa");
            renderer.sharedMaterial = SignMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        static Material SignMaterial()
        {
            const string folder = "Assets/Design/DS09";
            const string path = folder + "/SignBackground.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Design", "DS09");
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", new Color(0.015f, 0.025f, 0.04f));
            AssetDatabase.CreateAsset(material, path);
            return material;
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
