using System;
using UnityEngine;

namespace Dreamscape.Characters
{
    /// <summary>
    /// Motor de locomoção compartilhado pelo protagonista e pela Moema.
    /// Quem controla (input do jogador ou IA) só informa intenção: direção, correr, agachar e pular.
    /// O motor cuida de aceleração, gravidade, pulo (coyote time + buffer), agachar com checagem de teto,
    /// rotação e parâmetros do Animator (todos opcionais).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Velocidades (m/s)")]
        [SerializeField] float walkSpeed = 2.2f;
        [SerializeField] float sprintSpeed = 5.0f;
        [SerializeField] float crouchSpeed = 1.2f;
        [Tooltip("Aceleração no chão (m/s²).")]
        [SerializeField] float acceleration = 25f;
        [Tooltip("Desaceleração no chão (m/s²).")]
        [SerializeField] float deceleration = 30f;
        [Tooltip("Fração da aceleração disponível no ar.")]
        [SerializeField, Range(0f, 1f)] float airControl = 0.35f;
        [Tooltip("Velocidade de giro (graus/s).")]
        [SerializeField] float turnSpeed = 720f;

        [Header("Pulo e gravidade")]
        [SerializeField] float jumpHeight = 1.1f;
        [SerializeField] float gravity = -20f;
        [SerializeField] float maxFallSpeed = 40f;
        [Tooltip("Tempo após sair do chão em que ainda dá para pular (s).")]
        [SerializeField] float coyoteTime = 0.12f;
        [Tooltip("Pulo apertado pouco antes de tocar o chão ainda vale (s).")]
        [SerializeField] float jumpBuffer = 0.15f;

        [Header("Agachar")]
        [SerializeField, Range(0.3f, 1f)] float crouchHeightFactor = 0.6f;
        [Tooltip("Velocidade da transição de altura do colisor (m/s).")]
        [SerializeField] float crouchTransitionSpeed = 4f;
        [Tooltip("Camadas que impedem de levantar.")]
        [SerializeField] LayerMask ceilingMask = ~0;

        [Header("Animação (opcional)")]
        [Tooltip("Vazio = procura nos filhos. Parâmetros usados se existirem no controller: " +
                 "Speed, MotionSpeed, Grounded, Crouch, VerticalSpeed, Jump (trigger).")]
        [SerializeField] Animator animator;

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
        static readonly int GroundedHash = Animator.StringToHash("Grounded");
        static readonly int CrouchHash = Animator.StringToHash("Crouch");
        static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        static readonly int JumpHash = Animator.StringToHash("Jump");

        CharacterController controller;
        float standHeight;
        Vector3 standCenter;

        Vector3 moveInput;
        Vector3 lookDirection;
        bool wantsSprint;
        bool wantsCrouch;
        float jumpRequestedAt = float.NegativeInfinity;
        float lastGroundedAt = float.NegativeInfinity;

        Vector3 horizontalVelocity;
        float verticalVelocity;
        bool wasGrounded;

        bool hasSpeed, hasMotionSpeed, hasGrounded, hasCrouch, hasVerticalSpeed, hasJump;
        readonly RaycastHit[] ceilingHits = new RaycastHit[8];

