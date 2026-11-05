using Dreamscape.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.Core
{
    /// <summary>
    /// Fluxo do protótipo: menu, duas fases e encerramento. O save guarda somente a fase atual.
    /// </summary>
    public static class GameFlow
    {
        public const string MenuScene = "MainMenu";
        public const string FirstPhaseScene = "DS07_Fase1_Jogavel";
        public const string SecondPhaseScene = "DS13_Fase2_Jogavel";
        public const string EndingScene = "PrototypeEnding";

        public static bool HasSave => SavedSceneAvailable(out _);
        public static bool CanStartNewGame => Application.CanStreamedLevelBeLoaded(FirstPhaseScene);
        public static bool MenuAvailable => Application.CanStreamedLevelBeLoaded(MenuScene);

        public static void NewGame()
        {
            if (!CanStartNewGame)
            {
                Debug.LogError("Dreamscape: nenhuma fase no build. Adicione as fases em Build Profiles.");
                return;
            }
            SaveSystem.Delete();
            LoadScene(FirstPhaseScene);
        }

        public static void Continue()
        {
            if (SavedSceneAvailable(out string scene))
                LoadScene(scene);
        }

        public static void ReturnToMenu() => LoadScene(MenuScene);

        public static bool CompletePhase(Scene phase)
        {
            if (GamePause.IsPaused || phase != SceneManager.GetActiveScene())
                return false;

            string next = phase.name == FirstPhaseScene ? SecondPhaseScene
                : phase.name == SecondPhaseScene ? EndingScene : null;
            if (next == null)
                return false;
            if (!Application.CanStreamedLevelBeLoaded(next))
            {
                Debug.LogError($"Dreamscape: adicione a cena {next} ao perfil de build.");
                return false;
            }

            if (next == EndingScene)
                SaveSystem.Delete();
            LoadScene(next);
            return true;
        }

        static void LoadScene(string scene)
        {
            GamePause.Resume();
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static bool SavedSceneAvailable(out string scene)
        {
            scene = SaveSystem.LoadScene();
            return IsPhase(scene) && Application.CanStreamedLevelBeLoaded(scene);
        }

        static bool IsPhase(string scene) => scene == FirstPhaseScene || scene == SecondPhaseScene;

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
