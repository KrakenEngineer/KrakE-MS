namespace MSEngine.Utility
{
    public interface IResource {}

    [System.Serializable]
    public struct Resource : IResource
    {
        public float Amount;
        public string ID;

        public Resource(string id, float amount)
        {
            Amount = amount;
            ID = id;
        }

        public Resource(Resource res)
        {
            Amount = res.Amount;
            ID = res.ID;
        }

        public static Resource Void => new Resource("", 0);

        public override string ToString()
        {
            return $"{Amount} {ID}";
        }

        public static Resource operator +(Resource left, Resource right)
        {
            float amount = left.Amount + right.Amount;

            if (left.ID != Void.ID && (left.ID == right.ID || right.ID == Void.ID))
                return new Resource(left.ID, amount);

            else if (left.ID == Void.ID && right.ID != Void.ID)
                return new Resource(right.ID, amount);

            else return Void;
        }

        public static Resource operator *(Resource resource, float number)
        {
            float amount = resource.Amount * number;
            return new Resource(resource.ID, amount);
        }

        public static bool operator ==(Resource left, Resource right)
        {
            bool amountEquality = left.Amount == right.Amount;
            return (left.ID == right.ID) && amountEquality;
        }

        public static bool operator !=(Resource left, Resource right) => !(left == right);
    }

    [System.Serializable]
    public struct SolidResource : IResource
    {
        public int Amount;
        public string ID;

        public SolidResource(string id, int amount)
        {
            Amount = amount;
            ID = id;
        }

        public SolidResource(SolidResource res)
        {
            Amount = res.Amount;
            ID = res.ID;
        }

        public static SolidResource Void => new SolidResource("", 0);

        public override string ToString()
        {
            return $"{Amount} {ID}";
        }

        public static SolidResource operator +(SolidResource left, SolidResource right)
        {
            int amount = left.Amount + right.Amount;

            if (left.ID != Void.ID && (left.ID == right.ID || right.ID == Void.ID))
                return new SolidResource(left.ID, amount);

            else if (left.ID == Void.ID && right.ID != Void.ID)
                return new SolidResource(right.ID, amount);

            else return Void;
        }

        public static SolidResource operator *(SolidResource resource, int number)
        {
            int amount = resource.Amount * number;
            return new SolidResource(resource.ID, amount);
        }

        public static bool operator ==(SolidResource left, SolidResource right)
        {
            bool amountEquality = left.Amount == right.Amount;
            return (left.ID == right.ID) && amountEquality;
        }

        public static bool operator !=(SolidResource left, SolidResource right) => !(left == right);
    }
}