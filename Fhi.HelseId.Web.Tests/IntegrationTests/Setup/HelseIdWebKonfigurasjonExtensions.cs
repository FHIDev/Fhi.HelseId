namespace Fhi.HelseId.Web.IntegrationTests.Setup
{
    internal class HelseIdWebKonfigurasjonBuilder
    {
        internal static HelseIdWebKonfigurasjon Create
        {
            get
            {
                return new HelseIdWebKonfigurasjon();
            }
        }
    }

    internal static class HelseIdWebKonfigurasjonExtensions
    {
        internal static HelseIdWebKonfigurasjon AddDefaultValues(this HelseIdWebKonfigurasjon config)
        {
            config.ClientId = Guid.NewGuid().ToString();
            config.Authority = "https://helseid-sts.test.nhn.no";
            return config;
        }

        internal static HelseIdWebKonfigurasjon WithSecurityLevel(this HelseIdWebKonfigurasjon config, string[] securityLevels)
        {
            config.SecurityLevels = securityLevels;
            return config;
        }
    }
}
