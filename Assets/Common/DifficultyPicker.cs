using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VrClass
{
    /// <summary>
    /// The difficulty buttons (Easy, Normal, Hard): each level sets how long a round lasts and how fast the belt moves.
    /// The buttons only work between rounds.
    /// </summary>
    public class DifficultyPicker : MonoBehaviour
    {
        static readonly Color SELECTED_COLOR = Color.white;
        static readonly Color DIMMED_COLOR = new Color(0.6f, 0.6f, 0.6f);

        [Serializable]
        public class Level
        {
            public string name = "Normal";
            public float timeLimit = 90f;
            public float beltSpeed = 0.08f;
        }

        [SerializeField] private PackingGame _game;
        [SerializeField] private GameTimer _timer;
        [SerializeField] private Button[] _buttons;   // one button per level, in the same order
        [SerializeField] private Level[] _levels;
        [SerializeField] private TMP_Text _info;
        [SerializeField] private int _startLevel = 1;

        void OnEnable()
        {
            _game.Started += OnStarted;
            _game.Finished += OnFinished;
        }

        void OnDisable()
        {
            _game.Started -= OnStarted;
            _game.Finished -= OnFinished;
        }

        void Start()
        {
            Choose(_startLevel);
        }

        // Called by each button: applies the level to the clock and to the belt.
        public void Choose(int index)
        {
            if (_levels == null || _levels.Length == 0) return;
            index = Mathf.Clamp(index, 0, _levels.Length - 1);
            var level = _levels[index];

            _timer.SetTimeLimit(level.timeLimit);
            _game.SetBeltSpeed(level.beltSpeed);
            Debug.Log("Difficulty: " + level.name);

            for (var i = 0; i < _buttons.Length; i++) _buttons[i].image.color = i == index ? SELECTED_COLOR : DIMMED_COLOR;
            if (_info != null) _info.text = level.name + ": " + level.timeLimit + " segundos";
        }

        void OnStarted() { SetButtonsActive(false); }

        void OnFinished(bool won) { SetButtonsActive(true); }

        void SetButtonsActive(bool active)
        {
            foreach (var button in _buttons) button.interactable = active;
        }
    }
}
