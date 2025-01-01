namespace ParkingSystem.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomRateLimitAttribute : Attribute
    {
        public string Period { get; }
        public int Limit { get; }

        public CustomRateLimitAttribute(string period, int limit)
        {
            Period = period;
            Limit = limit;
        }
    }
}
