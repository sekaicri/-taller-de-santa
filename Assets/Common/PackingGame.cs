using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VrClass
{
    /// <summary>
    /// The packing game: press Start, count 3-2-1, then pack every toy of the order into the crate.
    /// Small extra scripts listen to its events: OrderList (Day 4), OrderPicker, WrongToyPenalty, ScoreDisplay, GameFeedback and GameHaptics (Day 6),
    /// GameTimer, GamePause, DifficultyPicker, ToyStreak, HighScore and StarRating (Day 7).
    /// </summary>
    public class PackingGame : MonoBehaviour
    {
        const string WRAPPER_NAME = "Envoltorio";
        const int POINTS_PER_TOY = 100;
        const int COUNTDOWN_FROM = 3;
        const float GO_SECONDS = 0.7f;
        const float REMOVE_DELAY = 1f;
        const float EJECT_UP = 3.5f;       // Throw Rejected: a wrong toy is thrown up and far back toward the player
        const float EJECT_BACK = 5f;
        const float DROP_UP = 1f;          // without it, the wrong toy only drops out in front of the crate
        const float DROP_BACK = 1f;
        const float EJECT_LIFT = 0.4f;     // the toy is lifted this much (meters) so it leaves the crate
        const float SOCKET_PAUSE = 0.6f;   // the crate ignores everything for a moment, so the toy cannot fall back in
        const float REJECT_COOLDOWN = 1f;   // the same wrong toy coming back right away is not punished (or announced) twice

        [SerializeField] private Day03.ConveyorBelt _belt;
        [SerializeField] private Day03.ToySpawner _spawner;
        [SerializeField] private XRSocketInteractor _socket;
        [SerializeField] private GameObject _startButton;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private string[] _toysToPack = { "Bola", "Baston", "Osito" };   // the shopping list: names of the toys to pack
        [SerializeField] private float _beltSpeed = 0.08f;
        [SerializeField] private bool _throwRejected;       // Day 6: a wrong toy is thrown far away instead of just dropping out

        Transform _lastRejected;
        float _lastRejectTime = -REJECT_COOLDOWN;

        bool[] _packed = new bool[0];   // one flag per order line: true once that toy is in the crate

        public event Action Started;   // a new round begins (before the countdown)
        public event Action OrderChanged;   // the order has new toys (random orders)
        public event Action<int> CountdownTick;   // 3, 2, 1 and then 0 for "GO"
        public event Action<int, int> ScoreChanged;   // the new score and how much it changed
        public event Action<string> ToyPacked;
        public event Action<string> ToyRejected;
        public event Action<bool> Finished;   // true = order complete, false = time is up

        public bool Running { get; private set; }
        public bool Paused { get; private set; }
        public int Score { get; private set; }
        public string[] ToysToPack => _toysToPack;   // OrderList draws one row for each name

        void Awake()
        {
            _packed = new bool[_toysToPack.Length];
            if (_belt != null) _belt.Speed = 0f;   // the belt waits for Start
        }

        void OnEnable()
        {
            if (_socket != null) _socket.selectEntered.AddListener(OnPacked);
        }

        void OnDisable()
        {
            if (_socket != null) _socket.selectEntered.RemoveListener(OnPacked);
        }

        // Called by the Start button (and by Restart).
        public void Begin()
        {
            StopAllCoroutines();
            StartCoroutine(StartRound());
        }

        // Clears the table, counts 3, 2, 1 and then starts the belt and the factory.
        IEnumerator StartRound()
        {
            Running = false; Paused = false; Score = 0;
            if (_socket != null) _socket.socketActive = true;   // in case a round restarts while the crate was paused
            _packed = new bool[_toysToPack.Length];
            if (_belt != null) _belt.Speed = 0f;
            if (_spawner != null) { _spawner.Halt(); _spawner.Clear(); }
            if (_startButton != null) _startButton.SetActive(false);
            Started?.Invoke();
            ScoreChanged?.Invoke(0, 0);

            for (var n = COUNTDOWN_FROM; n >= 1; n--)
            {
                Say(n.ToString());
                CountdownTick?.Invoke(n);
                yield return new WaitForSeconds(1f);
            }
            Say("¡YA!");
            CountdownTick?.Invoke(0);
            yield return new WaitForSeconds(GO_SECONDS);

            Running = true;
            if (_belt != null) _belt.Speed = _beltSpeed;
            if (_spawner != null) _spawner.Begin();
            Say("¡A empacar!");
            Debug.Log("Game started.");
        }

        // Stops or resumes the belt and the factory (used by the pause button).
        public void SetPaused(bool paused)
        {
            if (!Running) return;
            Paused = paused;
            if (_belt != null) _belt.Speed = paused ? 0f : _beltSpeed;
            if (_spawner != null) { if (paused) _spawner.Halt(); else _spawner.Begin(); }
            Say(paused ? "En pausa" : "¡A empacar!");
            Debug.Log(paused ? "Game paused." : "Game resumed.");
        }

        // Used by DifficultyPicker (Day 7): a new belt speed for the next round (or right now, if a round is running).
        public void SetBeltSpeed(float speed)
        {
            _beltSpeed = speed;
            if (Running && !Paused && _belt != null) _belt.Speed = speed;
        }

        // Extra points from other scripts (for example the speed bonus).
        public void AddPoints(int points)
        {
            ChangeScore(points);
        }

        // Adds (or takes away) points; the score never goes below 0. Listeners learn how much it really changed.
        void ChangeScore(int change)
        {
            var before = Score;
            Score = Mathf.Max(0, Score + change);
            ScoreChanged?.Invoke(Score, Score - before);
        }

        // Used by OrderPicker (Day 6): replaces the toys of the order and tells the listeners.
        public void SetOrder(string[] toys)
        {
            _toysToPack = (string[])toys.Clone();
            _packed = new bool[_toysToPack.Length];
            OrderChanged?.Invoke();
        }

        // A toy entered the crate: accept it or push it back out.
        void OnPacked(SelectEnterEventArgs args)
        {
            if (!Running || Paused) return;

            var toy = args.interactableObject.transform;
            var wrapper = toy.Find(WRAPPER_NAME);
            var wrapped = wrapper != null && wrapper.gameObject.activeSelf;
            var line = FindLine(toy.name);

            if (Array.IndexOf(_toysToPack, toy.name) < 0) { Reject(args, toy.name + " is not a toy from the order.", "Eso no es un juguete del pedido"); return; }
            if (!wrapped) { Reject(args, toy.name + " is not wrapped yet.", "Falta envolverlo: aprieta el gatillo"); return; }
            if (line < 0) { Reject(args, toy.name + " was already packed.", "Ese juguete ya lo empacaste"); return; }

            _packed[line] = true;
            ChangeScore(POINTS_PER_TOY);
            Debug.Log("Packed " + toy.name + ". Score: " + Score);
            Say("Empacado: " + toy.name);
            ToyPacked?.Invoke(toy.name);
            Destroy(toy.gameObject, REMOVE_DELAY);

            if (Array.IndexOf(_packed, false) < 0) Finish(true);
        }

        // First order line with this toy name that is not packed yet, or -1.
        int FindLine(string toyName)
        {
            for (var i = 0; i < _toysToPack.Length; i++)
                if (_toysToPack[i] == toyName && !_packed[i]) return i;
            return -1;
        }

        // Pushes a wrong object back out of the crate, toward the player.
        void Reject(SelectEnterEventArgs args, string reason, string message)
        {
            var toy = args.interactableObject;
            StartCoroutine(PushOut(toy));

            // The ejected toy can fall back into the crate: the second time it is ignored, so nothing repeats or keeps sounding.
            if (toy.transform == _lastRejected && Time.time - _lastRejectTime < REJECT_COOLDOWN) return;
            _lastRejected = toy.transform;
            _lastRejectTime = Time.time;

            Debug.LogWarning("Rejected: " + reason);
            Say(message);
            ToyRejected?.Invoke(message);
        }

        // Takes the wrong object out of the crate. With Throw Rejected (Day 6) it is thrown far, toward the player;
        // without it, it only drops out in front of the crate.
        // The socket cannot let go while it is still announcing the toy, so the exit waits a frame, and the body
        // is only free to fly one frame after that.
        IEnumerator PushOut(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable toy)
        {
            var part = toy as Component;
            yield return null;
            if (part == null) yield break;   // the toy was removed in the meantime

            // Let go only if the crate still holds it (switching the crate off would also let it go, and a second exit is an error).
            if (_socket.IsSelecting(toy)) _socket.interactionManager.SelectExit(_socket, toy);
            _socket.socketActive = false;   // the crate ignores everything while the wrong toy leaves
            yield return null;

            var body = part == null ? null : part.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = false;
                body.useGravity = true;
                body.position += Vector3.up * EJECT_LIFT;   // out of the crate, so the socket cannot take it again
                body.linearVelocity = _throwRejected ? new Vector3(0f, EJECT_UP, -EJECT_BACK) : new Vector3(0f, DROP_UP, -DROP_BACK);
            }

            yield return new WaitForSeconds(SOCKET_PAUSE);
            _socket.socketActive = true;
        }

        // Ends the round: stop the belt and the factory, show the result and bring the Start button back.
        public void Finish(bool won)
        {
            Running = false; Paused = false;
            if (_belt != null) _belt.Speed = 0f;
            if (_spawner != null) _spawner.Halt();
            if (_startButton != null) _startButton.SetActive(true);
            Debug.Log((won ? "Order complete" : "Time is up") + ". Score: " + Score);
            Say(won ? "¡Pedido completo! Puntos: " + Score : "¡Se acabó el tiempo! Puntos: " + Score);
            Finished?.Invoke(won);
        }

        public void Say(string message)
        {
            if (_statusText != null) _statusText.text = message;
        }

        // Used by OrderList to draw the check boxes.
        public bool IsPacked(int line)
        {
            return line < _packed.Length && _packed[line];
        }
    }
}
