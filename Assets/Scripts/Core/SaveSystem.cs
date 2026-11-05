using System;
using System.IO;
using UnityEngine;

namespace Dreamscape.Core
{
    /// <summary>Save único em JSON: guarda a fase em que o jogador está. O checkpoint é por fase, como no GDD.</summary>
    public static class SaveSystem
    {
        [Serializable]
        class SaveData
        {
            public string scene;
        }

        static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        /// <summary>Fase salva, ou null se não houver save legível.</summary>
        public static string LoadScene()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return null;
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                return data == null || string.IsNullOrEmpty(data.scene) ? null : data.scene;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            {
                Debug.LogWarning($"Dreamscape: não foi possível ler o save: {e.Message}");
                return null;
            }
        }

        public static void SaveScene(string scene)
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(new SaveData { scene = scene }));
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning($"Dreamscape: não foi possível salvar: {e.Message}");
            }
        }

        public static void Delete()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning($"Dreamscape: não foi possível apagar o save: {e.Message}");
            }
        }
    }
}
