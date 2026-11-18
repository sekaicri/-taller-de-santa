using UnityEngine;

namespace Day02
{
    /// <summary>
    /// Closes a gift again some seconds after it was opened: the lid closes (Animator trigger "Cerrar", or it is shown again when there is no Animator) and the surprise hides.
    /// The gift calls CloseLater() from its Activated event.
    /// </summary>
    public class GiftAutoClose : MonoBehaviour
    {
        const float DEFAULT_DELAY = 4f;

        [SerializeField] private GameObject _lid;
        [SerializeField] private GameObject _surprise;
        [SerializeField] private Animator _lidAnimator;
        [SerializeField] private float _delay = DEFAULT_DELAY;

        public GameObject Lid { set => _lid = value; }
        public GameObject Surprise { set => _surprise = value; }
        public Animator LidAnimator { set => _lidAnimator = value; }

        // Called by the Activated event: schedules the close.
        public void CloseLater()
        {
            CancelInvoke(nameof(Close));
            Invoke(nameof(Close), _delay);
        }

        void Close()
        {
            if (_lidAnimator != null) _lidAnimator.SetTrigger("Cerrar");
            else if (_lid != null) _lid.SetActive(true);
            if (_surprise != null) _surprise.SetActive(false);
        }
    }
}
