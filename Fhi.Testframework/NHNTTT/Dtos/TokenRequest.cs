namespace Fhi.TestFramework.NHNTTT.Dtos
{
    public class TokenRequest(string audience)
    {
        public string? Audience { get; set; } = audience;

        public ParametersGeneration? ClientClaimsParametersGeneration { get; set; }

        public ParametersGeneration? UserClaimsParametersGeneration { get; set; }

        public bool? WithoutDefaultGeneralClaims { get; set; }

        public bool? WithoutDefaultUserClaims { get; set; }

        public bool? CreateDPoPTokenWithDPoPProof { get; set; }

        public bool? CreateTillitsrammeverkClaims { get; set; }

        public bool? SignJwtWithInvalidSigningKey { get; set; }

        public bool? SetInvalidIssuer { get; set; }

        public bool? SetInvalidAudience { get; set; }

        public bool? GetPersonFromPersontjenesten { get; set; }

        public bool? OnlySetNameForPerson { get; set; }

        public bool? GetHprNumberFromHprregisteret { get; set; }

        public bool? SetPidPseudonym { get; set; }

        public bool? SetSubject { get; set; }

        public ExpirationParameters? ExpirationParameters { get; set; }

        public HeaderParameters? HeaderParameters { get; set; }

        public GeneralClaimsParameters? GeneralClaimsParameters { get; set; }

        public UserClaimsParameters? UserClaimsParameters { get; set; }

        public TillitsrammeverkClaimsParameters? TillitsrammeverkClaimsParameters { get; set; }

        public DPoPProofParameters? DPoPProofParameters { get; set; }

        public ICollection<ApiSpecificClaim>? ApiSpecificClaims { get; set; }
    }
}
