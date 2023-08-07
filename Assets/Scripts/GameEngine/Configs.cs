using System.Collections.Generic;
using UnityEngine;
using System;
using MSEngine.Utility;
using MSEngine.Spaceships;
using MSEngine.UI;

namespace MSEngine.Saves.Configs
{
    [CreateAssetMenu(fileName = "new ObjectConfig", menuName = "Configs/new ObjectConfig")]
    public sealed class ObjectConfig : ScriptableObject
    {
        public string Name;
        public string Description;
        public CenterOfMass CenterOfMass;
        public Sprite Sprite;

        public ColliderData Collider = new ColliderData();
        public ConfigExtension MainExtension = new ConfigExtension();
        public List<ConfigExtension> Extensions = new List<ConfigExtension>();

        public ObjectConfig() { }

        public ConfigExtensionType Type => MainExtension.Type;

        public ObjectConfig Copy()
        {
            var output = new ObjectConfig();

            output.Name = Name;
            output.Description = Description;
            output.CenterOfMass = CenterOfMass;
            output.Sprite = Sprite;

            output.Collider = Collider.Copy(output);
            output.MainExtension = MainExtension.Copy(output);
            output.Extensions = new List<ConfigExtension>();

            foreach (var extension in Extensions)
                output.Extensions.Add(extension.Copy(output));

            return output;
        }

        public List<ConfigExtension> GetExtensions(ConfigExtensionType type)
        {
            List<ConfigExtension> output = new List<ConfigExtension>();

            foreach (var extention in Extensions)
                if (extention.Type == type)
                    output.Add(extention);

            return output;
        }

        public bool HasExtension(ConfigExtensionType type)
        {
            bool output = false;

            foreach (var extension in Extensions)
            {
                if (extension.Type == type)
                {
                    output = true;
                    break;
                }
            }

            return output;
        }

        public bool IsValid()
        {
            int len = (int)ConfigExtensionType.Count + 1;
            int[] ExtensionsCount = new int[len];

            foreach (var extension in Extensions)
                ExtensionsCount[(int)extension.Type] += 1;

            bool isValid = ExtensionsCount[0] == 0 && ExtensionsCount[ExtensionsCount.Length - 1] == 0;
            isValid = isValid && (ExtensionsCount[(int)ConfigExtensionType.Part]) == 0;
            isValid = isValid && ConfigConstants.ConfigTypes.Contains(Type);

            return isValid;
        }

        public GameObject MakeObject(Transform parent = null)
        {
            GameObject obj = new GameObject(Name);

            obj.transform.localScale = new Vector3(Collider.Size.x, Collider.Size.y, 0);
            obj.transform.parent = parent;

            obj.AddComponent<SpriteRenderer>().sprite = Sprite;
            AddCollider(obj, Collider);
            AddExtension(obj, MainExtension);

            foreach (var extension in Extensions)
                AddExtension(obj, extension);

            if (obj.TryGetComponent(out ResourceComponent c))
                c.GenerateCTRS(obj.GetComponents<ResourceRelated>());

            return obj;
        }

        public void AddCollider(GameObject obj, ColliderData colliderData)
        {
            Type type = ConfigConstants.ColliderTypeToCollider[colliderData.Type];
            ConfigConstants.ComponentToFillMethod[type]
                .Invoke(typeof(ComponentFiller), new object[] { obj.AddComponent(type), colliderData });
        }

        public void AddExtension(GameObject obj, ConfigExtension ext)
        {
            Type type = ConfigConstants.ConfigExtensionTypeToComponent[ext.Type];
            ConfigConstants.ComponentToFillMethod[type]
                .Invoke(typeof(ComponentFiller), new object[] { obj.AddComponent(type), ext });
        }
    }

    [Serializable]
    public class ConfigExtension
    {
        [HideInInspector] public ObjectConfig Config;
        public ConfigExtensionType Type;
        //Part
        public PartCategory Category;
        public int StartHealth;
        public int ImpactHealth;
        //IResourceRelated
        public List<Resource> Consumption;
        public List<Resource> Output;
        //Engine
        public float MinThurst;
        public float MaxThurst;
        public float ExhaustLengthMultiplier;
        public float CurrentThurst;
        public Vector2 ThurstPosition;
        //Gyro
        public float MinTorque;
        public float MaxTorque;
        public float CurrentTorque;
        //Storage
        public IndicatorSettings Indicator;
        public SolidResource StartItem;
        public Resource StartItemF;
        public int Stack;

        public ConfigExtension() { }

        public ConfigExtension Copy(ObjectConfig config)
        {
            var output = new ConfigExtension();
            output.Config = config;
            output.Type = Type;
            output.Category = Category;

            output.StartHealth = StartHealth;
            output.ImpactHealth = ImpactHealth;
            output.Consumption = Consumption;
            output.Output = Output;

            output.MinThurst = MinThurst;
            output.MaxThurst = MaxThurst;
            output.ExhaustLengthMultiplier = ExhaustLengthMultiplier;
            output.CurrentThurst = CurrentThurst;
            output.ThurstPosition = ThurstPosition;

            output.MinTorque = MinTorque;
            output.MaxTorque = MaxTorque;
            output.CurrentTorque = CurrentTorque;

            output.Indicator = Indicator;
            output.StartItem = StartItem;
            output.StartItemF = StartItemF;
            output.Stack = Stack;
            return output;
        }
    }

    [Serializable]
    public class ColliderData
    {
        [HideInInspector] public ObjectConfig Config;
        public ConfigColliderType Type = ConfigColliderType.Rect;

        public Vector2 Size;
        public float Radius;

        public ColliderData() { }

        public ColliderData Copy(ObjectConfig config)
        {
            var output = new ColliderData();
            output.Config = config;

            output.Type = Type;
            output.Size = Size;
            output.Radius = Radius;

            return output;
        }
    }

    public enum ConfigExtensionType
    {
        None,
        ControlBlock,
        Engine,
        Generator,
        Gyro,
        Part,
        Storage,
        SolidStorage,
        Count
    }

    public enum ConfigColliderType
    {
        None,
        Rect,
        Circle,
        Count
    }
}