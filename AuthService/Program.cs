using System.Text;
using AuthService.Context;
using AuthService.Interfaces;
using AuthService.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try {
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Secret"] ?? throw new Exception("JWT Secret not configured")
                    )
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            };
            options.Events = new JwtBearerEvents {
                OnTokenValidated = async context => {
                    var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    if (await userRepository.IsTokenBlacklisted(token)) {
                        context.Fail("Token is blacklisted");
                    }
                }
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options => {
        options.SwaggerDoc("v1", new OpenApiInfo {
            Title = "AuthService API",
            Version = "v1"
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token like this: Bearer {your token}"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.Use(async (context, next) => {
        if (context.Request.Path == "/") {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
} 
catch (Exception ex) {
    Log.Fatal(ex, "Application terminated unexpectedly");
} 
finally {
    Log.CloseAndFlush();
}