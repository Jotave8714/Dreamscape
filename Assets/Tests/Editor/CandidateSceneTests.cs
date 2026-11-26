using System;
using System.Linq;
using Dreamscape.EditorTools;
using NUnit.Framework;
using UnityEditor;

namespace Dreamscape.Tests
{
    public sealed class CandidateSceneTests
    {
        static string[] Scenes => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        [Test]
        public void FullCampaignIncludesGardenBeforeEnding()
        {
            string[] scenes = CandidateBuild.SelectScenes(Scenes, "full");
            Assert.That(scenes.Length, Is.EqualTo(20));
            Assert.That(scenes[18], Is.EqualTo("Assets/Scenes/Fase18_JardimDasEntrelinhas.unity"));
            Assert.That(scenes[19], Is.EqualTo("Assets/Scenes/PrototypeEnding.unity"));
        }

        [Test]
        public void PrototypePreservesTheOriginalTwoPhaseDelivery()
        {
            var scenes = CandidateBuild.SelectScenes(Scenes, "prototype");
            Assert.That(scenes, Is.EqualTo(new[]
            {
                "Assets/Scenes/MainMenu.unity", "Assets/Scenes/DS07_Fase1_Jogavel.unity",
                "Assets/Scenes/DS09_Fase2_Jogavel.unity", "Assets/Scenes/PrototypeEnding.unity"
            }));
        }

        [Test]
        public void GardenDemoStartsTheNewPhaseDirectly()
        {
            Assert.That(CandidateBuild.SelectScenes(Scenes, "phase18"), Is.EqualTo(new[]
            {
                "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Fase18_JardimDasEntrelinhas.unity",
                "Assets/Scenes/PrototypeEnding.unity"
            }));
        }

        [Test]
        public void MissingEndingIsRejectedBeforeBuilding()
        {
            Assert.Throws<InvalidOperationException>(() => CandidateBuild.SelectScenes(Scenes.Take(Scenes.Length - 1).ToArray(), "full"));
        }

        [Test]
        public void MissingMiddlePhaseIsRejectedBeforeBuilding()
        {
            Assert.Throws<InvalidOperationException>(() => CandidateBuild.SelectScenes(Scenes.Where(p => !p.Contains("Fase8_")).ToArray(), "full"));
        }

        [Test]
        public void DuplicatePhaseAndUnknownScopeAreRejected()
        {
            var scenes = Scenes;
            scenes[4] = scenes[3];
            Assert.Throws<InvalidOperationException>(() => CandidateBuild.SelectScenes(scenes, "full"));
            Assert.Throws<ArgumentException>(() => CandidateBuild.SelectScenes(Scenes, "unknown"));
        }
    }
}
