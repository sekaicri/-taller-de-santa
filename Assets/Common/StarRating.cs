using UnityEngine;
using UnityEngine.UI;

namespace VrClass
{
    /// <summary>
    /// The stars of the result card: one, two or three, depending on the score of the round.
    /// </summary>
    public class StarRating : MonoBehaviour
    {
        static readonly Color LIT_COLOR = new Color(1f, 0.85f, 0.35f);
        static readonly Color OFF_COLOR = new Color(0.25f, 0.25f, 0.25f, 0.6f);

        [SerializeField] private PackingGame _game;
        [SerializeField] private Image[] _stars;
        [SerializeField] private int[] _scoreForStar = { 200, 450, 650 };   // the score needed for the first, second and third star

        void OnEnable() { _game.Finished += OnFinished; }

        void OnDisable() { _game.Finished -= OnFinished; }

        void OnFinished(bool won)
        {
            var lit = 0;
            for (var i = 0; i < _scoreForStar.Length; i++)
                if (_game.Score >= _scoreForStar[i]) lit = i + 1;

            for (var i = 0; i < _stars.Length; i++) _stars[i].color = i < lit ? LIT_COLOR : OFF_COLOR;
            Debug.Log("Stars: " + lit);
        }
    }
}
