using TMPro;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The live score on the sign: "Puntos: 125". When the score changes, the change appears next to it
    /// for a moment: green "+100" when you pack a toy, red "-25" when you make a mistake.
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        const float SHOW_SECONDS = 1.2f;
        const string GAIN_COLOR = "8CFF8C";
        const string LOSS_COLOR = "FF8A80";

        [SerializeField] private PackingGame _game;
        [SerializeField] private TMP_Text _label;

        int _score;
        int _change;
        float _changeTimer;

        void OnEnable()
        {
            _game.ScoreChanged += OnScoreChanged;
            _game.Started += OnStarted;
        }

        void OnDisable()
        {
            _game.ScoreChanged -= OnScoreChanged;
            _game.Started -= OnStarted;
        }

        void Start() { Redraw(); }

        // The game sends the new score and how much it changed.
        void OnScoreChanged(int score, int change)
        {
            _score = score;
            if (change != 0)
            {
                _change = _changeTimer > 0f ? _change + change : change;   // changes at the same moment are added up
                _changeTimer = SHOW_SECONDS;
            }
            Redraw();
        }

        void OnStarted()
        {
            _score = 0; _change = 0; _changeTimer = 0f;
            Redraw();
        }

        // Writes the score, and the last change while it is still showing.
        void Redraw()
        {
            var text = "Puntos: " + _score;
            if (_changeTimer > 0f && _change != 0)
            {
                var color = _change > 0 ? GAIN_COLOR : LOSS_COLOR;
                text += "  <color=#" + color + ">" + (_change > 0 ? "+" : "") + _change + "</color>";
            }
            _label.text = text;
        }

        void Update()
        {
            if (_changeTimer <= 0f) return;
            _changeTimer -= Time.deltaTime;
            if (_changeTimer <= 0f) Redraw();
        }
    }
}
