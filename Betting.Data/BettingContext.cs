using Betting.Data.Common;
using Betting.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Betting.Data
{
	public class BettingContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
	{
		private static readonly MethodInfo SetIsDeletedQueryFilterMethod =
		   typeof(BettingContext).GetMethod(
			   nameof(SetIsDeletedQueryFilter),
			   BindingFlags.NonPublic | BindingFlags.Static);

		public BettingContext(DbContextOptions<BettingContext> options)
		 : base(options)
		{

		}

		public DbSet<Sport> Sports { get; set; }
		public DbSet<Event> Events{ get; set; }

		public DbSet<Match> Matches { get; set; }

		public DbSet<Bet> Bets { get; set; }

		public DbSet<Odd> Odds { get; set; }

		public override int SaveChanges() => this.SaveChanges(true);

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			var entityTypes = builder.Model.GetEntityTypes().ToList();

			var deletableEntityTypes = entityTypes
				.Where(et => et.ClrType != null && typeof(IDeletableEntity).IsAssignableFrom(et.ClrType));
			foreach (var deletableEntityType in deletableEntityTypes)
			{
				var method = SetIsDeletedQueryFilterMethod.MakeGenericMethod(deletableEntityType.ClrType);
				method.Invoke(null, new object[] { builder });
			}

		}
		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			this.ApplyAuditInfoRules();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		private static void SetIsDeletedQueryFilter<T>(ModelBuilder builder)
		   where T : class, IDeletableEntity
		{
			builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
		}

		private void ApplyAuditInfoRules()
		{
			var changedEntries = this.ChangeTracker
				.Entries()
				.Where(e =>
					e.Entity is IAuditInfo &&
					(e.State == EntityState.Added || e.State == EntityState.Modified));

			foreach (var entry in changedEntries)
			{
				var entity = (IAuditInfo)entry.Entity;
				if (entry.State == EntityState.Added && entity.CreatedOn == default)
				{
					entity.CreatedOn = DateTime.UtcNow;
				}
				else
				{
					entity.ModifiedOn = DateTime.UtcNow;
				}
			}
		}
	}
}
