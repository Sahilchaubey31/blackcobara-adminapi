using System.Reflection;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication1.Repositories;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Use Railway PORT env variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();

// CORS: allow your frontend origin and required headers/methods
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://blackcobara.in",
                "https://www.blackcobara.in"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register IDbConnection factory for Dapper usage
builder.Services.AddTransient<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repository (scoped for DB work)
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();

// Register call forwarding service
builder.Services.AddScoped<ICallForwardingService, CallForwardingService>();

// Swagger (if you want)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Subdirectory support for blackcobara.in/adminapi
app.UsePathBase("/adminapi");

// Apply CORS policy BEFORE MapControllers
app.UseCors("AllowReactDev");

app.UseSwagger(c => c.RouteTemplate = "swagger/{documentName}/swagger.json");
app.UseSwaggerUI(c => c.SwaggerEndpoint("/adminapi/swagger/v1/swagger.json", "Admin API v1"));

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
