using Microsoft.AspNetCore.Identity;

namespace Betting.Data.Entities
{
	public class ApplicationUser:IdentityUser<string>
	{
		public ApplicationUser()
		{
			this.Id = Guid.NewGuid().ToString();
		}
	}
}
