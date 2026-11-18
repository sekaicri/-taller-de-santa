using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Day03
{
    /// <summary>
    /// Empties a socket: whatever enters it disappears after a short delay, so the next object can be placed.
    /// </summary>
    [RequireComponent(typeof(XRSocketInteractor))]
    public class SocketClearer : MonoBehaviour
    {
        const string WRAPPER_NAME = "Envoltorio";
        const float DEFAULT_DELAY = 1f;

        [SerializeField] private float _delay = DEFAULT_DELAY;

        XRSocketInteractor _socket;

        void OnEnable()
        {
            _socket = GetComponent<XRSocketInteractor>();
            _socket.selectEntered.AddListener(OnEntered);
        }

        void OnDisable()
        {
            if (_socket != null) _socket.selectEntered.RemoveListener(OnEntered);
        }

        // The object that entered the socket is removed after the delay.
        void OnEntered(SelectEnterEventArgs args)
        {
            var packed = args.interactableObject.transform;
            if (packed.Find(WRAPPER_NAME) == null)
                Debug.LogWarning(packed.name + " is not a toy: it has no wrapper. It should not go in the crate.");
            else
                Debug.Log(packed.name + " delivered to the crate.");

            Destroy(packed.gameObject, _delay);
        }
    }
}
