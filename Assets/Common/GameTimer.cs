using TMPro;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The clock of the game: counts down, shrinks the time bar, shows time and points,
    /// ends the round at zero and gives a bonus for packing fast.
    /// </summary>
    public class GameTimer : MonoBehaviour
    {
        [SerializeField] private PackingGame _game;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private RectTransform _timeBar;
        [SerializeField] private float _timeLimit = 90f;
        [SerializeField] private bool _timeBonus = true;

        float _timeLeft;

        void OnEnable()
        {
            _game.Started += OnStarted;
            _game.ToyPacked += OnPacked;
        }

        void OnDisable()
        {
            _game.Started -= OnStarted;
            _game.ToyPacked -= OnPacked;
        }

        void OnStarted() { _timeLeft = _timeLimit; }

        // Used by DifficultyPicker: the time of the next round.
        public void SetTimeLimit(float seconds) { _timeLimit = seconds; }

        // Each toy gives the seconds that are left as extra points.
        void OnPacked(string toy)
        {
            if (_timeBonus) _game.AddPoints(Mathf.CeilToInt(_timeLeft));
        }

        void Update()
        {
            if (!_game.Running) return;
            if (!_game.Paused) _timeLeft -= Time.deltaTime;

            if (_timeLeft <= 0f) { _timeLeft = 0f; _game.Finish(false); return; }

            _timeBar.localScale = new Vector3(_timeLeft / _timeLimit, 1f, 1f);
            _timeText.text = "Tiempo: " + Mathf.CeilToInt(_timeLeft) + " s";
        }
    }
}
