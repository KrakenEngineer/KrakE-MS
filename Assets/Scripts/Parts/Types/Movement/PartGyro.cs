using UnityEngine;

namespace MSEngine.Spaceships.Parts
{
    [RequireComponent(typeof(ShipPart))]
    public class PartGyro : MovingPart
    {
        [SerializeField] private float _torque;
        public float Torque => _torque;

        public void Initialize(float torque, float rotation)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize this gyro because it is initialized");
            if (torque <= 0)
                throw new System.Exception("Can't initialize gyro with non-positive torque");

            _torque = torque;
            _relativeDirection = DefaultDirection;
            _initialized = true;
        }

        public override float GetTorque(bool clockwise) => clockwise ? _torque : -_torque;
        public override Vector2 GetForce() => Vector2.zero;
    }
}