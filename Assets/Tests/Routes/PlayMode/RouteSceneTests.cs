#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Dreamscape.Characters;
using Dreamscape.Core;
using Dreamscape.PowerUps;
using Dreamscape.Routes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Dreamscape.Tests
{
    public sealed class RouteSceneTests
    {
        PhaseRouteGraph graph;
        byte[] originalSave;
        string savePath;
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
            Time.captureDeltaTime = 1f / 60f;
            yield return SceneManager.LoadSceneAsync("DS07_Fase1_Jogavel");
            yield return null;
            graph = Object.FindAnyObjectByType<PhaseRouteGraph>();
            Assert.That(graph, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GamePause.Resume();
            Time.captureDeltaTime = previousCaptureDeltaTime;
            yield return SceneManager.LoadSceneAsync("MainMenu");
        }

        [Test]
        public void PhaseAutomaticallyValidatesTheRouteThroughThePickup()
        {
            Assert.That(graph.LastResult, Is.Not.Null);
            Assert.That(graph.LastResult.Found, Is.True);
            Assert.That(graph.LastResult.Path, Is.EqualTo(new[] { 0, 1, 4, 5, 6, 7, 8 }));
            Assert.That(Object.FindObjectsByType<PhaseRouteGraph>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<DoubleJumpPickup>().Length, Is.EqualTo(1));

            Physics.SyncTransforms();
            var points = new SerializedObject(graph).FindProperty("points");
            for (int i = 0; i < points.arraySize; i++)
            {
                var point = points.GetArrayElementAtIndex(i);
                var marker = (Transform)point.FindPropertyRelative("marker").objectReferenceValue;
                var floor = (Collider)point.FindPropertyRelative("floor").objectReferenceValue;
                Vector3 projected = new Vector3(marker.position.x, floor.bounds.center.y, marker.position.z);
                Assert.That(floor.bounds.Contains(projected), Is.True,
                    $"O marcador {point.FindPropertyRelative("id").stringValue} deve corresponder ao piso vinculado.");
            }
        }

        [Test]
        public void MissingPickupMakesThePowerRouteUnreachable()
        {
            Object.FindAnyObjectByType<DoubleJumpPickup>().enabled = false;
            Assert.That(graph.Evaluate().Found, Is.False);
        }

        [Test]
        public void RemovingTheLandingFloorBreaksConnectivity()
        {
            var floor = GameObject.Find("N4 - Plataforma B [exige pulo duplo - calibrar]").GetComponent<Collider>();
            floor.enabled = false;
            Assert.That(graph.Evaluate().Found, Is.False);
            floor.enabled = true;
            Assert.That(graph.Evaluate().Found, Is.True);
        }

        [Test]
        public void MovingTheLandingFloorAwayFromItsMarkerBreaksConnectivity()
        {
            var floor = GameObject.Find("N4 - Plataforma B [exige pulo duplo - calibrar]").GetComponent<Collider>();
            Vector3 original = floor.transform.position;
            floor.transform.position += Vector3.right * 20f;
            Physics.SyncTransforms();
            Assert.That(graph.Evaluate().Found, Is.False);
            floor.transform.position = original;
            Physics.SyncTransforms();
            Assert.That(graph.Evaluate().Found, Is.True);
        }

        [Test]
        public void InvalidDemonstrationDoesNotChangeThePlayableGraph()
        {
            Assert.That(graph.Evaluate(true).Found, Is.False);
            LogAssert.Expect(LogType.Warning, new Regex("DS-14 demonstração: sem rota segura"));
            graph.DemonstrateBlockedRoute();
            Assert.That(graph.Evaluate().Found, Is.True);
            Assert.That(graph.LastResult.Found, Is.True);
        }

        [Test]
        public void MissingSceneReferenceIsReportedInsteadOfApprovingTheRoute()
        {
            var serialized = new SerializedObject(graph);
            serialized.FindProperty("points").GetArrayElementAtIndex(5)
                .FindPropertyRelative("floor").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(graph.Evaluate().Found, Is.False);
            Assert.That(graph.Evaluate().Error, Is.Not.Null.And.Not.Empty);
        }

        [UnityTest]
        public IEnumerator ThreeDeathsRecreateAndRevalidateTheGraphWithThePickupRestored()
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                PhaseRouteGraph previousGraph = graph;
                var player = Object.FindAnyObjectByType<PlayerInputController>();
                player.SetInputEnabled(false);
                var motor = player.Motor;
                motor.enabled = false;
                var pickup = Object.FindAnyObjectByType<DoubleJumpPickup>();
                motor.Teleport(pickup.transform.position, Quaternion.identity);
                motor.Controller.Move(Vector3.down * 0.01f);
                yield return new WaitForFixedUpdate();
                yield return null;
                Assert.That(motor.MaxAirJumps, Is.EqualTo(1));
                var zone = Object.FindAnyObjectByType<PhaseFallZone>().GetComponent<BoxCollider>();
                motor.Teleport(zone.bounds.center, Quaternion.identity);
                motor.Controller.Move(Vector3.down * 0.01f);
                for (int frame = 0; frame < 120 && previousGraph != null; frame++)
                    yield return null;
                Assert.That(previousGraph == null, Is.True);
                yield return null;
                graph = Object.FindAnyObjectByType<PhaseRouteGraph>();
                Assert.That(graph.LastResult, Is.Not.Null);
                Assert.That(graph.LastResult.Found, Is.True);
                Assert.That(graph.LastResult.Path, Is.EqualTo(new[] { 0, 1, 4, 5, 6, 7, 8 }));
                Assert.That(Object.FindAnyObjectByType<DoubleJumpPickup>(), Is.Not.Null);
                Assert.That(Object.FindAnyObjectByType<PlayerInputController>().Motor.MaxAirJumps, Is.Zero);
            }
        }
    }
}
#endif
