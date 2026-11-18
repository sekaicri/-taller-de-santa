using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace VrClass
{
    /// <summary>
    /// Tells the player where they are standing and lets the game buttons work only from the start station.
    /// The sign guides the player while they are away from a station; on a station it shows its message for a moment and goes away,
    /// so it never covers the game sign.
    /// </summary>
    public class StationGate : MonoBehaviour
    {
        const float SHOW_SECONDS = 2f;   // how long the sign stays on a station before it hides
        [SerializeField] private TeleportationAnchor _startStation;
        [SerializeField] private TeleportationAnchor _deliveryStation;
        [SerializeField] private TeleportationArea _floor;
        [SerializeField] private Button[] _gatedButtons;   // buttons that only work from the start station
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _sign;   // the whole station sign (its Canvas), switched off on the stations

        enum Place { Nowhere, Start, Delivery }

        Place _place = Place.Nowhere;
        Coroutine _hiding;

        // Listens to the teleport events of the three places while this object is active.
        void OnEnable()
        {
            _startStation.teleporting.AddListener(OnStart);
            _deliveryStation.teleporting.AddListener(OnDelivery);
            _floor.teleporting.AddListener(OnFloor);
            Refresh();
        }

        void OnDisable()
        {
            _startStation.teleporting.RemoveListener(OnStart);
            _deliveryStation.teleporting.RemoveListener(OnDelivery);
            _floor.teleporting.RemoveListener(OnFloor);
        }

        void OnStart(TeleportingEventArgs args) { Go(Place.Start); }
        void OnDelivery(TeleportingEventArgs args) { Go(Place.Delivery); }
        void OnFloor(TeleportingEventArgs args) { Go(Place.Nowhere); }

        void Go(Place place)
        {
            _place = place;
            Refresh();
        }

        // Updates the sign and switches the buttons on or off.
        void Refresh()
        {
            switch (_place)
            {
                case Place.Start: _label.text = "Estación de inicio: ¡ya puedes empezar!"; break;
                case Place.Delivery: _label.text = "Estación de entrega"; break;
                default: _label.text = "Teletranspórtate a la Estación de inicio"; break;
            }
            foreach (var button in _gatedButtons)
                if (button != null) button.interactable = _place == Place.Start;

            // Away from the stations the sign stays on; on a station it hides after a moment.
            if (_hiding != null) StopCoroutine(_hiding);
            if (_sign == null) return;
            _sign.SetActive(true);
            if (_place != Place.Nowhere) _hiding = StartCoroutine(HideSign());
        }

        IEnumerator HideSign()
        {
            yield return new WaitForSeconds(SHOW_SECONDS);
            _sign.SetActive(false);
        }
    }
}
