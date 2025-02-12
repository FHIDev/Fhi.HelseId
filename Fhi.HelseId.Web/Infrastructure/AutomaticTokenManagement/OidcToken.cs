using System;
using System.Net;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public record OidcToken
    {
        public OidcToken(HttpStatusCode httpStatusCode, string accessToken, string refreshToken, DateTimeOffset expiresAt, string json)
        {
            HttpStatusCode = httpStatusCode;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            IsError = false;
            ErrorDescription = "";
            Json = json;
        }

        public OidcToken(HttpStatusCode httpStatusCode, string errorDescription, string json)
        {
            HttpStatusCode = httpStatusCode;
            AccessToken = "";
            RefreshToken = "";
            ExpiresAt = DateTimeOffset.MinValue;
            IsError = true;
            ErrorDescription = errorDescription;
            Json = json;
        }

        public HttpStatusCode HttpStatusCode { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsError { get; set; }
        public string ErrorDescription { get; set; }
        public string? Json { get; set; }
    }
}