        /// <summary>Disparado no frame em que o pulo sai.</summary>
        public event Action Jumped;
        /// <summary>Disparado ao tocar o chão depois de estar no ar.</summary>
        public event Action Landed;

        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsSprinting { get; private set; }
        public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;
        public float HorizontalSpeed => horizontalVelocity.magnitude;
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public CharacterController Controller => controller;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            standHeight = controller.height;
            standCenter = controller.center;
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            CacheAnimatorParameters();
        }

        void OnValidate()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            crouchSpeed = Mathf.Clamp(crouchSpeed, 0f, walkSpeed);
            jumpHeight = Mathf.Max(0f, jumpHeight);
            gravity = Mathf.Min(-0.01f, gravity);
        }

        // ------------------------------------------------------------ API de controle

        /// <summary>Direção desejada no mundo (só XZ é usado). Magnitude 0..1 = intensidade.</summary>
        public void SetMoveInput(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            moveInput = Vector3.ClampMagnitude(worldDirection, 1f);
        }

        /// <summary>Para onde olhar quando parado (ex.: Moema virando para o jogador). Zero = mantém.</summary>
        public void SetLookDirection(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            lookDirection = worldDirection;
        }

        public void SetSprint(bool sprint) => wantsSprint = sprint;

        public void SetCrouch(bool crouch) => wantsCrouch = crouch;

        public void RequestJump() => jumpRequestedAt = Time.time;

        /// <summary>Move instantaneamente (checkpoints, cutscenes, Moema alcançando o jogador).</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            Physics.SyncTransforms();
        }

        // ------------------------------------------------------------ simulação

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            IsGrounded = controller.isGrounded;
            if (IsGrounded)
                lastGroundedAt = Time.time;

            UpdateCrouch(dt);
            UpdateHorizontal(dt);
            UpdateVertical(dt);

            CollisionFlags flags = controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;

            IsGrounded = controller.isGrounded;
            if (IsGrounded && !wasGrounded)
                Landed?.Invoke();
            wasGrounded = IsGrounded;

            UpdateRotation(dt);
            UpdateAnimator();
        }

        void UpdateHorizontal(float dt)
        {
            bool moving = moveInput.sqrMagnitude > 0.0001f;
            IsSprinting = wantsSprint && moving && !IsCrouching;
            float maxSpeed = IsCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
            Vector3 target = moveInput * maxSpeed;

            float rate = target.sqrMagnitude >= horizontalVelocity.sqrMagnitude ? acceleration : deceleration;
            if (!IsGrounded)
                rate *= airControl;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, rate * dt);
        }

        void UpdateVertical(float dt)
        {
            bool buffered = Time.time - jumpRequestedAt <= jumpBuffer;
            bool coyote = Time.time - lastGroundedAt <= coyoteTime;
            if (buffered && coyote && verticalVelocity <= 0.01f && !IsCrouching)
            {
                verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
                jumpRequestedAt = float.NegativeInfinity;
                lastGroundedAt = float.NegativeInfinity;
                if (hasJump)
                    animator.SetTrigger(JumpHash);
                Jumped?.Invoke();
                return;
            }

            if (IsGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f; // mantém colado em rampas e degraus
            else
                verticalVelocity = Mathf.Max(verticalVelocity + gravity * dt, -maxFallSpeed);
        }

        void UpdateRotation(float dt)
        {
            Vector3 face = moveInput.sqrMagnitude > 0.0001f ? moveInput : lookDirection;
            if (face.sqrMagnitude < 0.0001f)
                return;
            Quaternion target = Quaternion.LookRotation(face.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * dt);
        }

        void UpdateCrouch(float dt)
        {
            bool crouch = wantsCrouch || (IsCrouching && !CanStand());
            IsCrouching = crouch;

            float targetHeight = crouch ? standHeight * crouchHeightFactor : standHeight;
            float height = Mathf.MoveTowards(controller.height, targetHeight, crouchTransitionSpeed * dt);
            if (Mathf.Approximately(height, controller.height))
                return;
            controller.height = height;
            // pés continuam no mesmo lugar: o centro desce junto com a altura
            controller.center = standCenter - Vector3.up * ((standHeight - height) * 0.5f);
        }

        bool CanStand()
        {
            float radius = controller.radius * 0.95f;
            Vector3 bottom = transform.TransformPoint(controller.center) - Vector3.up * (controller.height * 0.5f - controller.radius);
            Vector3 top = bottom + Vector3.up * (controller.height - 2f * controller.radius);
            float distance = standHeight - controller.height;
            if (distance <= 0.001f)
                return true;
            int count = Physics.SphereCastNonAlloc(top, radius, Vector3.up, ceilingHits, distance, ceilingMask,
                QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (ceilingHits[i].collider != controller && !ceilingHits[i].collider.transform.IsChildOf(transform))
                    return false;
            }
            return true;
        }

        // ------------------------------------------------------------ animação

        void CacheAnimatorParameters()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                hasSpeed |= p.nameHash == SpeedHash;
                hasMotionSpeed |= p.nameHash == MotionSpeedHash;
                hasGrounded |= p.nameHash == GroundedHash;
                hasCrouch |= p.nameHash == CrouchHash;
                hasVerticalSpeed |= p.nameHash == VerticalSpeedHash;
                hasJump |= p.nameHash == JumpHash;
            }
        }

        void UpdateAnimator()
        {
            if (animator == null)
                return;
            float speed = HorizontalSpeed;
            if (hasSpeed) animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
            if (hasMotionSpeed) animator.SetFloat(MotionSpeedHash, sprintSpeed > 0f ? speed / sprintSpeed : 0f);
            if (hasGrounded) animator.SetBool(GroundedHash, IsGrounded);
            if (hasCrouch) animator.SetBool(CrouchHash, IsCrouching);
            if (hasVerticalSpeed) animator.SetFloat(VerticalSpeedHash, verticalVelocity);
        }
    }
}
