using API.Data;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure; // Добавьте эту строку
using Scalar.AspNetCore;
using System.Reflection;


namespace API;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
            
            // Добавляем поддержку JWT аутентификации в Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введите JWT токен в формате: Bearer {ваш токен}"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        // Tilføj CORS for specifikke Blazor WASM domæner
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowSpecificOrigins",
                builder =>
                {
                    builder
                        .WithOrigins(
                            "http://localhost:5085",
                            "http://localhost:8052",
                            "https://h2.mercantec.tech",
                            "https://malevolentshrine.mercantec.tech/",
                            "http://localhost:3000",    // React dev server
                            "http://localhost:3001",    // React альтернативный порт
                            "http://127.0.0.1:3000",    // React альтернативный хост
                            "http://localhost:5173",    // Vite стандартный порт
                            "http://localhost:5174",    // Vite альтернативный порт
                            "https://localhost:5173",   // HTTPS версии
                            "https://localhost:7285",
                            "https://localhost:5174",
                            "http://localhost:3000",    // React dev server альтернативный
                            "http://127.0.0.1:5173",    // localhost через IP
                            "http://127.0.0.1:5174",
                            "http://127.0.0.1:3001"     // React альтернативный хост
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition");
                }
            );
        });
        


        IConfiguration Configuration = builder.Configuration;

        string connectionString = Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

        Console.WriteLine("connectionString: " + connectionString);

        builder.Services.AddDbContext<AppDBContext>(options =>
                options.UseNpgsql(connectionString));
       


        // Tilføj basic health checks
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), ["live"]);
        
        // JWT Authentication
        builder.Services.AddScoped<JwtService>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_Issuer"),
                    ValidAudience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("Jwt_Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt_SecretKey"]!)) ?? new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Jwt_SecretKey")!))
                };
            });
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<PasswordHasher<User>>();
        
        builder.Services.AddDbContext<AppDBContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        var app = builder.Build();
            
        
        // Brug CORS - skal være før anden middleware
        app.UseCors("AllowSpecificOrigins");

        // Map health checks
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        app.MapOpenApi();

        // Scalar Middleware for OpenAPI
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("MAGSLearn")
                .WithTheme(ScalarTheme.Mars)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        // Map the Swagger UI
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
