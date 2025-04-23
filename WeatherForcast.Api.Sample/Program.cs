using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Require Dpop does not seem to work for Blazor
var helseIdConfiguration = new HelseIdApiKonfigurasjon()
{
    Authority = "https://helseid-sts.test.nhn.no/",
    ApiName = "fhi:weather",
    RequireContextIdentity = true,
    RequireDPoPTokens = true,
    AllowDPoPTokens = true
};
builder.Services.AddHelseIdApiAuthentication(helseIdConfiguration);

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
