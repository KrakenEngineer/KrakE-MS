using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;
#region Game engine usings
using MSEngine.Spaceships;
using MSEngine.Spaceships.Parts;
using MSEngine.Saves.Configs;
using MSEngine.Saves.Spaceships;
using MSEngine.Utility;
#endregion

namespace MSEngine
{
    public static class DataLoader
    {
        public static InputPreset CurrentInputPreset;

        public static Dictionary<string, InputPreset> AllInputPresets;
        public static Dictionary<string, Sprite> Sprites;
        public static Dictionary<UIPrefabAssignment, UnityEngine.Object> UIPrefabs;
        public static Dictionary<PartCategory, PartCategoryData> PartCategories;
        public static Dictionary<string, Item> Items;

        public static void Clear()
        {
            CurrentInputPreset = null;
            AllInputPresets = new Dictionary<string, InputPreset>();
            Sprites = new Dictionary<string, Sprite>();
            UIPrefabs = new Dictionary<UIPrefabAssignment, UnityEngine.Object>();
            PartCategories = new Dictionary<PartCategory, PartCategoryData>();
            Items = new Dictionary<string, Item>();
        }

        public static void LoadAll()
        {
            LoadInputPresets();
            LoadSprites();
            LoadUIPrefabs();
            LoadPartCategories();
            LoadItems();
        }

        private static void LoadInputPresets()
        {
            InputPreset[] presets = Resources.LoadAll<InputPreset>(Constants.LoadPaths[PathType.InputPresets]);

            foreach (var preset in presets)
                TryAddInputPreset(preset);
        }

        private static void LoadSprites()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(Constants.LoadPaths[PathType.Sprites]);

            foreach (var sprite in sprites)
                Sprites.Add(sprite.name, sprite);
        }

        private static void LoadUIPrefabs()
        {
            UnityEngine.Object[] prefabs = Resources.LoadAll<UnityEngine.Object>(Constants.LoadPaths[PathType.UIPrefabs]);

            foreach (var prefab in prefabs)
                UIPrefabs.Add(Constants.StringToUIPrefabAssigment[prefab.name], prefab);
        }

        private static void LoadPartCategories()
        {
            PartCategoryData[] Categories = Resources.LoadAll<PartCategoryData>(Constants.LoadPaths[PathType.PartCategories]);

            foreach (var category in Categories)
            {
                category.Parts = new Dictionary<string, ObjectConfig>();
                PartCategories.Add(category.Category, category);
            }

            LoadParts();
        }

        private static void LoadParts()
        {
            ObjectConfig[] Parts = Resources.LoadAll<ObjectConfig>(Constants.LoadPaths[PathType.PartConfigs]);

            foreach (var part in Parts)
            {
                foreach (var ext in part.Extensions) ext.Config = part;
                if (part.IsValid())
                    PartCategories[part.MainExtension.Category].Parts.Add(part.Name, part);
            }
        }

        private static void LoadItems()
        {
            Item[] items = Resources.LoadAll<Item>(Constants.LoadPaths[PathType.Items]);

            foreach (var item in items)
                Items.Add(item.ID, item);
        }

        private static bool TryAddInputPreset(InputPreset preset)
        {
            bool canAdd = preset.Validate();

            if (canAdd)
            {
                AllInputPresets.Add(preset.Name, preset);
                if (preset.Name == "Default")
                    CurrentInputPreset = preset;
            }

            return canAdd;
        }

        public static bool GetKeyUp(KeyAssignment assigment)
        {
            return Input.GetKeyUp(CurrentInputPreset.FindKey(assigment));
        }

        public static bool GetKey(KeyAssignment assigment)
        {
            return Input.GetKey(CurrentInputPreset.FindKey(assigment));
        }

        public static bool GetKeyDown(KeyAssignment assigment)
        {
            return Input.GetKeyDown(CurrentInputPreset.FindKey(assigment));
        }

