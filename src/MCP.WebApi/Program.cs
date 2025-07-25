using MCP.Application.Configuration;
using MCP.Infrastructure.Configuration;
using MCP.WebApi.Extensions;
using MCP.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Note: Removed AddControllers() for minimal API approach

// Add application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add startup service
builder.Services.AddSingleton<StartupService>();

// Add security services
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiKeyAuthentication(builder.Configuration);
builder.Services.AddCustomAuthorization();
builder.Services.AddCustomRateLimiting();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");

// Add security middleware
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Configure API endpoints - secured versions
app.MapAuthEndpoints();
app.MapSecuredHealthEndpoints();
app.MapWeatherEndpoints();

// Initialize startup operations (device registration and browser opening)
var startupService = app.Services.GetRequiredService<StartupService>();
await startupService.InitializeAsync();

app.Run();
