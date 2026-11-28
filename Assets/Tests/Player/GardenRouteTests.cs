using System.Collections;
using System.IO;
using Dreamscape.Characters;
using Dreamscape.Core;
using Dreamscape.Hazards;
using Dreamscape.PowerUps;
using Dreamscape.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Dreamscape.Tests.Player
{
    public sealed class GardenRouteTests
    {
        const string Garden = "Fase18_JardimDasEntrelinhas";
        byte[] save;
        string savePath;
        float captureDelta;
        CharacterMotor motor;

        [OneTimeSetUp]
        public void PreserveSave()
        {
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
            save = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
        }

        [OneTimeTearDown]
        public void RestoreSave()
        {
            if (save == null) File.Delete(savePath);
            else File.WriteAllBytes(savePath, save);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            captureDelta = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            GamePause.Resume();
            yield return LoadGarden();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            Time.captureDeltaTime = captureDelta;
            yield return SceneManager.LoadSceneAsync(GameFlow.MenuScene);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SafeRouteIsPlayableAt30Fps() => CompleteRoute(30);

        [UnityTest]
        public IEnumerator SafeRouteIsPlayableAt60Fps() => CompleteRoute(60);

        [UnityTest]
        public IEnumerator SafeRouteIsPlayableAt120Fps() => CompleteRoute(120);

        IEnumerator CompleteRoute(int fps)
        {
            Time.captureDeltaTime = 1f / fps;
            // Não teleporta: anda do spawn até a porta, usando o motor e os triggers reais.
            yield return Reach(new Vector3(-4f, 0f, 2f));
            yield return Reach(new Vector3(-5f, 0f, 3f));
            yield return Reach(new Vector3(-5f, 0f, 17.5f));
            yield return Reach(new Vector3(0f, 0f, 19f));
            Assert.That(motor.MaxAirJumps, Is.EqualTo(1));
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1f));
            Assert.That(Object.FindAnyObjectByType<CrumblingFloor>().GetComponent<Collider>().enabled, Is.True);
            yield return Reach(new Vector3(0f, 0f, 21.2f));
            yield return JumpTo(new Vector3(0f, 1.7f, 26f), true);
            yield return Reach(new Vector3(0f, 1.7f, 27.2f));
            yield return JumpTo(new Vector3(0f, 3.4f, 32f), true);
            yield return Reach(new Vector3(0f, 3.4f, 33.2f));
            yield return JumpTo(new Vector3(0f, 3.4f, 39f), false);
            yield return Reach(new Vector3(0f, 3.4f, 52f));
            motor.SetMoveInput(Vector3.forward);
            float deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name == Garden && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameFlow.EndingScene));
            Assert.That(SaveSystem.LoadScene(), Is.Null);
            Assert.That(Object.FindAnyObjectByType<PrototypeEnding>(), Is.Not.Null);
            Debug.Log($"FASE18_ROUTE_OK: {fps} FPS simulados; percurso completo sem teleporte.");
        }

        [UnityTest]
        public IEnumerator FalseShortcutCollapsesAndFiveDeathsRestoreThePhase()
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var old = motor;
                var floor = Object.FindAnyObjectByType<CrumblingFloor>();
                // Teleporte só prepara o cenário de falha, não aprova o percurso.
                motor.Teleport(new Vector3(0f, 0.05f, 8f), Quaternion.identity);
                motor.SetMoveInput(Vector3.zero);
                float deadline = Time.realtimeSinceStartup + 12f;
                while (old != null && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.That(old == null, Is.True, "A falsa página deve ceder e reiniciar a fase.");
                yield return null;
                BindPlayer();
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Garden));
                Assert.That(motor.MaxAirJumps, Is.Zero);
                Assert.That(motor.SpeedMultiplier, Is.EqualTo(1f));
                Assert.That(Object.FindAnyObjectByType<DoubleJumpPickup>(), Is.Not.Null);
                Assert.That(Object.FindAnyObjectByType<CrumblingFloor>().GetComponent<Collider>().enabled, Is.True);
                Assert.That(motor.transform.position.z, Is.EqualTo(-1f).Within(0.2f));
            }
        }

        [UnityTest]
        public IEnumerator DoubleJumpExpiresAndCanBeCollectedAgainWithoutStacking()
        {
            var pickup = Object.FindAnyObjectByType<DoubleJumpPickup>();
            motor.Teleport(new Vector3(0f, 0.05f, 19f), Quaternion.identity);
            yield return new WaitForSeconds(0.15f);
            Assert.That(motor.MaxAirJumps, Is.EqualTo(1));
            motor.Teleport(new Vector3(3f, 0.05f, 19f), Quaternion.identity);
            var active = motor.GetComponent<ActivePowerUps>();
            Assert.That(active.Effects.Count, Is.EqualTo(1));
            Time.captureDeltaTime = 0.25f;
            yield return new WaitForSeconds(pickup.Duration + 0.5f);
            Assert.That(motor.MaxAirJumps, Is.Zero);
            Assert.That(pickup.gameObject.activeSelf, Is.True);
            motor.Teleport(new Vector3(0f, 0.05f, 19f), Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
            Assert.That(motor.MaxAirJumps, Is.EqualTo(1));
            Assert.That(active.Effects.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PreviousPhaseReachesGardenAndEndingReturnsToMenu()
        {
            yield return SceneManager.LoadSceneAsync("Fase17_Despertar");
            yield return null;
            var previous = Object.FindAnyObjectByType<PlayerInputController>();
            previous.SetInputEnabled(false);
            var exit = Object.FindAnyObjectByType<PhaseGoal>().GetComponent<Collider>();
            previous.Motor.Teleport(exit.bounds.center - Vector3.up * 0.8f, Quaternion.identity);
            yield return WaitForScene(Garden);
            Assert.That(SaveSystem.LoadScene(), Is.EqualTo(Garden));
            BindPlayer();
            Assert.That(motor.MaxAirJumps, Is.Zero);
            exit = Object.FindAnyObjectByType<PhaseGoal>().GetComponent<Collider>();
            motor.Teleport(exit.bounds.center - Vector3.up * 0.8f, Quaternion.identity);
            yield return WaitForScene(GameFlow.EndingScene);
            foreach (var button in Object.FindAnyObjectByType<PrototypeEnding>().GetComponentsInChildren<Button>())
                if (button.name == "MainMenu") button.onClick.Invoke();
            yield return WaitForScene(GameFlow.MenuScene);
            Assert.That(GameFlow.HasSave, Is.False);
        }

        IEnumerator LoadGarden()
        {
            yield return SceneManager.LoadSceneAsync(Garden);
            yield return null;
            BindPlayer();
            yield return new WaitForSeconds(0.2f);
        }

        void BindPlayer()
        {
            var player = Object.FindAnyObjectByType<PlayerInputController>();
            Assert.That(player, Is.Not.Null);
            player.SetInputEnabled(false);
            motor = player.Motor;
            motor.SetMoveInput(Vector3.zero);
            motor.SetSprint(false);
        }

        IEnumerator Reach(Vector3 target)
        {
            for (int frame = 0; frame < 3000; frame++)
            {
                Assert.That(motor != null, Is.True, "Queda antes de " + target);
                Vector3 delta = Vector3.ProjectOnPlane(target - motor.transform.position, Vector3.up);
                if (delta.magnitude < 0.18f) yield break;
                motor.SetMoveInput(delta.normalized);
                motor.SetSprint(true);
                yield return null;
            }
            Assert.Fail("Não chegou a " + target + "; posição " + motor.transform.position);
        }

        IEnumerator JumpTo(Vector3 target, bool doubleJump)
        {
            motor.SetMoveInput(Vector3.forward);
            motor.SetSprint(true);
            motor.RequestJump();
            float started = Time.time;
            bool second = false, airborne = false;
            while (Time.time - started < 3f)
            {
                Assert.That(motor != null, Is.True, "Queda no salto até " + target);
                if (!motor.IsGrounded) airborne = true;
                if (doubleJump && !second && Time.time - started >= 0.28f)
                {
                    motor.RequestJump();
                    second = true;
                }
                if (motor.transform.position.z >= target.z - 0.3f)
                    motor.SetMoveInput(Vector3.zero);
                if (airborne && motor.IsGrounded)
                {
                    Assert.That(motor.transform.position.y, Is.EqualTo(target.y).Within(0.2f), "Altura de aterrissagem");
                    yield return Reach(target);
                    yield break;
                }
                yield return null;
            }
            Assert.Fail("Salto sem aterrissagem em " + target);
        }

        static IEnumerator WaitForScene(string name)
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(name));
            yield return null;
        }
    }
}
