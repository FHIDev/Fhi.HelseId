namespace Fhi.HelseId.Common.Identity;

public static class ClientClaims
{
    public const string Prefix = HelseIdUriPrefixes.Claims + "client/";

    public const string ClientName = Prefix + "client_name";
}

public static class IdentityClaims
{
    public const string Prefix = HelseIdUriPrefixes.Claims + "identity/";

    public const string AssuranceLevel = Prefix + "assurance_level";
    public const string Pid = Prefix + "pid";
    public const string PidPseudonym = Prefix + "pid_pseudonym";
    public const string SecurityLevel = Prefix + "security_level";
    public const string Network = Prefix + "network";
    public const string Name = "name";
    public const string Givenname = "given_name";
    public const string Familyname = "family_name";
    public const string Scopes = "scope";
}
