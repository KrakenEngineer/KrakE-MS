using System.Collections.Generic;
using UnityEngine;

namespace MSEngine.Spaceships
{
    [RequireComponent(typeof(ShipPart))]
    public class ResourceComponent : MonoBehaviour, IShipPartBehaviour
    {
        [SerializeField] private Dictionary<ResourceRelated, Dictionary<string, int>> _componentsToResSystems;
        [SerializeField] private List<ResourceSystemID> _resourceSystems;
        [SerializeField] private bool _initialized = false;

        public void Initialize(List<ResourceSystemID> IDs)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize this ResourceComponent because it is initialized");

            _resourceSystems = new List<ResourceSystemID>();
            foreach (var id in IDs)
                TryAddResourceSystem(id);
        }

        public void GenerateCTRS(ResourceRelated[] components)
        {
            if (components == null)
                return;

            _componentsToResSystems = new Dictionary<ResourceRelated, Dictionary<string, int>>();
            foreach (var component in components)
            {
                if (component == null || component.PartComponent != PartComponent) continue;

                _componentsToResSystems.Add(component, new Dictionary<string, int>());
                foreach (var system in _resourceSystems)
                    if (component.RelatedTo.Contains(system.resource))
                        _componentsToResSystems[component].Add(system.resource, system.number);
            }
        }

        public bool TryAddResourceSystem(ResourceSystemID id)
        {
            if (_resourceSystems.Contains(id))
                return false;

            _resourceSystems.Add(id);
            return true;
        }

        public ResourceSystem GetResourceSystem(ResourceRelated component, string item)
        {
            if (PartComponent.Ship == null)
                throw new System.Exception("Can't get resource system because this part is not in ship");

            var id = new ResourceSystemID(_componentsToResSystems[component][item], item);
            return PartComponent.Ship.GetResourceSystem(id);
        }

        public ShipPart PartComponent => GetComponent<ShipPart>();
        public List<ResourceSystemID> ResourceSystems => new List<ResourceSystemID>(_resourceSystems);
    }
}