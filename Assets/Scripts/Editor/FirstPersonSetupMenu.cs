using System.Linq;
using Dreamscape.CameraSystem;
using Dreamscape.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Dreamscape.EditorTools
{
    public static class FirstPersonSetupMenu
    {
        public const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        public const string ScenePath = "Assets/Scenes/DS04_Movimento.unity";

        /// <summary>Configura a mesma visão em primeira pessoa para jogadores com ou sem modelo.</summary>
        public static void ConfigurePlayerView(GameObject player, Camera camera)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            ThirdPersonCamera orbit = camera.GetComponent<ThirdPersonCamera>();
            if (orbit != null)
                Undo.DestroyObjectImmediate(orbit);

            Undo.RecordObject(camera, "Câmera em primeira pessoa");
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f;
            Undo.SetTransformParent(camera.transform, player.transform, "Câmera do jogador");
            Undo.RecordObject(camera.transform, "Posição dos olhos");
            camera.transform.localPosition = controller.center + Vector3.up * (controller.height * 0.5f - 0.12f);
            camera.transform.localRotation = Quaternion.identity;

            FirstPersonCamera view = camera.GetComponent<FirstPersonCamera>();
            if (view == null)
                view = Undo.AddComponent<FirstPersonCamera>(camera.gameObject);
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("character").objectReferenceValue = controller;
            serializedView.ApplyModifiedProperties();

            var serializedInput = new SerializedObject(player.GetComponent<PlayerInputController>());
            serializedInput.FindProperty("cameraTransform").objectReferenceValue = camera.transform;
            serializedInput.ApplyModifiedProperties();
            var serializedMotor = new SerializedObject(player.GetComponent<CharacterMotor>());
            serializedMotor.FindProperty("rotateTowardsMovement").boolValue = false;
            serializedMotor.ApplyModifiedProperties();

            // A cabeça e o corpo não cobrem a visão por dentro; a sombra do protagonista permanece.
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>(true))
            {
                Undo.RecordObject(renderer, "Modelo em primeira pessoa");
                renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
        }

        [MenuItem("Dreamscape/Converter fases para primeira pessoa")]
        public static void ConvertPlayableScenes()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes.Where(s => s.enabled))
                {
                    Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                    ThirdPersonCamera[] cameras = Object.FindObjectsByType<ThirdPersonCamera>(FindObjectsInactive.Include);
                    if (cameras.Length == 0)
                        continue;
                    foreach (ThirdPersonCamera orbit in cameras)
                    {
                        var serializedOrbit = new SerializedObject(orbit);
                        Transform target = serializedOrbit.FindProperty("target").objectReferenceValue as Transform;
                        GameObject player = target != null ? target.gameObject : Object.FindAnyObjectByType<PlayerInputController>().gameObject;
                        ConfigurePlayerView(player, orbit.GetComponent<Camera>());
                    }
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"FIRST_PERSON_CONVERTED: {entry.path}");
                }
            }
            finally
            {
                if (previous.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        [MenuItem("Dreamscape/Criar cena de teste DS-04")]
        public static void CreateTestScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject player = new GameObject("Player") { tag = "Player" };
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.76f;
            controller.radius = 0.28f;
            controller.center = Vector3.up * 0.88f;
            controller.skinWidth = 0.028f;
            controller.stepOffset = 0.3f;
            controller.minMoveDistance = 0f;
            player.AddComponent<CharacterMotor>();
            player.AddComponent<PlayerInputController>();

            GameObject view = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            ConfigurePlayerView(player, view.GetComponent<Camera>());

            PrefabUtility.SaveAsPrefabAssetAndConnect(player, PlayerPath, InteractionMode.AutomatedAction);
            player.transform.position = new Vector3(0f, 0.1f, -5f);

            Block("Chao", new Vector3(0f, -0.5f, 0f), new Vector3(20f, 1f, 20f));
            Block("Parede para colisao", new Vector3(0f, 1.5f, 4f), new Vector3(8f, 3f, 0.5f));
            Block("Teto para agachar", new Vector3(-4f, 1.35f, 0f), new Vector3(3f, 0.3f, 4f));
            Block("Plataforma para salto", new Vector3(4f, 0.3f, 0f), new Vector3(3f, 0.6f, 3f));
            var light = new GameObject("Directional Light", typeof(Light));
            light.GetComponent<Light>().type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player;
        }

        static void Block(string name, Vector3 position, Vector3 scale)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
        }
    }
}
