using Serilog;
using System.Net;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try {
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddHttpForwarder();
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
        .ConfigureHttpClient((context, handler) => {
            handler.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true;
        });

    builder.Services.AddLogging(logging => {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    });

    builder.Services.AddCors(options => {
        options.AddPolicy("AllowAll", builder => {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    app.UseForwardedHeaders();
    app.UseSerilogRequestLogging();
    app.UseCors("AllowAll");

    // DEBUG ENDPOINT: Test if we can reach auth-service
    app.MapGet("/test-connection", async (IConfiguration config) => {
        var authUrl = config["ReverseProxy:Clusters:authcluster:Destinations:authdest:Address"] ?? "Not Set";
        try {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync(authUrl + "swagger/v1/swagger.json");
            return Results.Ok(new { 
                Target = authUrl, 
                Status = response.StatusCode.ToString(), 
                Success = response.IsSuccessStatusCode 
            });
        } catch (Exception ex) {
            return Results.Problem($"Failed to reach {authUrl}. Error: {ex.Message}");
        }
    });

    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger-auth/v1/swagger.json", "AuthService API");
        c.SwaggerEndpoint("/swagger-posts/v1/swagger.json", "PostService API");
        c.SwaggerEndpoint("/swagger-comments/v1/swagger.json", "CommentService API");
        c.SwaggerEndpoint("/swagger-feed/v1/swagger.json", "FeedService API");
        c.SwaggerEndpoint("/swagger-follows/v1/swagger.json", "FollowService API");
        c.SwaggerEndpoint("/swagger-likes/v1/swagger.json", "LikeService API");
        c.RoutePrefix = "swagger"; 
    });

    app.Use(async (context, next) => {
        if (context.Request.Path == "/") {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });

    app.MapReverseProxy();

    app.Run();
} catch (Exception ex) {
    Log.Fatal(ex, "Application terminated unexpectedly");
} finally {
    Log.CloseAndFlush();
}
