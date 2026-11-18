using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// Turns an object (like the order sign) to face the player's head, so it can be read from anywhere.
    /// Only turns around the vertical axis, so the sign never tilts.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            var head = Camera.main;
            if (head == null) return;

            // Direction from the head to the sign, flattened to the floor.
            var away = transform.position - head.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.LookRotation(away);
        }
    }
}
