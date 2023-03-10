using Betting.Data;
using Betting.Data.Entities;
using Betting.Services;
using Betting.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hangfire.SqlServer;
using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics;
using Betting.Web.Hubs;
using Betting.Web.Background;
using Microsoft.AspNetCore.SignalR;
using Betting.Web.Hubs.Interfaces;

namespace Betting
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			//DB
			builder.Services.AddDbContext<BettingContext>(opt =>
			opt.UseSqlServer("name=ConnectionStrings:DefaultConnection").UseLazyLoadingProxies());

			//Not specified in the requirements.
			//Identity 
			builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
				.AddEntityFrameworkStores<BettingContext>()
				.AddDefaultTokenProviders();

			builder.Services.Configure<IdentityOptions>(options =>
			{
				// Default Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;
			});

			//Register services
			builder.Services.AddTransient<IMatchService, MatchService>();

			builder.Services.AddTransient<Betting.Services.Utils.MIddlewares.ExceptionHandlerMiddleware, 
				Betting.Services.Utils.MIddlewares.ExceptionHandlerMiddleware>();

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			//Add hangfire 


			builder.Services.AddHangfire(configuration => configuration
				  .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
				  .UseSimpleAssemblyNameTypeSerializer()
				  .UseRecommendedSerializerSettings()
				  .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
				  {
					  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
					  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
					  QueuePollInterval = TimeSpan.Zero,
					  UseRecommendedIsolationLevel = true,
					  DisableGlobalLocks = true
				  }));

			builder.Services.AddHangfireServer();

			builder.Services.AddSignalR();

			var app = builder.Build();

			app.UseHangfireDashboard("/hangfire", new DashboardOptions());

			var manager = new RecurringJobManager();

			var everyMinuteCron = "* * * * *";
			manager.AddOrUpdate("MinutelyJob",
				Job.FromExpression(() => new MinutelyJobs(
					new object() as IMatchService,
					new object() as IHubContext<MatchHub,IMatchHub>
				).StartJobs()), everyMinuteCron);

			using (var serviceScope = app.Services.CreateScope())
			{
				var dbContext = serviceScope.ServiceProvider.GetRequiredService<BettingContext>();
				dbContext.Database.Migrate();
			}

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseMiddleware<Betting.Services.Utils.MIddlewares.ExceptionHandlerMiddleware>();

			app.MapControllers();
			app.MapHub<MatchHub>("/hubs/match");
			app.Run();
		}
	}
}