using TMPro;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The Pause and Restart buttons of the sign. Pause stops the belt, the factory and the clock.
    /// </summary>
    public class GamePause : MonoBehaviour
    {
        [SerializeField] private PackingGame _game;
        [SerializeField] private TMP_Text _pauseLabel;

        // Called by the Pause button.
        public void Toggle()
        {
            _game.SetPaused(!_game.Paused);
        }

        // Called by the Restart button.
        public void Restart()
        {
            _game.Begin();
        }

        // The pause button says SEGUIR while the game is paused.
        void Update()
        {
            _pauseLabel.text = _game.Paused ? "SEGUIR" : "PAUSA";
        }
    }
}
