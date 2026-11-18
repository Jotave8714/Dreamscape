using System;
using Dreamscape.Characters;
using Dreamscape.Core;
using UnityEngine;

namespace Dreamscape.Phases
{
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent]
    public sealed class TemporaryBridge : MonoBehaviour
    {
        [SerializeField] GameObject[] floorTiles;
        [Tooltip("Tempo entre cruzar a faixa de ativação e a queda de todos os pisos.")]
        [SerializeField, Min(0.1f)] float collapseDelay = 1.6f;
        [SerializeField] TextMesh statusLabel;

        public bool HasStarted { get; private set; }
        public bool HasCollapsed { get; private set; }
        public bool HasCrossed { get; private set; }
        public float TimeRemaining { get; private set; }
        public float CollapseDelay => collapseDelay;
        public event Action Collapsed;
        public event Action Crossed;

        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            if (floorTiles == null || floorTiles.Length == 0 || System.Array.Exists(floorTiles, tile => tile == null))
            {
                Debug.LogError("Configure os pisos da ponte temporária no Inspector.", this);
                enabled = false;
                return;
            }
            TimeRemaining = collapseDelay;
            RefreshLabel();
        }

        void OnTriggerEnter(Collider other) => TryStart(other);

        void OnTriggerStay(Collider other) => TryStart(other);

        void TryStart(Collider other)
        {
            if (!enabled || HasStarted || GamePause.IsPaused || !other.GetComponentInParent<PlayerInputController>())
                return;

            HasStarted = true;
            RefreshLabel();
        }

        public bool CompleteCrossing()
        {
            if (!HasStarted || HasCollapsed || HasCrossed || GamePause.IsPaused)
                return false;
            HasCrossed = true;
            RefreshLabel();
            Crossed?.Invoke();
            return true;
        }

        void Update()
        {
            if (!HasStarted || HasCollapsed || GamePause.IsPaused)
                return;

            TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
            if (TimeRemaining <= 0f)
            {
                HasCollapsed = true;
                foreach (GameObject tile in floorTiles)
                    tile.SetActive(false);
                Collapsed?.Invoke();
            }
            RefreshLabel();
        }

        void RefreshLabel()
        {
            if (statusLabel == null)
                return;
            statusLabel.color = HasCrossed ? Color.green : HasCollapsed ? Color.red : HasStarted ? Color.yellow : Color.white;
            statusLabel.text = HasCrossed
                ? "TRAVESSIA CONCLUIDA\nSIGA ATE A SAIDA"
                : HasCollapsed
                ? "PISO CEDEU\nESC: REINICIAR FASE"
                : HasStarted
                    ? $"CORRA!\n{TimeRemaining:0.0} s"
                    : $"PONTE TEMPORARIA\nA FAIXA INICIA {collapseDelay:0.0} s";
        }
    }
}
