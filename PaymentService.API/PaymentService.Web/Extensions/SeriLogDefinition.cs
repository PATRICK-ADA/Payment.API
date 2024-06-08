using Microsoft.EntityFrameworkCore;
using RoomService.Infrastructure.Data;
using Serilog;

namespace Notification.API.Notification.Web.Extensions
{
    public static class SeriLogDefinition
    {
        public static void UseSerilogMigrationSetUpInfo(this WebApplication app)
        {

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    Log.Information("Connecting to Db");
                    if (scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.CanConnect())
                    {
                        Log.Information("Starting Db Migration");
                        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
                        Log.Information("Db Migration Completed");
                    }
                    else
                    {
                        Log.Error("Could not connect to Database");
                    }

                    Log.Information("Application Starting");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "An error occurred while applying Migration");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }
    }
}