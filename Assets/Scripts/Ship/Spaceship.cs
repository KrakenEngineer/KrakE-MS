using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MSEngine.Utility;
using MSEngine.Spaceships.Parts;

namespace MSEngine.Spaceships
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Spaceship : MonoBehaviour
    {
        [SerializeField] private Vector2Int _start;
        [SerializeField] private Vector2Int _size;
        [SerializeField] private CenterOfMass _centerOfMass;
        
        [SerializeField] private List<ShipPart> _parts;
        [SerializeField] private List<PartControlBlock> _controlBlocks;
        [SerializeField] private List<ResourceSystemID> _resourceSystemIDs;

        [SerializeField] private Dictionary<ResourceSystemID, ResourceSystem> _resourceSystems;

        public Spaceship(List<ShipPart> parts, List<ResourceSystemID> resourceSystems, Vector2Int size)
        {
            _controlBlocks = new List<PartControlBlock>();
            _size = size;
            _parts = new List<ShipPart>();
            foreach (var part in parts)
                if(TryAdd(part))
                    if (part.TryGetComponent(out PartControlBlock control))
                        _controlBlocks.Add(control);
            //_resourceSystems = GenerateResourceSystems(resourceSystems);
        }

        private void Update()
        {
            if (IsControlled())
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
            }
        }

        public bool Contains(ShipPart part) => _parts.Contains(part);
        public bool CanAdd(ShipPart part) => part != null && !Contains(part);
        public bool CanRemove(ShipPart part) => Contains(part);

        public bool IsControlled()
        {
            foreach (PartControlBlock block in _controlBlocks)
                if (block.CurrentPlayer != null)
                    return true;
            return false;
        }

        public Vector2 Start => _start;
        public Vector2 Size => _size;
        public Vector2 Position => new Vector2(transform.position.x, transform.position.y);
        public Rigidbody2D Rigidbody => GetComponent<Rigidbody2D>();

        public List<ShipPart> Parts => new List<ShipPart>(_parts);

        public void ConfigueRigidbody(Vector2 velocity, float angularVelocity)
        {
            Rigidbody.gravityScale = 0;
            Rigidbody.angularDrag = ShipConstants.AngularDrag;
            Rigidbody.drag = ShipConstants.LinearDrag;
            Rigidbody.velocity = velocity;
            Rigidbody.angularVelocity = angularVelocity;
        }

        public void Initialize(float rotation, Vector2Int start, Vector2Int size, CenterOfMass centerOfMass, Transform parent, List<ShipPart> parts, List<ResourceSystemID> resourceSystems)
        {
            _controlBlocks = new List<PartControlBlock>();
            _parts = new List<ShipPart>();
            foreach (var part in parts)
                if (TryAdd(part))
                    if (part.TryGetComponent(out PartControlBlock control))
                        _controlBlocks.Add(control);

            _start = start;
            _size = size;
            _centerOfMass = centerOfMass;
            //_resourceSystems = GenerateResourceSystems(resourceSystems);
            SetParent(parent);
            TranslateTo(Vector2.zero, rotation);
        }

        public bool TryAdd(ShipPart part)
        {
            var canAdd = CanAdd(part);
            if (canAdd) _parts.Add(part);
            return canAdd;
        }

        public bool TryRemove(ShipPart part)
        {
            bool canRemove = CanRemove(part);
            if (canRemove) _parts.Remove(part);
            return canRemove;
        }

        public ResourceSystem GetResourceSystem(ResourceSystemID id) => _resourceSystems[id];

        private void SetParent(Transform parent) => transform.parent = parent;

        private void TranslateTo(Vector2 position, float rotation)
        {
            transform.localPosition = new Vector3(position.x, position.y, ShipConstants.ShipDepth);
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        private IEnumerable<KeyAssignment> GetPressedKeys()
        {
            foreach (var key in ShipConstants.ShipControlKeys)
                if (DataLoader.GetKey(key))
                    yield return key;
        }

        /*private Dictionary<ResourceSystemID, ResourceSystem> GenerateResourceSystems(List<ResourceSystemID> resourceSystemIDs)
        {
            var output = new Dictionary<ResourceSystemID, ResourceSystem>();
            var resourceRelated = new List<ResourceRelated>();
            foreach (var part in _parts)
                resourceRelated.AddRange(part.GetComponents<ResourceRelated>());

            foreach (var id in resourceSystemIDs)
            {
                List<ResourceRelated> relatedToID = resourceRelated.Where(c =>
                    c.PartComponent.ResourceSystems.Contains(id) && c.RelatedTo.Contains(id.resource)).ToList();

                if (relatedToID.Count > 0)
                {
                    output.Add(id, new ResourceSystem(id.resource, relatedToID));
                    resourceSystemIDs.Add(id);
                }
            }

            return output;
        }*/
    }

    [System.Serializable]
    public sealed class ResourceSystem : Storage<Resource>
    {
        [SerializeField] private Resource _currentResource;
        [SerializeField] private Resource _consumption;
        [SerializeField] private Resource _output;
        [SerializeField] private List<ResourceRelated> _components;
        public ResourceSystem() => _components = new List<ResourceRelated>();

        public ResourceSystem(string itemID, List<ResourceRelated> components)
        {
            if (!Validate(itemID, components))
                throw new System.Exception("Invalid list of components for composite storage");

            _components = new List<ResourceRelated>(components);
            _currentResource = new Resource(itemID, 0f);

            foreach (var component in components)
            {
                if (component is CompositeStoragePart CSPart)
                {
                    Stack += CSPart.Stack;
                    TryAddResource(new Resource(_currentResource.ID, CSPart.GetContent().Amount));
                }

                AddConsumption(itemID, component.Consumption);
                AddOutput(itemID, component.Output);
            }
        }

        public int Stack { get; private set; }
        public override Resource GetContent() => _currentResource;

        public override List<Resource> Consumption => new List<Resource>();
        public override List<Resource> Output => new List<Resource>();
        public override List<string> RelatedTo => new List<string>() { GetContent().ID };
        public override ShipPart PartComponent => null;

        public override bool HasSpace(Resource resource)
        {
            if (_currentResource.Equals(Resource.Void))
                return Stack >= resource.Amount;

            else if (_currentResource.ID == resource.ID)
                return Stack - _currentResource.Amount >= resource.Amount;

            return false;
        }

        public override bool HasResource(Resource resource)
        {
            if (_currentResource.ID != resource.ID)
                return false;

            return _currentResource.Amount >= resource.Amount;
        }

        public override bool TryAddResource(Resource resource)
        {
            if (!HasSpace(resource))
                return false;

            _currentResource += resource;
            return true;
        }

        public override bool TryRemoveResource(Resource resource)
        {
            if (!HasResource(resource))
                return false;

            float newAmount = _currentResource.Amount - resource.Amount;
            _currentResource = newAmount == 0 ?
                Resource.Void : new Resource(_currentResource.ID, newAmount);

            return true;
        }

        public override void Clear() => _currentResource = Resource.Void;

        private void AddConsumption(string itemID, List<Resource> consumption)
        {
            foreach (var item in consumption)
                if (item.ID == itemID)
                    _consumption += item;
        }

        private void AddOutput(string itemID, List<Resource> output)
        {
            foreach (var item in output)
                if (item.ID == itemID)
                    _output += item;
        }

        private bool Validate(string itemID, List<ResourceRelated> components)
        {
            if (components.Count == 0)
                return false;

            foreach (var component in components)
                if (!component.RelatedTo.Contains(itemID))
                    return false;

            return true;
        }
    }
}