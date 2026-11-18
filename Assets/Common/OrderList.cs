using System;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The order sign: creates one row for each toy of the order and ticks it when that toy is packed.
    /// The rows go inside a Vertical Layout Group, so the list grows by itself with the number of toys.
    /// The rows are built when the scene starts and again each time the game changes the order; the boxes change only when the game tells it something happened.
    /// </summary>
    public class OrderList : MonoBehaviour
    {
        // The name of a toy in the scene, the text to show for it and its picture.
        [Serializable]
        public class ToyInfo
        {
            public string toyName;
            public string label;
            public Sprite icon;
        }

        [SerializeField] private PackingGame _game;
        [SerializeField] private Transform _listRoot;   // the object that has the Vertical Layout Group
        [SerializeField] private OrderRow _rowPrefab;
        [SerializeField] private ToyInfo[] _toys;
        [SerializeField] private Sprite _emptySprite;
        [SerializeField] private Sprite _checkedSprite;

        OrderRow[] _rows = new OrderRow[0];

        void Start() { Build(); }

        // Builds one row for each name in the order, in the same order of the list.
        void Build()
        {
            // Removes the old rows (and any preview rows left in the scene), so only the new rows remain.
            for (var i = _listRoot.childCount - 1; i >= 0; i--)
            {
                var old = _listRoot.GetChild(i).gameObject;
                old.SetActive(false);   // switched off now, so the layout ignores it until it is destroyed
                Destroy(old);
            }

            var names = _game.ToysToPack;
            _rows = new OrderRow[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                var info = Find(names[i]);
                _rows[i] = Instantiate(_rowPrefab, _listRoot);
                _rows[i].Show(info != null ? info.label : names[i], info != null ? info.icon : null);
            }
        }

        // Looks for the label and the picture that belong to a toy name.
        ToyInfo Find(string toyName)
        {
            foreach (var toy in _toys)
                if (toy.toyName == toyName) return toy;
            return null;
        }

        void OnEnable()
        {
            _game.OrderChanged += Build;  // new random order: draw the rows again
            _game.Started += Refresh;     // new round: clear all the boxes
            _game.ToyPacked += OnPacked;  // a toy was packed: tick its box
        }

        void OnDisable()
        {
            _game.OrderChanged -= Build;
            _game.Started -= Refresh;
            _game.ToyPacked -= OnPacked;
        }

        void OnPacked(string toy) { Refresh(); }

        // Each box shows whether its toy is already in the crate.
        void Refresh()
        {
            for (var i = 0; i < _rows.Length; i++)
                _rows[i].SetPacked(_game.IsPacked(i), _emptySprite, _checkedSprite);
        }
    }
}
