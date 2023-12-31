using UnityEngine;
using MSEngine.Saves.Configs;
using System.Collections.Generic;
using System.Linq;
using MSEngine.Utility;

namespace MSEngine.Spaceships
{
    [RequireComponent(typeof(ResourceComponent))]
    public sealed class ShipPart : MonoBehaviour, IShipPartBehaviour
    {
        //readonly
        [SerializeField] private PartOrientation _orientation;
        [SerializeField] private CenterOfMass _centerOfMass;
        [SerializeField] private Vector2Int _start;
        [SerializeField] private Vector2Int _end;

        [SerializeField] private bool _initialized = false;
        [SerializeField] private bool _oriented = false;
        [SerializeField] private bool _placed = false;

        [SerializeField] private List<ShipPart> _neighbors;

        #region Properties
        public ObjectConfig Config { get; private set; }
        public Spaceship Ship { get; private set; }
        public ShipPart PartComponent => this;
        public ResourceComponent ResourceComponent => GetComponent<ResourceComponent>();
        public List<ShipPart> Neighbors => new List<ShipPart>(_neighbors);

        public PartOrientation Orientation => _orientation;
        public CenterOfMass CenterOfMass => _centerOfMass;
        public Vector2Int Start => _start;
        public Vector2Int End => _end;

        public List<ResourceSystemID> ResourceSystems => ResourceComponent.ResourceSystems;
        public bool Placed => _placed;

        public Vector3 GetExactScale()
        {
            float x = Mathf.Abs(transform.localScale.x);
            float y = Mathf.Abs(transform.localScale.y);
            var output = (int)Orientation.Direction % 2 == 0 ? new Vector3(x, y, 0) : new Vector3(y, x, 0);
            return output;
        }

        public Vector3 GetMovement(CenterOfMass centerOfMass, List<KeyAssignment> keys)
        {
            var movement = Vector3.zero;
            var moveComponents = GetComponents<MovingPart>();

            foreach (var component in moveComponents)
                movement += component.GetMovement(centerOfMass, keys);

            return movement;
        }
        #endregion

        #region For external using
        public void UpdatePart()
        {
            if (Placed)
                _neighbors = _neighbors.Where(neighbour => neighbour != null).ToList();
        }

        public void Pick()
        {
            if (!_placed)
                throw new System.Exception("Can't pick this part because it isn't placed");

            foreach (var part in Neighbors)
                part._neighbors.Remove(this);

            _neighbors = null;
            _placed = false;
            _start = Vector2Int.zero;
            _end = Vector2Int.zero;
        }

        public void Place(PartField field, Vector2Int start)
        {
            if (_placed)
                throw new System.Exception("Can't place this part because it is placed");
            if (!_oriented)
                throw new System.Exception("Can't place this part because it is not oriented");

            _placed = true;
            _start = start;
            _end = start + StaticMethods.Vector3ToVector2Int(GetExactScale());

            GenerateNeighboursList(field);
            foreach (var part in Neighbors)
                part._neighbors.Add(this);
        }

        public void Initialize(ObjectConfig config)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize this part because it is initialized");
            if (config == null || config.Type != ConfigExtensionType.Part)
                throw new System.Exception("Invalid config for part initialization");

            GetComponent<ResourceComponent>().Initialize(new List<ResourceSystemID>());
            
            Config = config;
            _centerOfMass = config.CenterOfMass;
            _initialized = true;
        }

        public void Orient(Vector2Int start, Vector2Int end, PartOrientation orientation)
        {
            if (_oriented)
                throw new System.Exception("Can't orient this part because it is oriented");
            if (orientation.Direction == PartDirection.None)
                throw new System.Exception("Invalid direction for part initialization");

            int flipx = orientation.Flip ? -1 : 1;
            transform.eulerAngles = new Vector3(0, 0, -(int)orientation.Direction * 90);
            transform.localScale = new Vector3(transform.localScale.x * flipx, transform.localScale.y, 0);

            _start = start;
            _end = end;
            _orientation = orientation;
            _oriented = true;
        }

        public bool TryAddResourceSystem(ResourceSystemID resourceSystem) =>
            ResourceComponent.TryAddResourceSystem(resourceSystem);

        public void SetShip(Spaceship ship)
        {
            if (!_initialized)
                throw new System.Exception("Can't set ship because this part is not initialized");
            if (ship == null || ship.transform == null)
                throw new System.Exception("Can't set this ship to part because it is null");

            Ship = ship;
            transform.parent = Ship.transform;
        }

        public void GenerateNeighboursList(PartField _field)
        {
            var output = new List<ShipPart>();
            ShipPart[,] field = _field.Parts;

            if (_start.x > 0)
                for (int y = _start.y; y < _end.y; y++)
                    output.Add(field[_start.x - 1, y]);

            if (_start.y > 0)
                for (int x = _start.x; x < _end.x; x++)
                    output.Add(field[x, _start.y - 1]);

            if (_end.x < _field.Size.x)
                for (int y = _start.y; y < _end.y; y++)
                    output.Add(field[_end.x, y]);

            if (_end.y < _field.Size.y)
                for (int x = _start.x; x < _end.x; x++)
                    output.Add(field[x, _end.y]);

            _neighbors = output.Cast<ShipPart>().Distinct().Where(part => part != null).ToList();
        }
        #endregion
    }
}