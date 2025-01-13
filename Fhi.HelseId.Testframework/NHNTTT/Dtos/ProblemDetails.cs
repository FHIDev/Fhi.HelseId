namespace Fhi.TestFramework.NHNTTT.Dtos
{
    internal record ProblemDetails
    {
        public string? Type { get; set; }

        public string? Title { get; set; }

        public int? Status { get; set; }

        public string? Detail { get; set; }

        public string? Instance { get; set; }

        private IDictionary<string, object>? _additionalProperties;

        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }
    }
}
