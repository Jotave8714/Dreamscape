using UnityEngine;
using UnityEngine.InputSystem;

namespace Dreamscape.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] private CharacterController target;
        [SerializeField] private float sensitivity = 0.12f;
        private float yaw;
        private float pitch;

        private void OnEnable()
        {
            yaw = transform.eulerAngles.y;
            CaptureCursor();
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LateUpdate()
        {
            if (!target)
                return;

            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                CaptureCursor();
            }

            if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null && Time.timeScale > 0f)
            {
                Vector2 look = Mouse.current.delta.ReadValue() * sensitivity;
                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
            }

            Vector3 eye = target.center + Vector3.up * (target.height * 0.5f - 0.12f);
            transform.SetPositionAndRotation(target.transform.TransformPoint(eye), Quaternion.Euler(pitch, yaw, 0f));
        }

        private static void CaptureCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
