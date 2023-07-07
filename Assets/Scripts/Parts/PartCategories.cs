using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSEngine.Saves.Configs
{
    [CreateAssetMenu(fileName = "new PartCategory", menuName = "Configs/new PartCategory")]
    public class PartCategoryData : ScriptableObject
    {
        public PartCategory Category;
        public Dictionary<string, ObjectConfig> Parts;
    }

    public enum PartCategory
    {
        Control,
        Storages,
        Engines,
        Count
    }
}