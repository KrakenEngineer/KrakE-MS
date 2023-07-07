using MSEngine.Saves.Spaceships;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSEngine.Spaceships
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Spaceship : MonoBehaviour
    {
        private Vector2Int _size;
        private CenterOfMass _centerOfMass;

        private PartField _partField;
        private List<ShipPart> _parts;

        private Queue<ShipPart> _partsToDestroy;
        
        public Spaceship(PartField field)
        {
            _partField = new PartField(field);
            _parts = field.Parts.Cast<ShipPart>().Where(part => part != null).Distinct().ToList();
            _size = field.Size;
        }

        public Spaceship(List<ShipPart> parts, Vector2Int size)
        {
            _partField = new PartField(_size);
            _size = size;
            _parts = new List<ShipPart>();
            foreach (var part in parts)
                TryAdd(part);
        }

        private void Update()
        {
            List<KeyAssignment> pressedKeys = GetPressedKeys().ToList();
            Vector3 movement = Vector3.zero;

            foreach (var part in _parts)
            {
                if (part == null) continue;
                else part.UpdatePart();

                if (part.enabled)
                    movement += part.GetMovement(_centerOfMass, pressedKeys);
            }

            Vector2 force = new Vector2(movement.x, movement.y);
            Rigidbody.AddRelativeForce(force);
            Rigidbody.AddTorque(-movement.z * Mathf.Deg2Rad);

            if (_partsToDestroy.Count > 0)
                DestroyPartsInQueue();
        }

        public bool Contains(ShipPart part) => _parts.Contains(part);
        public bool CanAdd(ShipPart part) => part != null && !Contains(part);
        public bool CanRemove(ShipPart part) => Contains(part);

        public Vector2 Size => _size;
        public Vector2 Position => new Vector2(transform.position.x, transform.position.y);
        public Rigidbody2D Rigidbody => GetComponent<Rigidbody2D>();

        public List<ShipPart> Parts => new List<ShipPart>(_parts);
        public Queue<ShipPart> PartsToDestroy => new Queue<ShipPart>(_partsToDestroy);

        public void ConfigueRigidbody(Vector2 velocity, float angularVelocity)
        {
            Rigidbody.gravityScale = 0;
            Rigidbody.angularDrag = ShipConstants.AngularDrag;
            Rigidbody.drag = ShipConstants.LinearDrag;
            Rigidbody.velocity = velocity;
            Rigidbody.angularVelocity = angularVelocity;
        }

        public void Destroy(ShipPart part)
        {
            if (part != null && !_parts.Contains(part))
                _partsToDestroy.Enqueue(part);
        }

        public void Initialize(float rotation, Vector2 position, Vector2Int size, CenterOfMass centerOfMass, Transform parent, List<ShipPart> parts)
        {
            _partField = new PartField(size);
            _parts = new List<ShipPart>();
            foreach (var part in parts)
                TryAdd(part);

            _size = size;
            _centerOfMass = centerOfMass;
            SetParent(parent);
            Translate(position, rotation);

            _partsToDestroy = new Queue<ShipPart>();
        }

        public bool TryAdd(ShipPart part)
        {
            var canAdd = _partField.TryRelease(part, null) && CanAdd(part);
            if (canAdd) _parts.Add(part);
            return canAdd;
        }

        public bool TryRemove(ShipPart part)
        {
            bool canRemove = CanRemove(part);

            if (canRemove)
            {
                _partField.Pick(part);
                _parts.Remove(part);
            }

            return canRemove;
        }

        private void DestroyPartsInQueue()
        {
            while (_partsToDestroy.Count > 0)
            {
                if (TryRemove(_partsToDestroy.Peek()))
                    Destroy(_partsToDestroy.Dequeue());
                else _partsToDestroy.Dequeue();
            }

            List<PartGraph> components = new PartGraph(_parts).Split();
            if (components.Count > 1)
                Split(components);
        }

        private void Split(List<PartGraph> components)
        {
            foreach (var component in components)
                CreateComponent(component, Rigidbody);

            Destroy(gameObject);
        }

        private void CreateComponent(PartGraph graph, Rigidbody2D original)
        {
            var ship = new GameObject(name);
            List<ShipPart> parts = graph.Parts;
            ship.AddComponent<Rigidbody2D>();

            var cShip = ship.AddComponent<Spaceship>();
            cShip.Initialize(0, Position, _size, _centerOfMass, transform.parent, parts);
            cShip.ConfigueRigidbody(original.velocity, original.angularVelocity);
            foreach (var part in parts)
                part.SetShip(cShip);
        }

        private void SetParent(Transform parent) => transform.parent = parent;

        private void Translate(Vector2 position, float rotation)
        {
            transform.position = new Vector3(position.x, position.y, ShipConstants.ShipDepth);
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        private IEnumerable<KeyAssignment> GetPressedKeys()
        {
            foreach (var key in ShipConstants.ShipControlKeys)
                if (DataLoader.GetKey(key))
                    yield return key;
        }
    }
}