using System;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// A new random order for every round: it picks Order Size toys from the Toy Pool (a toy can repeat),
    /// sorts them so equal toys stay together, and gives them to PackingGame.
    /// </summary>
    public class OrderPicker : MonoBehaviour
    {
        [SerializeField] private PackingGame _game;
        [SerializeField] private string[] _toyPool = { };   // names of the toys that can be asked for
        [SerializeField] private int _orderSize = 3;        // how many toys an order has

        void OnEnable() { _game.Started += Pick; }
        void OnDisable() { _game.Started -= Pick; }

        // The first order is picked when the scene starts.
        void Start() { Pick(); }

        void Pick()
        {
            if (_toyPool == null || _toyPool.Length == 0) return;

            var order = new string[_orderSize];
            for (var i = 0; i < _orderSize; i++) order[i] = _toyPool[UnityEngine.Random.Range(0, _toyPool.Length)];
            Array.Sort(order);
            _game.SetOrder(order);
        }
    }
}
