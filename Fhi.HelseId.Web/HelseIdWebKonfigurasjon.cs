﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Web.Handlers;

namespace Fhi.HelseId.Web;

public interface IHelseIdHprFeatures
{
    bool IncludeHprNumber { get; }
    bool UseHprNumber { get; }
    bool UseHprPolicy { get; }
    string HprUsername { get; set; }
    string HprPassword { get; set; }
    string HprUrl { get; set; }
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


[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
public class HelseIdWebKonfigurasjon : HelseIdClientKonfigurasjon, IHelseIdWebKonfigurasjon
{
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
            if (IncludeHprNumber || UseHprNumber || UseHprPolicy)
            {
                list.Add("helseid://scopes/hpr/hpr_number");
            }
            list.AddRange(base.FixedScopes);
            return list;
        }
    }

    public bool IncludeHprNumber { get; set; } = false;
    public bool UseHpr { get; set; } = false;
    public bool UseHprNumber { get; set; } = false;
    public bool UseHprPolicy { get; set; } = false;

    public string HprUsername { get; set; } = "";
    public string HprPassword { get; set; } = "";

    public string HprUrl { get; set; } = "";

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