using System.IO;
using Dreamscape.Characters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.Core
{
    /// <summary>
    /// Navegação entre o menu e as fases. A primeira fase é a primeira cena do build depois do menu, então adicionar
    /// ou trocar fases só exige mexer nas cenas do build. Entrar numa fase com jogador salva o progresso.
    /// </summary>
    public static class GameFlow
    {
        public const string MenuScene = "MainMenu";
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
            SceneManager.LoadScene(first);
        }

        public static void Continue()
        {
            if (SavedSceneAvailable(out string scene))
                SceneManager.LoadScene(scene);
        }

        public static void ReturnToMenu() => SceneManager.LoadScene(MenuScene);

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
            // o save pode apontar para uma fase que saiu do build numa versão nova do jogo
            return scene != null && Application.CanStreamedLevelBeLoaded(scene);
        }

        static string FirstPhase()
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string name = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if (name != MenuScene && name != UnusedTemplateScene)
                    return name;
            }
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single && scene.buildIndex >= 0 && Object.FindAnyObjectByType<PlayerInputController>() != null)
                SaveSystem.SaveScene(scene.name);
        }
    }
}
