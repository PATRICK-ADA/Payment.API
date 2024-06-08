using Serilog;
using RoomService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Notification.API.Notification.Web.Extensions;




public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, config) =>
        {
            config.Enrich.FromLogContext()
                .WriteTo.Console()
                .ReadFrom.Configuration(context.Configuration);

        });

        builder.Services.AppServices(builder.Configuration);
        builder.Services.ConfigureKafka();
        builder.Services.AddSwaggerServices();



        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

        var app = builder.Build();


        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors(x => x
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .SetIsOriginAllowed(origin => true)
                  .AllowCredentials());

        app.UseAuthorization();
        app.MapControllers();
        app.UseSerilogMigrationSetUpInfo();

        app.Run();
    }

}