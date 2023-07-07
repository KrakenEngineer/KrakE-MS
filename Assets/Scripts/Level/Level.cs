using System.Collections.Generic;
using UnityEngine;
#region Game engine usings
using MSEngine.PlayerInput;
using MSEngine.Spaceships;
using MSEngine.Saves.Spaceships;
#endregion

namespace MSEngine.Scenes
{
    public class Level : Scene
    {
        public List<Player> Players;
        public List<Spaceship> Ships;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            Ships = Buffer.LastLaunchedShip.Load(ShipLoadMode.Level, transform, out List<ShipPart> parts);
        }
    }
}