using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dreamscape.UI
{
    /// <summary>Permite ajustar uma opção do menu com esquerda e direita (setas, A/D, direcional, analógico).</summary>
    public sealed class LeftRightInput : MonoBehaviour, IMoveHandler
    {
        public Action<int> Changed;

        public void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left)
                Changed?.Invoke(-1);
            else if (eventData.moveDir == MoveDirection.Right)
                Changed?.Invoke(1);
        }
    }
}
