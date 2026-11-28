using System.IO;
using Dreamscape.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.Core
{
    /// <summary>
    /// Navegação entre o menu, as fases e o encerramento. As fases seguem a ordem do build: a primeira é a primeira
    /// cena depois do menu e concluir uma fase carrega a cena seguinte. O encerramento fecha a sequência; cenas de
    /// demonstração ficam depois dele no build, fora do fluxo. Entrar numa fase com jogador salva o progresso.
    /// </summary>
    public static class GameFlow
    {
        public const string MenuScene = "MainMenu";
        public const string EndingScene = "PrototypeEnding";
        const string UnusedTemplateScene = "SampleScene"; // cena vazia do template da Unity

        public static bool HasSave => SavedSceneAvailable(out _);
        public static bool CanStartNewGame => FirstPhase() != null;
        public static bool MenuAvailable => Application.CanStreamedLevelBeLoaded(MenuScene);

        public static void NewGame()
        {
            string first = FirstPhase();
            if (first == null)
            {
                Debug.LogError("Dreamscape: nenhuma fase no build. Adicione as fases em Build Profiles.");
                return;
            }
            SaveSystem.Delete();
            LoadScene(first);
        }

        public static void Continue()
        {
            if (SavedSceneAvailable(out string scene))
                LoadScene(scene);
        }

        public static void ReturnToMenu() => LoadScene(MenuScene);

        /// <summary>
        /// Conclui a fase ativa: carrega a próxima cena do build ou, sem próxima, o encerramento (ou o menu).
        /// Chegar ao encerramento apaga o save. Recusa durante a pausa ou para uma cena que não é a ativa.
        /// </summary>
        public static bool CompletePhase(Scene phase, string nextScene = null)
        {
            if (GamePause.IsPaused || phase != SceneManager.GetActiveScene())
                return false;

            string next = !string.IsNullOrEmpty(nextScene) && Application.CanStreamedLevelBeLoaded(nextScene)
                ? nextScene
                : NextInBuild(phase);
            if (next == null)
                next = Application.CanStreamedLevelBeLoaded(EndingScene) ? EndingScene : MenuAvailable ? MenuScene : null;
            if (next == null)
                return false;

            if (next == EndingScene)
                SaveSystem.Delete();
            LoadScene(next);
            return true;
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Verdadeiro para cenas jogáveis (fora menu, template e encerramento) presentes no build.</summary>
        public static bool IsPhase(string scene) =>
            !string.IsNullOrEmpty(scene) && scene != MenuScene && scene != UnusedTemplateScene && scene != EndingScene &&
            Application.CanStreamedLevelBeLoaded(scene);

        static void LoadScene(string scene)
        {
            GamePause.Resume();
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }

        static string NextInBuild(Scene phase)
        {
            int next = phase.buildIndex + 1;
            if (phase.buildIndex < 0 || next >= SceneManager.sceneCountInBuildSettings)
                return null;
            return SceneName(next);
        }

        static bool SavedSceneAvailable(out string scene)
        {
            scene = SaveSystem.LoadScene();
            // o save pode apontar para uma fase que saiu do build numa versão nova do jogo
            return IsPhase(scene);
        }

        static string FirstPhase()
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string name = SceneName(i);
                if (name == EndingScene)
                    return null;
                if (name != MenuScene && name != UnusedTemplateScene)
                    return name;
            }
            return null;
        }

        static string SceneName(int buildIndex) =>
            Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single && IsPhase(scene.name) && Object.FindAnyObjectByType<PlayerInputController>() != null)
                SaveSystem.SaveScene(scene.name);
        }
    }
}
