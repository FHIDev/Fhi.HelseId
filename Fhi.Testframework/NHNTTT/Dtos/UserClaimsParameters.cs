namespace Fhi.TestFramework.NHNTTT.Dtos
{
    public record UserClaimsParameters
    {
        public string? Pid { get; set; }

        public string? PidPseudonym { get; set; }

        public string? HprNumber { get; set; }

        public string? Name { get; set; }

        public string? GivenName { get; set; }

        public string? MiddleName { get; set; }

        public string? FamilyName { get; set; }

        public string? IdentityProvider { get; set; }

        public string? SecurityLevel { get; set; }

        public string? AssuranceLevel { get; set; }

        public string? Network { get; set; }

        public string? Amr { get; set; }

        public string? Subject { get; set; }

        public string? Sid { get; set; }
    }
}
