using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Hpr.Core;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Web.Services;

/// <summary>
/// For Web backend
/// </summary>
public interface ICurrentUser
{
    string? Id { get; }
    string? Name { get; }
    string? HprNummer { get; }
    List<OId9060> HprGodkjenninger { get; }
    bool ErHprGodkjent { get; }
    string? PidPseudonym { get; }
    string? Pid { get; }
    string? SecurityLevel { get; }
    string? AssuranceLevel { get; }
    string? Network { get; }
}

/// <summary>
/// For Web backend
/// </summary>
public class CurrentHttpUser : ICurrentUser
{
    public CurrentHttpUser(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext
                ?? throw new NoHttpContextException($"{nameof(CurrentHttpUser)}.ctor : No HttpContext found. This has to be called when there is a request");
        Id = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
        
        HprNummer = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimsPrincipalExtensions.HprNummer)?.Value;
        var hprDetailsClaim = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimsPrincipalExtensions.HprDetails);
        // TODO: This is duplicated in UserByIdentity
        if (hprDetailsClaim != null)
        {
            var approvalResponse = JsonSerializer.Deserialize<ApprovalResponse>(hprDetailsClaim.Value);
            if (approvalResponse == null)
            {
                throw new HprClaimMissingException("HprDetails claim missing or could not be deserialized.");
            }
            HprGodkjenninger = approvalResponse.Approvals
                .SelectMany(approval => Kodekonstanter.KodeList
                    .Where(oid9060 => approval.Profession == oid9060.Value)).ToList();
            ErHprGodkjent = approvalResponse.Approvals.Any();
        }

        Name = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Name)?.Value;
        Pid = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
        PidPseudonym = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.PidPseudonym)?.Value;
        SecurityLevel = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.SecurityLevel)?.Value;
        AssuranceLevel = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.AssuranceLevel)?.Value;
        Network = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Network)?.Value;
    }

    public string? Id { get; }
    public string? Name { get; }
    public string? HprNummer { get; }
    public List<OId9060> HprGodkjenninger { get; } = new List<OId9060>();
    public bool ErHprGodkjent { get; }
    public string? PidPseudonym { get; }
    public string? Pid { get; }
    public string? SecurityLevel { get; }
    public string? AssuranceLevel { get; }
    public string? Network { get; }
}

public class NoHttpContextException : Exception
{
    public NoHttpContextException()
    {
    }

    public NoHttpContextException(string message) : base(message)
    {
    }

    public NoHttpContextException(string message, Exception inner) : base(message, inner)
    {
    }
}