#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Dreamscape.Characters;
using Dreamscape.Core;
using Dreamscape.PowerUps;
using Dreamscape.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Dreamscape.Tests
{
    public sealed class PrototypeFlowTests
    {
        byte[] originalSave;
        string savePath;
        Keyboard keyboard;
        InputSettings.BackgroundBehavior previousBackground;
        InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
        float previousCaptureDeltaTime;

        [OneTimeSetUp]
        public void PreserveSave()
        {
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
            originalSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
        }

        [OneTimeTearDown]
        public void RestoreSave()
        {
            if (originalSave != null)
                File.WriteAllBytes(savePath, originalSave);
            else
                File.Delete(savePath);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GamePause.Resume();
            SaveSystem.Delete();
            previousCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            previousBackground = InputSystem.settings.backgroundBehavior;
            previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            if (!Application.isBatchMode)
                EditorApplication.ExecuteMenuItem("Window/General/Game");
            yield return Load(GameFlow.MenuScene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            yield return Load(GameFlow.MenuScene);
            InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
            Time.captureDeltaTime = previousCaptureDeltaTime;
        }

        [UnityTest]
        public IEnumerator MenuStartsFirstPhaseWithoutSkippingIt()
        {
            yield return OpenMainMenu();
            Button start = Object.FindAnyObjectByType<MainMenu>().GetComponentsInChildren<Button>(true)
                .Single(button => button.name == "NOVO JOGO");
            Assert.That(start.interactable, Is.True);
            start.onClick.Invoke();
            yield return WaitForScene(GameFlow.FirstPhaseScene);
            Assert.That(SaveSystem.LoadScene(), Is.EqualTo(GameFlow.FirstPhaseScene));
            yield return new WaitForSeconds(0.2f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameFlow.FirstPhaseScene));
        }

        [UnityTest]
        public IEnumerator BothPhasesHavePlayerExitFallZoneAndTheirOwnPower()
        {
            foreach (string phase in new[] { GameFlow.FirstPhaseScene, GameFlow.SecondPhaseScene })
            {
                yield return Load(phase);
                Assert.That(Object.FindObjectsByType<PlayerInputController>().Length, Is.EqualTo(1));
                Assert.That(Object.FindObjectsByType<PhaseExit>().Length, Is.EqualTo(1));
                Assert.That(Object.FindAnyObjectByType<PhaseExit>().GetComponent<BoxCollider>().isTrigger, Is.True);
                Assert.That(Object.FindAnyObjectByType<PhaseFallZone>().GetComponent<BoxCollider>().isTrigger, Is.True);
                Assert.That(Object.FindObjectsByType<Camera>().Count(camera => camera.enabled), Is.EqualTo(1));
                Assert.That(Object.FindObjectsByType<PowerUpPickup>().Length, Is.EqualTo(1));
                Assert.That(phase == GameFlow.FirstPhaseScene
                    ? Object.FindAnyObjectByType<DoubleJumpPickup>() != null
                    : Object.FindAnyObjectByType<SpeedPickup>() != null, Is.True);
            }
        }

        [UnityTest]
        public IEnumerator CompanionCannotActivateTheExit()
        {
            yield return Load(GameFlow.FirstPhaseScene);
            MoemaCompanion companion = Object.FindAnyObjectByType<MoemaCompanion>();
            companion.enabled = false;
            CharacterMotor motor = companion.GetComponent<CharacterMotor>();
            motor.Teleport(ExitPosition(), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameFlow.FirstPhaseScene));
        }

        [UnityTest]
        public IEnumerator FirstExitLoadsSecondPhaseWithFreshPlayerAndCheckpoint()
        {
            yield return Load(GameFlow.FirstPhaseScene);
            yield return Collect<DoubleJumpPickup>();
            Assert.That(PlayerMotor().MaxAirJumps, Is.EqualTo(1));
            PlayerMotor().Teleport(ExitPosition(), Quaternion.identity);
            yield return WaitForScene(GameFlow.SecondPhaseScene);
            Assert.That(PlayerMotor().MaxAirJumps, Is.Zero);
            Assert.That(PlayerMotor().SpeedMultiplier, Is.EqualTo(1f));
            Assert.That(SaveSystem.LoadScene(), Is.EqualTo(GameFlow.SecondPhaseScene));
        }

        [UnityTest]
        public IEnumerator FinalExitShowsEndingAndPlayAgainClearsSpeed()
        {
            yield return Load(GameFlow.SecondPhaseScene);
            yield return Collect<SpeedPickup>();
            Assert.That(PlayerMotor().SpeedMultiplier, Is.EqualTo(1.5f));
            PlayerMotor().Teleport(ExitPosition(), Quaternion.identity);
            yield return WaitForScene(GameFlow.EndingScene);
            Assert.That(Object.FindAnyObjectByType<PlayerInputController>(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<PrototypeEnding>(), Is.Not.Null);
            Assert.That(SaveSystem.LoadScene(), Is.Null);
            Assert.That(GameFlow.HasSave, Is.False);
            EndingButton("NewGame").onClick.Invoke();
            yield return WaitForScene(GameFlow.FirstPhaseScene);
            Assert.That(PlayerMotor().SpeedMultiplier, Is.EqualTo(1f));
            Assert.That(PlayerMotor().MaxAirJumps, Is.Zero);
        }

        [UnityTest]
        public IEnumerator EndingMenuButtonReturnsToMenuWithoutAContinueSave()
        {
            yield return Load(GameFlow.SecondPhaseScene);
            PlayerMotor().Teleport(ExitPosition(), Quaternion.identity);
            yield return WaitForScene(GameFlow.EndingScene);
            EndingButton("MainMenu").onClick.Invoke();
            yield return WaitForScene(GameFlow.MenuScene);
            Assert.That(GameFlow.HasSave, Is.False);
            yield return OpenMainMenu();
            Button resume = Object.FindAnyObjectByType<MainMenu>().GetComponentsInChildren<Button>(true)
                .Single(button => button.name == "CONTINUAR");
            Assert.That(resume.interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator ContinueAcceptsOnlyTheTwoPlayablePhases()
        {
            foreach (string invalid in new[] { GameFlow.MenuScene, GameFlow.EndingScene, "SampleScene", "CenaRemovida" })
            {
                SaveSystem.SaveScene(invalid);
                Assert.That(GameFlow.HasSave, Is.False);
                GameFlow.Continue();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameFlow.MenuScene));
            }
            SaveSystem.SaveScene(GameFlow.SecondPhaseScene);
            Assert.That(GameFlow.HasSave, Is.True);
            GameFlow.Continue();
            yield return WaitForScene(GameFlow.SecondPhaseScene);
            Assert.That(PlayerMotor().MaxAirJumps, Is.Zero);
            Assert.That(PlayerMotor().SpeedMultiplier, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator FiveDeathsRestorePhaseTwoPowerAndSpawn()
        {
            yield return Load(GameFlow.SecondPhaseScene);
            Vector3 spawn = Object.FindAnyObjectByType<PlayerInputController>().transform.position;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                yield return Collect<SpeedPickup>();
                Assert.That(PlayerMotor().SpeedMultiplier, Is.EqualTo(1.5f));
                PlayerInputController oldPlayer = Object.FindAnyObjectByType<PlayerInputController>();
                Vector3 fall = Object.FindAnyObjectByType<PhaseFallZone>().GetComponent<BoxCollider>().bounds.center;
                oldPlayer.Motor.Teleport(fall, Quaternion.identity);
                float deadline = Time.realtimeSinceStartup + 5f;
                while (oldPlayer != null && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.That(oldPlayer == null, Is.True, "A zona de queda deve recarregar a fase.");
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameFlow.SecondPhaseScene));
                Assert.That(PlayerMotor().SpeedMultiplier, Is.EqualTo(1f));
                Assert.That(PlayerMotor().MaxAirJumps, Is.Zero);
                Assert.That(Object.FindAnyObjectByType<SpeedPickup>(), Is.Not.Null);
                Assert.That(Vector3.Distance(PlayerMotor().transform.position, spawn), Is.LessThan(0.2f));
            }
        }

        [UnityTest]
        public IEnumerator PauseBlocksExitAndReturningToMenuRestoresTimeAndAudio()
        {
            yield return Load(GameFlow.SecondPhaseScene);
            Keys(Key.Escape);
            yield return null;
            yield return null;
            Keys();
            yield return null;
            Assert.That(GamePause.IsPaused, Is.True);
            Assert.That(GameFlow.CompletePhase(SceneManager.GetActiveScene()), Is.False);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameFlow.SecondPhaseScene));
            GameFlow.ReturnToMenu();
            yield return WaitForScene(GameFlow.MenuScene);
            Assert.That(GamePause.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(AudioListener.pause, Is.False);
            Assert.That(Object.FindAnyObjectByType<PauseMenu>().transform.Find("Painel").gameObject.activeSelf, Is.False);
            GameFlow.NewGame();
            yield return WaitForScene(GameFlow.FirstPhaseScene);
            Assert.That(PlayerMotor().SpeedMultiplier, Is.EqualTo(1f));
        }

        IEnumerator OpenMainMenu()
        {
            Keys(Key.F6);
            yield return null;
            yield return null;
            Keys();
            yield return null;
        }

        static IEnumerator Collect<T>() where T : PowerUpPickup
        {
            T pickup = Object.FindAnyObjectByType<T>();
            Assert.That(pickup, Is.Not.Null);
            PlayerMotor().Teleport(pickup.GetComponent<Collider>().bounds.center - Vector3.up * 0.5f, Quaternion.identity);
            yield return new WaitForSeconds(0.12f);
            Assert.That(pickup.gameObject.activeSelf, Is.False, "O jogador deve coletar pelo trigger real.");
        }

        static CharacterMotor PlayerMotor() => Object.FindAnyObjectByType<PlayerInputController>().Motor;

        static Vector3 ExitPosition() => Object.FindAnyObjectByType<PhaseExit>().GetComponent<BoxCollider>().bounds.center - Vector3.up * 0.8f;

        static Button EndingButton(string name) => Object.FindAnyObjectByType<PrototypeEnding>()
            .GetComponentsInChildren<Button>().Single(button => button.name == name);

        static IEnumerator Load(string scene)
        {
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
        }

        static IEnumerator WaitForScene(string scene)
        {
            float deadline = Time.realtimeSinceStartup + 5f;
            while (SceneManager.GetActiveScene().name != scene && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(scene));
            yield return null;
        }

        void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    }
}
#endif
