﻿using System.Text;
using System.Text.Json;
using Fhi.TestFramework.Extensions;
using Fhi.TestFramework.NHNTTT.Dtos;

namespace Fhi.TestFramework.NHNTTT;

/// <summary>
/// Test token tjeneseten (TTT) https://utviklerportal.nhn.no/informasjonstjenester/helseid/tilgang-til-helseid/test-token-tjenesten/docs/test-token-tjenesten_no_nnmd/
/// See swagger https://helseid-ttt.test.nhn.no/swagger/index.html#/TokenService
/// </summary>
internal static class TTTTokenService
{
    /// <summary>
    /// Get a test token from NHN Test-token-tjenesten
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    internal static async Task<string> GetHelseIdToken(TokenRequest request)
    {
        return await CreateTestTokenWithApiKey_V2(request);
    }

    /// <summary>
    /// Request access token using endpoint v2/create-test-token-with-key
    /// </summary>
    /// <param name="tokenRequest">Request describing the properties of the token</param>
    /// <param name="baseUri">uri to NHN Test token tjeneste </param>
    /// <param name="authKey">Generated key to allow requesting test tokens</param>
    /// <returns></returns>
    internal static async Task<string> CreateTestTokenWithApiKey_V2(TokenRequest tokenRequest,
        string baseUri = "https://helseid-ttt.test.nhn.no",
        string authKey = "1814d016-9ffe-4334-a6e1-94eff536360b")
    {
        HttpClient client = new() { BaseAddress = new Uri(baseUri) };
        client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);

        var content = tokenRequest.Serialize();
        var response = await client.PostAsync("v2/create-test-token-with-key", new StringContent(content, Encoding.UTF8, "application/json"));
        var responseString = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = responseString.Deserialize<TokenResponse>();
            return tokenResponse?.SuccessResponse.AccessTokenJwt ?? string.Empty;
        }

        response.EnsureSuccessStatusCode();
        return responseString;
    }

}