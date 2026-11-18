using TMPro;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The streak: toys packed one after another without a mistake. From the second toy on, each one gives extra points.
    /// A wrong toy (or a new round) sets the streak back to zero.
    /// </summary>
    public class ToyStreak : MonoBehaviour
    {
        [SerializeField] private PackingGame _game;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private int _bonusPerToy = 10;

        int _streak;

        void OnEnable()
        {
            _game.Started += Reset;
            _game.ToyPacked += OnPacked;
            _game.ToyRejected += OnRejected;
        }

        void OnDisable()
        {
            _game.Started -= Reset;
            _game.ToyPacked -= OnPacked;
            _game.ToyRejected -= OnRejected;
        }

        void Start() { Show(); }

        void OnPacked(string toy)
        {
            _streak++;
            if (_streak > 1) _game.AddPoints((_streak - 1) * _bonusPerToy);
            Show();
        }

        void OnRejected(string reason)
        {
            _streak = 0;
            Show();
        }

        void Reset()
        {
            _streak = 0;
            Show();
        }

        void Show() { _label.text = "Racha: x" + _streak; }
    }
}
