using System;
using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Web.Handlers;

namespace Fhi.HelseId.Web;

public interface IHelseIdHprFeatures
{
    bool IncludeHprNumber { get; }
    bool RequireHprNumber { get; }
    bool RequireValidHprAuthorization { get; }
}

public interface IHelseIdWebKonfigurasjon : IHelseIdHprFeatures, IHelseIdClientKonfigurasjon
{
    string[] SecurityLevels { get; }
    bool UseProtectedPaths { get; set; }
    RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; set; }
    ApiOutgoingKonfigurasjon[] Apis { get; set; }
    bool UseApis { get; }
    NoAuthenticationUser NoAuthenticationUser { get; }

    /// <summary>
    /// Enables DPoP support in the authorizaiton code flow.
    /// </summary>
    public bool UseDPoPTokens { get; }
}


public class HelseIdWebKonfigurasjon : HelseIdClientKonfigurasjon, IHelseIdWebKonfigurasjon
{
    private bool _useHprNumber = false;
    private bool _requireValidHprAuthorization = false;

    public string DevelopmentRedirectUri { get; set; } = "/";

    public string[] SecurityLevels { get; set; } = ["3", "4"];

    protected override IEnumerable<string> FixedScopes
    {
        get
        {
            var list = new List<string>
            {
                "openid",
                "profile",
                "helseid://scopes/identity/pid",
                "helseid://scopes/identity/pid_pseudonym",
                "helseid://scopes/identity/security_level"
            };
            if (IncludeHprNumber || RequireHprNumber || RequireValidHprAuthorization)
            {
                list.Add("helseid://scopes/hpr/hpr_number");
            }
            list.AddRange(base.FixedScopes);
            return list;
        }
    }

    public bool IncludeHprNumber { get; set; } = false;

    public bool RequireHprNumber
    {
        get => _useHprNumber;
        set
        {
            if (value)
            {
                IncludeHprNumber = value;
            }
            _useHprNumber = value;
        }
    }

    public bool RequireValidHprAuthorization
    {
        get => _requireValidHprAuthorization;
        set
        {
            if (value)
            {
                IncludeHprNumber = value;
                RequireHprNumber = value;
            }
            _requireValidHprAuthorization = value;
        }
    }

    public bool UseProtectedPaths { get; set; } = true;

    public bool UseApis => Apis.Any();

    public RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; set; } = new();

    public int Validate()
    {
        throw new NotImplementedException();
    }

    public ApiOutgoingKonfigurasjon[] Apis { get; set; } = Array.Empty<ApiOutgoingKonfigurasjon>();

    public NoAuthenticationUser NoAuthenticationUser { get; set; } = new();

    public bool UseDPoPTokens { get; set; }

    public Uri UriToApiByName(string name)
    {
        var url = Apis.FirstOrDefault(o => o.Name == name)?.Url ?? throw new InvalidApiNameException(name);
        return new Uri(url);
    }
}