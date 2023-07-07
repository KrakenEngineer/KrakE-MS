using System.Collections.Generic;
using UnityEngine;
using MSEngine.Saves.Configs;

namespace MSEngine.Saves.Levels
{
    public struct LevelSaveData
    {
        public string Name;
        public Vector2 Size;
        public List<LevelObjectData> Objects;
    }

    public struct LevelObjectData
    {
        public ObjectConfig Config;
        public LevelObjectTransform Transform;
    }

    public struct LevelObjectTransform
    {
        public Vector2 Velocity;
        public Vector2 Position;
        public Vector2 Scale;

        public float Rotation;
        public bool Static;
        public bool Indestructible;
    }
}