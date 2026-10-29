using UnityEngine;
using UnityEngine.InputSystem;

namespace Dreamscape.Characters
{
    /// <summary>
    /// Lê o Input System (mapa "Player" do InputSystem_Actions) e comanda o <see cref="CharacterMotor"/>
    /// do protagonista. O movimento é relativo à câmera.
    /// Teclado: WASD anda, Shift corre, C agacha, Espaço pula. Controle: analógico esquerdo, L3, B/Círculo, A/X.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [DisallowMultipleComponent]
    public class PlayerInputController : MonoBehaviour
    {
        [Tooltip("Vazio = ações project-wide (Project Settings > Input System Package).")]
        [SerializeField] InputActionAsset actions;
        [SerializeField] string actionMap = "Player";
        [Tooltip("Vazio = Camera.main.")]
        [SerializeField] Transform cameraTransform;

        [Header("Preferências")]
        [Tooltip("Agachar alterna ao apertar (desligado = segurar).")]
        [SerializeField] bool toggleCrouch = true;
        [Tooltip("Correr alterna ao apertar (desligado = segurar). Desliga sozinho ao parar.")]
        [SerializeField] bool toggleSprint;

        CharacterMotor motor;
        InputActionMap map;
        InputAction move, sprint, crouch, jump;
        bool crouchToggled, sprintToggled;
        bool inputEnabled = true;

        public CharacterMotor Motor => motor;

        void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            InputActionAsset asset = actions != null ? actions : InputSystem.actions;
            if (asset == null)
            {
                Debug.LogError($"{name}: nenhum InputActionAsset configurado nem ações project-wide.", this);
                enabled = false;
                return;
            }
            map = asset.FindActionMap(actionMap, true);
            move = map.FindAction("Move", true);
            sprint = map.FindAction("Sprint", true);
            crouch = map.FindAction("Crouch", true);
            jump = map.FindAction("Jump", true);
        }

        void OnEnable()
        {
            if (map == null)
                return;
            map.Enable();
            jump.performed += OnJump;
            crouch.performed += OnCrouch;
            sprint.performed += OnSprint;
        }

        void OnDisable()
        {
            if (map == null)
                return;
            jump.performed -= OnJump;
            crouch.performed -= OnCrouch;
            sprint.performed -= OnSprint;
            StopMotor();
        }

        /// <summary>Liga/desliga o controle do jogador (menus, diálogos, cutscenes).</summary>
        public void SetInputEnabled(bool value)
        {
            inputEnabled = value;
            if (!value)
                StopMotor();
        }

        void Update()
        {
            if (!inputEnabled)
                return;

            Vector2 input = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
            motor.SetMoveInput(CameraRelative(input));

            if (input.sqrMagnitude < 0.01f)
                sprintToggled = false;
            motor.SetSprint(toggleSprint ? sprintToggled : sprint.IsPressed());
            motor.SetCrouch(toggleCrouch ? crouchToggled : crouch.IsPressed());
        }

        Vector3 CameraRelative(Vector2 input)
        {
            Transform cam = cameraTransform != null ? cameraTransform : Camera.main != null ? Camera.main.transform : null;
            if (cam == null)
                return new Vector3(input.x, 0f, input.y);

            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f) // câmera olhando reto para baixo
                forward = Vector3.ProjectOnPlane(cam.up, Vector3.up);
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            return forward * input.y + right * input.x;
        }

        void OnJump(InputAction.CallbackContext _)
        {
            if (!inputEnabled)
                return;
            if (crouchToggled)
            {
                crouchToggled = false; // pular levanta
                return;
            }
            motor.RequestJump();
        }

        void OnCrouch(InputAction.CallbackContext _)
        {
            if (!inputEnabled || !toggleCrouch)
                return;
            crouchToggled = !crouchToggled;
            if (crouchToggled)
                sprintToggled = false;
        }

        void OnSprint(InputAction.CallbackContext _)
        {
            if (!inputEnabled)
                return;
            crouchToggled = false; // correr levanta
            if (toggleSprint)
                sprintToggled = !sprintToggled;
        }

        void StopMotor()
        {
            if (motor == null)
                return;
            motor.SetMoveInput(Vector3.zero);
            motor.SetSprint(false);
        }
    }
}
