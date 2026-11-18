using TMPro;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The end-of-round card: appears when the game finishes and hides when a new round starts.
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private PackingGame _game;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _points;

        // Listens to the game events while this object is active.
        void OnEnable()
        {
            _game.Finished += Show;
            _game.Started += Hide;
        }

        void OnDisable()
        {
            _game.Finished -= Show;
            _game.Started -= Hide;
        }

        // Shows the card with the result of the round.
        void Show(bool won)
        {
            _title.text = won ? "¡LO LOGRASTE!" : "¡SE ACABÓ EL TIEMPO!";
            _points.text = "Puntos: " + _game.Score;
            _panel.SetActive(true);
        }

        void Hide()
        {
            _panel.SetActive(false);
        }
    }
}
