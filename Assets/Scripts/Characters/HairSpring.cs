using System.Collections.Generic;
using UnityEngine;

namespace Dreamscape.Characters
{
    /// <summary>
    /// Faz uma corrente de ossos (o rabo de cavalo da Moema) balançar com o movimento do corpo, depois que o Animator
    /// posicionou o esqueleto. Cada osso tem uma ponta com inércia, puxada de volta para a pose da animação.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HairSpring : MonoBehaviour
    {
        [Tooltip("Nomes dos ossos, da raiz para a ponta. A última ponta continua na direção do último osso.")]
        [SerializeField] string[] boneNames = { "Ponytail1", "Ponytail2", "Ponytail3" };
        [Tooltip("Força que puxa de volta para a pose. Maior = cabelo mais rígido.")]
        [SerializeField] float stiffness = 14f;
        [Tooltip("Perda de velocidade por segundo. Maior = balança menos tempo.")]
        [SerializeField] float damping = 5f;
        [Tooltip("Quanto o cabelo cai com o próprio peso (m/s²).")]
        [SerializeField] float gravity = 6f;
        [Tooltip("Ângulo máximo em relação à pose (graus), para não atravessar o corpo.")]
        [SerializeField] float maxAngle = 55f;
        [Tooltip("Acima desta distância em um quadro é teleporte: reposiciona sem balançar (m).")]
        [SerializeField] float teleportDistance = 2f;

        sealed class Link
        {
            public Transform Bone;
            public Quaternion RestLocal;
            public Vector3 LocalTip;
            public float Length;
            public Vector3 Tip, PreviousTip;
        }

        readonly List<Link> links = new List<Link>();
        Vector3 lastRootPosition;
        bool settled;

        public Transform FirstBone => links.Count > 0 ? links[0].Bone : null;
        public bool IsValid => links.Count > 0;

        void Awake()
        {
            var bones = new List<Transform>();
            foreach (string boneName in boneNames)
            {
                Transform bone = Find(transform, boneName);
                if (bone != null)
                    bones.Add(bone);
            }

            for (int i = 0; i < bones.Count; i++)
            {
                // a pose de repouso precisa ser a do modelo, então é lida antes de qualquer animação
                Vector3 toNext = i + 1 < bones.Count
                    ? bones[i].InverseTransformPoint(bones[i + 1].position)
                    : bones[i].InverseTransformPoint(bones[i].position + (bones[i].position - bones[i - 1].position));
                links.Add(new Link
                {
                    Bone = bones[i],
                    RestLocal = bones[i].localRotation,
                    LocalTip = toNext,
                    Length = bones[i].TransformVector(toNext).magnitude,
                });
            }

            if (links.Count == 0)
                enabled = false;
        }

        static Transform Find(Transform root, string boneName)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == boneName)
                    return t;
            return null;
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
                return; // pausado: mantém o cabelo como está

            if (!settled || (transform.position - lastRootPosition).sqrMagnitude > teleportDistance * teleportDistance)
                Settle();
            lastRootPosition = transform.position;

            // passos grandes (queda de FPS) não podem fazer a mola explodir
            dt = Mathf.Min(dt, 1f / 30f);
            float keepVelocity = Mathf.Exp(-damping * dt);
            float pull = 1f - Mathf.Exp(-stiffness * dt);

            foreach (Link link in links)
            {
                link.Bone.localRotation = link.RestLocal;
                Vector3 rest = link.Bone.TransformPoint(link.LocalTip);

                Vector3 velocity = (link.Tip - link.PreviousTip) * keepVelocity;
                link.PreviousTip = link.Tip;
                Vector3 tip = link.Tip + velocity + Vector3.down * (gravity * dt * dt);
                tip = Vector3.Lerp(tip, rest, pull);

                Vector3 restDirection = rest - link.Bone.position;
                Vector3 direction = tip - link.Bone.position;
                if (direction.sqrMagnitude < 0.000001f)
                    direction = restDirection;
                direction = Vector3.RotateTowards(restDirection.normalized, direction.normalized, maxAngle * Mathf.Deg2Rad, 0f);
                link.Tip = link.Bone.position + direction * link.Length;

                link.Bone.rotation = Quaternion.FromToRotation(restDirection, direction) * link.Bone.rotation;
            }
        }

        void Settle()
        {
            foreach (Link link in links)
            {
                link.Bone.localRotation = link.RestLocal;
                link.Tip = link.PreviousTip = link.Bone.TransformPoint(link.LocalTip);
            }
            settled = true;
        }
    }
}
