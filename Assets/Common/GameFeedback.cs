using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// Sound, light and sparks for the game: it listens to the events of PackingGame (the vibration is GameHaptics).
    /// Countdown: a tick for each number and a higher sound on "GO". Accepted toy: a rising sound, a green flash and sparks.
    /// Rejected toy: a falling sound and a red flash. End of the round: a happy or a sad melody.
    /// </summary>
    public class GameFeedback : MonoBehaviour
    {
        const float FLASH_INTENSITY = 3f;
        const float FLASH_SECONDS = 0.4f;

        [SerializeField] private PackingGame _game;
        [SerializeField] private AudioSource _sound;
        [SerializeField] private Light _flash;
        [SerializeField] private ParticleSystem _sparks;

        float _flashTimer;

        void OnEnable()
        {
            _game.CountdownTick += OnCountdown;
            _game.ToyPacked += OnPacked;
            _game.ToyRejected += OnRejected;
            _game.Finished += OnFinished;
        }

        void OnDisable()
        {
            _game.CountdownTick -= OnCountdown;
            _game.ToyPacked -= OnPacked;
            _game.ToyRejected -= OnRejected;
            _game.Finished -= OnFinished;
        }

        // 3, 2, 1 tick; 0 means "GO".
        void OnCountdown(int number) { Play(number > 0 ? FeedbackTones.Tick : FeedbackTones.Go); }

        void OnPacked(string toy)
        {
            Play(FeedbackTones.Good);
            Flash(Color.green);
            if (_sparks != null) _sparks.Play();
        }

        void OnRejected(string reason)
        {
            Play(FeedbackTones.Bad);
            Flash(Color.red);
        }

        void OnFinished(bool won)
        {
            Play(won ? FeedbackTones.Win : FeedbackTones.Lose);
            Flash(won ? Color.green : Color.red);
        }

        void Play(AudioClip clip)
        {
            if (_sound == null) return;
            _sound.Stop();   // a new sound cuts the old one, so sounds never pile up
            _sound.PlayOneShot(clip);
        }

        // Turns the light on with this color for a moment.
        void Flash(Color color)
        {
            if (_flash == null) return;
            _flash.color = color;
            _flash.intensity = FLASH_INTENSITY;
            _flashTimer = FLASH_SECONDS;
        }

        // Turns the light off when its time is over.
        void Update()
        {
            if (_flashTimer <= 0f) return;
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f && _flash != null) _flash.intensity = 0f;
        }
    }
}
