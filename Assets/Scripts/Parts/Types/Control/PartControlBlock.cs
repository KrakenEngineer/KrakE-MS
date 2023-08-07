using UnityEngine;
using MSEngine.PlayerInput;

namespace MSEngine.Spaceships.Parts
{
    public class PartControlBlock : MonoBehaviour, IShipPartBehaviour
    {
        public Player CurrentPlayer { get; private set; }
        public ShipPart PartComponent => GetComponent<ShipPart>();

        void Update()
        {
            if (CurrentPlayer != null)
                CurrentPlayer.SetPosition(new Vector2(transform.position.x, transform.position.y));
        }

        public bool TrySetPlayer(Player player)
        {
            if (player == null || CurrentPlayer == null)
            {
                CurrentPlayer = player;
                return true;
            }

            else return false;
        }

        public bool TryRemovePlayer()
        {
            if (CurrentPlayer != null)
            {
                CurrentPlayer = null;
                return true;
            }

            return false;
        }
    }
}