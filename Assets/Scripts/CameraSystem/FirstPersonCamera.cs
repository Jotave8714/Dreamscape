using Dreamscape.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dreamscape.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] CharacterController character;
        [SerializeField] Vector2 mouseSensitivity = new Vector2(0.12f, 0.1f);
        [SerializeField] Vector2 stickSensitivity = new Vector2(180f, 120f);
        [SerializeField, Range(0.05f, 0.3f)] float eyeInset = 0.12f;
        [SerializeField, Range(1f, 89f)] float pitchLimit = 85f;

        InputAction look;
        float yaw;
        float pitch;
        bool wasPaused;

        void Awake()
        {
            if (character == null)
                character = GetComponentInParent<CharacterController>();
            look = InputSystem.actions?.FindAction("Player/Look", true);
            yaw = transform.eulerAngles.y;
        }

        void OnEnable()
        {
            look?.Enable();
            SetCursorLocked(!GamePause.IsPaused);
        }

        void OnDisable() => SetCursorLocked(false);

        void Update()
        {
            if (GamePause.IsPaused)
            {
                wasPaused = true;
                SetCursorLocked(false);
                return;
            }
            if (wasPaused)
            {
                wasPaused = false;
                SetCursorLocked(true);
                return;
            }
            if (look == null || Time.deltaTime <= 0f)
                return;

            bool pointer = look.activeControl?.device is Pointer;
            if (pointer && Cursor.lockState != CursorLockMode.Locked)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    SetCursorLocked(true);
                return;
            }

            Vector2 delta = look.ReadValue<Vector2>();
            Vector2 step = pointer
                ? Vector2.Scale(delta, mouseSensitivity)
                : Vector2.Scale(delta, stickSensitivity) * Time.deltaTime;
            yaw += step.x;
            pitch = Mathf.Clamp(pitch - step.y, -pitchLimit, pitchLimit);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        void LateUpdate()
        {
            if (character == null)
                return;
            Vector3 eye = character.center + Vector3.up * (character.height * 0.5f - eyeInset);
            transform.position = character.transform.TransformPoint(eye);
            // O motor gira o corpo ao andar; a direção da visão permanece independente.
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        static void SetCursorLocked(bool value)
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }
}
