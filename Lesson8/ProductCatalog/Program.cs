using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace ProductCatalog
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.CreateBootstrapLogger();
			Log.Information("Начало работы");
			try
			{
				CreateHostBuilder(args).Build().Run();
			} catch (Exception e)
			{
				Log.Fatal(e, "Сервер рухнул!");
			}
			finally
			{
				Log.Information("Окончание работы");
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog((ctx, conf) =>
				{
					conf
					.MinimumLevel.Debug()
					.WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
					.WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day);
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
