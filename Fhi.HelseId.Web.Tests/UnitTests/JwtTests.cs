using System.IdentityModel.Tokens.Jwt;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Services;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests
{
    public class JwtTests
    {
        [Test]
        public void GeneretateClientAssertion_HelseIdJwkSecretHandler()
        {
            var config = new HelseIdWebKonfigurasjon()
            {
                ClientSecret = $"{{\"d\":\"Q4x8XiZ3JKn0-ijW-H9plfw7QF4VLK43jHxYtPJvX6GcBuEk_rMedziQuqbBCZrK6aWVspnYS6dQtj33Z2TtSkXu2gy_1xR2nR8h9XeZ6h6QRbL9bj1Qxrk70ry7bXz5WIjyyuPmY73aPw9OFrZ_NDeUQjiEofzTHkr86ZIVjAmNLarVufG9P2V6fz14wwHc3aLBVgUt7Rxx5sFOQR30zYGpd1BH-xK6ykA6n6BdaIc4luWw_SkmVowwO4toScj07qoAYTUR4IFQHYt7sQZNufFG89nB-v_Er0a2tRvtME2NnU_4rn4ea1yyGFlYH_6Amtb8u4-TAeOESjrMw9ylBkvb6vIvtqT0lQdBJJEPI_Hx-655ElvO4zT48HBS6oVZHCARN17d7pQWrnxiSusYEdM9RwJET57ieVayo-baQe3NOvj2Y5V2H034cWCJt_DTh7ye9RXD4gtMnHDQ-tgV6ztwW8GkGvbJzXUnkqGXUvKqjeJAnOc2Ahoxpc-9cnMnW2DrwPnI0f9Jsq0n3hQyqwnnyimIeZn32WVe2Q4XC7d_VB21E8oDZhdeUlxuTZX-foTrYB3xvDKB6tLCaaMbfpzvUsSfSYqbAXQfqhQWosyt7w-ZIYJOY05fWspR3mlpo5IMGkaDp8clvz51f8zdMfSYFTml4e_zjoduvlz2wyE\",\"dp\":\"GsxR4BGYEb460zNcX1_SROtq2zVG8IfYVFy-3pFQvmerfiRJr0uuWvZ1WCtqalXKV33ACdf5njmkKdA-z-RbH07axt52b8SQZOTQ8527i5p_zJ6QGp6Lw4iuepAX64T_POtqmUDKcusIOGxxZbC_SjVr7dtYHVWuqPZlNjFbqQpWcerQybQvsyBeVDzDYkcdM7dhW8wXuIPDukU0zkgKBvW-23LR6dN9t7zh3suLdhkaV381-lODAU_U02-wIhXwDcHKi_8a1dMtMKQFxKyngp0d3P8R5hDT71UOttD0zMxt6HB_c6cwLmOYPclYXsK0-bIIDgJq7rERIA6KF0GqaQ\",\"dq\":\"CSI90DDVZFFj0DWpUXcpWjH5NOP7Yca9dTeFkWqnhmpS_XOMNvCLa9pyTO7o-Iw-aRbVhlpyIQN4pSdMmjnW5eEpBg8zsd8LcV8gkv9sL08bL-8dWcqy5kD9pgBEK49HgiobTWpdKd02PErgXbY28hWRx4JafVRjk7PkRXD4yJjK_qJ-fwlY51K0ynAIX4L7C14LW7AVYp4QkRjaHd_O1CRorVijR_N_sEvfa1jfZHNtBmgaUbJxn_4rYVZfehu5nbqoXLB4VqJBwVf26rNyT-fxMbAW4OH0ubjWrcTCedfAIMMegbXG7cHxrrbAL-50PggVWWjxbKAt0gBoN5KNIQ\",\"e\":\"AQAB\",\"kid\":\"-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4\",\"kty\":\"RSA\",\"n\":\"iu9EmQLoIJBPBqm3jLYW4oI8yLkOxvKg-OagE8HlzP-RQnDXH9hBe2cTRZ3oNqG1viWmv6-dxNtKU1QxOpezWLx-N-AJ7dIlXTMUGkCheHUorPSzakeBUOCHtvT1Tdv9Mzue9fVt3JxpPX6mQNlsOzwk9L8HmbgojMcApKmQcfNriVV72byLuaAoh9fcXSNm6TUuwO5cPmnHgS5B5Hfe5P0OIte027oZyjPiYm-QbV4YJNjwwwZnPvkLaRjw6L8sV5TAOLvNQIt63OpF8UHPjBsM8LJHdHFUMgx2BaMaJC8tNCi_8UWGG59sd4-_vJC78s3wZNEGL6OwCngpF7NLwaP9Zqxx8DDkOY71MvvcAyu4i0D6_8A8_qewLvb_SPxNpCe8zH5MJIKNJB38InWd8FpvpbPuEJt4oK1gfUBWLWQ39YIHzodKhkN-qAXYWGyzJ2nJdNIMAclefw251Cvjcyf3gmVATXDBAo-piUJIGXC3y7yqfyMupe_4oRe69DFBZTecXSLEdbAbUtiaH9r4rY5oeYCiZ70wcFcieHFZLwfleCPm5Cz8rEQxK8KjMis2kb1aRxVytTj_0pOkw1HEJU1tv_TWmD136RgoRtiqnVoxmCM6Q4XxXrOnGMPZR0_ScYHdW_YjDgnJBQykAbzW0nC47d3KSotktz1cPejo5_s\",\"p\":\"wSqXfloa5ikf6d_G3N2x-IBjTEB7mibEif9qTNED7x4f7J8_vB_rdrsxoTCdY3R5j2BIS3XKyiTIkonfV5mYQvSu9RwxgX4ImcTsTXMxewuyQ031OJz0ruvlgvHELzdgkyo4q4NQaDigejfp_DsoEM0nLHeNXYfTz_AuTlkG8BA0JzrBK0aoBPUdwB1_WoJoJYVhST-B_PHQ3eaFOQNGXQXoYc6YZt4WKmu3WModjezdqnKVKSaORTuG5-mTQLS2jxZr1XlEDXWN53tH_Oon2WMDGbSCVz_qYVZUWorbjvUmxJnYP0H-lIxLRfxYE68DnYlXSWOzcfpdD6VIlP972w\",\"q\":\"uCCszPIHfiHe5UOa0poW8WELpL_YItCdK_AnwulRHOM2FIQbmWBVZBMRguQJuMCvjIAWdQNEOrZYt1BhIknziUHSvSnLU0qszJ_ZHByZS54834CIxc-0etVkKbpnJGpjzgucvmEPNzrUko2ip512iOY9WTqB54Awg56rS-3oBw2_iqEzioAI30pD9-AX5xDRBEJcRJ8mPxw0iSDGVkxwIQNLBlML79cGNGjdrzsJyjPuMMDTLadHnRSAybmqhVTJYwEH5t37_f_fho2aoPu_sW65LdoW6Z-V042xnieMm3XhA8yZxonao2LH6Bh7Xk7qinWYpYuF6UpKqhtrpI8OYQ\",\"qi\":\"Z7GWLxO3q2BolVFaOjuskhYc0V4GZ-b-SA2Rv110HovMDatlUGZKgoAOponFcvEddJSNf7stRM35HWiTN9W5iSUP4VLqAlJ2W7ftIsRJv0D-Lcs4HoXTAHcny36j0eEzhNLUYwfS9Y7ICWEWHv_WTG8Iz_I87JKLCnjGutZDmsM_fHDPmUkv7Pf2GG9r9hzS8uylC43ik4gfrp0Hm_6rAHKB4EHVHfYu51zl9yLkPgqq8ycHi0tF7VmVtDUsIMJdz7nlGOCS-468WI95dAfdfTC8v9JKXj9JL3ylM3dDbiC3m0p-rpaM2VzuO4OrMk-jWFCuYbDCYS-bcYFG4XwmtA\"}}",
                ClientId = "clientId"
            };
            var secretHandler = new HelseIdJwkSecretHandler(config);
            var jwt = secretHandler.GetSecurityKey();
            var clientAssertion = ClientAssertion.Generate(config.ClientId, "http://issuer", jwt);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(clientAssertion);

            Assert.Multiple(() =>
            {
                Assert.That(token.Issuer, Is.EqualTo("clientId"), "Issuer mismatch");
                Assert.That(token.Claims.Count, Is.EqualTo(7), "Unexpected claim count");

                var audClaim = token.Claims.SingleOrDefault(x => x.Type == "aud");
                Assert.That(audClaim, Is.Not.Null, "Missing 'aud' claim");
                Assert.That(audClaim!.Value, Is.EqualTo("http://issuer"), "Invalid 'aud' claim value");

                var subClaim = token.Claims.SingleOrDefault(x => x.Type == "sub");
                Assert.That(subClaim, Is.Not.Null, "Missing 'sub' claim");
                Assert.That(subClaim!.Value, Is.EqualTo("clientId"), "Invalid 'sub' claim value");

                Assert.That(token.Claims.Any(x => x.Type == "jti"), Is.True, "Missing 'jit' claim");
                Assert.That(token.Claims.Any(x => x.Type == "nbf"), Is.True, "Missing 'nbf' claim");
                Assert.That(token.Claims.Any(x => x.Type == "iat"), Is.True, "Missing 'iat' claim");
                Assert.That(token.Claims.Any(x => x.Type == "exp"), Is.True, "Missing 'exp' claim");

                var typ = token.Header.SingleOrDefault(x => x.Key == "typ");
                Assert.That(typ.Value, Is.EqualTo("client-authentication+jwt"));

                var alg = token.Header.SingleOrDefault(x => x.Key == "alg");
                Assert.That(alg.Value, Is.EqualTo(SecurityAlgorithms.RsaSha512));

                var kid = token.Header.SingleOrDefault(x => x.Key == "kid");
                Assert.That(kid.Value, Is.EqualTo("-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"));
            });
        }
    }
}
