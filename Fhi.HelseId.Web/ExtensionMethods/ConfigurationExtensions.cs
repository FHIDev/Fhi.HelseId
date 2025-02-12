using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Common.ExtensionMethods;
using Fhi.HelseId.Web.DataProtection;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class ConfigurationExtensions
    {
        public static DataProtectionConfig GetDataProtectionConfig(this IConfiguration configuration)
            => configuration
            .GetSection(nameof(DataProtectionConfig))
            .Get<DataProtectionConfig>()
                ?? throw new MissingConfigurationException(nameof(DataProtectionConfig));

        public static HelseIdWebKonfigurasjon GetWebKonfigurasjon(this IConfiguration configuration)
            => configuration
            .GetConfig<HelseIdWebKonfigurasjon>(nameof(HelseIdWebKonfigurasjon))
                ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));
    }
}
