using UnityEngine;

namespace Dreamscape.Core
{
    /// <summary>Estado de pause do jogo: congela o tempo e o áudio. Não conhece nenhuma UI.</summary>
    public static class GamePause
    {
        public static bool IsPaused { get; private set; }

        public static void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        public static void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
