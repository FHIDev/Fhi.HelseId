namespace Fhi.TestFramework.NHNTTT.Dtos
{
    public record DPoPProofParameters
    {
        public string? HtuClaimValue { get; set; }

        public string? HtmClaimValue { get; set; }

        public string? PrivateKeyForProofCreation { get; set; }

        //// Removed the implementation as it was wrong. This is a property in TTT that
        //// will be reimplemented correct when tests for Dpop is in place
        //// public InvalidDPoPProofParameters? InvalidDPoPProofParameters { get; set; }
    }
}
