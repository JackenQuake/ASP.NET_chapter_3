using DomainEventServices;
using MailServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductCatalog.Models;
using ProductCatalog.Services;
using SiteMetrics;
using System;
using System.Threading.Tasks;

namespace ProductCatalog
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Включаем логирование тела запросов и логирование тела ответов
			services.AddHttpLogging(options =>
			{
				options.LoggingFields = HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody;
			});
			services.Configure<SmtpCredentials>(Configuration.GetSection("SmtpCredentials"));
			services.Configure<NotificationSettings>(Configuration.GetSection("NotificationSettings"));
			services.AddSingleton<IMailSender, MailSender>();
			services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
			services.AddHostedService<NotificationService>();
			services.AddSingleton<ICatalogStorage, CatalogStorage>();
			services.AddScoped<ICatalogModel, CatalogModel>();
			services.AddSingleton<IMetricsStorage, MetricsStorage>();
			services.AddControllersWithViews();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			} else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			// Блокируем работу во всех браузерах, кроме Edge
			app.Use(async (HttpContext context, Func<Task> next) => 
			{
				var userAgent = context.Request.Headers.UserAgent.ToString();
				if (!userAgent.Contains("Edg"))
				{
					context.Response.Headers.ContentType = "text/plain; charset=UTF-8";
					await context.Response.WriteAsync("Ваш браузер не поддерживается.");
					return;
				}
				await next();
			});
			// Включаем логирование
			app.UseHttpLogging();
			// Подключаем подсчет ссылок
			app.UseMiddleware<MetricsCounter>();
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
