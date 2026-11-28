using System.Collections.Generic;
using System.Linq;
using Dreamscape.CameraSystem;
using Dreamscape.Characters;
using Dreamscape.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Dreamscape.Tests
{
    public sealed class FirstPersonSceneTests
    {
        public static IEnumerable<string> PlayableScenes => EditorBuildSettings.scenes
            .Where(s => s.enabled && s.path != "Assets/Scenes/MainMenu.unity" &&
                        s.path != "Assets/Scenes/PrototypeEnding.unity")
            .Select(s => s.path);

        [TestCaseSource(nameof(PlayableScenes))]
        public void CampaignSceneUsesOneCameraAtThePlayersEyes(string path)
        {
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                PlayerInputController[] players = Object.FindObjectsByType<PlayerInputController>(FindObjectsInactive.Exclude);
                Assert.That(players.Length, Is.EqualTo(1), path);
                GameObject player = players[0].gameObject;
                Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude).Where(c => c.enabled).ToArray();
                Assert.That(cameras.Length, Is.EqualTo(1), path);
                Camera camera = cameras[0];
                Assert.That(camera.GetComponent<FirstPersonCamera>(), Is.Not.Null, path);
                Assert.That(Object.FindObjectsByType<ThirdPersonCamera>(FindObjectsInactive.Include), Is.Empty, path);
                Assert.That(camera.transform.IsChildOf(player.transform), Is.True, path);
                CharacterController controller = player.GetComponent<CharacterController>();
                Vector3 eye = controller.transform.TransformPoint(controller.center + Vector3.up * (controller.height * 0.5f - 0.12f));
                Assert.That(Vector3.Distance(camera.transform.position, eye), Is.LessThan(0.001f), path);
                Assert.That(Quaternion.Angle(camera.transform.rotation, player.transform.rotation), Is.LessThan(0.001f), path);
                Assert.That(camera.nearClipPlane, Is.EqualTo(0.05f).Within(0.001f), path);
                Assert.That(new SerializedObject(players[0]).FindProperty("cameraTransform").objectReferenceValue,
                    Is.EqualTo(camera.transform), path);
                Assert.That(new SerializedObject(player.GetComponent<CharacterMotor>()).FindProperty("rotateTowardsMovement").boolValue,
                    Is.False, path);
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude).Count(a => a.enabled), Is.EqualTo(1), path);
                foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>(true))
                    Assert.That(renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.ShadowsOnly), renderer.name);
            }
            finally
            {
                if (previous.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        [Test]
        public void CharacterSetupReplacesOrbitWithoutHidingCompanionOrDuplicatingCamera()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            Scene previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            try
            {
                var player = new GameObject("Player", typeof(CharacterController), typeof(CharacterMotor), typeof(PlayerInputController));
                player.transform.SetPositionAndRotation(new Vector3(3f, 1f, 7f), Quaternion.Euler(0f, 90f, 0f));
                CharacterController controller = player.GetComponent<CharacterController>();
                controller.height = 1.76f;
                controller.center = Vector3.up * 0.88f;
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.transform.SetParent(player.transform, false);
                var companion = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(ThirdPersonCamera)).GetComponent<Camera>();

                FirstPersonSetupMenu.ConfigurePlayerView(player, camera);
                FirstPersonSetupMenu.ConfigurePlayerView(player, camera);

                Assert.That(camera.GetComponent<ThirdPersonCamera>(), Is.Null);
                Assert.That(camera.GetComponents<FirstPersonCamera>().Length, Is.EqualTo(1));
                Assert.That(camera.transform.parent, Is.EqualTo(player.transform));
                Assert.That(camera.transform.localPosition.y, Is.EqualTo(1.64f).Within(0.001f));
                Assert.That(Quaternion.Angle(camera.transform.rotation, player.transform.rotation), Is.LessThan(0.001f));
                Assert.That(new SerializedObject(camera.GetComponent<FirstPersonCamera>()).FindProperty("character").objectReferenceValue,
                    Is.EqualTo(controller));
                Assert.That(body.GetComponent<Renderer>().shadowCastingMode, Is.EqualTo(ShadowCastingMode.ShadowsOnly));
                Assert.That(companion.GetComponent<Renderer>().shadowCastingMode, Is.EqualTo(ShadowCastingMode.On));
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
