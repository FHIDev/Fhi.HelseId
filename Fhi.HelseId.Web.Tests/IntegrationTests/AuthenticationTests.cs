﻿using System.Collections.Specialized;
using System.Net;
using System.Web;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Common;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.IntegrationTests.Setup;
using Fhi.HelseId.Web.OIDC;
using Fhi.HelseId.Web.Services;
using Fhi.TestFramework;
using Fhi.TestFramework.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using NUnit.Framework;
using Fhi.HelseId.Common.ExtensionMethods;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using System.Text;

namespace Fhi.HelseId.Web.IntegrationTests
{
    /// <summary>
    /// The purpose of these tests is to test the buildt in OIDC authentication scheme in AddHelseIdWebAuthentication
    /// </summary>
    public class AuthenticationTests
    {
        [Test]
        public async Task DefaultHelseIdConfiguration_NoAuthCookieOnApiCall_Return401WithLocationToRedirectToIdentityProvider()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create
                .Default()
                .CreateConfigurationRoot();

            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(config)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(config)
                   .Build();
                })
                .BuildApp(UseEndpointAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/testauthentication");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location!.Query);
            AssertStandardScopes(queryParams);
            Assert.That(queryParams["redirect_uri"], Is.EqualTo("http://localhost/signin-callback"));
        }

        [Test]
        public async Task ScopeConfigured_NoAuthCookieOnApiCall_Return401WithLocationToRedirectToIdentityProviderAndConfiguredScopes()
        {
            var helseIdConfig = HelseIdWebKonfigurasjonBuilder.Create
                .Default();
            helseIdConfig.Scopes = ["scope1", "scope2"];
            var config = helseIdConfig.CreateConfigurationRoot();

            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(config)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(config)
                   .Build();
                })
                .BuildApp(UseEndpointAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/testauthentication");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location!.Query);
            AssertStandardScopes(queryParams);
            Assert.That(queryParams["scope"], Does.Contain("scope1 scope2"));
        }

        [Test]
        public async Task IncludeHprNumber_NoAuthCookieOnApiCall_Return401WithLocationToRedirectToIdentityProviderAndHprNumberScope()
        {
            var helseIdConfig = HelseIdWebKonfigurasjonBuilder.Create
                .Default()
                .WithIncludeHprNumber(true);

            var config = helseIdConfig.CreateConfigurationRoot();

            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(config)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(config)
                   .Build();
                })
                .BuildApp(UseEndpointAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/testauthentication");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location!.Query);
            AssertStandardScopes(queryParams);
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/hpr/hpr_number"));
        }

        [Test]
        public async Task UseJwkKeySecretHandler()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "true" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" },
                { "HelseIdWebKonfigurasjon:ClientId", "66d474a8-07df-4dc4-abb0-6b759c2b99aa" },
                { "HelseIdWebKonfigurasjon:ClientSecret", $"{{\"d\":\"Q4x8XiZ3JKn0-ijW-H9plfw7QF4VLK43jHxYtPJvX6GcBuEk_rMedziQuqbBCZrK6aWVspnYS6dQtj33Z2TtSkXu2gy_1xR2nR8h9XeZ6h6QRbL9bj1Qxrk70ry7bXz5WIjyyuPmY73aPw9OFrZ_NDeUQjiEofzTHkr86ZIVjAmNLarVufG9P2V6fz14wwHc3aLBVgUt7Rxx5sFOQR30zYGpd1BH-xK6ykA6n6BdaIc4luWw_SkmVowwO4toScj07qoAYTUR4IFQHYt7sQZNufFG89nB-v_Er0a2tRvtME2NnU_4rn4ea1yyGFlYH_6Amtb8u4-TAeOESjrMw9ylBkvb6vIvtqT0lQdBJJEPI_Hx-655ElvO4zT48HBS6oVZHCARN17d7pQWrnxiSusYEdM9RwJET57ieVayo-baQe3NOvj2Y5V2H034cWCJt_DTh7ye9RXD4gtMnHDQ-tgV6ztwW8GkGvbJzXUnkqGXUvKqjeJAnOc2Ahoxpc-9cnMnW2DrwPnI0f9Jsq0n3hQyqwnnyimIeZn32WVe2Q4XC7d_VB21E8oDZhdeUlxuTZX-foTrYB3xvDKB6tLCaaMbfpzvUsSfSYqbAXQfqhQWosyt7w-ZIYJOY05fWspR3mlpo5IMGkaDp8clvz51f8zdMfSYFTml4e_zjoduvlz2wyE\",\"dp\":\"GsxR4BGYEb460zNcX1_SROtq2zVG8IfYVFy-3pFQvmerfiRJr0uuWvZ1WCtqalXKV33ACdf5njmkKdA-z-RbH07axt52b8SQZOTQ8527i5p_zJ6QGp6Lw4iuepAX64T_POtqmUDKcusIOGxxZbC_SjVr7dtYHVWuqPZlNjFbqQpWcerQybQvsyBeVDzDYkcdM7dhW8wXuIPDukU0zkgKBvW-23LR6dN9t7zh3suLdhkaV381-lODAU_U02-wIhXwDcHKi_8a1dMtMKQFxKyngp0d3P8R5hDT71UOttD0zMxt6HB_c6cwLmOYPclYXsK0-bIIDgJq7rERIA6KF0GqaQ\",\"dq\":\"CSI90DDVZFFj0DWpUXcpWjH5NOP7Yca9dTeFkWqnhmpS_XOMNvCLa9pyTO7o-Iw-aRbVhlpyIQN4pSdMmjnW5eEpBg8zsd8LcV8gkv9sL08bL-8dWcqy5kD9pgBEK49HgiobTWpdKd02PErgXbY28hWRx4JafVRjk7PkRXD4yJjK_qJ-fwlY51K0ynAIX4L7C14LW7AVYp4QkRjaHd_O1CRorVijR_N_sEvfa1jfZHNtBmgaUbJxn_4rYVZfehu5nbqoXLB4VqJBwVf26rNyT-fxMbAW4OH0ubjWrcTCedfAIMMegbXG7cHxrrbAL-50PggVWWjxbKAt0gBoN5KNIQ\",\"e\":\"AQAB\",\"kid\":\"-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4\",\"kty\":\"RSA\",\"n\":\"iu9EmQLoIJBPBqm3jLYW4oI8yLkOxvKg-OagE8HlzP-RQnDXH9hBe2cTRZ3oNqG1viWmv6-dxNtKU1QxOpezWLx-N-AJ7dIlXTMUGkCheHUorPSzakeBUOCHtvT1Tdv9Mzue9fVt3JxpPX6mQNlsOzwk9L8HmbgojMcApKmQcfNriVV72byLuaAoh9fcXSNm6TUuwO5cPmnHgS5B5Hfe5P0OIte027oZyjPiYm-QbV4YJNjwwwZnPvkLaRjw6L8sV5TAOLvNQIt63OpF8UHPjBsM8LJHdHFUMgx2BaMaJC8tNCi_8UWGG59sd4-_vJC78s3wZNEGL6OwCngpF7NLwaP9Zqxx8DDkOY71MvvcAyu4i0D6_8A8_qewLvb_SPxNpCe8zH5MJIKNJB38InWd8FpvpbPuEJt4oK1gfUBWLWQ39YIHzodKhkN-qAXYWGyzJ2nJdNIMAclefw251Cvjcyf3gmVATXDBAo-piUJIGXC3y7yqfyMupe_4oRe69DFBZTecXSLEdbAbUtiaH9r4rY5oeYCiZ70wcFcieHFZLwfleCPm5Cz8rEQxK8KjMis2kb1aRxVytTj_0pOkw1HEJU1tv_TWmD136RgoRtiqnVoxmCM6Q4XxXrOnGMPZR0_ScYHdW_YjDgnJBQykAbzW0nC47d3KSotktz1cPejo5_s\",\"p\":\"wSqXfloa5ikf6d_G3N2x-IBjTEB7mibEif9qTNED7x4f7J8_vB_rdrsxoTCdY3R5j2BIS3XKyiTIkonfV5mYQvSu9RwxgX4ImcTsTXMxewuyQ031OJz0ruvlgvHELzdgkyo4q4NQaDigejfp_DsoEM0nLHeNXYfTz_AuTlkG8BA0JzrBK0aoBPUdwB1_WoJoJYVhST-B_PHQ3eaFOQNGXQXoYc6YZt4WKmu3WModjezdqnKVKSaORTuG5-mTQLS2jxZr1XlEDXWN53tH_Oon2WMDGbSCVz_qYVZUWorbjvUmxJnYP0H-lIxLRfxYE68DnYlXSWOzcfpdD6VIlP972w\",\"q\":\"uCCszPIHfiHe5UOa0poW8WELpL_YItCdK_AnwulRHOM2FIQbmWBVZBMRguQJuMCvjIAWdQNEOrZYt1BhIknziUHSvSnLU0qszJ_ZHByZS54834CIxc-0etVkKbpnJGpjzgucvmEPNzrUko2ip512iOY9WTqB54Awg56rS-3oBw2_iqEzioAI30pD9-AX5xDRBEJcRJ8mPxw0iSDGVkxwIQNLBlML79cGNGjdrzsJyjPuMMDTLadHnRSAybmqhVTJYwEH5t37_f_fho2aoPu_sW65LdoW6Z-V042xnieMm3XhA8yZxonao2LH6Bh7Xk7qinWYpYuF6UpKqhtrpI8OYQ\",\"qi\":\"Z7GWLxO3q2BolVFaOjuskhYc0V4GZ-b-SA2Rv110HovMDatlUGZKgoAOponFcvEddJSNf7stRM35HWiTN9W5iSUP4VLqAlJ2W7ftIsRJv0D-Lcs4HoXTAHcny36j0eEzhNLUYwfS9Y7ICWEWHv_WTG8Iz_I87JKLCnjGutZDmsM_fHDPmUkv7Pf2GG9r9hzS8uylC43ik4gfrp0Hm_6rAHKB4EHVHfYu51zl9yLkPgqq8ycHi0tF7VmVtDUsIMJdz7nlGOCS-468WI95dAfdfTC8v9JKXj9JL3ylM3dDbiC3m0p-rpaM2VzuO4OrMk-jWFCuYbDCYS-bcYFG4XwmtA\"}}" }
            };
            var config = appsettingsConfig.BuildInMemoryConfiguration();
            var builder = WebApplicationBuilderTestHost.CreateWebHostBuilder()
             .WithServices(services =>
             {
                 services.AddSingleton<FakeOIDCEvents>();
                 services.AddHelseIdWebAuthentication(config)
                 .UseJwkKeySecretHandler()
                 .Build(mvcOptions =>
                 {
                 }, new ConfigureAuthentication()
                 {
                     ConfigureOpenIdConnect = (options) =>
                     {
                         options.EventsType = typeof(FakeOIDCEvents);
                         string metadata = "{\"issuer\":\"https://helseid-sts.test.nhn.no\",\"jwks_uri\":\"https://helseid-sts.test.nhn.no/.well-known/openid-configuration/jwks\",\"authorization_endpoint\":\"https://helseid-sts.test.nhn.no/connect/authorize\",\"token_endpoint\":\"https://helseid-sts.test.nhn.no/connect/token\",\"userinfo_endpoint\":\"https://helseid-sts.test.nhn.no/connect/userinfo\",\"end_session_endpoint\":\"https://helseid-sts.test.nhn.no/connect/endsession\",\"check_session_iframe\":\"https://helseid-sts.test.nhn.no/connect/checksession\",\"revocation_endpoint\":\"https://helseid-sts.test.nhn.no/connect/revocation\",\"introspection_endpoint\":\"https://helseid-sts.test.nhn.no/connect/introspect\",\"device_authorization_endpoint\":\"https://helseid-sts.test.nhn.no/connect/deviceauthorization\",\"backchannel_authentication_endpoint\":\"https://helseid-sts.test.nhn.no/connect/ciba\",\"pushed_authorization_request_endpoint\":\"https://helseid-sts.test.nhn.no/connect/par\",\"require_pushed_authorization_requests\":false,\"frontchannel_logout_supported\":\"true\",\"frontchannel_logout_session_supported\":\"true\",\"backchannel_logout_supported\":\"true\",\"backchannel_logout_session_supported\":\"true\",\"claims_supported\":[\"name\",\"family_name\",\"given_name\",\"middle_name\",\"helseid://claims/identity/assurance_level\",\"helseid://claims/identity/pid\",\"helseid://claims/identity/pid_pseudonym\",\"helseid://claims/identity/security_level\",\"helseid://claims/identity/network\",\"helseid://claims/hpr/hpr_number\",\"helseid://claims/client/client_name\",\"helseid://claims/client/client_tenancy\",\"helseid://claims/client/claims/orgnr_parent\",\"helseid://claims/client/claims/orgnr_child\",\"helseid://claims/client/claims/orgnr_supplier\",\"client_amr\"],\"scopes_supported\":[\"openid\",\"profile\",\"offline_access\",\"helseid://scopes/client/info\",\"helseid://scopes/client/client_name\",\"helseid://scopes/identity/assurance_level\",\"helseid://scopes/identity/pid\",\"helseid://scopes/identity/pid_pseudonym\",\"helseid://scopes/identity/security_level\",\"helseid://scopes/identity/network\",\"nhn:tillitsrammeverk:parameters\",\"nhn:sfm:journal-id\",\"helseid://scopes/hpr/hpr_number\"],\"grant_types_supported\":[\"authorization_code\",\"client_credentials\",\"refresh_token\",\"implicit\",\"urn:ietf:params:oauth:grant-type:device_code\",\"urn:openid:params:grant-type:ciba\",\"urn:ietf:params:oauth:grant-type:token-exchange\"],\"response_types_supported\":[\"code\",\"token\",\"id_token\",\"id_token token\",\"code id_token\",\"code token\",\"code id_token token\"],\"response_modes_supported\":[\"form_post\",\"query\",\"fragment\"],\"token_endpoint_auth_methods_supported\":[\"client_secret_basic\",\"client_secret_post\",\"private_key_jwt\"],\"id_token_signing_alg_values_supported\":[\"RS256\"],\"subject_types_supported\":[\"public\"],\"code_challenge_methods_supported\":[\"plain\",\"S256\"],\"request_parameter_supported\":true,\"request_object_signing_alg_values_supported\":[\"RS256\",\"RS384\",\"RS512\",\"PS256\",\"PS384\",\"PS512\",\"ES256\",\"ES384\",\"ES512\",\"HS256\",\"HS384\",\"HS512\"],\"prompt_values_supported\":[\"none\",\"login\",\"consent\",\"select_account\"],\"authorization_response_iss_parameter_supported\":true,\"backchannel_token_delivery_modes_supported\":[\"poll\"],\"backchannel_user_code_parameter_supported\":true,\"dpop_signing_alg_values_supported\":[\"RS256\",\"RS384\",\"RS512\",\"PS256\",\"PS384\",\"PS512\",\"ES256\",\"ES384\",\"ES512\"],\"expired_jwks_uri\":\"https://helseid-sts.test.nhn.no/connect/expiredjwks\",\"available_idps\":\"https://helseid-sts.test.nhn.no/connect/availableidps\"}";
                         OpenIdConnectConfiguration? config = metadata.Deserialize<OpenIdConnectConfiguration>();
                         options.ConfigurationManager = new FakeStaticConfigurationManager(config ?? new OpenIdConnectConfiguration());
                         options.Backchannel = new HttpClient(new FakeOpenIdConnectHandler())
                         {
                             BaseAddress = new Uri("https://fake-identity-provider")
                         };

                     }
                 });
             });

            var app = builder.BuildApp(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapGet("/test-endpoint", [Authorize] async (context) =>
                {
                    var name = context.User.Name();
                    await context.Response.WriteAsync($"{name}");
                });
            });
            app.Start();

            var client = app.GetTestClient();
            var response = await client.GetAsync("/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location!.Query);
            var cookie = response.Headers.FirstOrDefault(x => x.Key == "Set-Cookie").Value;
            Dictionary<string, string?> callbackRequest = new Dictionary<string, string?>
            {
                ["code"] = "D4551ADD9D230EF3FB4052AEFA6A9A81206BF71CB6EE417CC744D6A252B0EF63-1",
                ["scope"] = "openid profile helseid://scopes/identity/pid helseid://scopes/identity/pid_pseudonym helseid://scopes/identity/security_level offline_access",
                ["state"] = queryParams["state"],
                ["iss"] = "https://helseid-sts.test.nhn.no"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://localhost/signin-callback "));
            request.Headers.Add("Cookie", cookie);
            request.Content = new FormUrlEncodedContent(callbackRequest);

            var callbackResponse = await client.SendAsync(request);
        }

        /// <summary>
        /// Test signin callback with token
        /// https://github.com/dotnet/aspnetcore/blob/81a2bab8704d87d324039b42eb1bab0d977f25b8/src/Security/Authentication/test/OpenIdConnect/OpenIdConnectEventTests_Handler.cs
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("Will be implemented later")]
        public async Task DefaultHelseIdConfiguration_InvalidTokenRecieved_()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.Default();
            var configRoot = config.CreateConfigurationRoot();
            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(configRoot)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(configRoot)
                   .Build();
                })
                .BuildApp(UseEndpointAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.PostAsync("/signin-callback", null);
        }

        private static void AssertStandardScopes(NameValueCollection queryParams)
        {
            Assert.That(queryParams["scope"], Does.Contain("openid"));
            Assert.That(queryParams["scope"], Does.Contain("profile"));
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/identity/pid"));
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/identity/pid_pseudonym"));
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/identity/security_level"));
            Assert.That(queryParams["scope"], Does.Contain("offline_access"));
        }

        private static Action<WebApplication> UseEndpointAuthenticationAndAuthorization()
        {
            return app =>
            {
                app.UseRouting();
                app.MapGet("/api/testauthentication",
                    [Authorize]
                () => "Hello world!");
                app.UseAuthentication();
                app.UseAuthorization();
            };
        }
    }

    internal class FakeOIDCEvents : OidcEvents
    {
        public FakeOIDCEvents(IHelseIdClientSecretHandler secretHandler, IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(secretHandler, helseIdWebKonfigurasjon)
        {
        }

        public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            return base.RedirectToIdentityProviderForSignOut(context);
        }

        public override Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            var endpoint = context.TokenEndpointRequest?.RequestUri;
            return base.AuthorizationCodeReceived(context);
        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            return base.AuthenticationFailed(context);
        }

        public override Task RemoteFailure(RemoteFailureContext context)
        {
            return base.RemoteFailure(context);
        }
    }

    internal class FakeStaticConfigurationManager : StaticConfigurationManager<OpenIdConnectConfiguration>
    {
        
        public FakeStaticConfigurationManager(OpenIdConnectConfiguration configuration) : base(configuration)
        {
        }

        public override Task<BaseConfiguration> GetBaseConfigurationAsync(CancellationToken cancel)
        {
            return base.GetBaseConfigurationAsync(cancel);
        }

        public override void RequestRefresh()
        {
            base.RequestRefresh();
        }
    }

    public class FakeOpenIdConnectHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Simulating a token response
            var fakeResponse = new
            {
                access_token = "fake-access-token",
                token_type = "Bearer",
                expires_in = 3600
            };

            var json = System.Text.Json.JsonSerializer.Serialize(fakeResponse);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }

    public class CustomOpenIdConnectConfiguration : OpenIdConnectConfiguration
    {
        [JsonPropertyName("claims_supported")] 
        public new List<string>? ClaimsSupported { get; set; }

        [JsonPropertyName("scopes_supported")]
        public new List<string>? ScopesSupported { get; set; }

        [JsonPropertyName("grant_types_supported")]
        public new List<string>? GrantTypesSupported { get; set; }

        [JsonPropertyName("response_types_supported")]
        public new List<string>? ResponseTypesSupported { get; set; }

        [JsonPropertyName("response_modes_supported")]
        public new List<string>? ResponseModesSupported { get; set; }

        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public new List<string>? TokenEndpointAuthMethodsSupported { get; set; }

        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public new List<string>? IdTokenSigningAlgValuesSupported { get; set; }

        [JsonPropertyName("code_challenge_methods_supported")]
        public new List<string>? CodeChallengeMethodsSupported { get; set; }
    }
}




