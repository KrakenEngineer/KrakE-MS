using UnityEngine;

namespace MSEngine.Spaceships.Parts
{
    [RequireComponent(typeof(ShipPart))]
    public class PartEngine : MovingPart
    {
        [SerializeField] private float _thurst;
        public float Thurst => _thurst;

        public void Initialize(float thurst, float rotation)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize this engine because it is initialized");
            if (thurst <= 0)
                throw new System.Exception("Can't initialize engine with non-positive thurst");

            _thurst = thurst;
            _relativeDirection = DefaultDirection;
            _initialized = true;
        }

        public override float GetTorque(bool clockwise) => 0;
        public override Vector2 GetForce() => _thurst * ShipConstants.Directions
            [StaticMethods.SumDirections(ShipPart.Orientation.Direction, _relativeDirection)];
    }
}