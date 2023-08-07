using UnityEngine;

namespace MSEngine.UI
{
    public class PositionWriter : MonoBehaviour
    {
        [SerializeField] private Transform _object;
        [SerializeField] private UnityEngine.UI.Text _text;

        private void Update()
        {
            _text.text = $"{_object.name} position is: {_object.position}";
        }
    }
}