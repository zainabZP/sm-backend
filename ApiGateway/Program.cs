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

    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddCors(options => {
        options.AddPolicy("AllowAll", builder => {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseCors("AllowAll");

    // Add Swagger UI to Gateway
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger-auth/v1/swagger.json", "AuthService API");
        c.SwaggerEndpoint("/swagger-posts/v1/swagger.json", "PostService API");
        c.SwaggerEndpoint("/swagger-comments/v1/swagger.json", "CommentService API");
        c.SwaggerEndpoint("/swagger-feed/v1/swagger.json", "FeedService API");
        c.SwaggerEndpoint("/swagger-follows/v1/swagger.json", "FollowService API");
        c.SwaggerEndpoint("/swagger-likes/v1/swagger.json", "LikeService API");
        c.RoutePrefix = "swagger"; 
    });

    // Make default route go to swagger
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
