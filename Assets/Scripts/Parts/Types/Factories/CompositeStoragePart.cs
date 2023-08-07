using MSEngine.UI;
using MSEngine.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace MSEngine.Spaceships
{
    public class CompositeStoragePart : Storage<Resource>
    {
        [SerializeField] private Indicator _indicator;
        [SerializeField] private ResourceSystem _storage;
        [SerializeField] private Resource _eigenResource;
        private bool _initialized;

        public int Stack { get; private set; }
        public override Resource GetContent()
        {
            try { return _storage.GetContent(); }
            catch { return _eigenResource; }
        }

        public override ShipPart PartComponent => GetComponent<ShipPart>();

        public override List<Resource> Consumption => new List<Resource>();
        public override List<Resource> Output => new List<Resource>();
        public override List<string> RelatedTo => new List<string>() { GetContent().ID };

        public void Initialize(int stack, Resource item, IndicatorSettings indicator)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize initialized storage");
            if (stack <= 0)
                throw new System.Exception("Can't initialize storage with non-positive stack");

            Stack = stack;
            _eigenResource = item;
            if (indicator.Mode != IndicatorMode.None)
                _indicator = Indicator.Create(_eigenResource.Amount / Stack, transform, indicator);

            _initialized = true;
        }

        public override bool HasSpace(Resource resource) => _storage.HasSpace(resource);
        public override bool HasResource(Resource resource) => _storage.HasResource(resource);

        public override bool TryAddResource(Resource resource) => _storage.TryAddResource(resource);
        public override bool TryRemoveResource(Resource resource) => _storage.TryRemoveResource(resource);

        public override void Clear() => _storage.Clear();
    }
}