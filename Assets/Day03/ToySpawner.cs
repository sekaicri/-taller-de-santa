using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Day03
{
    /// <summary>
    /// The toy factory: puts a new toy (or a bad object like coal) at the start of the belt every few seconds.
    /// Templates are switched-off copies kept in the scene. Spawned objects that nobody grabs are removed after a while.
    /// </summary>
    public class ToySpawner : MonoBehaviour
    {
        const float DEFAULT_INTERVAL = 6f;
        const float DEFAULT_LIFETIME = 45f;
        const float FIRST_DELAY = 0.5f;
        const float FALL_LIMIT_Y = 0.2f;
        static readonly Vector3 CLEAR_HALF_SIZE = new Vector3(0.1f, 0.09f, 0.1f);

        class Item
        {
            public GameObject Object;
            public XRGrabInteractable Grab;
            public float Age;
        }

        [SerializeField] private GameObject[] _templates;
        [SerializeField] private float _interval = DEFAULT_INTERVAL;
        [SerializeField] private float _lifetime = DEFAULT_LIFETIME;
        [SerializeField] private bool _autoStart = true;

        readonly List<Item> _items = new List<Item>();
        bool _running;
        float _timer;
        int _last = -1;

        public GameObject[] Templates { set => _templates = value; }
        public bool AutoStart { set => _autoStart = value; }

        void Start()
        {
            if (_autoStart) Begin();
        }

        // Starts producing objects.
        public void Begin()
        {
            _running = true;
            _timer = FIRST_DELAY;
        }

        // Stops producing objects (the ones already on the belt stay).
        public void Halt()
        {
            _running = false;
        }

        // Removes every toy and bad object lying around, whoever put it there (used when a round starts).
        // Objects are recognized by the name of a template; the switched-off templates themselves stay.
        public void Clear()
        {
            var names = new HashSet<string>();
            foreach (var template in _templates) names.Add(template.name);

            foreach (var toy in FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (names.Contains(toy.gameObject.name)) Destroy(toy.gameObject);
            _items.Clear();
        }

        void Update()
        {
            if (_running)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f && IsClear()) { Spawn(); _timer = _interval; }
            }
            Clean();
        }

        // True when nothing is standing where the next object would appear (so objects never overlap and burst apart).
        bool IsClear()
        {
            return !Physics.CheckBox(transform.position, CLEAR_HALF_SIZE, Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }

        // Clones a random template (never the same one twice in a row) at the spawner position.
        void Spawn()
        {
            if (_templates == null || _templates.Length == 0) return;
            var index = Random.Range(0, _templates.Length);
            if (_templates.Length > 1 && index == _last) index = (index + 1) % _templates.Length;
            _last = index;

            var template = _templates[index];
            var clone = Instantiate(template, transform.position, Quaternion.identity);
            clone.name = template.name;
            clone.SetActive(true);
            _items.Add(new Item { Object = clone, Grab = clone.GetComponent<XRGrabInteractable>() });
        }

        // Removes objects that fell, or that stayed too long on the belt without being held.
        void Clean()
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                if (item.Object == null) { _items.RemoveAt(i); continue; }

                if (item.Grab != null && item.Grab.isSelected) item.Age = 0f;
                else item.Age += Time.deltaTime;

                if (item.Age > _lifetime || item.Object.transform.position.y < FALL_LIMIT_Y)
                {
                    Destroy(item.Object);
                    _items.RemoveAt(i);
                }
            }
        }
    }
}
