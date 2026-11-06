using Dreamscape.PowerUps;
using UnityEngine;

namespace Dreamscape.Phases
{
    [DisallowMultipleComponent]
    public sealed class PhaseTwoFeedback : MonoBehaviour
    {
        [SerializeField] SpeedPickup pickup;
        [SerializeField] TemporaryBridge bridge;
        [SerializeField] AudioClip collectionSound;
        [SerializeField] AudioClip failureSound;
        [SerializeField] AudioClip successSound;
        [SerializeField] TextMesh pickupLabel;
        [SerializeField] Renderer[] floorRenderers;

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        MaterialPropertyBlock colors;
        Color[] initialColors;
        bool failureReported;

        void Awake()
        {
            if (pickup == null || bridge == null || pickupLabel == null || floorRenderers == null ||
                floorRenderers.Length == 0 || System.Array.Exists(floorRenderers, item => item == null))
            {
                Debug.LogError("Configure o coletável, a ponte, a placa e os pisos do retorno da Fase 2.", this);
                enabled = false;
                return;
            }
            colors = new MaterialPropertyBlock();
            initialColors = new Color[floorRenderers.Length];
            for (int i = 0; i < floorRenderers.Length; i++)
                initialColors[i] = floorRenderers[i].sharedMaterial.GetColor(BaseColor);
        }

        void OnEnable()
        {
            pickup.Collected += OnCollected;
            bridge.Crossed += OnCrossed;
            bridge.Collapsed += OnCollapsed;
        }

        void OnDisable()
        {
            if (pickup != null)
                pickup.Collected -= OnCollected;
            if (bridge == null)
                return;
            bridge.Crossed -= OnCrossed;
            bridge.Collapsed -= OnCollapsed;
        }

        void Update()
        {
            if (!bridge.HasStarted || bridge.HasCollapsed || bridge.HasCrossed)
                return;
            float urgency = 1f - bridge.TimeRemaining / bridge.CollapseDelay;
            for (int i = 0; i < floorRenderers.Length; i++)
            {
                floorRenderers[i].GetPropertyBlock(colors);
                colors.SetColor(BaseColor, Color.Lerp(initialColors[i], new Color(1f, 0.45f, 0.2f), urgency));
                floorRenderers[i].SetPropertyBlock(colors);
            }
        }

        void OnCollected()
        {
            pickupLabel.text = "VELOCIDADE COLETADA\nSHIFT PARA CORRER";
            pickupLabel.color = Color.yellow;
            Play(collectionSound);
        }

        void OnCrossed() => Play(successSound);

        void OnCollapsed()
        {
            if (!bridge.HasCrossed)
                ReportFailure();
        }

        public void ReportFailure()
        {
            if (failureReported)
                return;
            failureReported = true;
            Play(failureSound);
        }

        public bool ReportSuccess() => !failureReported && bridge.CompleteCrossing();

        static void Play(AudioClip clip)
        {
            if (clip == null)
                return;
            var sound = new GameObject("DS15 - " + clip.name);
            DontDestroyOnLoad(sound);
            var source = sound.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.7f;
            source.clip = clip;
            source.Play();
            // A duração escalada acompanha a pausa do AudioListener e permite terminar o som após a recarga.
            Destroy(sound, clip.length);
        }
    }
}
