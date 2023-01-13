using Betting.Services.Interfaces;
using Betting.Web.Hubs;
using Betting.Web.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Betting.Web.Background
{
	public class MinutelyJobs
	{
		private readonly IMatchService matchService;
		private readonly IHubContext<MatchHub, IMatchHub> hubContext;

		public MinutelyJobs(IMatchService matchService, IHubContext<MatchHub, IMatchHub> hubContext)
		{
			this.matchService = matchService;
			this.hubContext = hubContext;
		}

		public async Task StartJobs()
		{
			try
			{
				var matchesResponse = await this.matchService.GetBettingInfo();
				await this.hubContext.Clients.All.NotifyUsers(matchesResponse.Matches);
			}
			catch (Exception e)
			{
				var a = e.Message;
				throw;
			}
		}
	}
}
