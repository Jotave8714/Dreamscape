using UnityEngine;

namespace Dreamscape.Core
{
    /// <summary>Volume e idioma, guardados entre sessões.</summary>
    public static class GameSettings
    {
        const string VolumeKey = "volume";
        const string EnglishKey = "english";

        public static float Volume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, 1f);
            set
            {
                float volume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(VolumeKey, volume);
                AudioListener.volume = volume;
            }
        }

        public static bool English
        {
            get => PlayerPrefs.GetInt(EnglishKey, 0) == 1;
            set => PlayerPrefs.SetInt(EnglishKey, value ? 1 : 0);
        }

        /// <summary>Texto no idioma escolhido.</summary>
        public static string Text(string portuguese, string english) => English ? english : portuguese;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Apply() => AudioListener.volume = Volume;
    }
}
