using Dreamscape.CameraSystem;
using Dreamscape.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.EditorTools
{
    public static class FirstPersonSetupMenu
    {
        public const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        public const string ScenePath = "Assets/Scenes/DS04_Movimento.unity";

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
            CharacterMotor motor = player.AddComponent<CharacterMotor>();
            var serializedMotor = new SerializedObject(motor);
            serializedMotor.FindProperty("rotateTowardsMovement").boolValue = false;
            serializedMotor.ApplyModifiedPropertiesWithoutUndo();
            PlayerInputController input = player.AddComponent<PlayerInputController>();

            GameObject view = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            view.transform.SetParent(player.transform, false);
            view.transform.localPosition = Vector3.up * 1.64f;
            view.GetComponent<Camera>().nearClipPlane = 0.05f;
            view.AddComponent<FirstPersonCamera>();
            var serializedInput = new SerializedObject(input);
            serializedInput.FindProperty("cameraTransform").objectReferenceValue = view.transform;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

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
