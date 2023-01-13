using Betting.Services.Models;

namespace Betting.Web.Hubs.Interfaces
{
	public interface IMatchHub
	{
		 Task NotifyUsers(ICollection<MatchResponseModel> matches);
	}
}
