namespace Fhi.HelseId.Common.DPoP;

public struct DPoPClaimNames
{
    /// <summary>
    /// The value of the HTTP method (Section 9.1 of [RFC9110]) of the request to which the JWT is attached.
    /// https://datatracker.ietf.org/doc/html/rfc9449#section-4.2-4.4
    /// </summary>
    public const string HttpMethod = "htm";

    /// <summary>
    /// The HTTP target URI (Section 7.1 of [RFC9110]) of the request to which the JWT is attached, without query and fragment parts.
    /// https://datatracker.ietf.org/doc/html/rfc9449#section-4.2-4.6
    /// </summary>
    public const string HttpUrl = "htu";

    /// <summary>
    /// DPoP access token hash
    /// </summary>
    public const string AccessTokenHash = "ath";

    /// <summary>
    /// The confirmation
    /// </summary>
    public const string Confirmation = "cnf";

    /// <summary>
    /// Values for strongly typed JWTs
    /// </summary>
    public struct JwtTypes
    {
        /// <summary>
        /// OAuth 2.0 access token
        /// </summary>
        public const string AccessToken = "at+jwt";

        /// <summary>
        /// DPoP proof token
        /// </summary>
        public const string DPoPProofToken = "dpop+jwt";
    }

    /// <summary>
    /// Values for the cnf claim
    /// </summary>
    public struct ConfirmationMethods
    {
        /// <summary>
        /// JSON web key thumbprint
        /// </summary>
        public const string JwkThumbprint = "jkt";
    }
}
