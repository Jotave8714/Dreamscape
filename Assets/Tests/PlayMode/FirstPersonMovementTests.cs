#if UNITY_EDITOR
using System.Collections;
using Dreamscape.CameraSystem;
using Dreamscape.Characters;
using Dreamscape.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Dreamscape.Tests
{
    public sealed class FirstPersonMovementTests
    {
        GameObject player, ground, obstacle;
        CharacterMotor motor;
        PlayerInputController input;
        Camera view;
        Keyboard keyboard;
        Mouse mouse;
        InputSettings.BackgroundBehavior previousBackground;
        InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
        float previousCaptureDeltaTime;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GamePause.Resume();
            previousCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            previousBackground = InputSystem.settings.backgroundBehavior;
            previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.actions.Enable();
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            if (!Application.isBatchMode)
                EditorApplication.ExecuteMenuItem("Window/General/Game");

            ground = Block(new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 40f));
            Physics.SyncTransforms();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FirstPersonPlayer.prefab");
            Assert.That(prefab, Is.Not.Null, "O prefab reutilizável deve estar salvo no projeto.");
            player = Object.Instantiate(prefab, Vector3.up * 0.05f, Quaternion.identity);
            motor = player.GetComponent<CharacterMotor>();
            input = player.GetComponent<PlayerInputController>();
            view = player.GetComponentInChildren<Camera>();
            yield return new WaitForSeconds(0.15f);
            Assert.That(motor.IsGrounded, Is.True,
                $"Posição: {player.transform.position}; colisão Default ignorada: {Physics.GetIgnoreLayerCollision(0, 0)}.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            Object.Destroy(player);
            Object.Destroy(ground);
            Object.Destroy(obstacle);
            yield return null;
            InputSystem.RemoveDevice(keyboard);
            InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
            Time.captureDeltaTime = previousCaptureDeltaTime;
        }

        [UnityTest]
        public IEnumerator WalkSprintAndCrouchUseExpectedSpeeds()
        {
            Keys(Key.W);
            yield return new WaitForSeconds(0.35f);
            Assert.That(motor.HorizontalSpeed, Is.EqualTo(2.2f).Within(0.1f));
            Assert.That(player.transform.position.z, Is.GreaterThan(0.2f));

            Keys(Key.W, Key.LeftShift);
            yield return new WaitForSeconds(0.35f);
            Assert.That(motor.HorizontalSpeed, Is.EqualTo(5f).Within(0.1f));

            float standingEye = view.transform.position.y - player.transform.position.y;
            Keys(Key.W, Key.LeftShift, Key.C);
            yield return new WaitForSeconds(0.35f);
            Assert.That(motor.IsCrouching, Is.True);
            Assert.That(motor.HorizontalSpeed, Is.EqualTo(1.2f).Within(0.1f));
            Assert.That(view.transform.position.y - player.transform.position.y, Is.LessThan(standingEye - 0.5f));
        }

        [UnityTest]
        public IEnumerator DiagonalMovementDoesNotIncreaseSpeed()
        {
            Keys(Key.W, Key.D);
            yield return new WaitForSeconds(0.35f);
            Assert.That(motor.HorizontalSpeed, Is.EqualTo(motor.WalkSpeed).Within(0.1f));
        }

        [UnityTest]
        public IEnumerator CharacterCannotCrossAWall()
        {
            obstacle = Block(new Vector3(0f, 1.5f, 1f), new Vector3(8f, 3f, 0.5f));
            Physics.SyncTransforms();
            Keys(Key.W, Key.LeftShift);
            yield return new WaitForSeconds(0.8f);
            Assert.That(player.transform.position.z, Is.InRange(0.35f, 0.55f));
            Assert.That(view.transform.position.z, Is.LessThan(0.75f));
        }

        [UnityTest]
        public IEnumerator RepeatedJumpPressesCannotCreateUnlimitedBasicJumps()
        {
            int jumps = 0;
            motor.Jumped += () => jumps++;
            Keys(Key.Space);
            yield return null;
            Keys();
            yield return new WaitForSeconds(0.12f);
            Assert.That(jumps, Is.EqualTo(1));
            for (int i = 0; i < 3; i++)
            {
                Keys(Key.Space);
                yield return null;
                Keys();
                yield return null;
            }
            Assert.That(jumps, Is.EqualTo(1));
            Assert.That(player.transform.position.y, Is.GreaterThan(0.2f));
            yield return new WaitForSeconds(0.8f);
            Assert.That(motor.IsGrounded, Is.True);
            Keys(Key.Space);
            yield return null;
            yield return null;
            Assert.That(jumps, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator LowCeilingPreventsStandingUntilPlayerLeaves()
        {
            Keys(Key.C);
            yield return new WaitForSeconds(0.3f);
            Keys();
            yield return null;
            obstacle = Block(new Vector3(0f, 1.35f, 0f), new Vector3(3f, 0.3f, 3f));
            Physics.SyncTransforms();
            Keys(Key.C);
            yield return new WaitForSeconds(0.3f);
            Assert.That(motor.IsCrouching, Is.True);
            Assert.That(motor.Controller.height, Is.LessThan(1.2f));

            motor.Teleport(new Vector3(5f, 0.05f, 0f), Quaternion.identity);
            Keys();
            yield return new WaitForSeconds(0.3f);
            Assert.That(motor.IsCrouching, Is.False);
            Assert.That(motor.Controller.height, Is.EqualTo(1.76f).Within(0.02f));
        }

        [UnityTest]
        public IEnumerator MouseLookClampsPitchAndMovementFollowsView()
        {
            InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(750f, 2000f));
            yield return null;
            yield return null;
            Assert.That(Vector3.Angle(Vector3.forward, Vector3.ProjectOnPlane(view.transform.forward, Vector3.up)),
                Is.EqualTo(90f).Within(2f));
            Assert.That(Vector3.Angle(Vector3.up, view.transform.forward), Is.EqualTo(5f).Within(2f));

            Keys(Key.W);
            yield return new WaitForSeconds(0.35f);
            Assert.That(player.transform.position.x, Is.GreaterThan(0.2f));
            Assert.That(Mathf.Abs(player.transform.position.z), Is.LessThan(0.1f));
        }

        [UnityTest]
        public IEnumerator PauseFreezesLookAndMovementAndResumeRestoresControl()
        {
            Keys(Key.W);
            yield return new WaitForSeconds(0.3f);
            motor.RequestJump();
            input.SetInputEnabled(false);
            GamePause.Pause();
            Vector3 position = player.transform.position;
            Quaternion rotation = view.transform.rotation;
            Keys(Key.W, Key.Space);
            InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(500f, 100f));
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Vector3.Distance(player.transform.position, position), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(view.transform.rotation, rotation), Is.LessThan(0.001f));
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));

            Keys();
            yield return null;
            input.SetInputEnabled(true);
            GamePause.Resume();
            yield return new WaitForSeconds(0.2f);
            Assert.That(motor.IsGrounded, Is.True, "Pulo anterior à pausa não deve sair ao retomar.");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked));
            Keys(Key.W);
            yield return new WaitForSeconds(0.3f);
            Assert.That(player.transform.position.z, Is.GreaterThan(position.z + 0.2f));
        }

        [UnityTest]
        public IEnumerator ExistingPowerUpModifiersStillWork()
        {
            motor.SetSpeedMultiplier(1.5f);
            Keys(Key.W);
            yield return new WaitForSeconds(0.4f);
            Assert.That(motor.HorizontalSpeed, Is.EqualTo(3.3f).Within(0.1f));
            motor.SetMaxAirJumps(1);
            int jumps = 0;
            motor.Jumped += () => jumps++;
            Keys(Key.Space);
            yield return new WaitForSeconds(0.15f);
            Keys();
            yield return null;
            Keys(Key.Space);
            yield return new WaitForSeconds(0.1f);
            Assert.That(jumps, Is.EqualTo(2));
        }

        void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

        static GameObject Block(Vector3 position, Vector3 scale)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.position = position;
            block.transform.localScale = scale;
            return block;
        }
    }
}
#endif
