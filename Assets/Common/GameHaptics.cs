using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VrClass
{
    /// <summary>
    /// Vibration for the answers of the game: both controllers buzz when the countdown ticks, when a toy is accepted
    /// or rejected and when the round ends. A short tap is a good thing, a long strong buzz is a mistake.
    /// (The simulator has no vibration; a headset such as the Quest does.)
    /// </summary>
    public class GameHaptics : MonoBehaviour
    {
        const float TICK_BUZZ = 0.2f, GO_BUZZ = 0.35f, TICK_SECONDS = 0.05f;
        const float GOOD_BUZZ = 0.4f, GOOD_SECONDS = 0.15f;
        const float BAD_BUZZ = 0.9f, BAD_SECONDS = 0.3f;
        const float WIN_BUZZ = 0.7f, WIN_SECONDS = 0.5f;
        const float LOSE_BUZZ = 0.5f, LOSE_SECONDS = 0.4f;

        [SerializeField] private PackingGame _game;

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
        void OnCountdown(int number) { Buzz(number > 0 ? TICK_BUZZ : GO_BUZZ, TICK_SECONDS); }
        void OnPacked(string toy) { Buzz(GOOD_BUZZ, GOOD_SECONDS); }
        void OnRejected(string reason) { Buzz(BAD_BUZZ, BAD_SECONDS); }
        void OnFinished(bool won) { Buzz(won ? WIN_BUZZ : LOSE_BUZZ, won ? WIN_SECONDS : LOSE_SECONDS); }

        // Makes every controller vibrate.
        void Buzz(float strength, float seconds)
        {
            foreach (var hand in FindObjectsByType<XRBaseInputInteractor>(FindObjectsSortMode.None))
                hand.SendHapticImpulse(strength, seconds);
        }
    }
}
