using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MSEngine.Utility;

namespace MSEngine.Spaceships.Parts
{
    [RequireComponent(typeof(ShipPart))]
    public class PartGenerator : ResourceRelated
    {
        [SerializeField] private List<Resource> _consumption;
        [SerializeField] private List<Resource> _output;

        [SerializeField] private bool _initialized = false;

        public override ShipPart PartComponent => GetComponent<ShipPart>();
        public override List<Resource> Consumption => new List<Resource>(_consumption);
        public override List<Resource> Output => new List<Resource>(_output);
        public override List<string> RelatedTo => GetRelatedTo();

        public void Initialize(List<Resource> consumption, List<Resource> output)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize this generator because it is initialized");

            _consumption = new List<Resource>(consumption);
            _output = new List<Resource>(output);
            _initialized = true;
        }

        private List<string> GetRelatedTo()
        {
            var output = new List<Resource>(_consumption);
            output.AddRange(_output);
            return output.Select(r => r.ID).Distinct().ToList();
        }
    }
}