using UnityEngine;
using UnityEngine.AI;

namespace Dreamscape.Characters
{
    /// <summary>
    /// IA de acompanhante da Moema: segue o protagonista usando o mesmo <see cref="CharacterMotor"/> do jogador.
    /// Com NavMesh assada na cena, contorna obstáculos pelo caminho calculado; sem NavMesh, vai em linha reta.
    /// Corre quando fica longe, agacha junto, pula atrás do jogador, tenta pular quando trava e, em último caso,
    /// teleporta para perto dele.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [DisallowMultipleComponent]
    public class MoemaCompanion : MonoBehaviour
    {
        [Tooltip("Vazio = primeiro PlayerInputController da cena.")]
        [SerializeField] Transform target;

        [Header("Distâncias (m)")]
        [Tooltip("Para de andar ao chegar nesta distância.")]
        [SerializeField] float followDistance = 2.0f;
        [Tooltip("Volta a andar quando o alvo passa desta distância (histerese evita ficar trepidando).")]
        [SerializeField] float resumeDistance = 3.0f;
        [Tooltip("Acima desta distância ela corre.")]
        [SerializeField] float sprintDistance = 6f;
        [Tooltip("Acima desta distância ela teleporta para perto do alvo.")]
        [SerializeField] float teleportDistance = 25f;
        [Tooltip("Posição de chegada do teleporte, no espaço local do alvo (atrás e ao lado).")]
        [SerializeField] Vector3 teleportOffset = new Vector3(1.2f, 0f, -1.5f);

        [Header("Comportamento")]
        [SerializeField] bool mirrorCrouch = true;
        [SerializeField] bool mirrorJump = true;
        [Tooltip("Atraso para copiar o pulo do jogador (s).")]
        [SerializeField] float jumpReactionDelay = 0.15f;
        [Tooltip("Tempo sem progresso até considerar que travou (s).")]
        [SerializeField] float stuckTime = 1.2f;

        [Header("Navegação")]
        [SerializeField] float repathInterval = 0.25f;
        [SerializeField] float waypointReachedDistance = 0.35f;
        [SerializeField] float navMeshSampleRadius = 2f;

        CharacterMotor motor;
        CharacterMotor targetMotor;
        NavMeshPath path;
        readonly Vector3[] corners = new Vector3[32];
        int cornerCount;
        int cornerIndex;
        float nextRepathAt;
        bool following;
        float stuckTimer;
        float pendingJumpAt = float.PositiveInfinity;
        Vector3 lastPosition;

        public Transform Target => target;
        public bool IsFollowing => following;

        void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            path = new NavMeshPath();
            lastPosition = transform.position;
        }

        void OnEnable() => Subscribe();

        void OnDisable() => Unsubscribe();

        /// <summary>Troca quem a Moema segue (null = para no lugar).</summary>
        public void SetTarget(Transform newTarget)
        {
            Unsubscribe();
            target = newTarget;
            cornerCount = 0;
            following = false;
            if (isActiveAndEnabled)
                Subscribe();
        }

        void Subscribe()
        {
            targetMotor = target != null ? target.GetComponent<CharacterMotor>() : null;
            if (targetMotor != null)
                targetMotor.Jumped += OnTargetJumped;
        }

        void Unsubscribe()
        {
            if (targetMotor != null)
                targetMotor.Jumped -= OnTargetJumped;
            targetMotor = null;
        }

        void Update()
        {
            if (target == null)
            {
                PlayerInputController player = FindAnyObjectByType<PlayerInputController>();
                if (player == null)
                {
                    Idle(Vector3.zero);
                    return;
                }
                SetTarget(player.transform);
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > teleportDistance)
            {
                TeleportNearTarget();
                return;
            }

            motor.SetCrouch(mirrorCrouch && targetMotor != null && targetMotor.IsCrouching);

            if (Time.time >= pendingJumpAt)
            {
                pendingJumpAt = float.PositiveInfinity;
                motor.RequestJump();
            }

            if (!following && distance > resumeDistance)
                following = true;
            else if (following && distance <= followDistance)
                following = false;

            if (!following)
            {
                Idle(toTarget);
                return;
            }

            Vector3 direction = NextDirection();
            // desacelera chegando perto, sem nunca ficar lenta demais para andar
            float intensity = Mathf.Clamp(Mathf.InverseLerp(followDistance, followDistance + 1.5f, distance), 0.4f, 1f);
            motor.SetMoveInput(direction * intensity);
            motor.SetSprint(distance > sprintDistance);
            motor.SetLookDirection(Vector3.zero);
            CheckStuck();
        }

        void Idle(Vector3 lookAt)
        {
            motor.SetMoveInput(Vector3.zero);
            motor.SetSprint(false);
            motor.SetLookDirection(lookAt);
            stuckTimer = 0f;
            lastPosition = transform.position;
        }

        void OnTargetJumped()
        {
            if (!mirrorJump || target == null)
                return;
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude < sprintDistance)
                pendingJumpAt = Time.time + jumpReactionDelay;
        }

        // ------------------------------------------------------------ navegação

        Vector3 NextDirection()
        {
            if (Time.time >= nextRepathAt)
            {
                nextRepathAt = Time.time + repathInterval;
                RecalculatePath();
            }

            while (cornerIndex < cornerCount)
            {
                Vector3 toCorner = corners[cornerIndex] - transform.position;
                toCorner.y = 0f;
                if (toCorner.magnitude > waypointReachedDistance)
                    return toCorner.normalized;
                cornerIndex++;
            }

            Vector3 straight = target.position - transform.position;
            straight.y = 0f;
            return straight.sqrMagnitude > 0.0001f ? straight.normalized : Vector3.zero;
        }

        void RecalculatePath()
        {
            cornerCount = 0;
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit from, navMeshSampleRadius, NavMesh.AllAreas))
                return;
            if (!NavMesh.SamplePosition(target.position, out NavMeshHit to, navMeshSampleRadius, NavMesh.AllAreas))
                return;
            if (!NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path) ||
                path.status == NavMeshPathStatus.PathInvalid)
                return;
            cornerCount = path.GetCornersNonAlloc(corners);
            cornerIndex = cornerCount > 1 ? 1 : 0; // o primeiro canto é a própria posição
        }

        void CheckStuck()
        {
            float moved = (transform.position - lastPosition).magnitude;
            lastPosition = transform.position;
            float expected = motor.WalkSpeed * 0.25f * Time.deltaTime;
            stuckTimer = moved < expected ? stuckTimer + Time.deltaTime : 0f;

            if (stuckTimer > stuckTime && motor.IsGrounded)
                motor.RequestJump();
            if (stuckTimer > stuckTime * 3f)
                TeleportNearTarget();
        }

        void TeleportNearTarget()
        {
            Vector3 position = target.position + target.rotation * teleportOffset;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSampleRadius * 1.5f, NavMesh.AllAreas))
                position = hit.position;
            Vector3 look = target.position - position;
            look.y = 0f;
            motor.Teleport(position, look.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(look) : transform.rotation);
            following = false;
            stuckTimer = 0f;
            cornerCount = 0;
            lastPosition = position;
        }
    }
}
