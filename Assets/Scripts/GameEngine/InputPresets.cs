using System.Collections.Generic;
using UnityEngine;

namespace MSEngine
{
    [CreateAssetMenu(fileName = "new InputPreset", menuName = "GameEngine/new InputPreset")]
    public sealed class InputPreset : ScriptableObject
    {
        public string Name = "";
        public List<InputSetting> Settings = new List<InputSetting>();

        public bool Validate()
        {
            if (Settings.Count == (int)KeyAssignment.Count)
            {
                foreach (var setting in Settings)
                    if (setting.Assigment == KeyAssignment.Count)
                        return false;

                return true;
            }

            return false;
        }

        public KeyCode FindKey(KeyAssignment assignment)
        {
            foreach (var setting in Settings)
                if (setting.Assigment ==  assignment)
                    return setting.Key;

            return KeyCode.None;
        }
    }

    [System.Serializable]
    public struct InputSetting
    {
        public KeyAssignment Assigment;
        public KeyCode Key;
    }

    public enum KeyAssignment
    {
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        RotateLeft,
        RotateRight,
        EditorUp,
        EditorDown,
        EditorLeft,
        EditorRight,
        EditorMoveFaster,
        EditorCopyPart,
        Count
    }
}