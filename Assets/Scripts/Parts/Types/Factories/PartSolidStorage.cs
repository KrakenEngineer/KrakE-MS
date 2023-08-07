using UnityEngine;
using MSEngine.Utility;
using MSEngine.UI;
using System.Collections.Generic;

namespace MSEngine.Spaceships.Parts
{
    public class PartSolidStorage : Storage<SolidResource>
    {
        [SerializeField] private Indicator _indicator;
        [SerializeField] private SolidResource _currentResource;

        private bool _initialized;

        public int Stack { get; private set; }
        public override SolidResource GetContent() => _currentResource;
        public override ShipPart PartComponent => GetComponent<ShipPart>();

        public override List<Resource> Consumption => new List<Resource>();
        public override List<Resource> Output => new List<Resource>();
        public override List<string> RelatedTo => new List<string>() { GetContent().ID };

        public void Initialize(int stack, SolidResource item, IndicatorSettings indicator)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize initialized storage");
            if (stack <= 0)
                throw new System.Exception("Can't initialize storage with non-positive stack");

            Stack = stack;
            Clear();
            TryAddResource(item);
            if (indicator.Mode != IndicatorMode.None)
                _indicator =  Indicator.Create(_currentResource.Amount / Stack, transform, indicator);

            _initialized = true;
        }

        public override bool HasSpace(SolidResource resource)
        {
            if (_currentResource == SolidResource.Void)
                return Stack >= resource.Amount;

            else if (_currentResource.ID == resource.ID)
                return Stack - _currentResource.Amount >= resource.Amount;

            return false;
        }

        public override bool HasResource(SolidResource resource)
        {
            if (_currentResource.ID != resource.ID)
                return false;

            return _currentResource.Amount >= resource.Amount;
        }

        public override bool TryAddResource(SolidResource resource)
        {
            if (!HasSpace(resource))
                return false;

            _currentResource += resource;
            return true;
        }

        public override bool TryRemoveResource(SolidResource resource)
        {
            if (!HasResource(resource))
                return false;

            int newAmount = _currentResource.Amount - resource.Amount;
            _currentResource = newAmount == 0 ?
                SolidResource.Void : new SolidResource(_currentResource.ID, newAmount);

            return true;
        }

        public override void Clear() => _currentResource = SolidResource.Void;
    }
}