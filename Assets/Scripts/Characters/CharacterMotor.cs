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
        [SerializeField] bool rotateTowardsMovement = true;

        [Header("Pulo e gravidade")]
        [SerializeField] float jumpHeight = 1.1f;
        [SerializeField] float gravity = -20f;
        [SerializeField] float maxFallSpeed = 40f;
        [Tooltip("Tempo após sair do chão em que ainda dá para pular (s).")]
        [SerializeField] float coyoteTime = 0.12f;
        [Tooltip("Pulo apertado pouco antes de tocar o chão ainda vale (s).")]
        [SerializeField] float jumpBuffer = 0.15f;
        [Tooltip("Altura do pulo no ar (power-up de pulo duplo).")]
        [SerializeField] float airJumpHeight = 1.1f;

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
        [Tooltip("Suavização da animação ao perder velocidade (s). Maior = transição mais longa de correndo para parado.")]
        [SerializeField, Min(0f)] float stopAnimationDamp = 0.2f;

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
        int maxAirJumps;
        int airJumpsUsed;
        float speedMultiplier = 1f;
        float gravityScale = 1f;

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
        /// <summary>Pulos permitidos no ar antes de tocar o chão (0 = sem power-up).</summary>
        public int MaxAirJumps => maxAirJumps;
        public float GravityScale => gravityScale;
        /// <summary>Multiplicador de andar e correr (1 = sem power-up).</summary>
        public float SpeedMultiplier => speedMultiplier;
        public CharacterController Controller => controller;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            standHeight = controller.height;
            standCenter = controller.center;
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (animator != null)
                animator.applyRootMotion = false; // quem desloca o personagem é o motor
        }

        // no Start, e não no Awake: o PlayerInputController pode atribuir o controller no Awake dele
        void Start() => CacheAnimatorParameters();

        void OnValidate()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            crouchSpeed = Mathf.Clamp(crouchSpeed, 0f, walkSpeed);
            jumpHeight = Mathf.Max(0f, jumpHeight);
            airJumpHeight = Mathf.Max(0f, airJumpHeight);
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

        public void ClearJumpRequest() => jumpRequestedAt = float.NegativeInfinity;

        /// <summary>Define quantos pulos no ar são permitidos (power-up de pulo duplo = 1).</summary>
        public void SetMaxAirJumps(int count) => maxAirJumps = Mathf.Max(0, count);

        /// <summary>Define o multiplicador de andar e correr (power-up de velocidade). Agachado não muda.</summary>
        public void SetSpeedMultiplier(float multiplier) => speedMultiplier = Mathf.Max(0f, multiplier);

        /// <summary>Escala a gravidade da queda (power-up Pluma). O impulso do pulo não muda, então o salto fica mais longo.</summary>
        public void SetGravityScale(float scale) => gravityScale = Mathf.Clamp(scale, 0.1f, 1f);

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
            {
                lastGroundedAt = Time.time;
                if (verticalVelocity <= 0f)
                    airJumpsUsed = 0;
            }

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
            // no ar o sprint não muda: segurar Shift pulando não acelera, só vale quem já corria ao sair do chão
            bool sprintState = IsGrounded ? wantsSprint : IsSprinting;
            IsSprinting = sprintState && moving && !IsCrouching;
            float maxSpeed = IsCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed) * speedMultiplier;
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
                StartJump(jumpHeight);
                lastGroundedAt = float.NegativeInfinity;
                return;
            }

            if (buffered && !coyote && !IsGrounded && !IsCrouching && airJumpsUsed < maxAirJumps)
            {
                airJumpsUsed++;
                StartJump(airJumpHeight);
                return;
            }

            if (IsGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f; // mantém colado em rampas e degraus
            else
                verticalVelocity = Mathf.Max(verticalVelocity + gravity * gravityScale * dt, -maxFallSpeed);
        }

        void StartJump(float height)
        {
            verticalVelocity = Mathf.Sqrt(2f * height * -gravity);
            jumpRequestedAt = float.NegativeInfinity;
            if (hasJump)
                animator.SetTrigger(JumpHash);
            Jumped?.Invoke();
        }

        void UpdateRotation(float dt)
        {
            if (!rotateTowardsMovement)
                return;
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
            float topSpeed = sprintSpeed * speedMultiplier;
            if (hasSpeed)
            {
                // o motor para em menos de 0,2 s; a animação desacelera em mais tempo para não cortar da corrida para o parado
                float damp = speed < animator.GetFloat(SpeedHash) ? stopAnimationDamp : 0.1f;
                animator.SetFloat(SpeedHash, speed, damp, Time.deltaTime);
            }
            if (hasMotionSpeed) animator.SetFloat(MotionSpeedHash, topSpeed > 0f ? speed / topSpeed : 0f);
            if (hasGrounded) animator.SetBool(GroundedHash, IsGrounded);
            if (hasCrouch) animator.SetBool(CrouchHash, IsCrouching);
            if (hasVerticalSpeed) animator.SetFloat(VerticalSpeedHash, verticalVelocity);
        }
    }
}
