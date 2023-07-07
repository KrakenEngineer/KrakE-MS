using UnityEngine;
using MSEngine.Spaceships.Parts;

namespace MSEngine.PlayerInput
{
    [RequireComponent(typeof(Camera))]
    public class Player : MonoBehaviour
    {
        private Camera Camera;
        private Vector2 _mousePosition => Camera.ScreenToWorldPoint(Input.mousePosition);

        private void Start()
        {
            Camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(_mousePosition, Vector2.zero);

                if (hit.collider != null)
                    if (hit.collider.gameObject.TryGetComponent(out PartControlBlock controlBlock))
                        controlBlock.TrySetPlayer(this);
            }
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, Constants.CameraDepth);
        }
    }
}