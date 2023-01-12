using Betting.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Betting.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MatchController : ControllerBase
	{
		private readonly IMatchService matchService;

		public MatchController(IMatchService matchService)
		{
			this.matchService = matchService;
		}

		[HttpGet]
		[Route("getMatches")]
		//Not specified in the requirements
		//[Authorize]
		public async Task<IActionResult> GetMatchesForTimePeriod(int hours = 24)
		{
			var response = await this.matchService.GetMatchesForTimePeriod(hours);
			return this.Ok(response);
		}

		[HttpGet]
		[Route("getMatch")]
		//Not specified in the requirements
		//[Authorize]
		public async Task<IActionResult> GetMatchById(Guid matchId)
		{
			return this.Ok();
		}

	}
}
