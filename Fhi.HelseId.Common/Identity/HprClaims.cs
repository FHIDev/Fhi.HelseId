using System.Linq;
using System.Security.Claims;

namespace Fhi.HelseId.Common.Identity;

public static class ClaimsPrincipalExtensions
{
    private const string Prefix = HelseIdUriPrefixes.Claims + "hpr/";
    public const string HprNummer = Prefix + "hpr_number";
    public const string HprDetails = "helseid://claims/hpr/hpr_details";

    public static string? HprNumber(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == HprNummer)?.Value;
    public static string? Id(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
    public static string? Name(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Name)?.Value;
    public static string? PidPseudonym(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == IdentityClaims.PidPseudonym)?.Value;
    public static string? Pid(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
    public static string? SecurityLevel(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == IdentityClaims.SecurityLevel)?.Value;
    public static string? AssuranceLevel(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == IdentityClaims.AssuranceLevel)?.Value;
    public static string? Network(this ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Network)?.Value;
}