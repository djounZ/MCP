using MCP.Application.Configuration;
using MCP.Infrastructure.Configuration;
using MCP.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Note: Removed AddControllers() for minimal API approach

// Add application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
// Note: Removed UseRouting() and UseAuthorization() as they're not needed for this minimal API

// Configure API endpoints
app.MapHealthEndpoints();
app.MapWeatherEndpoints();

app.Run();
