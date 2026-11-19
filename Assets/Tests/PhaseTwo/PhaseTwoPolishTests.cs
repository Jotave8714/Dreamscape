#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Dreamscape.Characters;
using Dreamscape.Core;
using Dreamscape.Phases;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Dreamscape.Tests
{
    public sealed class PhaseTwoPolishTests
    {
        CharacterMotor motor;
        TemporaryBridge bridge;
        float previousDelta;
        float previousVolume;
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
            if (originalSave == null)
                File.Delete(savePath);
            else
                File.WriteAllBytes(savePath, originalSave);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GamePause.Resume();
            previousDelta = Time.captureDeltaTime;
            previousVolume = AudioListener.volume;
            Time.captureDeltaTime = 1f / 60f;
            yield return Reload();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            Time.captureDeltaTime = previousDelta;
            AudioListener.volume = previousVolume;
            ClearSounds();
            yield return SceneManager.LoadSceneAsync("MainMenu");
        }

        IEnumerator Reload()
        {
            ClearSounds();
            yield return SceneManager.LoadSceneAsync("DS09_Fase2_Jogavel");
            yield return null;
            var player = Object.FindAnyObjectByType<PlayerInputController>();
            player.SetInputEnabled(false);
            motor = player.Motor;
            var companion = Object.FindAnyObjectByType<MoemaCompanion>();
            companion.enabled = false;
            companion.GetComponent<CharacterMotor>().enabled = false;
            bridge = Object.FindAnyObjectByType<TemporaryBridge>();
        }

        [Test]
        public void AudioAssetsContainDistinctNonSilentSignals()
        {
            float previousLength = 0f;
            foreach (string name in new[] { "Coleta", "Erro", "Acerto" })
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/DS15/{name}.wav");
                Assert.That(clip, Is.Not.Null);
                Assert.That(clip.channels, Is.EqualTo(1));
                Assert.That(clip.length, Is.GreaterThan(previousLength));
                var samples = new float[clip.samples];
                Assert.That(clip.GetData(samples, 0), Is.True);
                Assert.That(samples.Max(value => Mathf.Abs(value)), Is.InRange(0.1f, 0.95f));
                Assert.That(Mathf.Abs(samples.Average()), Is.LessThan(0.01f));
                previousLength = clip.length;
            }
        }

        [UnityTest]
        public IEnumerator ActualCollectionPlaysItsSoundAndShowsTheSpeedInstruction()
        {
            motor.enabled = false;
            yield return Contact(new Vector3(0f, 0.05f, 3f));
            Assert.That(motor.SpeedMultiplier, Is.EqualTo(1.5f));
            Assert.That(Sound("Coleta"), Is.Not.Null);
            Assert.That(GameObject.Find("Inicio").GetComponent<TextMesh>().text, Does.Contain("VELOCIDADE COLETADA"));
            Assert.That(bridge.HasStarted, Is.False);
        }

        [UnityTest]
        public IEnumerator RealFallKeepsTheErrorSoundAliveAcrossTheReload()
        {
            CharacterMotor old = motor;
            motor.enabled = false;
            var zone = Object.FindAnyObjectByType<PhaseFallZone>().GetComponent<BoxCollider>();
            yield return Contact(zone.bounds.center);
            for (int frame = 0; frame < 120 && old != null; frame++)
                yield return null;
            Assert.That(old == null, Is.True);
            Assert.That(Sound("Erro"), Is.Not.Null);
            Assert.That(Sound("Erro").gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad"));
        }

        [UnityTest]
        public IEnumerator SuccessRequiresTheActivatedBridgeAndIsReportedOnce()
        {
            var feedback = Object.FindAnyObjectByType<PhaseTwoFeedback>();
            Assert.That(feedback.ReportSuccess(), Is.False);
            motor.enabled = false;
            yield return Contact(new Vector3(-6f, 0.05f, 18.65f));
            Assert.That(feedback.ReportSuccess(), Is.True);
            Assert.That(feedback.ReportSuccess(), Is.False);
            Assert.That(Object.FindObjectsByType<AudioSource>().Count(source => source.clip != null && source.clip.name == "Acerto"), Is.EqualTo(1));
            yield return new WaitForSeconds(bridge.CollapseDelay + 0.1f);
            Assert.That(bridge.HasCollapsed, Is.True);
            Assert.That(bridge.HasCrossed, Is.True);
            Assert.That(Sound("Erro"), Is.Null);
            Assert.That(GameObject.Find("Piso").GetComponent<TextMesh>().text, Does.Contain("TRAVESSIA CONCLUIDA"));
        }

        [UnityTest]
        public IEnumerator TimeoutAndSubsequentFallProduceOnlyOneFailureSound()
        {
            motor.enabled = false;
            yield return Contact(new Vector3(-6f, 0.05f, 18.65f));
            yield return new WaitForSeconds(bridge.CollapseDelay);
            var first = Sound("Erro");
            Assert.That(first, Is.Not.Null);
            Object.FindAnyObjectByType<PhaseTwoFeedback>().ReportFailure();
            Assert.That(Sound("Erro"), Is.SameAs(first));
            Assert.That(Object.FindObjectsByType<AudioSource>().Count(source => source.clip != null && source.clip.name == "Erro"), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FeedbackUsesGlobalVolumeAndPausesWithTheListener()
        {
            AudioListener.volume = 0f;
            motor.enabled = false;
            yield return Contact(new Vector3(0f, 0.05f, 3f));
            var sound = Sound("Coleta");
            Assert.That(sound, Is.Not.Null);
            Assert.That(sound.ignoreListenerVolume, Is.False);
            Assert.That(sound.ignoreListenerPause, Is.False);
            Assert.That(sound.spatialBlend, Is.Zero);
            GamePause.Pause();
            for (int frame = 0; frame < 30; frame++)
                yield return null;
            Assert.That(AudioListener.pause, Is.True);
            Assert.That(sound != null, Is.True, "A fonte pausada não deve ser destruída antes de terminar.");
            GamePause.Resume();
        }

        [UnityTest]
        public IEnumerator BridgeColorChangesWithoutChangingTheSharedMaterial()
        {
            var tile = GameObject.Find("Piso temporario 1 [DS09]").GetComponent<Renderer>();
            Color original = tile.sharedMaterial.GetColor("_BaseColor");
            motor.enabled = false;
            yield return Contact(new Vector3(-6f, 0.05f, 18.65f));
            yield return new WaitForSeconds(0.6f);
            var properties = new MaterialPropertyBlock();
            tile.GetPropertyBlock(properties);
            Assert.That(properties.GetColor("_BaseColor"), Is.Not.EqualTo(original));
            Assert.That(tile.sharedMaterial.GetColor("_BaseColor"), Is.EqualTo(original));
        }

        [UnityTest]
        public IEnumerator CrossingIsRepeatableAtThreeFrameRatesAndThreePositions()
        {
            float minimumMargin = float.PositiveInfinity;
            float maximumCrossing = 0f;
            foreach (int fps in new[] { 30, 60, 120 })
            foreach (float lane in new[] { -6.8f, -6f, -5.2f })
            {
                Time.captureDeltaTime = 1f / fps;
                yield return Reload();
                yield return RunTo(new Vector3(0f, 0f, 3f));
                Assert.That(motor.SpeedMultiplier, Is.EqualTo(1.5f));
                yield return RunTo(new Vector3(lane, 0f, 7f));
                yield return RunTo(new Vector3(lane, 0f, 17f));
                Assert.That(bridge.HasStarted, Is.False);
                bool signalled = false;
                float margin = 0f;
                bridge.Crossed += () =>
                {
                    signalled = Sound("Acerto") != null;
                    margin = bridge.TimeRemaining;
                };
                yield return RunTo(new Vector3(lane, 0f, 29f));
                Assert.That(bridge.HasCrossed, Is.True, $"A travessia falhou a {fps} FPS, X={lane}.");
                Assert.That(signalled, Is.True, "O contato real de chegada deve disparar o som de acerto.");
                minimumMargin = Mathf.Min(minimumMargin, margin);
                maximumCrossing = Mathf.Max(maximumCrossing, bridge.CollapseDelay - margin);
                Debug.Log($"DS-15: {fps} FPS; X={lane}; travessia={bridge.CollapseDelay - margin:0.000}s; margem={margin:0.000}s.");
            }
            Assert.That(minimumMargin, Is.GreaterThan(0.1f));
            Debug.Log($"DS-15: maior travessia={maximumCrossing:0.000}s; menor margem={minimumMargin:0.000}s.");
        }

        IEnumerator RunTo(Vector3 destination)
        {
            for (int frame = 0; frame < 1200; frame++)
            {
                Assert.That(motor != null, Is.True, $"Queda antes de {destination}.");
                Vector3 direction = destination - motor.transform.position;
                direction.y = 0f;
                if (direction.magnitude < 0.18f)
                    break;
                motor.SetMoveInput(direction.normalized);
                motor.SetSprint(true);
                yield return null;
            }
            Vector3 remaining = destination - motor.transform.position;
            remaining.y = 0f;
            Assert.That(remaining.magnitude, Is.LessThan(0.18f));
            motor.SetMoveInput(Vector3.zero);
            motor.SetSprint(false);
            yield return new WaitForSeconds(0.3f);
        }

        IEnumerator Contact(Vector3 position)
        {
            motor.Teleport(position, Quaternion.identity);
            motor.Controller.Move(Vector3.down * 0.01f);
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        static AudioSource Sound(string name) => Object.FindObjectsByType<AudioSource>()
            .FirstOrDefault(source => source.clip != null && source.clip.name == name);

        static void ClearSounds()
        {
            foreach (var source in Object.FindObjectsByType<AudioSource>())
                if (source.name.StartsWith("DS15 - "))
                    Object.Destroy(source.gameObject);
        }
    }
}
#endif
