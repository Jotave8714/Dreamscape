using System;
using Dreamscape.PowerUps;
using Dreamscape.Routes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    public static class PhaseOneGraphSetup
    {
        public const string ScenePath = "Assets/Scenes/DS07_Fase1_Jogavel.unity";

        [MenuItem("Dreamscape/DS-14/Configurar grafo na Fase 1")]
        public static void Setup()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var scene = EditorSceneManager.OpenScene(ScenePath);
            SetupActiveScene();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Dreamscape/DS-14/Configurar grafo na cena aberta")]
        public static void SetupActiveScene()
        {
            var definitions = new[]
            {
                ("N0", "N0 Entrada", "N0 - Entrada e checkpoint"),
                ("N1", "N1 Coletavel", "N0 - Entrada e checkpoint"),
                ("N2", "N2 Isca", "N2 - Aproximacao da falsa saida"),
                ("T0", "T0 Chao falso", "T0 - Chao falso - ceder ao pisar [DS08]"),
                ("N3", "N3 Plataforma A", "N3 - Plataforma A"),
                ("N4", "N4 Plataforma B", "N4 - Plataforma B [exige pulo duplo - calibrar]"),
                ("N5", "N5 Plataforma C", "N5 - Plataforma C"),
                ("N6", "N6 Patamar final", "N6 - Patamar final"),
                ("N7", "N7 Conclusao", "N6 - Patamar final")
            };
            var markers = new Transform[definitions.Length];
            var floors = new Collider[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
            {
                markers[i] = Required(definitions[i].Item2).transform;
                floors[i] = Required(definitions[i].Item3).GetComponent<Collider>();
                if (floors[i] == null)
                    throw new InvalidOperationException($"Piso sem colisor: {definitions[i].Item3}");
            }
            GameObject pickup = Required("N1 - Coletavel pulo duplo [DS10]");
            PowerUpSetupMenu.Setup();

            Undo.SetCurrentGroupName("Configurar grafo DS-14");
            int group = Undo.GetCurrentGroup();
            GameObject root = GameObject.Find("DS14 - Grafo da Fase 1");
            if (root == null)
            {
                root = new GameObject("DS14 - Grafo da Fase 1");
                Undo.RegisterCreatedObjectUndo(root, "Grafo da Fase 1");
            }
            PhaseRouteGraph graph = root.GetComponent<PhaseRouteGraph>();
            if (graph == null)
                graph = Undo.AddComponent<PhaseRouteGraph>(root);
            var serialized = new SerializedObject(graph);
            var points = serialized.FindProperty("points");
            points.arraySize = definitions.Length;
            for (int i = 0; i < definitions.Length; i++)
            {
                var point = points.GetArrayElementAtIndex(i);
                point.FindPropertyRelative("id").stringValue = definitions[i].Item1;
                point.FindPropertyRelative("marker").objectReferenceValue = markers[i];
                point.FindPropertyRelative("floor").objectReferenceValue = floors[i];
                point.FindPropertyRelative("safe").boolValue = definitions[i].Item1 != "T0";
                point.FindPropertyRelative("pickup").objectReferenceValue = i == 1 ? pickup.GetComponent<DoubleJumpPickup>() : null;
            }
            var edges = new[]
            {
                new PowerRouteSearch.Connection(0, 1), new PowerRouteSearch.Connection(1, 0),
                new PowerRouteSearch.Connection(0, 4), new PowerRouteSearch.Connection(1, 4),
                new PowerRouteSearch.Connection(0, 2), new PowerRouteSearch.Connection(2, 3),
                new PowerRouteSearch.Connection(4, 5, true), new PowerRouteSearch.Connection(5, 6),
                new PowerRouteSearch.Connection(6, 7), new PowerRouteSearch.Connection(7, 8)
            };
            var connections = serialized.FindProperty("connections");
            connections.arraySize = edges.Length;
            for (int i = 0; i < edges.Length; i++)
            {
                var edge = connections.GetArrayElementAtIndex(i);
                edge.FindPropertyRelative("from").intValue = edges[i].from;
                edge.FindPropertyRelative("to").intValue = edges[i].to;
                edge.FindPropertyRelative("requiresDoubleJump").boolValue = edges[i].requiresDoubleJump;
                edge.FindPropertyRelative("blocked").boolValue = false;
            }
            serialized.FindProperty("start").intValue = 0;
            serialized.FindProperty("goal").intValue = 8;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(root.scene);
            Undo.CollapseUndoOperations(group);
            graph.ValidateAndLog();
        }

        static GameObject Required(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
                throw new InvalidOperationException($"Objeto da Fase 1 ausente: {name}");
            return go;
        }
    }
}
