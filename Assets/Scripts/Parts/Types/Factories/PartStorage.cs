using UnityEngine;
using MSEngine.Utility;

namespace MSEngine.Spaceships.Parts
{
    public class PartStorage : MonoBehaviour
    {
        [SerializeField] private int _stack;
        [SerializeField] private ResourceAmount _currentItem;

        public ResourceAmount CurrentItem
        {
            get => _currentItem;
            set
            {
                if (value.Amount < 0 || (value.ID == "" && value.Amount != 0))
                    throw new System.Exception("Invalid item for storage");

                _currentItem = value;
            }
        }
        public int Stack => _stack;

        private bool _initialized;

        public void Initialize(int stack, ResourceAmount item)
        {
            if (_initialized)
                throw new System.Exception("Can't initialize initialized storage");
            if (stack <= 0)
                throw new System.Exception("Can't initialize storage with non-positive stack");

            _stack = stack;
            CurrentItem = item;
            _initialized = true;
        }
    }
}