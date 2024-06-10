using Extensions.NewSeriLog;
using Notification.API.Notification.Web.Extensions;



public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
       
        builder.AddSerilog();

        builder.Services.AppServices(builder.Configuration);
        builder.Services.ConfigureKafka();
        builder.Services.AddSwaggerServices();

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
        app.UseSerilogDbMigrationLogging();

        app.Run();
    }

}