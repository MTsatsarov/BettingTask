using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Betting.Data
{
	public class DesignTimeDbContext : IDesignTimeDbContextFactory<BettingContext>
	{
		public BettingContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				 .SetBasePath(Directory.GetCurrentDirectory())
				 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				 .Build();

			var builder = new DbContextOptionsBuilder<BettingContext>();
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			builder.UseSqlServer(connectionString);

			return new BettingContext(builder.Options);
		}
	}
}
