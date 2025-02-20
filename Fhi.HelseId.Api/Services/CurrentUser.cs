using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Api.Services;

/// <summary>
/// For Api only
/// </summary>
public interface ICurrentUser
{
    string? Id { get; }
    string? HprNummer { get; }
    string? Name { get; }
    string? Pid { get; }
    public string? PidPseudonym { get; }
    public IEnumerable<string> Scopes { get; }
    string Fornavn { get; set; }
    string Etternavn { get; set; }
}

/// <summary>
/// For Api access only. For web backends use CurrentWebUser
/// Ensure your config specifies "RequireContextIdentity" : true
/// </summary>
public class CurrentHttpUser : ICurrentUser
{
    public CurrentHttpUser(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        Id = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value ?? "";
        HprNummer = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimsPrincipalExtensions.HprNummer)
            ?.Value ?? "";
        Fornavn = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Givenname)?.Value ?? "";
        Etternavn = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Familyname)?.Value ?? "";
        Pid = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value ?? "";
        PidPseudonym = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.PidPseudonym)?.Value ?? "";
        Scopes = httpContext.User.FindAll("scope").Select(o => o.Value).ToList();
        ClientName = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClientClaims.ClientName)?.Value ?? "";
    }

    public string Id { get; }
    public string HprNummer { get; }
    public string Name => $"{Fornavn} {Etternavn}";
    public string Pid { get; }
    public string PidPseudonym { get; }
    public IEnumerable<string> Scopes { get; }

    public string Fornavn { get; set; }
    public string Etternavn { get; set; }

    public string ClientName { get; set; }
}
