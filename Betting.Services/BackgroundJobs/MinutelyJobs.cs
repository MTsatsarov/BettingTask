using Betting.Services.Interfaces;

namespace Betting.Services.BackgroundJobs
{
	public class MinutelyJobs
	{
		private readonly IMatchService matchService;

		public MinutelyJobs(IMatchService matchService)
		{
			this.matchService = matchService;
		}

		public async Task StartJobs()
		{
			try
			{
				await this.matchService.GetBettingInfo();

			}
			catch (Exception e)
			{
				var a = e.Message;
				throw;
			}
		}
	}
}
