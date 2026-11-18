using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VrClass
{
    /// <summary>
    /// One line of the order sign: a check box, the toy picture and its name.
    /// This is the prefab that OrderList creates once for each toy of the order.
    /// </summary>
    public class OrderRow : MonoBehaviour
    {
        [SerializeField] private Image _check;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;

        // Sets the name and the picture of the toy.
        public void Show(string label, Sprite icon)
        {
            _label.text = label;
            _icon.sprite = icon;
            _icon.enabled = icon != null;
        }

        // Ticks or clears the check box.
        public void SetPacked(bool packed, Sprite emptyBox, Sprite checkedBox)
        {
            if (emptyBox != null && checkedBox != null) _check.sprite = packed ? checkedBox : emptyBox;
            else _check.color = packed ? Color.green : Color.white;
        }
    }
}
