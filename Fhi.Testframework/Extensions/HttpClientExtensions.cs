using System.Net.Http.Headers;

namespace Fhi.TestFramework.Extensions
{
    public static class HttpClientExtensions
    {
        public static HttpClient AddBearerAuthorizationHeader(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token);

            return client;
        }
    }
}
