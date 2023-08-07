using System.Collections.Generic;
using MSEngine.Utility;
using UnityEngine;

namespace MSEngine.Spaceships.Parts
{
    [RequireComponent(typeof(ShipPart))]
    public class PartEngine : MovingPart
    {
        [SerializeField] private float _thurst;
        [SerializeField] private float _exhaustLengthMultiplier;

        public void Initialize(float thurst, float exLenM, Vector2 thurstPosition, List<Resource> consumption, List<Resource> output)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize this engine because it is initialized");
            if (thurst <= 0)
                throw new System.Exception("Can't initialize engine with non-positive thurst");

            _thurst = thurst;
            _exhaustLengthMultiplier = exLenM;
            _relativeDirection = DefaultDirection;
            MovementPoint = thurstPosition;
            _consumption = new List<Resource>(consumption);
            _output = new List<Resource>(output);
            _initialized = true;
        }

        public override float GetTorque(bool clockwise) => 0;
        public override Vector2 GetForce()
        {
            PartDirection direction = StaticMethods.SumDirections(PartComponent.Orientation.Direction, _relativeDirection);
            if (CanUse()) return _thurst * ShipConstants.Directions[direction];
            else return Vector2.zero;
        }

        private bool CanUse()
        {
            var hits = new List<RaycastHit2D>(Physics2D.RaycastAll(transform.position, -GetDirection() * _thurst * _exhaustLengthMultiplier));

            foreach (var hit in hits)
                if (hit.collider != null && hit.collider.gameObject != null)
                    if (hit.collider.gameObject.TryGetComponent(out ShipPart part) && part != PartComponent)
                        if (part.Ship == PartComponent.Ship)
                            return false;

            return true;
        }

        private Vector3 GetDirection()
        {
            PartDirection direction = StaticMethods.SumDirections(PartComponent.Orientation.Direction, _relativeDirection);
            switch (direction)
            {
                default:
                    return Vector3.zero;

                case PartDirection.Up:
                    return transform.up;

                case PartDirection.Down:
                    return -transform.up;

                case PartDirection.Left:
                    return -transform.right;

                case PartDirection.Right:
                    return transform.right;
            }
        }
    }
}