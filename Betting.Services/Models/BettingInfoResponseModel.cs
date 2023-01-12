using Betting.Data.Entities;
namespace Betting.Services.Models
{
	public class BettingInfoResponseModel
	{
		public Sport Sport { get; set; }

		public ICollection<Event> Events{ get; set; }
	}
}
