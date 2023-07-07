using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSEngine.Saves.Configs
{
    [CreateAssetMenu(fileName = "new Item", menuName = "Configs/new Item")]
    public class Item : ScriptableObject
    {
        public string ID;
        public string Name;
        public float Mass;

        public Sprite Sprite;
    }
}