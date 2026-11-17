using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class DreamscapePresentationWindow : EditorWindow
{
    [MenuItem("Dreamscape/Apresentação")]
    public static void ShowPanel()
    {
        GetWindow<DreamscapePresentationWindow>("Apresentação");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Dreamscape", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if (GUILayout.Button("Jogar apresentação de cinco fases"))
                OpenScene("Dreamscape_CincoFases");

            if (GUILayout.Button("Rascunhos das fases 1 e 2"))
                OpenScene("Apresentacao_DuasFases");

            if (GUILayout.Button("Fase 1 atual — versão jogável"))
                OpenScene("Fase1_Prototipo");
        }

        EditorGUILayout.HelpBox("Use a aba Game para mostrar os rascunhos. Na versão jogável, aperte Play. Saia do Play para trocar de cena.", MessageType.Info);
    }

    private static void OpenScene(string sceneName)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity");
    }
}

