using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder();

var authBuilder = builder
    .AddHelseIdWebAuthentication()
    .Build();

var app = builder.Build();

app.UseHelseIdProtectedPaths();

app.Run();

public partial class Program
{
}