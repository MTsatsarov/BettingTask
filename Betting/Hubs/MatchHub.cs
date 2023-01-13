using Betting.Services.Models;
using Betting.Web.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Betting.Web.Hubs
{
	public class MatchHub:Hub<IMatchHub>
	{
	}
}
