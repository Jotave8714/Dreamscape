using Dreamscape.CameraSystem;
using Dreamscape.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Menu "Dreamscape > Montar personagens na cena": cria (ou atualiza) o Player com o modelo do protagonista,
    /// a Moema acompanhante e a câmera em terceira pessoa, já com CharacterController dimensionado.
    /// Pode rodar de novo sem duplicar nada.
    /// </summary>
    public static class CharacterSetupMenu
    {
        const string ProtagonistModel = "Assets/Art/Characters/Protagonist/Protagonist.fbx";
        const string MoemaModel = "Assets/Art/Characters/Moema/Moema.fbx";

        [MenuItem("Dreamscape/Montar personagens na cena")]
        static void Setup()
        {
            Undo.SetCurrentGroupName("Montar personagens");
            int group = Undo.GetCurrentGroup();

            GameObject player = CreateCharacter("Player", ProtagonistModel, Vector3.zero, height: 1.76f, radius: 0.28f);
            player.tag = "Player";
            Configure<CharacterMotor>(player, ("walkSpeed", 2.2f), ("sprintSpeed", 5.0f), ("jumpHeight", 1.1f));
            GetOrAdd<PlayerInputController>(player);

            // emas são corredoras: a Moema anda parecido e corre mais que o protagonista
            GameObject moema = CreateCharacter("Moema", MoemaModel, new Vector3(1.5f, 0f, -1.5f), height: 1.8f, radius: 0.26f);
            Configure<CharacterMotor>(moema, ("walkSpeed", 2.3f), ("sprintSpeed", 6.5f), ("jumpHeight", 1.3f));
            MoemaCompanion companion = GetOrAdd<MoemaCompanion>(moema);
            SetReference(companion, "target", player.transform);

            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
                Undo.RegisterCreatedObjectUndo(go, "Câmera");
                cam = go.GetComponent<Camera>();
            }
            ThirdPersonCamera tpc = GetOrAdd<ThirdPersonCamera>(cam.gameObject);
            SetReference(tpc, "target", player.transform);
            Undo.RecordObject(cam.transform, "Câmera");
            cam.transform.SetPositionAndRotation(player.transform.position + new Vector3(0f, 2.5f, -4f),
                Quaternion.Euler(15f, 0f, 0f));

            EnsureGround(player.transform.position);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(player.scene);
            Selection.activeGameObject = player;
            Debug.Log("Dreamscape: Player, Moema e câmera montados. Dê Play para testar (WASD, Shift, C, Espaço).");
        }

        static GameObject CreateCharacter(string name, string modelPath, Vector3 position, float height, float radius)
        {
            GameObject root = GameObject.Find(name);
            if (root == null)
            {
                root = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(root, name);
                root.transform.position = SnapToGround(position);
            }

            if (root.transform.childCount == 0)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (model == null)
                {
                    Debug.LogWarning($"Dreamscape: modelo não encontrado em {modelPath}; {name} fica só com o colisor.");
                }
                else
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
                    Undo.RegisterCreatedObjectUndo(instance, name);
                    instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
            }

            CharacterController cc = GetOrAdd<CharacterController>(root);
            Undo.RecordObject(cc, name);
            cc.height = height;
            cc.radius = radius;
            cc.center = new Vector3(0f, height * 0.5f, 0f);
            cc.stepOffset = 0.3f;
            cc.slopeLimit = 50f;
            cc.skinWidth = radius * 0.1f;
            cc.minMoveDistance = 0f;
            GetOrAdd<CharacterMotor>(root);
            return root;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : Undo.AddComponent<T>(go);
        }

        static void Configure<T>(GameObject go, params (string field, float value)[] values) where T : Component
        {
            var so = new SerializedObject(go.GetComponent<T>());
            foreach ((string field, float value) in values)
                so.FindProperty(field).floatValue = value;
            so.ApplyModifiedProperties();
        }

        static void SetReference(Object component, string field, Object value)
        {
            var so = new SerializedObject(component);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        static Vector3 SnapToGround(Vector3 position)
        {
            return Physics.Raycast(position + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f,
                ~0, QueryTriggerInteraction.Ignore)
                ? hit.point
                : position;
        }

        static void EnsureGround(Vector3 at)
        {
            if (Physics.Raycast(at + Vector3.up * 0.5f, Vector3.down, 2f, ~0, QueryTriggerInteraction.Ignore))
                return;
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground (teste)";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
            Undo.RegisterCreatedObjectUndo(ground, "Chão");
            Debug.Log("Dreamscape: nenhum chão sob o Player; criado um plano de 50x50 m para teste.");
        }
    }
}