        public static Sprite GetSprite(string id)
        {
            return Sprites[id];
        }

        public static UnityEngine.Object GetUIPrefab(UIPrefabAssignment assigment)
        {
            return UIPrefabs[assigment];
        }

        public static PartCategoryData GetPartCategory(PartCategory Category)
        {
            return PartCategories[Category];
        }

        public static ObjectConfig GetPart(string ID)
        {
            return PartCategories[ConfigConstants.StringToPartCategory[ID.Split(':')[2]]].Parts[ID];
        }

        public static Item GetItem(string ID)
        {
            return Items[ID];
        }
    }

    public static class StaticMethods
    {
        public static bool IsVectorClamped(Vector3 input, Vector3 min, Vector3 max)
        {
            bool x = min.x <= input.x && input.x <= max.x;
            bool y = min.y <= input.y && input.y <= max.y;
            bool z = min.z <= input.z && input.z <= max.z;
            return x && y && z;
        }

        public static float GetDistance(Vector2 start, Vector2 line)
        {
            if (line.x == 0 && line.y == 0) return 0;
            if (line.x == 0) return -start.x;
            if (line.y == 0) return -start.y;

            Vector2 function = new Vector2(line.y/line.x, start.y - start.x*line.y/line.x);
            Vector2 functionZeros = new Vector2(-function.y / function.x, function.y);

            if (functionZeros == Vector2.zero) return 0;
            return functionZeros.x * functionZeros.y / functionZeros.magnitude;
        }

        public static PartDirection SumDirections(PartDirection left, PartDirection right)
        {
            return (PartDirection)(((int)left + (int)right) % (int)PartDirection.None);
        }

        public static Vector2Int Vector3ToVector2Int(Vector3 input)
        {
            int x = Mathf.RoundToInt(input.x);
            int y = Mathf.RoundToInt(input.y);

            return new Vector2Int(x, y);
        }

        public static Vector3 Vector2IntToVector3(Vector2Int input)
        {
            return new Vector3(input.x, input.y, 0);
        }

        public static Vector3 Clamp(Vector3 input, Vector3 min, Vector3 max)
        {
            float x = Mathf.Clamp(input.x, min.x, max.x);
            float y = Mathf.Clamp(input.y, min.y, max.y);
            float z = Mathf.Clamp(input.z, min.z, max.z);

            Vector3 output = new Vector3(x, y, z);
            return output;
        }

        public static Vector3 Abs(Vector3 input)
        {
            float x = Mathf.Abs(input.x);
            float y = Mathf.Abs(input.y);
            float z = Mathf.Abs(input.z);

            Vector3 output = new Vector3(x, y, z);
            return output;
        }

        public static float GetTorque(Vector2 position, Vector2 force) =>
            force.magnitude * GetDistance(position, force);
    }

    public static class Constants
    {
        public static readonly int MinHealth = -10;
        public static readonly float CameraDepth = -10;
        
        public static readonly Dictionary<PathType, string> LoadPaths = new Dictionary<PathType, string>
        {
            { PathType.InputPresets, "Loadable/ScriptableObjects/InputPresets" },
            { PathType.Sprites, "Loadable/Sprites" },
            { PathType.UIPrefabs, "Loadable/Prefabs/UI" },
            { PathType.PartCategories, "Loadable/ScriptableObjects/PartCategories" },
            { PathType.PartConfigs,"Loadable/ScriptableObjects/Parts" },
            { PathType.Parts,"Loadable/Prefabs/Parts" },
            { PathType.Items,"Loadable/ScriptableObjects/Items" }
        };

        public static readonly Dictionary<string, UIPrefabAssignment> StringToUIPrefabAssigment = new Dictionary<string, UIPrefabAssignment>
        {
            { "PartButton", UIPrefabAssignment.PartButton },
            { "Panel", UIPrefabAssignment.Panel },
            { "Control", UIPrefabAssignment.Control },
            { "Storages", UIPrefabAssignment.Storages },
            { "Engines", UIPrefabAssignment.Engines },
            { "Generators", UIPrefabAssignment.Generators },
            { "Struct", UIPrefabAssignment.Struct },
            { "Launch", UIPrefabAssignment.Launch },
            { "ToEditor", UIPrefabAssignment.ToEditor }
        };

        public static readonly List<UIPrefabAssignment> PartCategories = new List<UIPrefabAssignment>
        {
            UIPrefabAssignment.Control,
            UIPrefabAssignment.Engines,
            UIPrefabAssignment.Struct
        };
    }

    public interface IDamagable
    {
        public void Break();
        public void Heal(int health);
        public void Damage(int health);
        public void OnCollisionEnter2D(Collision2D collision);
    }

    public enum PathType
    {
        None,
        InputPresets,
        Sprites,
        UIPrefabs,
        PartCategories,
        PartConfigs,
        Parts,
        Items,
        Count
    }

    public enum UIPrefabAssignment
    {
        PartButton,
        Panel,
        Control,
        Storages,
        Engines,
        Generators,
        Struct,
        Launch,
        ToEditor,
        Count
    }

    namespace Scenes
    {
        public static class Buffer
        {
            public static ShipSaveData LastLaunchedShip;
        }

        public static class SceneConstants
        {
            public static readonly Vector3 EditorGridOffset = new Vector3(-0.5f, -0.5f, -1);

            public static readonly Dictionary<SceneAssigment, int> SceneNumbers = new Dictionary<SceneAssigment, int>
            {
                { SceneAssigment.MainTest, 1 },
                { SceneAssigment.ShipEditor, 0 }
            };
        }

        public abstract class Scene : MonoBehaviour
        {
            protected virtual void Awake()
            {
                DataLoader.Clear();
                DataLoader.LoadAll();
            }

            public static void Load(SceneAssigment scene)
            {
                int _scene = SceneConstants.SceneNumbers[scene];
                UnityEngine.SceneManagement.SceneManager.LoadScene(_scene);
            }

            public static void LoadShipEditor() { Load(SceneAssigment.ShipEditor); }
            public static void LoadMainTest() { Load(SceneAssigment.MainTest); }
        }

        public enum SceneAssigment
        {
            None,
            MainTest,
            ShipEditor,
            Count
        }
    }

    namespace Saves.Configs
    {
        public static class ComponentFiller
        {
            public static void FillShipPart(ShipPart part, ConfigExtension ext)
            {
                part.Initialize(ext.Config);
            }

            public static void FillControlBlock(PartControlBlock part, ConfigExtension ext) { }

            public static void FillStorage(CompositeStoragePart part, ConfigExtension ext)
            {
                part.Initialize(ext.Stack, ext.StartItemF, ext.Indicator);
            }

            public static void FillSolidStorage(PartSolidStorage part, ConfigExtension ext)
            {
                part.Initialize(ext.Stack, ext.StartItem, ext.Indicator);
            }

            public static void FillEngine(PartEngine part, ConfigExtension ext)
            {
                part.Initialize(ext.CurrentThurst, ext.ExhaustLengthMultiplier, ext.ThurstPosition, ext.Consumption, ext.Output);
            }

            public static void FillGenerator(PartGenerator part, ConfigExtension ext)
            {
                part.Initialize(ext.Consumption, ext.Output);
            }

            public static void FillGyro(PartGyro part, ConfigExtension ext)
            {
                part.Initialize(ext.CurrentTorque, ext.Consumption, ext.Output);
            }

            public static void FillRectCollider(BoxCollider2D collider, ColliderData data)
            {
                collider.size = data.Size;
            }
        }

        public static class ConfigConstants
        {
            public static readonly List<ConfigExtensionType> ConfigTypes = new List<ConfigExtensionType>
            {
                ConfigExtensionType.Part
            };

            public static readonly Dictionary<string, PartCategory> StringToPartCategory = new Dictionary<string, PartCategory>
            {
                { "Control", PartCategory.Control },
                { "Storages", PartCategory.Storages },
                { "Engines", PartCategory.Engines }
            };

            public static readonly Dictionary<ConfigExtensionType, Type> ConfigExtensionTypeToComponent = new Dictionary<ConfigExtensionType, Type>
            {
                { ConfigExtensionType.ControlBlock, typeof(PartControlBlock) },
                { ConfigExtensionType.Engine, typeof(PartEngine) },
                { ConfigExtensionType.Generator, typeof(PartGenerator) },
                { ConfigExtensionType.Gyro, typeof(PartGyro) },
                { ConfigExtensionType.Part, typeof(ShipPart) },
                { ConfigExtensionType.Storage, typeof(CompositeStoragePart) },
                { ConfigExtensionType.SolidStorage, typeof(PartSolidStorage) }
            };

            public static readonly Dictionary<ConfigColliderType, Type> ColliderTypeToCollider = new Dictionary<ConfigColliderType, Type>
            {
                { ConfigColliderType.Rect, typeof(BoxCollider2D)},
                { ConfigColliderType.Circle, typeof(CircleCollider2D)},
            };

            public static readonly Dictionary<Type, MethodInfo> ComponentToFillMethod = new Dictionary<Type, MethodInfo>
            {
                { typeof(PartControlBlock), typeof(ComponentFiller).GetMethod("FillControlBlock") },
                { typeof(PartEngine), typeof(ComponentFiller).GetMethod("FillEngine") },
                { typeof(PartGenerator), typeof(ComponentFiller).GetMethod("FillGenerator") },
                { typeof(PartGyro), typeof(ComponentFiller).GetMethod("FillGyro") },
                { typeof(ShipPart), typeof(ComponentFiller).GetMethod("FillShipPart") },
                { typeof(CompositeStoragePart), typeof(ComponentFiller).GetMethod("FillStorage") },
                { typeof(PartSolidStorage), typeof(ComponentFiller).GetMethod("FillSolidStorage") },
                { typeof(BoxCollider2D), typeof(ComponentFiller).GetMethod("FillRectCollider") }
            };
        }
    }

    namespace Spaceships
    {
        public sealed class PartField
        {
            private ShipPart[,] _parts;

            public PartField(Vector2Int size)
            {
                if (size.x < 0 || size.y < 0)
                    throw new System.Exception("Invalid size for ShipEditorField");

                _parts = new ShipPart[size.x, size.y];
            }

            public PartField(ShipPart[,] parts)
            {
                _parts = (ShipPart[,])parts.Clone();
            }

            public PartField(PartField field)
            {
                _parts = field.Parts;
            }

            public Vector2Int Size => new Vector2Int(_parts.GetLength(0), _parts.GetLength(1));
            public ShipPart[,] Parts => (ShipPart[,])_parts.Clone();
            public List<ShipPart> PartsList => _parts.Cast<ShipPart>().Distinct().Where(part => part != null).ToList();

            public void Update(ShipPart part, Vector2Int start, Vector2Int end)
            {
                if (!ValidatePositions(start, end))
                    throw new System.Exception("Incorrect positions for field update");

                for (int x = start.x; x < end.x; x++)
                    for (int y = start.y; y < end.y; y++)
                        _parts[x, y] = part;
            }

            public void Pick(ShipPart part)
            {
                if (part == null)
                    throw new Exception("Can't pick part because it is null");

                part.Pick();
                for (int x = 0; x < Size.x; x++)
                    for (int y = 0; y < Size.y; y++)
                        if (_parts[x, y] != null && _parts[x, y] == part)
                            _parts[x, y] = null;
            }

            public bool Contains(ShipPart _part)
            {
                foreach (var part in _parts)
                    if (part == _part)
                        return true;

                return false;
            }

            public bool CheckForEmployment(Vector2Int start, Vector2Int end)
            {
                if (!ValidatePositions(start, end))
                    throw new System.Exception("Incorrect positions for employment check");

                bool output = true;

                for (int x = start.x; x < end.x; x++)
                {
                    for (int y = start.y; y < end.y; y++)
                    {
                        if (_parts[x, y] != null)
                        {
                            output = false;
                            break;
                        }
                    }
                }

                return output;
            }

            public bool TryRelease(ShipPart part, Scenes.Editors.ShipEditor editor)
            {
                Transform t_part = part.transform;
                Vector3 partSize = t_part.GetComponent<ShipPart>().GetExactScale();
                Vector2 partPosition = t_part.position - partSize / 2;

                bool isPartInField = partPosition.x >= 0 && partPosition.y >= 0 &&
                    partPosition.x + partSize.x <= Size.x + 0.5f && partPosition.y + partSize.y <= Size.y + 0.5f;

                if (isPartInField) return TryPlacePart(t_part, partPosition, partSize, editor);

                else
                {
                    UnityEngine.Object.DestroyImmediate(part.gameObject);
                    return true;
                }
            }

            public bool TryPlacePart(Transform part, Vector3 partPosition3d, Vector3 partSize3d, Scenes.Editors.ShipEditor editor)
            {
                bool canPlace = false;
                Vector2Int partPosition = StaticMethods.Vector3ToVector2Int(partPosition3d);
                Vector2Int partSize = StaticMethods.Vector3ToVector2Int(partSize3d);
                canPlace = CheckForEmployment(partPosition, partPosition + partSize);

                if (canPlace)
                {
                    part.position = StaticMethods.Vector2IntToVector3(partPosition) + partSize3d + Scenes.SceneConstants.EditorGridOffset;
                    Vector2Int partEndPosition = partPosition + partSize;
                    Update(part.gameObject.GetComponent<ShipPart>(), partPosition, partEndPosition);

                    part.GetComponent<ShipPart>().Place(this, partPosition);
                    if (editor != null)
                        editor._ui.CurrentPart = null;
                }

                return canPlace;
            }

            public bool TryGetPart(Vector3 position, out ShipPart Part)
            {
                bool output = false;
                Part = null;
                RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.TryGetComponent(out ShipPart part))
                    {
                        Part = part;
                        output = true;
                    }
                }

                return output;
            }

            private bool ValidatePosition(Vector2Int position) => (0 <= position.x && position.x <= Size.x) && (0 <= position.y && position.y <= Size.y);

            private bool ValidatePositions(Vector2Int start, Vector2Int end) =>
                ValidatePosition(start) && ValidatePosition(end) && start.x <= end.x || start.y <= end.y;
        }

        [RequireComponent(typeof(ShipPart))]
        public abstract class MovingPart : ResourceRelated
        {
            [SerializeField] protected bool _initialized;
            [SerializeField] protected PartDirection _relativeDirection;

            [SerializeField] protected List<Resource> _consumption;
            [SerializeField] protected List<Resource> _output;

            public readonly PartDirection DefaultDirection = PartDirection.Up;

            public Vector2 MovementPoint { get; protected set; }
            public override ShipPart PartComponent => GetComponent<ShipPart>();

            public override List<Resource> Consumption => new List<Resource>(_consumption);
            public override List<Resource> Output => new List<Resource>(_output);
            public override List<string> RelatedTo => GetRelatedTo();

            public abstract float GetTorque(bool clockwise);
            public abstract Vector2 GetForce();
            public PartDirection Direction =>
                StaticMethods.SumDirections(PartComponent.Orientation.Direction, _relativeDirection);

            public Vector3 GetMovement(CenterOfMass centerOfMass, List<KeyAssignment> keys)
            {
                Vector3 movement = Vector3.zero;
                bool rotateLeft = keys.Contains(KeyAssignment.RotateLeft);
                bool rotateRight = keys.Contains(KeyAssignment.RotateRight);

                if (keys.Contains(ShipConstants.DirectionToKey[Direction]))
                {
                    Vector2 force = GetForce();
                    Vector2 forcePosition =
                        PartComponent.Start - PartComponent.Ship.Start + MovementPoint - centerOfMass.Position;
                    float torque = StaticMethods.GetTorque(forcePosition, force);
                    movement += new Vector3(force.x, force.y, torque);
                }

                if (rotateLeft && !rotateRight) movement += new Vector3(0, 0, GetTorque(false));
                if (rotateRight && !rotateLeft) movement += new Vector3(0, 0, GetTorque(true));

                return movement;
            }

            protected List<string> GetRelatedTo()
            {
                var output = new List<Resource>(_consumption);
                output.AddRange(_output);
                return output.Select(r => r.ID).Distinct().ToList();
            }
        }

        public static class ShipConstants
        {
            public static readonly int MinPartHealth = -10;
            public static readonly float AngularDrag = 0.75f;
            public static readonly float CollisionDamageMultiplier = 0.4f;
            public static readonly float LinearDrag = 0.8f;
            public static readonly float PartDepth = -1;
            public static readonly float ShipDepth = -1;
            
            public static readonly List<KeyAssignment> ShipControlKeys = new List<KeyAssignment>()
            {
                KeyAssignment.MoveForward,
                KeyAssignment.MoveBackward,
                KeyAssignment.MoveLeft,
                KeyAssignment.MoveRight,
                KeyAssignment.RotateLeft,
                KeyAssignment.RotateRight
            };

            public static readonly Dictionary<PartDirection, KeyAssignment> DirectionToKey = new Dictionary<PartDirection, KeyAssignment>()
            {
                { PartDirection.Up, KeyAssignment.MoveForward },
                { PartDirection.Down, KeyAssignment.MoveBackward },
                { PartDirection.Left, KeyAssignment.MoveLeft },
                { PartDirection.Right, KeyAssignment.MoveRight }
            };

            public static readonly Dictionary<PartDirection, Vector2> Directions = new Dictionary<PartDirection, Vector2>()
            {
                { PartDirection.Up, Vector2.up },
                { PartDirection.Down, Vector2.down },
                { PartDirection.Left, Vector2.left },
                { PartDirection.Right, Vector2.right },
                { PartDirection.None, Vector2.zero}
            };
        }

        [Serializable]
        public struct CenterOfMass
        {
            [SerializeField] private Vector2 _position;
            [SerializeField] private float _mass;

            public CenterOfMass(Vector2 position, float mass)
            {
                if (position.x < 0 || position.y < 0)
                    throw new Exception("Invalid position for center of mass");
                if (mass < 0)
                    throw new Exception("Invalid mass for center of mass");

                _position = position;
                _mass = mass;
            }

            public Vector2 Position => _position;
            public float Mass => _mass;

            public static CenterOfMass zero => new CenterOfMass(Vector2.zero, 0);

            public static CenterOfMass operator +(CenterOfMass com, Vector2 offset) =>
                new CenterOfMass(com.Position + offset, com.Mass);

            public static CenterOfMass operator +(CenterOfMass com, float mass) =>
                new CenterOfMass(com.Position, com.Mass + mass);

            public static CenterOfMass operator *(CenterOfMass com, float mass) =>
                new CenterOfMass(com.Position, com.Mass * mass);

            public static CenterOfMass operator +(CenterOfMass left, CenterOfMass right) =>
                new CenterOfMass(left.Position + right.Position, left.Mass + right.Mass);

            public static CenterOfMass operator *(CenterOfMass left, CenterOfMass right) =>
                new CenterOfMass(
                    new Vector2(left.Position.x * right.Position.x, left.Position.y * right.Position.y),
                    left.Mass * right.Mass);

            public static CenterOfMass Average(List<CenterOfMass> list)
            {
                CenterOfMass output = zero;

                foreach (var item in list)
                    output += item;

                output._position /= output._mass;
                return output;
            }

            public static CenterOfMass Average(Vector2 start, List<ShipPart> ship)
            {
                CenterOfMass output = zero;

                foreach (var item in ship)
                    output += item.CenterOfMass + new CenterOfMass(item.Start - start, 0);

                output._position /= output._mass;
                return output;
            }
        }

        [Serializable]
        public struct ResourceSystemID
        {
            [SerializeField] private int _number;
            [SerializeField] private string _resource;

            public ResourceSystemID(int number, string resource)
            {
                if (number < -1)
                    throw new Exception("Invalid number of ResourceSystemID");
                if (number == -1 && resource != Resource.Void.ID)
                    throw new Exception("Invalid resource for 'none' resource system id");

                _number = number;
                _resource = resource;
            }

            public string resource => _resource;
            public int number => _number;

            public static ResourceSystemID None => new ResourceSystemID(-1, Resource.Void.ID);

            public static bool operator == (ResourceSystemID left, ResourceSystemID right) =>
                left._number == right._number && left._resource == right._resource;

            public static bool operator != (ResourceSystemID left, ResourceSystemID right) => !(left == right);
        }

        [Serializable]
        public struct PartOrientation
        {
            private static readonly Dictionary<PartOrientation, PartOrientation> _rotateClockwise = new Dictionary<PartOrientation, PartOrientation>
            {
                { Up, Right },
                { UpFlip, RightFlip },
                { Right, Down },
                { RightFlip, DownFlip },
                { Down, Left },
                { DownFlip, LeftFlip },
                { Left, Up },
                { LeftFlip, UpFlip }
            };

            private static readonly Dictionary<PartOrientation, PartOrientation> _xFlip = new Dictionary<PartOrientation, PartOrientation>
            {
                { Up, UpFlip },
                { UpFlip, Up },
                { Right, LeftFlip },
                { RightFlip, Left },
                { Down, DownFlip },
                { DownFlip, Down },
                { Left, RightFlip },
                { LeftFlip, Right }
            };

            private static readonly Dictionary<PartOrientation, PartOrientation> _yFlip = new Dictionary<PartOrientation, PartOrientation>
            {
                { Up, DownFlip },
                { UpFlip, Down },
                { Right, RightFlip },
                { RightFlip, Right },
                { Down, UpFlip },
                { DownFlip, Up },
                { Left, LeftFlip },
                { LeftFlip, Left }
            };

            [SerializeField] private PartDirection _direction;
            [SerializeField] private bool _flip;

            public PartOrientation(PartDirection direction, bool flip)
            {
                if (direction == PartDirection.None)
                    throw new Exception("Invalid direction for PartOrientation");

                _direction = direction;
                _flip = flip;
            }

            public PartDirection Direction => _direction;
            public bool Flip => _flip;

            public static PartOrientation Up => new PartOrientation(PartDirection.Up, false);
            public static PartOrientation UpFlip => new PartOrientation(PartDirection.Up, true);
            public static PartOrientation Right => new PartOrientation(PartDirection.Right, false);
            public static PartOrientation RightFlip => new PartOrientation(PartDirection.Right, true);
            public static PartOrientation Down => new PartOrientation(PartDirection.Down, false);
            public static PartOrientation DownFlip => new PartOrientation(PartDirection.Down, true);
            public static PartOrientation Left => new PartOrientation(PartDirection.Left, false);
            public static PartOrientation LeftFlip => new PartOrientation(PartDirection.Left, true);

            public PartOrientation RotateClockwise() => _rotateClockwise[this];
            public PartOrientation FlipX() => _xFlip[this];
            public PartOrientation FlipY() => _yFlip[this];
        }

        public enum PartDirection
        {
            Up,
            Right,
            Down,
            Left,
            None
        }
    }
}