using Betting.Services.Models;

namespace Betting.Services.Interfaces
{
	public interface IMatchService
	{
		Task<MatchesResponseModel> GetBettingInfo();

		Task<MatchesResponseModel> GetMatchesForTimePeriod(int hours);

		Task<MatchResponseModel> GetMatchById(Guid matchId);
	}
}
