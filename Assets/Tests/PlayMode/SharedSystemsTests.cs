#if UNITY_EDITOR
using System.Collections;
using System.IO;
using Dreamscape.Characters;
using Dreamscape.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Dreamscape.Tests
{
    public sealed class SharedSystemsTests
    {
        GameObject character, ground, zoneObject;
        CharacterMotor motor;
        float previousCaptureDeltaTime;
        SimulationMode previousSimulationMode;
        string savePath;
        byte[] originalSave;

        [OneTimeSetUp]
        public void PreserveSave()
        {
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
            originalSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
        }

        [OneTimeTearDown]
        public void RestoreSave()
        {
            if (originalSave == null)
                File.Delete(savePath);
            else
                File.WriteAllBytes(savePath, originalSave);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GamePause.Resume();
            previousCaptureDeltaTime = Time.captureDeltaTime;
            previousSimulationMode = Physics.simulationMode;
            Time.captureDeltaTime = 1f / 60f;
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            character = new GameObject("Character");
            character.transform.position = Vector3.up * 0.05f;
            CharacterController controller = character.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.center = Vector3.up;
            motor = character.AddComponent<CharacterMotor>();
            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.2f);
            Assert.That(motor.IsGrounded, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            Physics.simulationMode = previousSimulationMode;
            Object.Destroy(character);
            Object.Destroy(ground);
            Object.Destroy(zoneObject);
            Time.captureDeltaTime = previousCaptureDeltaTime;
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TeleportDiscardsBufferedJump()
        {
            int jumps = 0;
            motor.Jumped += () => jumps++;
            motor.SetMaxAirJumps(1);
            motor.RequestJump();
            motor.Teleport(new Vector3(20f, 5f, 0f), Quaternion.identity);
            yield return null;
            yield return null;
            Assert.That(jumps, Is.Zero, "O pedido anterior ao teleporte não deve executar no destino.");
            Assert.That(motor.Velocity.y, Is.LessThan(0f));
        }

        [UnityTest]
        public IEnumerator TeleportDoesNotCarryGroundContactOrCoyoteTime()
        {
            int jumps = 0;
            motor.Jumped += () => jumps++;
            motor.Teleport(new Vector3(20f, 5f, 0f), Quaternion.identity);
            Assert.That(motor.IsGrounded, Is.False, "O chão anterior não é o chão do destino.");
            motor.RequestJump();
            yield return null;
            yield return null;
            Assert.That(jumps, Is.Zero, "Sem poder, o contato anterior não permite pular no ar após teleportar.");
        }

        [UnityTest]
        public IEnumerator TeleportRestoresAirJumpBudgetAndKeepsPowerSettings()
        {
            motor.SetMaxAirJumps(1);
            motor.SetSpeedMultiplier(1.5f);
            motor.Teleport(new Vector3(20f, 5f, 0f), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
            int jumps = 0;
            motor.Jumped += () => jumps++;
            motor.RequestJump();
            yield return null;
            yield return null;
            Assert.That(jumps, Is.EqualTo(1));
            motor.Teleport(new Vector3(30f, 5f, 0f), Quaternion.Euler(0f, 90f, 0f));
            Assert.That(motor.Velocity, Is.EqualTo(Vector3.zero));
            Assert.That(motor.MaxAirJumps, Is.EqualTo(1));
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1.5f));
            motor.RequestJump();
            yield return null;
            yield return null;
            Assert.That(jumps, Is.EqualTo(2), "O destino deve permitir usar novamente o pulo no ar.");
            motor.RequestJump();
            yield return null;
            yield return null;
            Assert.That(jumps, Is.EqualTo(2), "O teleporte não deve permitir pulos ilimitados.");
        }

        [UnityTest]
        public IEnumerator TeleportOntoGroundStillAllowsNormalJumpAndLanding()
        {
            motor.Teleport(new Vector3(0f, 0.05f, 0f), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
            Assert.That(motor.IsGrounded, Is.True);
            motor.RequestJump();
            yield return new WaitForSeconds(0.1f);
            Assert.That(motor.Velocity.y, Is.GreaterThan(0f));
            yield return new WaitForSeconds(1f);
            Assert.That(motor.IsGrounded, Is.True);
        }

        [Test]
        public void FallZoneActivatesAsTriggerEvenWhenSerializedAsSolid()
        {
            zoneObject = new GameObject("Fall zone");
            zoneObject.SetActive(false);
            var box = zoneObject.AddComponent<BoxCollider>();
            zoneObject.AddComponent<PhaseFallZone>();
            box.isTrigger = false;
            zoneObject.SetActive(true);
            Assert.That(box.isTrigger, Is.True, "Uma zona sólida bloqueia a queda e não reinicia a fase.");
        }

        [UnityTest]
        public IEnumerator FallZoneIgnoresNonPlayerContacts()
        {
            CreateZone(new Vector3(20f, 5f, 0f));
            Physics.simulationMode = SimulationMode.Script;
            motor.Teleport(zoneObject.transform.position, Quaternion.identity);
            var body = character.AddComponent<Rigidbody>();
            body.isKinematic = true;
            character.AddComponent<SphereCollider>();
            SimulateContact();
            SimulateContact();
            yield return null;
            Assert.That(zoneObject != null, Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PausedContactWaitsForResumeThenRestartsCurrentPhase()
        {
            Object.Destroy(character);
            Object.Destroy(ground);
            yield return SceneManager.LoadSceneAsync("DS07_Fase1_Jogavel");
            yield return null;
            PlayerInputController player = Object.FindAnyObjectByType<PlayerInputController>();
            Assert.That(player, Is.Not.Null);
            player.SetInputEnabled(false);
            CharacterMotor oldMotor = player.Motor;
            oldMotor.SetMaxAirJumps(1);
            oldMotor.SetSpeedMultiplier(1.5f);
            PhaseFallZone zone = Object.FindAnyObjectByType<PhaseFallZone>();
            Assert.That(zone, Is.Not.Null);
            Physics.simulationMode = SimulationMode.Script;
            GamePause.Pause();
            var contact = new GameObject("Player contact", typeof(SphereCollider), typeof(Rigidbody));
            contact.transform.SetParent(player.transform, false);
            contact.GetComponent<Rigidbody>().isKinematic = true;
            contact.transform.position = zone.GetComponent<BoxCollider>().bounds.center;
            SimulateContact();
            SimulateContact();
            yield return null;
            yield return null;
            Assert.That(oldMotor != null, Is.True, "O contato não deve recarregar a cena durante a pausa.");
            Assert.That(GamePause.IsPaused, Is.True);
            GamePause.Resume();
            SimulateContact();
            SimulateContact();
            float deadline = Time.realtimeSinceStartup + 5f;
            while (oldMotor != null && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(oldMotor == null, Is.True, "O contato mantido deve reiniciar a fase ao retomar.");
            yield return null;
            PlayerInputController restored = Object.FindAnyObjectByType<PlayerInputController>();
            Assert.That(restored, Is.Not.Null);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("DS07_Fase1_Jogavel"));
            Assert.That(restored.Motor.MaxAirJumps, Is.Zero);
            Assert.That(restored.Motor.SpeedMultiplier, Is.EqualTo(1f));
        }

        void CreateZone(Vector3 position)
        {
            zoneObject = new GameObject("Fall zone", typeof(BoxCollider));
            zoneObject.transform.position = position;
            zoneObject.GetComponent<BoxCollider>().isTrigger = true;
            zoneObject.AddComponent<PhaseFallZone>();
        }

        static void SimulateContact()
        {
            Physics.SyncTransforms();
            Physics.Simulate(0.02f);
        }
    }
}
#endif
