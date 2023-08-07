using MSEngine.Utility;
using System.Collections.Generic;

namespace MSEngine.Spaceships
{
    public interface IShipPartBehaviour
    {
        public ShipPart PartComponent { get; }
    }

    public abstract class ResourceRelated : UnityEngine.MonoBehaviour, IShipPartBehaviour
    {
        public abstract List<Resource> Consumption { get; }
        public abstract List<Resource> Output { get; }

        public abstract List<string> RelatedTo { get; }

        public abstract ShipPart PartComponent { get; }
    }

    public abstract class Storage<T> : ResourceRelated where T : struct, IResource
    {
        public abstract T GetContent();

        public abstract bool HasSpace(T resource);
        public abstract bool HasResource(T resource);

        public abstract bool TryAddResource(T resource);
        public abstract bool TryRemoveResource(T resource);
               
        public abstract void Clear();
    }
}