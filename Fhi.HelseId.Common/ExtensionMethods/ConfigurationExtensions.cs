using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Common.ExtensionMethods
{
    public static class ConfigurationExtensions
    {
        public static T? GetConfig<T>(this IConfiguration root, string name)
        {
            var section = root.GetSection(name);
            var config = section.Get<T>();
            return config;
        }
    }
}
