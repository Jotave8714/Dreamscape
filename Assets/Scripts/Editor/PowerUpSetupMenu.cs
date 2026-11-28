using Dreamscape.PowerUps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Menu "Dreamscape > Configurar power-ups na cena": liga os scripts aos coletáveis desenhados nas fases,
    /// reconhecidos pela marca do card no nome. Pode rodar de novo sem duplicar nada.
    /// </summary>
    public static class PowerUpSetupMenu
    {
        [MenuItem("Dreamscape/Configurar power-ups na cena")]
        public static void Setup()
        {
            Undo.SetCurrentGroupName("Configurar power-ups");
            int group = Undo.GetCurrentGroup();

            int count = Bind<DoubleJumpPickup>("[DS10]") + Bind<SpeedPickup>("[DS11]");

            Undo.CollapseUndoOperations(group);
            Debug.Log($"Dreamscape: {count} coletável(is) de power-up configurado(s). Salve a cena para manter.");
        }

        static int Bind<T>(string marker) where T : PowerUpPickup
        {
            int count = 0;
            foreach (Collider collider in Object.FindObjectsByType<Collider>(FindObjectsInactive.Include))
            {
                if (!collider.name.Contains(marker))
                    continue;

                Undo.RecordObject(collider, "Configurar power-ups");
                collider.isTrigger = true;
                if (!collider.TryGetComponent<T>(out _))
                    Undo.AddComponent<T>(collider.gameObject);
                EditorSceneManager.MarkSceneDirty(collider.gameObject.scene);
                count++;
            }
            return count;
        }
    }
}
