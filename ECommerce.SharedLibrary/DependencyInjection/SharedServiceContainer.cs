using ECommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ECommerce.SharedLibrary.DependencyInjection;

public static class SharedServiceContainer
{
    public static IServiceCollection AddSharedServices<TContext>
        (this IServiceCollection services, IConfiguration config, string fileName) where TContext : DbContext
    {
        // Add Generic Database context
        services.AddDbContext<TContext>(
            (options) => options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
            )
        );

        // Configure Serilog Logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.File(path: $"{fileName}-.text", 
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
                        
        //Add jwt authentication scheme
        JwtAuthenticationScheme.AddJwtAuthentication(services, config);
        
        return services;
    }
    
    
    public static IApplicationBuilder UseSharedPolicy(this IApplicationBuilder app)
    {
        //use global exception 
        app.UseMiddleware<HandleGlobalExceptions>();
        
        //register middleware to block all request except from Api Gateway
        app.UseMiddleware<ListenToOnlyApiGateway>();

        return app;
    }
}