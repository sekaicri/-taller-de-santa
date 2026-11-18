using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Day03
{
    /// <summary>
    /// A conveyor belt: loose objects resting on it are carried along a direction.
    /// Objects held by a hand are not pushed. The belt remembers what touches it and wakes sleeping bodies,
    /// so it also works when it starts stopped (Speed 0) and is switched on later.
    /// </summary>
    public class ConveyorBelt : MonoBehaviour
    {
        const float DEFAULT_SPEED = 0.12f;

        [SerializeField] private Vector3 _direction = Vector3.right;
        [SerializeField] private float _speed = DEFAULT_SPEED;

        readonly HashSet<Rigidbody> _touching = new HashSet<Rigidbody>();

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public float Speed { get => _speed; set => _speed = value; }

        // The belt has no friction: otherwise friction cancels the belt speed on every physics step.
        void Awake()
        {
            var surface = GetComponent<Collider>();
            if (surface == null) return;
            surface.sharedMaterial = new PhysicsMaterial("Cinta")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody != null) _touching.Add(collision.rigidbody);
        }

        void OnCollisionExit(Collision collision)
        {
            if (collision.rigidbody != null) _touching.Remove(collision.rigidbody);
        }

        // Every physics step, set the horizontal speed of whatever rests on the belt to the belt speed.
        // With Speed 0 the objects are stopped, so they do not keep sliding on the frictionless belt.
        void FixedUpdate()
        {
            _touching.RemoveWhere(body => body == null);

            var push = _speed > 0f ? _direction.normalized * _speed : Vector3.zero;
            foreach (var body in _touching)
            {
                if (body.isKinematic) continue;
                var grab = body.GetComponent<XRGrabInteractable>();
                if (grab != null && grab.isSelected) continue;

                if (_speed > 0f && body.IsSleeping()) body.WakeUp();
                var velocity = body.linearVelocity;
                body.linearVelocity = new Vector3(push.x, velocity.y, push.z);
            }
        }
    }
}
