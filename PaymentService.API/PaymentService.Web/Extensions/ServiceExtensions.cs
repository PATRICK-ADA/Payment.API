using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Notification.API.Nofication.Core.Abstraction;
using RoomService.Infrastructure.Data;
using Services.Notification_Publisher;


namespace Notification.API.Notification.Web.Extensions
{
    public static class ServiceRegistrations
    {
        public static IServiceCollection ConfigureKafka(this IServiceCollection services)
        { 

            services.AddHostedService<InvoiceConsumer>();
            return services;    
        }

        public static IServiceCollection AppServices(this IServiceCollection services, IConfiguration configuration)
        {



            services.AddAuthentication();
            services.AddAuthorization();
            services.AddControllers();

            services.AddScoped<INotification_Publisher, Notification_Publisher>();

            services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),

                 b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)), ServiceLifetime.Transient);



            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithMethods("GET", "PUT", "DELETE", "POST")
                    );
            });
            return services;
        }


        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();


            services.AddSwaggerGen
                (g =>
                {
                    g.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "Auction Payment Management API",
                        Description = "Documentation for Auction Payment Management API"

                    });

                });
            return services;
        }
    }
}