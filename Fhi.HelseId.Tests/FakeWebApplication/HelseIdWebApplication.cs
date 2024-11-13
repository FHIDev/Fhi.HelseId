using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Fhi.HelseId.Tests.FakeWebApplication
{
    public class HelseIdWebApplication<T> : WebApplicationFactory<T> where T : class
    {
        public Dictionary<string, string?> _inMemory { get; set; }

        public HelseIdWebApplication(Dictionary<string, string?> config)
        {
            _inMemory = config;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(httpContextAccessor);
            });

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            foreach (var keyValue in _inMemory)
            {
                Environment.SetEnvironmentVariable(keyValue.Key, keyValue.Value);
            }
        }
    }
}