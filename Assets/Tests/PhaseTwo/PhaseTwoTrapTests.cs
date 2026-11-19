#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Dreamscape.Characters;
using Dreamscape.Core;
using Dreamscape.Phases;
using Dreamscape.PowerUps;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Dreamscape.Tests
{
    public sealed class PhaseTwoTrapTests
    {
        const string Phase = "DS09_Fase2_Jogavel";
        CharacterMotor motor;
        TemporaryBridge bridge;
        GameObject[] tiles;
        float previousCaptureDeltaTime;
        SimulationMode previousSimulationMode;
        byte[] originalSave;
        string savePath;

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
            previousCaptureDeltaTime = Time.captureDeltaTime;
            previousSimulationMode = Physics.simulationMode;
            Time.captureDeltaTime = 1f / 60f;
            yield return SceneManager.LoadSceneAsync(Phase);
            yield return null;
            BindScene();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            Physics.simulationMode = previousSimulationMode;
            Time.captureDeltaTime = previousCaptureDeltaTime;
            yield return SceneManager.LoadSceneAsync("MainMenu");
        }

        void BindScene()
        {
            var player = Object.FindAnyObjectByType<PlayerInputController>();
            player.SetInputEnabled(false);
            motor = player.Motor;
            foreach (var companion in Object.FindObjectsByType<MoemaCompanion>())
            {
                companion.enabled = false;
                companion.GetComponent<CharacterMotor>().enabled = false;
            }
            bridge = Object.FindAnyObjectByType<TemporaryBridge>();
            tiles = Enumerable.Range(1, 4).Select(i => GameObject.Find($"Piso temporario {i} [DS09]")).ToArray();
        }

        [Test]
        public void SceneHasOnePlayerAndFourContiguousBridgeTiles()
        {
            Assert.That(Object.FindObjectsByType<PlayerInputController>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<TemporaryBridge>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Camera>().Count(camera => camera.enabled), Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<SpeedPickup>().Length, Is.EqualTo(1));
            Assert.That(Object.FindAnyObjectByType<PhaseFallZone>(), Is.Not.Null);
            Assert.That(bridge.GetComponent<BoxCollider>().isTrigger, Is.True);
            float nextEdge = 19f;
            foreach (GameObject tile in tiles)
            {
                Bounds bounds = tile.GetComponent<BoxCollider>().bounds;
                Assert.That(tile.activeSelf, Is.True);
                Assert.That(bounds.min.z, Is.EqualTo(nextEdge).Within(0.01f));
                Assert.That(bounds.max.y, Is.EqualTo(0f).Within(0.01f));
                nextEdge = bounds.max.z;
            }
            Assert.That(nextEdge, Is.EqualTo(27f).Within(0.01f));
            Assert.That(GameObject.Find("Pista").GetComponent<TextMesh>().text, Does.Contain("OLHE PARA O LADO"));
            Assert.That(GameObject.Find("Rota direta enganosa - termina em queda [DS09]")
                .GetComponent<BoxCollider>().bounds.max.z, Is.EqualTo(17f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator CollectingSpeedDoesNotStartTheBridge()
        {
            motor.enabled = false;
            yield return ContactAt(new Vector3(0f, 0.05f, 3f));
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1.5f));
            Assert.That(Object.FindAnyObjectByType<SpeedPickup>(), Is.Null);
            Assert.That(bridge.HasStarted, Is.False);
            yield return new WaitForSeconds(bridge.CollapseDelay + 0.1f);
            Assert.That(tiles.All(tile => tile.activeSelf), Is.True);
        }

        [UnityTest]
        public IEnumerator CompanionCannotStartTheBridge()
        {
            var companion = Object.FindAnyObjectByType<MoemaCompanion>().GetComponent<CharacterMotor>();
            companion.Teleport(new Vector3(-6f, 0.05f, 18.65f), Quaternion.identity);
            companion.Controller.Move(Vector3.down * 0.01f);
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(bridge.HasStarted, Is.False);
        }

        [UnityTest]
        public IEnumerator ReenteringTheActivationStripDoesNotRestartTheTimer()
        {
            yield return StartBridge();
            yield return new WaitForSeconds(0.3f);
            float remaining = bridge.TimeRemaining;
            yield return ContactAt(new Vector3(-6f, 0.05f, 17f));
            yield return ContactAt(new Vector3(-6f, 0.05f, 18.65f));
            Assert.That(bridge.TimeRemaining, Is.LessThan(remaining));
            Assert.That(bridge.HasStarted, Is.True);
        }

        [UnityTest]
        public IEnumerator PausedContactStartsOnlyAfterResume()
        {
            motor.enabled = false;
            Physics.simulationMode = SimulationMode.Script;
            GamePause.Pause();
            motor.Teleport(new Vector3(-6f, 0.05f, 18.65f), Quaternion.identity);
            motor.Controller.Move(Vector3.down * 0.01f);
            Physics.SyncTransforms();
            Physics.Simulate(0.02f);
            Physics.Simulate(0.02f);
            yield return null;
            Assert.That(bridge.HasStarted, Is.False);
            GamePause.Resume();
            Physics.Simulate(0.02f);
            Physics.Simulate(0.02f);
            yield return null;
            Assert.That(bridge.HasStarted, Is.True);
        }

        [UnityTest]
        public IEnumerator PauseFreezesTheCountdown()
        {
            yield return StartBridge();
            GamePause.Pause();
            float remaining = bridge.TimeRemaining;
            for (int frame = 0; frame < 5; frame++)
                yield return null;
            Assert.That(bridge.TimeRemaining, Is.EqualTo(remaining));
            Assert.That(tiles.All(tile => tile.activeSelf), Is.True);
            GamePause.Resume();
            yield return new WaitForSeconds(0.2f);
            Assert.That(bridge.TimeRemaining, Is.LessThan(remaining - 0.1f));
        }

        [UnityTest]
        public IEnumerator ExpirationRemovesAllFloorCollisionsAndShowsFailure()
        {
            yield return StartBridge();
            Assert.That(GameObject.Find("Piso").GetComponent<TextMesh>().text, Does.Contain("CORRA!"));
            yield return new WaitForSeconds(bridge.CollapseDelay + 0.1f);
            Assert.That(bridge.HasCollapsed, Is.True);
            Assert.That(bridge.TimeRemaining, Is.Zero);
            Assert.That(tiles.All(tile => !tile.activeSelf && !tile.GetComponent<BoxCollider>().gameObject.activeInHierarchy), Is.True);
            Assert.That(GameObject.Find("Piso").GetComponent<TextMesh>().text, Does.Contain("PISO CEDEU"));
        }

        [UnityTest]
        public IEnumerator DirectRouteFallsAndReturnsToPhaseTwoWithTheSameClue()
        {
            CharacterMotor previous = motor;
            string clue = GameObject.Find("Pista").GetComponent<TextMesh>().text;
            motor.SetMoveInput(Vector3.forward);
            motor.SetSprint(true);
            bool collected = false;
            for (int frame = 0; frame < 600 && previous != null; frame++)
            {
                collected |= previous.SpeedMultiplier > 1f;
                yield return null;
            }
            Assert.That(collected, Is.True, "O percurso deve passar pelo coletável real.");
            Assert.That(previous == null, Is.True, "Correr reto deve levar à zona de queda e reiniciar.");
            yield return null;
            BindScene();
            AssertRestored();
            Assert.That(GameObject.Find("Pista").GetComponent<TextMesh>().text, Is.EqualTo(clue));
        }

        [UnityTest]
        public IEnumerator PoweredPlayerCanWalkTheDetourAndRunAcrossTheBridge()
        {
            CharacterMotor original = motor;
            yield return RunTo(new Vector3(0f, 0f, 3f));
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1.5f));
            yield return RunTo(new Vector3(-6f, 0f, 7f));
            yield return RunTo(new Vector3(-6f, 0f, 17f));
            Assert.That(bridge.HasStarted, Is.False);
            yield return RunTo(new Vector3(-6f, 0f, 29f));
            Assert.That(motor, Is.SameAs(original));
            Assert.That(bridge.HasStarted, Is.True);
            Assert.That(motor.transform.position.z, Is.GreaterThan(27.5f));
            Assert.That(motor.transform.position.y, Is.GreaterThan(-0.1f));
        }

        [UnityTest]
        public IEnumerator RunningAcrossWithoutSpeedFallsWhenTheBridgeExpires()
        {
            motor.Teleport(new Vector3(-6f, 0.05f, 16f), Quaternion.identity);
            CharacterMotor previous = motor;
            motor.SetMoveInput(Vector3.forward);
            motor.SetSprint(true);
            for (int frame = 0; frame < 600 && previous != null; frame++)
                yield return null;
            Assert.That(previous == null, Is.True, "A travessia sem o poder deve falhar antes do patamar final.");
            yield return null;
            BindScene();
            AssertRestored();
        }

        [UnityTest]
        public IEnumerator ThreeRestartsRestoreTilesTimerPickupAndSpeed()
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                motor.enabled = false;
                yield return ContactAt(new Vector3(0f, 0.05f, 3f));
                Assert.That(motor.SpeedMultiplier, Is.EqualTo(1.5f));
                yield return StartBridge();
                yield return new WaitForSeconds(bridge.CollapseDelay + 0.1f);
                Assert.That(bridge.HasCollapsed, Is.True);
                CharacterMotor previous = motor;
                Vector3 failure = Object.FindAnyObjectByType<PhaseFallZone>().GetComponent<BoxCollider>().bounds.center;
                yield return ContactAt(failure);
                for (int frame = 0; frame < 120 && previous != null; frame++)
                    yield return null;
                Assert.That(previous == null, Is.True);
                yield return null;
                BindScene();
                AssertRestored();
            }
        }

        void AssertRestored()
        {
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Phase));
            Assert.That(Vector3.Distance(motor.transform.position, new Vector3(0f, 0.1f, 0f)), Is.LessThan(0.3f));
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1f));
            Assert.That(motor.MaxAirJumps, Is.Zero);
            Assert.That(Object.FindAnyObjectByType<SpeedPickup>(), Is.Not.Null);
            Assert.That(tiles.All(tile => tile.activeSelf), Is.True);
            Assert.That(bridge.HasStarted, Is.False);
            Assert.That(bridge.TimeRemaining, Is.EqualTo(bridge.CollapseDelay));
            Assert.That(SaveSystem.LoadScene(), Is.EqualTo(Phase));
        }

        IEnumerator StartBridge()
        {
            motor.enabled = false;
            yield return ContactAt(new Vector3(-6f, 0.05f, 18.65f));
            Assert.That(bridge.HasStarted, Is.True);
        }

        IEnumerator ContactAt(Vector3 position)
        {
            motor.Teleport(position, Quaternion.identity);
            motor.Controller.Move(Vector3.down * 0.01f);
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        IEnumerator RunTo(Vector3 destination)
        {
            for (int frame = 0; frame < 600; frame++)
            {
                Assert.That(motor != null, Is.True, $"O jogador caiu antes de chegar a {destination}.");
                Vector3 direction = destination - motor.transform.position;
                direction.y = 0f;
                if (direction.magnitude < 0.18f)
                    break;
                motor.SetMoveInput(direction.normalized);
                motor.SetSprint(true);
                yield return null;
            }
            Assert.That(motor != null, Is.True);
            Vector3 remaining = destination - motor.transform.position;
            remaining.y = 0f;
            Assert.That(remaining.magnitude, Is.LessThan(0.18f), $"Destino não alcançado: {destination}.");
            motor.SetMoveInput(Vector3.zero);
            motor.SetSprint(false);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
#endif
