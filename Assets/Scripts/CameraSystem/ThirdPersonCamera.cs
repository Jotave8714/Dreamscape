using Dreamscape.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dreamscape.CameraSystem
{
    /// <summary>
    /// Câmera orbital em terceira pessoa para o protagonista. Gira com a ação "Look" (mouse ou analógico direito),
    /// segue o alvo com suavização e se aproxima quando há parede entre ela e o personagem.
    /// Esc libera o cursor; clique na tela captura de novo.
    /// </summary>
    [DisallowMultipleComponent]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Tooltip("Vazio = primeiro PlayerInputController da cena.")]
        [SerializeField] Transform target;
        [Tooltip("Ponto de mira relativo aos pés do alvo (altura do ombro/cabeça).")]
        [SerializeField] Vector3 pivotOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] float distance = 4f;

        [Header("Input")]
        [Tooltip("Vazio = ações project-wide.")]
        [SerializeField] InputActionAsset actions;
        [SerializeField] string actionMap = "Player";
        [Tooltip("Graus por pixel de mouse.")]
        [SerializeField] Vector2 mouseSensitivity = new Vector2(0.12f, 0.1f);
        [Tooltip("Graus por segundo com o analógico no máximo.")]
        [SerializeField] Vector2 stickSensitivity = new Vector2(180f, 120f);
        [SerializeField] bool invertY;
        [SerializeField] float minPitch = -30f;
        [SerializeField] float maxPitch = 70f;

        [Header("Suavização e colisão")]
        [SerializeField] float followSharpness = 15f;
        [SerializeField] float zoomOutSharpness = 6f;
        [SerializeField] float collisionRadius = 0.2f;
        [SerializeField] float minDistance = 0.6f;
        [SerializeField] LayerMask collisionMask = ~0;
        [SerializeField] bool lockCursor = true;

        InputAction look;
        float yaw;
        float pitch = 15f;
        float currentDistance;
        Vector3 pivot;
        readonly RaycastHit[] hits = new RaycastHit[16];

        void Awake()
        {
            InputActionAsset asset = actions != null ? actions : InputSystem.actions;
            look = asset != null ? asset.FindActionMap(actionMap, true).FindAction("Look", true) : null;
            if (look == null)
                Debug.LogWarning($"{name}: ação Look não encontrada; a câmera só vai seguir o alvo.", this);
        }

        void OnEnable()
        {
            look?.Enable();
            if (lockCursor)
                SetCursorLocked(true);
        }

        void OnDisable() => SetCursorLocked(false);

        void Start()
        {
            ResolveTarget();
            yaw = target != null ? target.eulerAngles.y : transform.eulerAngles.y;
            currentDistance = distance;
            if (target != null)
                pivot = target.position + pivotOffset;
        }

        /// <summary>Troca o alvo (ex.: focar a Moema num diálogo) e reposiciona sem suavizar.</summary>
        public void SetTarget(Transform newTarget, bool snap = true)
        {
            target = newTarget;
            if (snap && target != null)
                pivot = target.position + pivotOffset;
        }

        void ResolveTarget()
        {
            if (target != null)
                return;
            PlayerInputController player = FindAnyObjectByType<PlayerInputController>();
            if (player != null)
                target = player.transform;
        }

        void LateUpdate()
        {
            ResolveTarget();
            if (target == null)
                return;

            float dt = Time.deltaTime;
            HandleCursor();
            ReadLook(dt);

            Vector3 desiredPivot = target.position + pivotOffset;
            pivot = Vector3.Lerp(pivot, desiredPivot, 1f - Mathf.Exp(-followSharpness * dt));

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 back = rotation * Vector3.back;
            float allowed = CollisionDistance(pivot, back);
            // entra na hora (não atravessa parede), sai devagar
            currentDistance = allowed < currentDistance
                ? allowed
                : Mathf.Lerp(currentDistance, allowed, 1f - Mathf.Exp(-zoomOutSharpness * dt));

            transform.SetPositionAndRotation(pivot + back * currentDistance, rotation);
        }

        void ReadLook(float dt)
        {
            if (look == null)
                return;
            Vector2 delta = look.ReadValue<Vector2>();
            bool pointer = look.activeControl != null && look.activeControl.device is Pointer;
            if (pointer && lockCursor && Cursor.lockState != CursorLockMode.Locked)
                return; // mouse solto (menu/Esc) não gira a câmera; analógico continua valendo
            Vector2 step = pointer ? Vector2.Scale(delta, mouseSensitivity) : Vector2.Scale(delta, stickSensitivity) * dt;
            yaw += step.x;
            pitch += invertY ? step.y : -step.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        float CollisionDistance(Vector3 origin, Vector3 direction)
        {
            int count = Physics.SphereCastNonAlloc(origin, collisionRadius, direction, hits, distance, collisionMask,
                QueryTriggerInteraction.Ignore);
            float best = distance;
            for (int i = 0; i < count; i++)
            {
                Collider c = hits[i].collider;
                // ignora personagens (jogador, Moema) para a câmera não "pular" quando alguém passa na frente
                if (c is CharacterController || c.transform.IsChildOf(target))
                    continue;
                if (hits[i].distance > 0f && hits[i].distance < best)
                    best = hits[i].distance;
            }
            return Mathf.Max(minDistance, best);
        }

        void HandleCursor()
        {
            if (!lockCursor)
                return;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SetCursorLocked(false);
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                     Cursor.lockState != CursorLockMode.Locked)
                SetCursorLocked(true);
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
