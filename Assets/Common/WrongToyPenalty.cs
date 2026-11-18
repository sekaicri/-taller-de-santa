using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// A wrong toy costs points. It listens to ToyRejected, takes the points off (the score never goes below 0)
    /// and adds the loss to the message of the sign, for example "Falta envolverlo: aprieta el gatillo (-25)".
    /// </summary>
    public class WrongToyPenalty : MonoBehaviour
    {
        [SerializeField] private PackingGame _game;
        [SerializeField] private int _points = 25;   // points lost for each wrong toy

        void OnEnable() { _game.ToyRejected += OnRejected; }
        void OnDisable() { _game.ToyRejected -= OnRejected; }

        void OnRejected(string message)
        {
            var before = _game.Score;
            _game.AddPoints(-_points);
            var lost = before - _game.Score;
            if (lost > 0) _game.Say(message + " (-" + lost + ")");
        }
    }
}
