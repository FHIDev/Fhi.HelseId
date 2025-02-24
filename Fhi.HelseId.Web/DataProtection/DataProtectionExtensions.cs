using AspNetCore.DataProtection.SqlServer;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Web.DataProtection
{
    public static class DataProtectionExtensions
    {
        /// <summary>
        /// Enables sql server based data protection of cookie keys, if enabled in the config file
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSqlDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProtectionConfig = configuration.GetDataProtectionConfig();
            if (!dataProtectionConfig.Enabled)
                return;
            services
                .AddDataProtection()
                .PersistKeysToSqlServer(dataProtectionConfig.ConnectionString, dataProtectionConfig.Schema, dataProtectionConfig.TableName);
        }
    }
}
