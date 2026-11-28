using System.Collections;
using System.IO;
using System.Linq;
using Dreamscape.Characters;
using Dreamscape.Core;
using Dreamscape.PowerUps;
using Dreamscape.Phases;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Dreamscape.Tests.Player
{
    /// <summary>Executável de testes nativo: física das duas fases e transições de toda a campanha.</summary>
    public sealed class PrototypePlayerTests
    {
        CharacterMotor motor;
        string savePath;
        byte[] originalSave;
        float previousCapture;

        [OneTimeSetUp]
        public void PreserveSave()
        {
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
            originalSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
        }

        [OneTimeTearDown]
        public void RestoreSave()
        {
            if (originalSave == null) File.Delete(savePath);
            else File.WriteAllBytes(savePath, originalSave);
        }

        [SetUp]
        public void Setup()
        {
            previousCapture = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            GamePause.Resume();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            Time.captureDeltaTime = previousCapture;
            yield return SceneManager.LoadSceneAsync(GameFlow.MenuScene);
        }

        [UnityTest]
        public IEnumerator BothOriginalPhasesCanBeCompletedWithRealMovement()
        {
            yield return Load("DS07_Fase1_Jogavel");
            yield return RunTo(new Vector3(-2f, 0f, 3f));
            Assert.That(motor.MaxAirJumps, Is.EqualTo(1));
            yield return RunTo(new Vector3(-6f, 0f, 3f));
            yield return RunTo(new Vector3(-6f, 0f, 5.2f));
            yield return JumpTo(new Vector3(-6f, 0.8f, 8f), false);
            yield return RunTo(new Vector3(-6f, 0.8f, 9f));
            yield return JumpTo(new Vector3(-6f, 1.4f, 15f), true);
            yield return RunTo(new Vector3(-6f, 1.4f, 16f));
            yield return JumpTo(new Vector3(-6f, 2f, 20.5f), true);
            yield return RunTo(new Vector3(-6f, 2f, 21.3f));
            yield return JumpTo(new Vector3(-6f, 2f, 24.5f), false);
            yield return RunTo(new Vector3(0f, 2f, 26f));
            yield return WalkThroughExit("DS09_Fase2_Jogavel");
            Bind();
            Assert.That(motor.MaxAirJumps, Is.Zero);
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1f));
            yield return RunTo(new Vector3(0f, 0f, 3f));
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1.5f));
            yield return RunTo(new Vector3(-6f, 0f, 7f));
            yield return RunTo(new Vector3(-6f, 0f, 17f));
            yield return RunTo(new Vector3(-6f, 0f, 29f));
            Assert.That(Object.FindAnyObjectByType<TemporaryBridge>().HasCrossed, Is.True);
            // A porta fica no patamar final; movimento até ela, sem teleporte.
            string next = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1));
            yield return WalkThroughExit(next);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(next));
            Debug.Log("DS22_TWO_PHASE_ROUTE_OK: ambas as fases concluídas por movimento e contatos físicos, sem teleporte.");
        }

        [UnityTest]
        public IEnumerator EveryCampaignPortalLoadsTheNextPhaseWithFreshPowers()
        {
            yield return Load("DS07_Fase1_Jogavel");
            for (int phase = 1; phase <= 18; phase++)
            {
                Assert.That(motor.MaxAirJumps, Is.Zero);
                Assert.That(motor.SpeedMultiplier, Is.EqualTo(1f));
                Assert.That(motor.GravityScale, Is.EqualTo(1f));
                Assert.That(SaveSystem.LoadScene(), Is.EqualTo(SceneManager.GetActiveScene().name));
                motor.SetMaxAirJumps(1);
                motor.SetSpeedMultiplier(1.5f);
                motor.SetGravityScale(0.5f);
                string next = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1));
                var exit = Object.FindAnyObjectByType<PhaseGoal>().GetComponent<Collider>();
                motor.Teleport(exit.bounds.center - Vector3.up * 0.8f, Quaternion.identity);
                yield return WaitForScene(next);
                if (phase < 18) Bind();
            }
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameFlow.EndingScene));
            Assert.That(SaveSystem.LoadScene(), Is.Null);
            Debug.Log("DS22_CAMPAIGN_PORTALS_OK: 18 transições; poderes e save verificados. Não é playtest das rotas intermediárias.");
        }

        IEnumerator Load(string name)
        {
            yield return SceneManager.LoadSceneAsync(name);
            yield return null;
            Bind();
            yield return new WaitForSeconds(0.2f);
        }

        void Bind()
        {
            var player = Object.FindAnyObjectByType<PlayerInputController>();
            Assert.That(player, Is.Not.Null);
            player.SetInputEnabled(false);
            motor = player.Motor;
            foreach (var companion in Object.FindObjectsByType<MoemaCompanion>())
            {
                companion.enabled = false;
                companion.GetComponent<CharacterMotor>().enabled = false;
            }
        }

        IEnumerator RunTo(Vector3 point)
        {
            for (int frame = 0; frame < 1800; frame++)
            {
                Assert.That(motor != null, Is.True, "Queda antes de " + point);
                Vector3 delta = Vector3.ProjectOnPlane(point - motor.transform.position, Vector3.up);
                if (delta.magnitude < 0.16f) yield break;
                motor.SetMoveInput(delta.normalized);
                motor.SetSprint(true);
                yield return null;
            }
            Assert.Fail("Percurso bloqueado até " + point + "; posição " + motor.transform.position);
        }

        IEnumerator JumpTo(Vector3 point, bool twice)
        {
            motor.SetMoveInput(Vector3.forward);
            motor.RequestJump();
            float start = Time.time;
            bool second = false, leftGround = false;
            while (Time.time - start < 3f)
            {
                Assert.That(motor != null, Is.True, "Queda saltando até " + point);
                leftGround |= !motor.IsGrounded;
                if (twice && !second && Time.time - start >= 0.30f)
                {
                    motor.RequestJump();
                    second = true;
                }
                if (motor.transform.position.z >= point.z - 0.4f) motor.SetMoveInput(Vector3.zero);
                if (leftGround && motor.IsGrounded)
                {
                    Assert.That(motor.transform.position.y, Is.EqualTo(point.y).Within(0.2f));
                    yield return RunTo(point);
                    yield break;
                }
                yield return null;
            }
            Assert.Fail("Salto sem aterrissagem até " + point);
        }

        IEnumerator WalkThroughExit(string next)
        {
            Vector3 target = Object.FindAnyObjectByType<PhaseGoal>().GetComponent<Collider>().bounds.center;
            float deadline = Time.realtimeSinceStartup + 15f;
            string current = SceneManager.GetActiveScene().name;
            while (SceneManager.GetActiveScene().name == current && Time.realtimeSinceStartup < deadline)
            {
                Assert.That(motor != null, Is.True);
                motor.SetMoveInput(Vector3.ProjectOnPlane(target - motor.transform.position, Vector3.up).normalized);
                motor.SetSprint(true);
                yield return null;
            }
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(next));
            yield return null;
        }

        static IEnumerator WaitForScene(string name)
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(name));
            yield return null;
        }
    }
}
