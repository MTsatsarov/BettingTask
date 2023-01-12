using Microsoft.AspNetCore.Identity;

namespace Betting.Data.Entities
{
	public class ApplicationRole : IdentityRole
	{
		public ApplicationRole()
		{
			this.Id = Guid.NewGuid().ToString();
		}
	}
}
