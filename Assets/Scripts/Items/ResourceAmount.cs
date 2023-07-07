namespace MSEngine.Utility
{
    [System.Serializable]
    public struct ResourceAmount
    {
        public float Amount;
        public string ID;

        public ResourceAmount(string id, float amount)
        {
            Amount = amount;
            ID = id;
        }

        public ResourceAmount(ResourceAmount amount)
        {
            Amount = amount.Amount;
            ID = amount.ID;
        }

        public ResourceAmount(string _string)
        {
            Amount = System.Convert.ToSingle(_string.Split()[1]);
            ID = _string.Split()[0];
        }

        public static ResourceAmount Void => new ResourceAmount("", 0);

        public override string ToString()
        {
            return $"{Amount} {ID}";
        }

        public static ResourceAmount operator +(ResourceAmount amount1, ResourceAmount amount2)
        {
            if (amount1.ID == amount2.ID) return new ResourceAmount(amount1.ID, amount1.Amount + amount2.Amount);
            else return new ResourceAmount(amount1);
        }

        public static ResourceAmount operator *(ResourceAmount amount, float number)
        {
            return new ResourceAmount(amount.ID, amount.Amount * number);
        }

        public static bool operator ==(ResourceAmount res1, ResourceAmount res2)
        {
            return (res1.ID == res2.ID) && (res1.Amount == res2.Amount);
        }

        public static bool operator !=(ResourceAmount res1, ResourceAmount res2)
        {
            return (res1.ID != res2.ID) || (res1.Amount != res2.Amount);
        }
    }
}