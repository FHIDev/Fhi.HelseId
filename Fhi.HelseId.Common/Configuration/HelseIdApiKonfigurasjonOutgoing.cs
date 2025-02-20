using System;

namespace Fhi.HelseId.Common.Configuration;

public interface IApiOutgoingKonfigurasjon
{
    string Name { get; set; }
    string Url { get; set; }
    string Scope { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    Uri Uri { get; }

    /// <summary>
    /// Default 10 minutes
    /// </summary>
    int Timeout { get; set; } // TODO: How is this default 10 minutes?
}

/// <summary>
/// This is the schema for the appsetting for outgoing APIs. Version 5
/// </summary>
public class ApiOutgoingKonfigurasjon : IApiOutgoingKonfigurasjon
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";

    public string Scope { get; set; } = "";

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Uri Uri => new(Url);

    public int Timeout { get; set; } = 10;  // Default 10 minutes
}
