using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSEngine.PlayerInput
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float _defaultSpeed;
        [SerializeField] private float _speedMultiplier;
        [SerializeField] private float _minZoom;
        [SerializeField] private float _maxZoom;
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private Vector2 _bounds;
        
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            List<int> pressedKeys = GetPressedKeys().ToList();
            TryMove(pressedKeys);
            TryZoom();
        }

        private bool TryMove(List<int> pressedKeys)
        {
            float speed = _defaultSpeed * (DataLoader.GetKey(KeyAssignment.EditorMoveFaster) ? _speedMultiplier : 1);
            float Speed = _camera.orthographicSize * speed * Time.deltaTime;
            Vector3 oldPosition = transform.position;

            float deltaX = (pressedKeys[0] - pressedKeys[1]) * speed;
            float deltaY = (pressedKeys[2] - pressedKeys[3]) * speed;

            float x = Mathf.Clamp(transform.position.x + deltaX, 0, _bounds.x);
            float y = Mathf.Clamp(transform.position.y + deltaY, 0, _bounds.y);
            Vector3 newPosition = new Vector3(x, y, Constants.CameraDepth);
            transform.position = newPosition;

            return oldPosition != newPosition;
        }

        private void TryZoom()
        {
            float zoom = _zoomSpeed * _camera.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
            _camera.orthographicSize -= zoom;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _minZoom, _maxZoom);
        }

        private IEnumerable<int> GetPressedKeys()
        {
            yield return System.Convert.ToInt32(DataLoader.GetKey(KeyAssignment.EditorRight));
            yield return System.Convert.ToInt32(DataLoader.GetKey(KeyAssignment.EditorLeft));
            yield return System.Convert.ToInt32(DataLoader.GetKey(KeyAssignment.EditorUp));
            yield return System.Convert.ToInt32(DataLoader.GetKey(KeyAssignment.EditorDown));
        }
    }
}