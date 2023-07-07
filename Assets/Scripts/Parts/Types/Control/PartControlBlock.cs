using UnityEngine;
using MSEngine.PlayerInput;

namespace MSEngine.Spaceships.Parts
{
    public class PartControlBlock : MonoBehaviour
    {
        public Player CurrentPlayer { get; private set; }

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
    }
}