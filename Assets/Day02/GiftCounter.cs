using UnityEngine;

namespace Day02
{
    /// <summary>
    /// Counts how many gifts were opened. Each gift calls Open() from its Activated event.
    /// </summary>
    public class GiftCounter : MonoBehaviour
    {
        [SerializeField] private int _total = 3;

        int _opened;

        // Called by the Activated event of a gift.
        public void Open()
        {
            _opened++;
            Debug.Log("Gifts opened: " + _opened + " of " + _total);
            if (_opened >= _total) Debug.Log("All gifts opened!");
        }
    }
}
