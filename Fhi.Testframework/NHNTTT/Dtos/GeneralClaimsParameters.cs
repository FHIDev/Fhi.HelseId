namespace Fhi.TestFramework.NHNTTT.Dtos
{
    public record GeneralClaimsParameters(ICollection<string> Scope,
        string? ClientId,
        string? SfmJournalId,
        string? OrgnrParent,
        string? OrgnrChild,
        string? OrgnrSupplier,
        bool? ClientTenancy,
        ICollection<string>? AuthenticationMethodsReferences,
        string? ClientAuthenticationMethodsReferences,
        string? ClientName,
        string? Jti,
        string? CnfJkt,
        string? CnfPublicKey)
    {
        public GeneralClaimsParameters(ICollection<string> scope) : this(scope, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null, null, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }
    }
}
