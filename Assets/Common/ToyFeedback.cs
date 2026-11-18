using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VrClass
{
    /// <summary>
    /// Sound and vibration in the hand of the player: a small tap when a toy is grabbed,
    /// a stronger one (and the wrapping sound) when the trigger is pressed.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ToyFeedback : MonoBehaviour
    {
        const float GRAB_HAPTIC = 0.3f;
        const float WRAP_HAPTIC = 0.8f;
        const float HAPTIC_SECONDS = 0.1f;
        const float SOUND_VOLUME = 0.6f;

        XRGrabInteractable _grab;

        void Awake()
        {
            _grab = GetComponent<XRGrabInteractable>();
        }

        void OnEnable()
        {
            _grab.selectEntered.AddListener(OnGrabbed);
            _grab.activated.AddListener(OnTrigger);
        }

        void OnDisable()
        {
            _grab.selectEntered.RemoveListener(OnGrabbed);
            _grab.activated.RemoveListener(OnTrigger);
        }

        void OnGrabbed(SelectEnterEventArgs args)
        {
            AudioSource.PlayClipAtPoint(FeedbackTones.Grab, transform.position, SOUND_VOLUME);
            Vibrate(args.interactorObject, GRAB_HAPTIC);
        }

        void OnTrigger(ActivateEventArgs args)
        {
            AudioSource.PlayClipAtPoint(FeedbackTones.Wrap, transform.position, SOUND_VOLUME);
            Vibrate(args.interactorObject, WRAP_HAPTIC);
        }

        // Vibrates the controller that holds the toy (the simulator has no vibration).
        static void Vibrate(IXRInteractor interactor, float amplitude)
        {
            if (interactor is XRBaseInputInteractor input) input.SendHapticImpulse(amplitude, HAPTIC_SECONDS);
        }
    }
}
