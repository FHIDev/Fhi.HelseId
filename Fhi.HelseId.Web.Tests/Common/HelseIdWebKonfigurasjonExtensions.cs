namespace Fhi.HelseId.Web.Common
{
    internal class HelseIdWebKonfigurasjonBuilder
    {
        internal static HelseIdWebKonfigurasjon Create => new();
    }

    internal static class HelseIdWebKonfigurasjonExtensions
    {
        internal static HelseIdWebKonfigurasjon Default(this HelseIdWebKonfigurasjon config)
        {
            config.AuthUse = true;
            config.ClientId = Guid.NewGuid().ToString();
            config.Authority = "https://helseid-sts.test.nhn.no";
            return config;
        }

        internal static HelseIdWebKonfigurasjon WithSecurityLevel(this HelseIdWebKonfigurasjon config, string[] securityLevels)
        {
            config.SecurityLevels = securityLevels;
            return config;
        }

        internal static HelseIdWebKonfigurasjon WithIncludeHprNumber(this HelseIdWebKonfigurasjon config, bool value)
        {
            config.IncludeHprNumber = value;
            return config;
        }

        public static HelseIdWebKonfigurasjon WithRequireHprNumber(this HelseIdWebKonfigurasjon config, bool value)
        {
            config.RequireHprNumber = value;
            return config;
        }

        public static HelseIdWebKonfigurasjon WithRequireValidHprAuthorization(this HelseIdWebKonfigurasjon config, bool value)
        {
            config.RequireValidHprAuthorization = value;
            return config;
        }
    }
}
