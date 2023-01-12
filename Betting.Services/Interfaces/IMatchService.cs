using Betting.Services.Models;

namespace Betting.Services.Interfaces
{
	public interface IMatchService
	{
		Task GetBettingInfo();

		Task<MatchesResponseModel> GetMatchesForTimePeriod(int hours);
	}
}
