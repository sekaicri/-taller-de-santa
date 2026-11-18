using TMPro;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The best score: it is saved on the device with PlayerPrefs, so it is still there the next time the game opens.
    /// When a round ends with a higher score it shows "new record".
    /// </summary>
    public class HighScore : MonoBehaviour
    {
        const string SAVE_KEY = "VrClass.HighScore";

        [SerializeField] private PackingGame _game;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _newRecordSign;

        public int Best { get; private set; }

        void Awake()
        {
            Best = PlayerPrefs.GetInt(SAVE_KEY, 0);
        }

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

        void Start() { Show(); }

        void OnStarted()
        {
            if (_newRecordSign != null) _newRecordSign.SetActive(false);
        }

        // A round ended: keep the score if it beats the best one.
        void OnFinished(bool won)
        {
            if (_game.Score <= Best) return;

            Best = _game.Score;
            PlayerPrefs.SetInt(SAVE_KEY, Best);
            PlayerPrefs.Save();
            if (_newRecordSign != null) _newRecordSign.SetActive(true);
            Show();
            Debug.Log("New record: " + Best);
        }

        void Show() { _label.text = "Récord: " + Best; }
    }
}
