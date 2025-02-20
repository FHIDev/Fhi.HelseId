namespace Fhi.TestFramework.NHNTTT.Dtos
{
    public record ExpirationParameters
    {
        public bool? SetExpirationTimeAsExpired { get; set; }

        public int? ExpirationTimeInSeconds { get; set; }

        public int? ExpirationTimeInDays { get; set; }
    }
}
