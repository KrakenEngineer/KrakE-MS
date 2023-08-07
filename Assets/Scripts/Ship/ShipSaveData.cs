
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#region Game engine usings
using MSEngine.Saves.Configs;
using MSEngine.Spaceships;
using MSEngine.Utility;
#endregion

namespace MSEngine.Saves.Spaceships
{
    public sealed class ShipSaveData
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }

        private List<ResourceSystemID> _resourceSystems;
        private List<ShipComponentData> _components;

        public static ShipSaveData Save(string name, string description, List<ShipPart> parts)
        {
            var data = new ShipSaveData();
            data.Name = name;
            data.Description = description;

            data._components = GenerateComponents(parts);
            //data._resourceSystems = GenerateResourceSystems(parts).Keys.ToList();
            data.TryGetBounds(data._components, out Vector2Int start, out Vector2Int end);

            data.Start = start;
            data.End = end;

            return data;
        }

        public List<Spaceship> Load(ShipLoadMode mode, Transform parent, out List<ShipPart> partsOut)
        {
            var output = new List<Spaceship>();
            partsOut = new List<ShipPart>();

            foreach (var component in _components)
            {
                output.Add(component.Load(mode, Name, parent, out List<ShipPart> parts, _resourceSystems));

                foreach (var part in parts)
                    partsOut.Add(part);
            }

            partsOut = partsOut.Distinct().Where(part => part != null).ToList();
            return output;
        }

        private bool TryGetBounds(List<ShipComponentData> components, out Vector2Int start, out Vector2Int end)
        {
            if (components.Count == 0)
            {
                start = Vector2Int.zero;
                end = Vector2Int.zero;
                return false;
            }

            Vector2Int _start = components[0].Start;
            Vector2Int _end = components[0].End;

            foreach (var component in components)
            {
                if (component.Start.x < _start.x) _start.x = component.Start.x;
                if (component.Start.y < _start.y) _start.y = component.Start.y;
                if (component.End.x > _end.x) _end.x = component.End.x;
                if (component.End.y > _end.y) _end.y = component.End.y;
            }

            start = _start;
            end = _end;
            return true;
        }

        private static List<ShipComponentData> GenerateComponents(List<ShipPart> parts)
        {
            List<PartGraph<ShipPart>> components = new PartGraph<ShipPart>(parts).Split();
            var output = new List<ShipComponentData>();
            foreach (var component in components)
                if (component.PartsCount != 0)
                    output.Add(ShipComponentData.Save(component.Parts));

            return output;
        }

        /*private static Dictionary<ResourceSystemID, List<ShipPart>> GenerateResourceSystems(List<ShipPart> parts)
        {
            List<ResourceRelated> _resourceRelatedBehaviours = new List<ResourceRelated>();
            foreach (var part in parts)
                _resourceRelatedBehaviours.AddRange(part.GetComponents<ResourceRelated>());

            Dictionary<string, List<List<ResourceRelated>>> systemGroups = SplitResourceSystem(_resourceRelatedBehaviours);
            var output = new Dictionary<ResourceSystemID, List<ShipPart>>();

            foreach (var systemGroup in systemGroups)
            {
                for (int i = 0; i < systemGroup.Value.Count; i++)
                {
                    var resourceSystemKey = new ResourceSystemID(i, systemGroup.Key);
                    List<ShipPart> partsList =
                        systemGroup.Value[i].Select(component => component.PartComponent).Distinct().ToList();

                    output.Add(resourceSystemKey, partsList);
                    foreach (var part in partsList)
                        part.TryAddResourceSystem(resourceSystemKey);
                }
            }

            return output;
        }

        private static Dictionary<string, List<List<ResourceRelated>>> SplitResourceSystem(List<ResourceRelated> components)
        {
            List<string> IDs = GenerateIDList(components);
            var output = new Dictionary<string, List<List<ResourceRelated>>>();

            foreach (var id in IDs)
            {
                List<ResourceRelated> relatedToID =
                    components.Where(component => component.RelatedTo.Contains(id)).ToList();
                List<PartGraph<ResourceRelated>> grahpList = new PartGraph<ResourceRelated>(relatedToID).Split();
                List<List<ResourceRelated>> systems = new List<List<ResourceRelated>>();

                foreach (var partGraph in grahpList)
                    systems.Add(partGraph.Parts);

                output.Add(id, systems);
            }

            return output;
        }

        private static List<string> GenerateIDList(List<ResourceRelated> components)
        {
            List<string> IDs = new List<string>();

            foreach (var component in components)
            {
                IDs.AddRange(component.Consumption.Select(item => item.ID));
                IDs.AddRange(component.Output.Select(item => item.ID));

                if (component is Storage<Resource> storage)
                    IDs.Add(storage.GetContent().ID);
                if (component is Storage<SolidResource> storageS)
                    IDs.Add(storageS.GetContent().ID);
            }

            return IDs.Distinct().ToList();
        }*/
    }

    internal sealed class ShipComponentData
    {
        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }

        private CenterOfMass _centerOfMass;

        private List<PartSaveData> _parts;

        public ShipComponentData()
        {
            Start = Vector2Int.zero;
            End = Vector2Int.zero;
            _parts = new List<PartSaveData>();
        }

        public bool HasPart(PartSaveData part)
        {
            return _parts.Contains(part);
        }

        public static ShipComponentData Save(List<ShipPart> ship)
        {
            var output = new ShipComponentData();

            foreach (var part in ship)
                output._parts.Add(PartSaveData.Save(part));

            TryGetBounds(output._parts, out Vector2Int start, out Vector2Int end);
            output.Start = start;
            output.End = end;
            output._centerOfMass = CenterOfMass.Average(start, ship);

            return output;
        }

        public Spaceship Load(ShipLoadMode mode, string name, Transform parent, out List<ShipPart> parts, List<ResourceSystemID> resourceSystems)
        {
            Spaceship ship;
            switch (mode)
            {
                case ShipLoadMode.Editor:
                    ship = LoadToEditor(name, parent);
                    break;

                case ShipLoadMode.Level:
                    ship = LoadToLevel(name, parent, resourceSystems);
                    break;

                default:
                    throw new System.Exception("Can't load ship without loading mode");
            }

            parts = ship.Parts;
            return ship;
        }

        public Spaceship LoadToLevel(string name, Transform parent, List<ResourceSystemID> resourceSystems)
        {
            var ship = new GameObject(name);
            List<ShipPart> parts = PartSaveData.Load(_parts, ship.transform);
            ship.AddComponent<Rigidbody2D>();

            var cShip = ship.AddComponent<Spaceship>();
            cShip.Initialize(0, Start, End - Start, _centerOfMass, parent, parts, resourceSystems);
            cShip.ConfigueRigidbody(Vector2.zero, 0);
            foreach (var part in parts)
                part.SetShip(cShip);

            return cShip;
        }

        public Spaceship LoadToEditor(string name, Transform parent)
        {
            Spaceship ship = new Spaceship(PartSaveData.Load(_parts, parent), new List<ResourceSystemID>(), End - Start);
            foreach (var part in ship.Parts) part.transform.position += part.GetExactScale() / 2;
            return ship;
        }

        private static bool TryGetBounds(List<PartSaveData> parts, out Vector2Int start, out Vector2Int end)
        {
            if (parts.Count == 0)
            {
                start = Vector2Int.zero;
                end = Vector2Int.zero;
                return false;
            }

            Vector2Int _start = parts[0].start;
            Vector2Int _end = parts[0].end;

            foreach (var part in parts)
            {
                if (part.start.x < _start.x) _start.x = part.start.x;
                if (part.start.y < _start.y) _start.y = part.start.y;
                if (part.end.x > _end.x) _end.x = part.end.x;
                if (part.end.y > _end.y) _end.y = part.end.y;
            }

            start = _start;
            end = _end;
            return true;
        }
    }

    internal class PartSaveData
    {
        public ObjectConfig Config { get; private set; }
        public PartTransform Transform { get; private set; }
        public List<ResourceSystemID> ResourceSystems { get; private set; }

        public Vector2Int start => Transform.Start;
        public Vector2Int end => Transform.End;

        public static PartSaveData Save(ShipPart part)
        {
            var data = new PartSaveData();

            data.Config = part.Config;
            data.Transform = PartTransform.Save(part);
            data.ResourceSystems = part.ResourceSystems;

            return data;
        }

        public static List<PartSaveData> Save(List<ShipPart> parts)
        {
            var output = new List<PartSaveData>();
            foreach (var part in parts) output.Add(Save(part));
            return output;
        }

        public static List<ShipPart> Load(List<PartSaveData> parts, Transform ship)
        {
            var output = new List<ShipPart>();

            foreach (var part in parts)
            {
                ShipPart lPart = part.Load(ship);

                if (ship.TryGetComponent(out Spaceship cShip))
                    lPart.transform.Translate(-cShip.Size);

                output.Add(lPart);
            }

            return output;
        }

        public ShipPart Load(Transform ship)
        {
            ShipPart output = Config.MakeObject(ship).GetComponent<ShipPart>();
            Transform.Load(output);
            foreach (var system in ResourceSystems)
                output.TryAddResourceSystem(system);
            return output;
        }

        public struct PartTransform
        {
            public PartOrientation Orientation;
            public Vector2Int Start;
            public Vector2Int End;

            public Vector2Int scale => End - Start;

            public static PartTransform Save(ShipPart part)
            {
                PartTransform output = new PartTransform();

                output.Orientation = part.Orientation;
                output.Start = part.Start;
                output.End = part.End;

                return output;
            }

            public void Load(ShipPart part)
            {
                part.transform.position = StaticMethods.Vector2IntToVector3((Start + End) / 2);
                part.Orient(Start, End, Orientation);
            }
        }
    }

    //using ChatGPT
    public class PartGraph<T> where T : MonoBehaviour, IShipPartBehaviour
    {
        private List<T> _parts;

        public PartGraph(List<T> parts) => _parts = parts.Where(part => part != null).ToList();

        public int PartsCount => _parts.Count;
        public List<T> Parts => new List<T>(_parts);

        public List<PartGraph<T>> Split()
        {
            var visited = new Dictionary<T, bool>();
            var components = new List<PartGraph<T>>();

            foreach (var obj in _parts)
                visited.Add(obj, false);

            foreach (var obj in _parts)
            {
                if (!visited[obj])
                {
                    var comp = new PartGraph<T>(new List<T>());
                    DFS(obj, visited.Keys.ToList(), visited, comp);
                    components.Add(comp);
                }
            }

            return components;
        }

        public void DFS(T obj, List<T> keys, Dictionary<T, bool> visited, PartGraph<T> component)
        {
            visited[obj] = true;
            component._parts.Add(obj);

            foreach (var neighbor in obj.PartComponent.Neighbors)
                if (keys.Where(c => c.PartComponent == neighbor).Count() >= 1)
                    if (!visited[neighbor.GetComponent<T>()])
                        DFS(neighbor.GetComponent<T>(), keys, visited, component);
        }
    }

    public enum ShipLoadMode
    {
        None,
        Editor,
        Level
    }
}